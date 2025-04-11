using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Flagrum.Core.Persistence;
using MemoryPack;

namespace Flagrum.Application.Features.ModManager.Data;

[MemoryPackable]
public partial class ModsState
{
    [MemoryPackIgnore] public string FilePath { get; set; }

    [MemoryPackInclude]
    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Local")]
    private Dictionary<Guid, ModState> ModStates { get; set; } = new();

    public bool Contains(Guid guid)
    {
        return ModStates.ContainsKey(guid);
    }

    public void Add(Guid modId, ModState state)
    {
        lock (this)
        {
            ModStates.Add(modId, state);
            Repository.Save(this, FilePath);
        }
    }

    public void Remove(Guid modId)
    {
        lock (this)
        {
            ModStates.Remove(modId);
            Repository.Save(this, FilePath);
        }
    }

    public bool GetActive(Guid modId)
    {
        return ModStates[modId].IsActive;
    }

    public void SetActive(Guid modId, bool isActive)
    {
        lock (this)
        {
            ModStates[modId].IsActive = isActive;
            Repository.Save(this, FilePath);
        }
    }

    public void SetAllInactive()
    {
        lock (this)
        {
            foreach (var guid in ModStates.Keys)
            {
                ModStates[guid].IsActive = false;
            }

            Repository.Save(this, FilePath);
        }
    }

    public bool GetPinned(Guid modId)
    {
        return ModStates[modId].IsPinned;
    }

    public void SetPinned(Guid modId, bool isPinned)
    {
        lock (this)
        {
            ModStates[modId].IsPinned = isPinned;
            Repository.Save(this, FilePath);
        }
    }

    public bool GetAnyActive()
    {
        return ModStates.Values.Any(s => s.IsActive);
    }
}

[MemoryPackable]
public partial class ModState
{
    public bool IsActive { get; set; }
    public bool IsPinned { get; set; }
}