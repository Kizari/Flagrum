using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flagrum.Abstractions;
using Flagrum.Core.Data;
using Flagrum.Core.Data.Bins;
using Flagrum.Core.Data.Tags;
using Flagrum.Core.Utilities;
using Flagrum.Core.Utilities.Extensions;
using Flagrum.Generators;
using Flagrum.Application.Features.ModManager.Mod;
using Flagrum.Application.Services;
using Flagrum.Application.Utilities;
using Injectio.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace Flagrum.Application.Features.ModManager.Instructions.Builders;

[RegisterScoped]
public partial class DataIndexBinaryDifferenceBuilder
{
    [Inject] private readonly AppStateService _appState;
    [Inject] private readonly IProfileService _profile;

    public async Task Build(int modId, string uri, byte[] data)
    {
        // Get the backup of the original file
        var hash = Cryptography.HashFileUri64(uri);
        var backupPath = Path.Combine(_profile.EarcModBackupsDirectory, $"{hash}.ffg");

        // Backup may not exist if a disabled mod is being exported, in this case pull the original from the archive
        byte[] originalData;
        if (File.Exists(backupPath))
        {
            var fragment = new FmodFragment();
            fragment.Read(backupPath);
            originalData = fragment.GetReadableData();
        }
        else
        {
            originalData = _appState.GetFileByUri(uri);
        }

        // Get the difference between the original and the replacement
        var difference = GenerateDifference(originalData, data);
        if (difference is {Count: > 0})
        {
            // Store the difference in the project folder for the mod (can create one for self-created mods)
            var path = Path.Combine(_profile.ModFilesDirectory, modId.ToString(), $"{hash}.fbd");
            IOHelper.EnsureDirectoriesExistForFilePath(path);
            await MessagePackHelper.SerializeCompressedAsync(path, difference);
        }
    }

    private List<ParameterTableDifference> GenerateDifference(byte[] originalData, byte[] newData)
    {
        try
        {
            var original = new DataIndexBinary();
            original.Read(originalData);

            var test = original.Write();
            if (!test.HashCompare(originalData))
            {
                // The writer didn't produce the correct output, so we can't rely on it to merge the binaries
                return null;
            }

            var modified = new DataIndexBinary();
            modified.Read(newData);

            return GenerateDifference(original, modified);
        }
        catch (Exception exception)
        {
            // If this fails for any reason (most likely the reader not yet supporting all permutations of the binary)
            // then just return null and the builder won't store the difference on disk
            return null;
        }
    }

    /// Ensures that the binary doesn't have any changes that this builder can't handle
    private List<ParameterTableDifference> GenerateDifference(DataIndexBinary original, DataIndexBinary modified)
    {
        var result = new List<ParameterTableDifference>();

        // Builder doesn't support adding or removing data indices
        if (original.Count != modified.Count)
        {
            return null;
        }

        for (var i = 0; i < original.Count; i++)
        {
            var originalItem = original.Items[i];
            var modifiedItem = modified.Items[i];

            // Builder doesn't support changing resource types
            if (originalItem.GetType() != modifiedItem.GetType())
            {
                return null;
            }

            if (originalItem is ParameterTableCollection originalCollection
                && modifiedItem is ParameterTableCollection modifiedCollection)
            {
                // Builder doesn't support adding or removing tables
                if (originalCollection.TableCount != modifiedCollection.TableCount)
                {
                    return null;
                }

                for (var t = 0; t < originalCollection.TableCount; t++)
                {
                    var originalTable = originalCollection.Tables[t];
                    var modifiedTable = modifiedCollection.Tables[t];

                    // Builder doesn't support table order changes or ID changes
                    if (originalTable.Id != modifiedTable.Id)
                    {
                        return null;
                    }

                    // Builder doesn't support adding or removing columns
                    if (originalTable.Tags.Length != modifiedTable.Tags.Length)
                    {
                        return null;
                    }

                    var difference = new ParameterTableDifference {Index = i, Fixid = originalTable.Id};

                    var originalRecordIds = originalTable.Elements.Select(e => e.Id).ToList();
                    var modifiedRecordIds = modifiedTable.Elements.Select(e => e.Id).ToList();
                    var matchingRecordIds = originalRecordIds.Intersect(modifiedRecordIds).ToList();

                    var newRecords = modifiedTable.Elements
                        .Where(e => !originalRecordIds.Contains(e.Id))
                        .ToList();

                    var deletedRecords = originalTable.Elements
                        .Where(e => !modifiedRecordIds.Contains(e.Id))
                        .ToList();

                    difference.NewRecords = newRecords.Select(r => new ParameterTableRecordDifference
                    {
                        Fixid = r.Id,
                        ColumnValues = originalTable.Tags.ToDictionary(tag => tag.Id, tag => r.Values[tag.Offset])
                    }).ToList();

                    difference.RemovedRecords = deletedRecords.Select(r => r.Id).ToList();

                    difference.ModifiedRecords = GetChangedRecords(
                        originalTable.Elements.Where(e => matchingRecordIds.Contains(e.Id)).ToList(),
                        modifiedTable.Elements.Where(e => matchingRecordIds.Contains(e.Id)).ToList(),
                        originalTable.Tags.ToDictionary(tag => tag.Id, tag => tag.Offset)
                    );

                    // Only add the difference if there were any differences
                    if (difference.NewRecords.Any() || difference.RemovedRecords.Any() ||
                        difference.ModifiedRecords.Any())
                    {
                        result.Add(difference);
                    }
                }
            }
        }

        return result;
    }

    private object GetValueByTag(ParameterTable table, ParameterTableElement element, ParameterTableTag tag)
    {
        if (tag.Flag.HasFlag(ParameterTableTagFlag.Array))
        {
            return table.GetArray(element, (ArrayTag)tag.Id);
        }

        return tag.Flag.HasFlag(ParameterTableTagFlag.Boolean)
            ? table.GetBoolean(element, (BooleanTag)tag.Id)
            : table.Get(element, tag.Id);
    }

    private List<ParameterTableRecordDifference> GetChangedRecords(List<ParameterTableElement> originalRecords,
        List<ParameterTableElement> modifiedRecords, Dictionary<uint, ushort> tags)
    {
        var results = new List<ParameterTableRecordDifference>();

        for (var i = 0; i < originalRecords.Count; i++)
        {
            var original = originalRecords[i];
            var modified = modifiedRecords[i];

            var hasChanges = false;
            var result = new ParameterTableRecordDifference {Fixid = original.Id};

            foreach (var (tag, offset) in tags)
            {
                var originalValue = original.Values[offset];
                var modifiedValue = modified.Values[offset];

                if (originalValue is object[] originalArray)
                {
                    if (!ObjectSequenceEqual(originalArray, (object[])modifiedValue))
                    {
                        hasChanges = true;
                        result.ColumnValues[tag] = modifiedValue;
                    }
                }
                else if (!ObjectEqual(originalValue, modifiedValue))
                {
                    hasChanges = true;
                    result.ColumnValues[tag] = modifiedValue;
                }
            }

            if (hasChanges)
            {
                results.Add(result);
            }
        }

        return results;
    }

    private bool ObjectSequenceEqual(object[] original, object[] modified)
    {
        return original[0] switch
        {
            uint => original.Cast<uint>().SequenceEqual(modified.Cast<uint>()),
            int => original.Cast<int>().SequenceEqual(modified.Cast<int>()),
            float => original.Cast<float>().SequenceEqual(modified.Cast<float>()),
            _ => throw new Exception("Unhandled array item type")
        };
    }

    private bool ObjectEqual(object original, object modified)
    {
        return original switch
        {
            uint u => u == (uint)modified,
            int i => i == (int)modified,
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            float f => f == (float)modified,
            _ => throw new Exception("Undhandled item type")
        };
    }
}