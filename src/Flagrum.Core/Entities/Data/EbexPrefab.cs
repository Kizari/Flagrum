using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Flagrum.Core.Scripting.Ebex.Configuration;
using Flagrum.Core.Scripting.Ebex.Type;

namespace Flagrum.Core.Scripting.Ebex.Data;

public class Prefab : EntityPackage
{
    public new const string ClassFullName = "SQEX.Ebony.Framework.Entity.Prefab.Prefab";
    public const string COLLISION_ENTITY_FULLNAME = "Black.Entity.MeshCoillisionEntity";
    private const string COLLISION_FILE_SUFFIX = "_COL.cmdl";
    private const string COLLISION_SOURCE_PATH_PROPERTY_NAME = "sourcePath_";
    public new const string EXTENSION = ".prefab";
    public new const string FILEDESCRIPTION = "プレハブ";
    private static bool differenceMode;
    private static string actualCreatedClassFullName;

    public Prefab(DataItem parent, DataType dataType)
        : base(parent, dataType) { }


    public static bool DifferenceMode
    {
        get => differenceMode;
        set
        {
            differenceMode = value;
            if (OnDifferenceModeChanged == null)
            {
                return;
            }

            OnDifferenceModeChanged();
        }
    }

    public static bool CharaEntryPrefabMode { get; set; }

    public override string Extension => ".prefab";

    public override string FileDescription => "プレハブ";

    public bool IsNewPrefab => string.IsNullOrEmpty(FullFilePath);

    public bool HasDifference => this["differences"] != null;

    public bool IsToArchiveDiffResource => GetBool("bArchiveDiffResource_");

    public static event Action OnDifferenceModeChanged;

    public void UpdateFromThisInstance(
        Prefab targetPrefab,
        List<SequenceContainer> referenceSequenceContainerList = null)
    {
        targetPrefab.reset(false, referenceSequenceContainerList);
        targetPrefab.Modified = false;
    }

    public void Reset()
    {
        reset(true);
    }

    public void ResetOneEntity(DataItem resetedEntity)
    {
        var parent = new EntityGroup(null);
        DataItem dataItem = null;
        using (var xmlReader = new EbexReader(parent))
        {
            dataItem = xmlReader.Read(FullFilePath) as Prefab;
        }

        if (dataItem == null)
        {
            // var num = (int)DocumentInterface.DocumentContainer.ShowMessageBox("Fail to load the file : \n" +
            //     FullFilePath);
        }
        else
        {
            foreach (var entity in ((EntityGroup)dataItem).Entities)
            {
                if (entity.Name == resetedEntity.Name)
                {
                    var insertPosition = Entities.Children.IndexOf(resetedEntity);
                    resetedEntity.Dispose();
                    Entities.Insert(insertPosition, entity);
                    entity.Parent = Entities;
                    ClearDiffernce(entity);
                    break;
                }
            }
        }
    }

    private void reset(
        bool isClearDifference,
        List<SequenceContainer> referenceSequenceContainerList = null)
    {
        List<Tuple<DataItem, string>> tupleList = null;
        if (DocumentInterface.DocumentContainer != null)
        {
            tupleList = new List<Tuple<DataItem, string>>();
            var result = new List<Entity>();
            AccumulateEntities(result, true);
            result.Insert(0, this);
            foreach (var entity in result)
            {
                foreach (var referencedItem in entity.GetReferencedItemList())
                {
                    var parentPackage = referencedItem.ParentPackage;
                    if (parentPackage != this && !ContainsAsSubPackage(parentPackage, true))
                    {
                        if (referencedItem.Value is Value obj7 && obj7.PrimitiveType == PrimitiveType.Pointer)
                        {
                            tupleList.Add(new Tuple<DataItem, string>(referencedItem, entity.FullPath.ToString()));
                        }

                        if (referenceSequenceContainerList != null &&
                            referencedItem.ParentEntity is SequenceContainer &&
                            !referenceSequenceContainerList.Contains(referencedItem.ParentEntity))
                        {
                            referenceSequenceContainerList.Add((SequenceContainer)referencedItem.ParentEntity);
                        }
                    }
                }
            }
        }

        ClearEntities();
        var parent = new EntityGroup(null);
        Prefab targetPrefab1 = null;
        using (var xmlReader = new EbexReader(parent))
        {
            if (xmlReader.Read(FullFilePath) is Prefab targetPrefab2)
            {
                targetPrefab1 = targetPrefab2;
                ResolveReferenceAfterPrefabReset(targetPrefab1, xmlReader.UnresolvedReferenceInfo);
            }
            // var num = (int)DocumentInterface.DocumentContainer.ShowMessageBox("Fail to load the file : \n" +
            //     FullFilePath);
        }

        if (targetPrefab1 == null)
        {
            return;
        }

        foreach (var entity in targetPrefab1.Entities)
        {
            Entities.Add(entity);
            entity.Parent = Entities;
        }

        if (DocumentInterface.DocumentContainer != null && tupleList != null)
        {
            foreach (var tuple in tupleList)
            {
                tuple.Item1.Value =
                    new Value(DocumentInterface.DocumentContainer.FindDataItem(new ItemPath(tuple.Item2)));
            }
        }

        if (isClearDifference)
        {
            ClearDiffernce();
        }
        else
        {
            ResolveDifference();
        }
    }

    public static void ResolveReferenceAfterPrefabReset(
        Prefab targetPrefab,
        List<EbexReader.ReaderUnresolvedReferenceInfo> unresolvedReferenceInfoList)
    {
        foreach (var unresolvedReferenceInfo in unresolvedReferenceInfoList)
        {
            var dataItem = unresolvedReferenceInfo.DataItem_;
            var itemPathString = unresolvedReferenceInfo.ItemPathString_;
            var sourcePathString = unresolvedReferenceInfo.PackageSourcePathString_;
            var parentPackage = dataItem.ParentPackage;
            var input = unresolvedReferenceInfo.ProxyConnectionOwnerPackageName_;
            EntityPackage entityPackage = null;
            if (sourcePathString != null)
            {
                if (input != null)
                {
                    if (sourcePathString.EndsWith(".prefab"))
                    {
                        entityPackage = parentPackage;
                    }
                    else
                    {
                        input = Regex.Replace(input, "\\(.+?\\)", "");
                        var relativePath = new ItemPath("entities_." + input);
                        entityPackage = parentPackage.GetChild(relativePath) as EntityPackage;
                    }
                }

                if (entityPackage == null)
                {
                    entityPackage =
                        targetPrefab.FindEntityPackageWithSourcePath(Project.GetDataFullPath(sourcePathString),
                            true);
                }
            }

            if (entityPackage != null)
            {
                var child = entityPackage.GetChild(new ItemPath(itemPathString));
                if (child != null)
                {
                    var relativePathString = unresolvedReferenceInfo.RelativePathString_;
                    if (relativePathString != null)
                    {
                        child = child.GetChild(new ItemPath(relativePathString));
                    }

                    if (child == null)
                    {
                        if (DocumentInterface.DocumentContainer != null)
                        {
                            // DocumentInterface.DocumentContainer.PrintWarning(
                            //     "ImportDataItem : UnresolvedReferenceInfo with RelativePath : referenceItem == null : relativePath=" +
                            //     relativePathString + " : path=" + itemPathString + " : packpath=" +
                            //     sourcePathString);
                        }
                    }
                    else if (dataItem is DynamicArray)
                    {
                        if (input != null)
                        {
                            dataItem.AddForceWithoutModifyFlag(child);
                        }
                        else
                        {
                            dataItem.AddWithoutModifyFlag(child);
                        }
                    }
                    else
                    {
                        dataItem.Value = new Value(PrimitiveType.Pointer, child);
                    }
                }
                else if (itemPathString == null)
                {
                    if (DocumentInterface.DocumentContainer != null)
                    {
                        // DocumentInterface.DocumentContainer.PrintWarning(
                        //     "ImportDataItem : UnresolvedReferenceInfo : targetPackage == null : path == null");
                    }
                }
                else if (DocumentInterface.DocumentContainer != null)
                {
                    // DocumentInterface.DocumentContainer.PrintWarning(
                    //     "ImportDataItem : UnresolvedReferenceInfo : referenceItem == null : path=" +
                    //     itemPathString + " : packpath=" + sourcePathString);
                }
            }
            else if (itemPathString == null)
            {
                if (DocumentInterface.DocumentContainer != null)
                {
                    // DocumentInterface.DocumentContainer.PrintWarning(
                    //     "ImportDataItem : UnresolvedReferenceInfo : targetPackage == null : path == null");
                }
            }
            else if (DocumentInterface.DocumentContainer != null)
            {
                // DocumentInterface.DocumentContainer.PrintWarning(
                //     "ImportDataItem : UnresolvedReferenceInfo : targetPackage == null : path=" + itemPathString +
                //     " : packpath=" + sourcePathString);
            }
        }
    }

    private string getCollisionFilePath()
    {
        return Path.Combine(Path.GetDirectoryName(FullFilePath),
            Path.GetFileNameWithoutExtension(FullFilePath) + "_COL.cmdl");
    }

    public bool IsExistCollisionFile()
    {
        return File.Exists(getCollisionFilePath());
    }

    public Entity GetCollisionEntity()
    {
        foreach (var entity in Entities)
        {
            if (entity is Entity && entity.DataType.FullName == "Black.Entity.MeshCoillisionEntity")
            {
                return (Entity)entity;
            }
        }

        return null;
    }

    public void SetCollisionFileToCollisionEntity()
    {
        var collisionEntity = GetCollisionEntity();
        if (collisionEntity == null)
        {
            return;
        }

        var collisionFilePath = getCollisionFilePath();
        collisionEntity.SetString("sourcePath_", Project.GetDataRelativePath(collisionFilePath));
    }

    public static string GetActualCreatedClassFullName()
    {
        if (actualCreatedClassFullName == null)
        {
            var basedOnAll = DocumentInterface.ModuleContainer.GetBasedOnAll("SQEX.Ebony.Framework.Entity.Entity");
            var baseType = DocumentInterface.ModuleContainer["SQEX.Ebony.Framework.Entity.Prefab.Prefab"] as Class;
            foreach (var @class in basedOnAll)
            {
                if (@class.Browsable && !@class.Deprecated && @class.IsBasedOn(baseType))
                {
                    actualCreatedClassFullName = @class.FullName;
                    break;
                }
            }

            if (actualCreatedClassFullName == null)
            {
                actualCreatedClassFullName = "SQEX.Ebony.Framework.Entity.Prefab.Prefab";
            }
        }

        return actualCreatedClassFullName;
    }

    public static bool CanCreatePrefab(List<DataItem> entities)
    {
        if (entities.Count == 0)
        {
            // var num = (int)DocumentInterface.DocumentContainer.ShowMessageBox(TextResourcesJenovaData
            //     .PrefabMessageNoEntity);
            return false;
        }

        var parentGroup = entities[0].ParentGroup;
        foreach (var entity in entities)
        {
            if (entity is EntityPackage && !(entity is Prefab))
            {
                // var num = (int)DocumentInterface.DocumentContainer.ShowMessageBox(TextResourcesJenovaData
                //     .PrefabMessageIncludePackage);
                return false;
            }

            if (entity.ParentGroup != parentGroup)
            {
                // var num = (int)DocumentInterface.DocumentContainer.ShowMessageBox(TextResourcesJenovaData
                //     .PrefabMessageDifferentParent);
                return false;
            }
        }

        return true;
    }
}