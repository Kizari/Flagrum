using System;
using System.Collections.Generic;
using Flagrum.Core.Scripting.Ebex.Configuration;
using Flagrum.Core.Scripting.Ebex.Type;

namespace Flagrum.Core.Scripting.Ebex.Data;

public class EntityPresetUtility
{
    public const string ENTITY_PRESET_EXTENSION = ".preset";
    public const string DEFAULT_ENTITYTYPE_NAME = "entity";
    public const string PRESET_FILEPATH_VARIABLE_NAME = "presetFilePath_";
    public const string USE_PRESET_DIFFERENCE_ATTRIBUTE = "UsePresetDifference";
    public const string NO_PRESET_SAVE_ATTRIBUTE = "NoPresetSave";


    public static bool IsEnablePresetDifference(Entity entity)
    {
        var flag = false;
        entity.DataType.TryGetAttributeBool("UsePresetDifference", out flag);
        if (flag)
        {
            flag = !string.IsNullOrEmpty(entity.GetString("presetFilePath_"));
        }

        return flag;
    }

    public static bool HasPresetFilePath(Entity entity)
    {
        return !string.IsNullOrEmpty(entity.GetString("presetFilePath_"));
    }

    public static bool SaveEntityPreset(Entity baseEntity)
    {
        if (baseEntity["presetFilePath_"] == null)
        {
            return false;
        }

        var dataFullPath = Project.GetDataFullPath(baseEntity["presetFilePath_"].Value.ToString());
        return saveEntityPresetCommon(baseEntity, dataFullPath);
    }

    public static bool SaveEntityPresetAs(Entity baseEntity)
    {
        var dataType = baseEntity.DataType;
        var attribute = dataType.GetAttribute("PresetExtension");
        string str1;
        string str2;
        if (!string.IsNullOrEmpty(attribute))
        {
            str1 = attribute + ".preset";
            str2 = dataType.DisplayName + "プリセット";
        }
        else
        {
            str1 = "entity.preset";
            str2 = "Entity プリセット（デフォルト拡張子）";
        }

        var flag1 = false;
        try
        {
            // var saveFileDialog = new SaveFileDialog();
            // saveFileDialog.Title = TextResourcesJenovaData.EntityPresetSaveMessage;
            // saveFileDialog.DefaultExt = str1;
            // saveFileDialog.Filter = str2 + "|*." + str1 + "|すべてのファイル|*.*";
            // saveFileDialog.ValidateNames = true;
            // saveFileDialog.AddExtension = true;
            // var nullable = saveFileDialog.ShowDialog();
            // var flag2 = true;
            // if ((nullable.GetValueOrDefault() == flag2) & nullable.HasValue)
            // {
            //     var fileName = saveFileDialog.FileName;
            //     flag1 = saveEntityPresetCommon(baseEntity, fileName);
            // }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Jenova: error: {0} {1}", ex.Message, ex.StackTrace);
            flag1 = false;
        }

        return flag1;
    }

    public static Entity CreateEntityFromEntityPreset(
        EntityGroup parentGroup,
        string filePath)
    {
        if (parentGroup == null)
        {
            return null;
        }

        var entity = loadEntityPreset(filePath);
        if (entity != null)
        {
            var entities = parentGroup.Entities;
            entities.Add(entity);
            entity.Parent = entities;
        }

        DocumentInterface.DocumentContainer.Select(getEntityPackage(parentGroup));
        return entity;
    }

    public static Entity ApplyEntityPresetFromSavedPath(Entity entity, bool isClearDifference = false)
    {
        var relativePath = entity.GetString("presetFilePath_");
        return !string.IsNullOrEmpty(relativePath)
            ? ApplyEntityPreset(entity, Project.GetDataFullPath(relativePath), isClearDifference)
            : entity;
    }

    public static Entity ApplyEntityPreset(
        Entity entity,
        string filePath,
        bool isClearDifference = true)
    {
        if (entity == null)
        {
            return null;
        }

        var entity1 = loadEntityPreset(filePath);
        if (entity1 == null)
        {
            return entity;
        }

        if (entity1.DataType.FullName != entity.DataType.FullName)
        {
            if (DocumentInterface.DocumentContainer != null)
            {
                throw new Exception("WIP");
            }

            return null;
        }

        var name = entity.Name;
        var dataItemList = new List<DataItem>();
        foreach (var child in entity.Children)
        {
            if (child.Field != null)
            {
                var flag = false;
                child.Field.TryGetAttributeBool("NoPresetSave", out flag);
                if (flag)
                {
                    entity1[child.Name].Value = child.Value;
                }
            }
        }

        if (!isClearDifference && entity["differences"] != null)
        {
            var targetArray = new DynamicArray(entity1);
            targetArray.name = "differences";
            targetArray.Field = new Field(targetArray.Name, "SQEX.Ebony.Std.DynamicArray", false);
            targetArray.Browsable = false;
            ((DynamicArray)entity["differences"]).CloneArrayTo(targetArray);
        }

        var num1 = entity.ParentPackageWithoutPrefab.Modified ? 1 : 0;
        var parent = entity.Parent;
        var insertPosition = parent.Children.IndexOf(entity);
        DocumentInterface.DocumentAction.Delete(entity);
        parent.Insert(insertPosition, entity1);
        entity1.Parent = parent;
        entity1.name = name;
        if (num1 == 0)
        {
            entity1.ParentPackageWithoutPrefab.Modified = false;
        }

        return entity1;
    }

    private static bool saveEntityPresetCommon(Entity baseEntity, string filePath)
    {
        var package = new EntityPackage(null,
            DocumentInterface.ModuleContainer["SQEX.Ebony.Framework.Entity.EntityPackage"]);
        var entities = package.Entities;
        entities.Add(baseEntity);
        var parent = baseEntity.Parent;
        baseEntity.Parent = entities;
        baseEntity.GetString("presetFilePath_");
        baseEntity.SetString("presetFilePath_", "");
        baseEntity.ClearDiffernce();
        var flag = false;
        using (var xmlWriter = new EbexWriter(package, filePath, false, false))
        {
            if (!xmlWriter.Write(filePath))
            {
                if (DocumentInterface.DocumentContainer != null)
                {
                    // var num = (int)DocumentInterface.DocumentContainer.ShowMessageBox(
                    //     TextResourcesJenovaData.EntityPresetSaveError + "\n" + filePath);
                    throw new Exception("WIP");
                }

                flag = false;
            }
            else
            {
                flag = true;
                // if (DocumentInterface.DocumentContainer != null)
                // {
                //     if (DocumentInterface.DocumentContainer.ShowMessageBox(
                //             TextResourcesJenovaData.EntityPresetSubmitConfirm, "Submit", MessageBoxButton.YesNo,
                //             MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.Yes)
                //     {
                //         SourceControl.Add(filePath);
                //         SourceControl.CheckOut(filePath);
                //         var str = Path.Combine(Luminous.EnvironmentSettings.EnvironmentSettings.GetSDKPath(),
                //             "tools\\perforce\\submitInP4V.bat");
                //         Process.Start(new ProcessStartInfo
                //         {
                //             CreateNoWindow = true,
                //             UseShellExecute = false,
                //             FileName = Environment.GetEnvironmentVariable("ComSpec"),
                //             Arguments = string.Format("/c " + str + " " + filePath)
                //         });
                //     }
                // }
            }
        }

        baseEntity.Parent = parent;
        package.Entities.Remove(baseEntity);
        baseEntity.SetString("presetFilePath_", Project.GetDataRelativePath(filePath));
        return flag;
    }

    private static Entity loadEntityPreset(string filePath)
    {
        var entityPackage1 = new EntityPackage(null,
            DocumentInterface.ModuleContainer["SQEX.Ebony.Framework.Entity.EntityPackage"]);
        DataItem dataItem = null;
        using (var xmlReader = new EbexReader(entityPackage1))
        {
            if (!(xmlReader.Read(filePath) is EntityPackage entityPackage3) || entityPackage3.Entities.Count != 1)
            {
                if (DocumentInterface.DocumentContainer != null)
                {
                    throw new Exception("WIP");
                    // var num = (int)DocumentInterface.DocumentContainer.ShowMessageBox(
                    //     TextResourcesJenovaData.EntityPresetLoadError + "\n" + filePath);
                }
            }
            else
            {
                dataItem = entityPackage3.Entities[0];
                dataItem.Parent = null;
                entityPackage3.Entities.Remove(dataItem);
                dataItem.SetString("presetFilePath_", Project.GetDataRelativePath(filePath));
            }
        }

        return (Entity)dataItem;
    }

    private static EntityPackage getEntityPackage(DataItem item)
    {
        while (true)
        {
            switch (item)
            {
                case null:
                case EntityPackage _:
                    goto label_3;
                default:
                    item = item.Parent;
                    continue;
            }
        }

        label_3:
        return item as EntityPackage;
    }
}