using System.Collections.Generic;
using Flagrum.Core.Scripting.Ebex.Type;

namespace Flagrum.Core.Scripting.Ebex.Data;

public class CharaEntryPackage : EntityPackage
{
    public enum LOAD_PRIORITY
    {
        LOAD_PRIORITY_0,
        LOAD_PRIORITY_1,
        LOAD_PRIORITY_2,
        LOAD_PRIORITY_3,
        LOAD_PRIORITY_4,
        LOAD_PRIORITY_5
    }

    public new const string ClassFullName = "Black.Entity.Data.CharacterEntry.CharaEntryPackage";

    public CharaEntryPackage(DataItem parent, DataType dataType)
        : base(parent, dataType)
    {
        SetBoolWithoutModifyFlag("startupLoad_", true);
    }

    public LOAD_PRIORITY LoadPriority
    {
        get => (LOAD_PRIORITY)(DocumentInterface.ModuleContainer[
                "Black.Entity.Data.CharacterEntry.CharaEntryPackage.LOAD_PRIORITY"] as Enum)
            .ValueOf(GetEnum("loadPriority_"));
        set
        {
            var e = DocumentInterface.ModuleContainer[
                "Black.Entity.Data.CharacterEntry.CharaEntryPackage.LOAD_PRIORITY"] as Enum;
            var str = e.NameOf((int)value);
            SetEnum("loadPriority_", e, str);
        }
    }

    public void StoreNoAutoLoadFixID()
    {
        if (!(this["noAutoLoadCharaEntryFixIDList_"] is DynamicArray dynamicArray))
        {
            return;
        }

        dynamicArray.ClearChild();
        if (!GetBool("noAutoLoad_"))
        {
            return;
        }

        var result = new List<EntityPackage>();
        AccumulateEntityPackages(result, false);
        foreach (var entityPackage in result)
        {
            if (entityPackage.IsChecked)
            {
                foreach (var entity in entityPackage.Entities)
                {
                    if (entity.DataType.FullName == "Black.Entity.Data.CharacterEntry.CharacterEntry")
                    {
                        var dataItem1 = entity["EntryId"];
                        if (dataItem1 != null)
                        {
                            var dataItem2 = DynamicArray.AddDynamicArrayItemProperty(dynamicArray)["Value"];
                            if (dataItem2 != null)
                            {
                                dataItem2.Value = new Value(dataItem1.Value as Value);
                            }
                        }

                        break;
                    }
                }
            }
        }
    }
}