using System;
using System.Collections.Generic;
using Flagrum.Core.Scripting.Ebex.Type;

namespace Flagrum.Core.Scripting.Ebex.Data;

public class EntityPackage : EntityPackageReference
{
    public enum EarcSizeDisplayPlatform
    {
        None,
        Win,
        PS4,
        XB1
    }

    public new const string ClassFullName = "SQEX.Ebony.Framework.Entity.EntityPackage";

    public EntityPackage(DataItem parent, DataType dataType)
        : base(parent, dataType)
    {
        DupulicatedOtherPackageList = new List<WeakReference<EntityPackage>>();
        SetString("name_", System.Guid.NewGuid().ToString());
        EarcSizeWin = 0L;
        EarcSizePS4 = 0L;
        EarcSizeXB1 = 0L;
    }

    public string UniqueName => this["name_"].Value.ToString();

    public virtual bool StartupLoad
    {
        get => true;
        set => SetBoolWithoutModifyFlag("startupLoad_", true);
    }

    public bool IsDirty { get; set; }
    public bool IsEditorDirty { get; set; }
    public virtual bool CanEditStartupLoad => false;

    public virtual bool StartupLoadAtSaveingAsTopPackage => true;

    public bool MarkedAsBuildTarget { get; set; }

    public bool IsRootPackage => ParentPackage == null;

    public virtual bool AlreadyReadObjectsAtLoading { get; set; }

    public bool ModifiedTrayNodeIndexAtLoading { get; set; }

    public long EarcSizeWin { get; set; }

    public long EarcSizePS4 { get; set; }

    public long EarcSizeXB1 { get; set; }

    public bool EarcCopyguard { get; set; }

    public DynamicArray PrefabDiffResourcePathList => this["prefabDiffResourcePathList_"] as DynamicArray;

    public List<WeakReference<EntityPackage>> DupulicatedOtherPackageList { get; }

    public bool ToExcludeToSendRuntimeAsDupulicatedPackage => DupulicatedOtherPackageList.Count > 0 &&
                                                              !IsRepresentativePackageInDupulicatedOnes &&
                                                              CheckDupulicatedEdit(false) != null;

    public bool IsRepresentativePackageInDupulicatedOnes { get; private set; }

    public long GetTotalEarcSize(
        EarcSizeDisplayPlatform platform,
        bool includeLazyCharaResource = true)
    {
        long num = 0;
        switch (platform)
        {
            case EarcSizeDisplayPlatform.Win:
                num += EarcSizeWin;
                break;
            case EarcSizeDisplayPlatform.PS4:
                num += EarcSizePS4;
                break;
            case EarcSizeDisplayPlatform.XB1:
                num += EarcSizeXB1;
                break;
        }

        var result = new List<EntityPackage>();
        Func<EntityPackage, bool> judgeFunc = package => package.IsChecked && package.StartupLoad &&
                                                         (!(package is CharaEntryPackage) ||
                                                          ((includeLazyCharaResource ||
                                                            ((CharaEntryPackage) package).LoadPriority <
                                                            CharaEntryPackage.LOAD_PRIORITY.LOAD_PRIORITY_2) &&
                                                           !package.GetBool("noAutoLoad_")));
        RetrieveEntityPackagesWithFunc(result, true, false, judgeFunc);
        var stringList = new List<string>();
        foreach (var entityPackage in result)
        {
            if (entityPackage is Prefab)
            {
                var fullFilePath = entityPackage.FullFilePath;
                if (!stringList.Contains(fullFilePath))
                {
                    stringList.Add(fullFilePath);
                }
                else
                {
                    continue;
                }
            }

            switch (platform)
            {
                case EarcSizeDisplayPlatform.Win:
                    num += entityPackage.EarcSizeWin;
                    continue;
                case EarcSizeDisplayPlatform.PS4:
                    num += entityPackage.EarcSizePS4;
                    continue;
                case EarcSizeDisplayPlatform.XB1:
                    num += entityPackage.EarcSizeXB1;
                    continue;
                default:
                    continue;
            }
        }

        return num;
    }

    public void ClearDuplicatedInfo()
    {
        IsRepresentativePackageInDupulicatedOnes = false;
        DupulicatedOtherPackageList.Clear();
    }

    public EntityPackage CheckDupulicatedEdit(bool updateRepresentativeFlag)
    {
        if (DupulicatedOtherPackageList == null)
        {
            return null;
        }

        foreach (var dupulicatedOtherPackage in DupulicatedOtherPackageList)
        {
            EntityPackage entityPackage = null;
            ref var local = ref entityPackage;
            dupulicatedOtherPackage.TryGetTarget(out local);
            if (entityPackage != null && entityPackage.IsRepresentativePackageInDupulicatedOnes)
            {
                return entityPackage;
            }
        }

        if (updateRepresentativeFlag && DupulicatedOtherPackageList.Count > 0)
        {
            IsRepresentativePackageInDupulicatedOnes = true;
        }

        return null;
    }

    public bool ContainsAsDupulicatedPackage(EntityPackage package)
    {
        foreach (var dupulicatedOtherPackage in DupulicatedOtherPackageList)
        {
            EntityPackage entityPackage = null;
            ref var local = ref entityPackage;
            dupulicatedOtherPackage.TryGetTarget(out local);
            if (entityPackage == package)
            {
                return true;
            }
        }

        return false;
    }

    public void AddDupulicatedPackage(EntityPackage package)
    {
        if (package == this || ContainsAsDupulicatedPackage(package))
        {
            return;
        }

        DupulicatedOtherPackageList.Add(new WeakReference<EntityPackage>(package, false));
    }

    public void RemoveDupulicatedPackage(EntityPackage package)
    {
        if (package == this)
        {
            return;
        }

        var num = 0;
        var index = -1;
        var flag = true;
        if (package.IsRepresentativePackageInDupulicatedOnes)
        {
            flag = false;
            package.IsRepresentativePackageInDupulicatedOnes = false;
        }

        foreach (var dupulicatedOtherPackage in DupulicatedOtherPackageList)
        {
            EntityPackage entityPackage = null;
            ref var local = ref entityPackage;
            dupulicatedOtherPackage.TryGetTarget(out local);
            if (entityPackage == package)
            {
                index = num;
            }
            else if (!flag && entityPackage.Modified)
            {
                entityPackage.IsRepresentativePackageInDupulicatedOnes = true;
                flag = true;
            }

            ++num;
        }

        if (!flag && Modified)
        {
            IsRepresentativePackageInDupulicatedOnes = true;
        }

        if (index < 0)
        {
            return;
        }

        DupulicatedOtherPackageList.RemoveAt(index);
    }
}