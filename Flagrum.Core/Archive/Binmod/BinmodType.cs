using System;
using System.Collections.Generic;
using System.Linq;

namespace Flagrum.Core.Archive.Binmod;

public enum BinmodType
{
    Cloth,
    Weapon,
    Character,
    StyleEdit,
    Multi_Weapon
}

public enum OutfitSoloTarget
{
    Noctis,
    Prompto,
    Ignis,
    Gladiolus
}

public enum OutfitMultiTarget
{
    Tops,
    Bottoms,
    Gloves,
    Shoes,
    Accessory,
    Wear,
    Face,
    Hair
}

public enum WeaponSoloTarget
{
    Sword,
    Large_Sword,
    Spear,
    Dagger,
    Gun,
    Shield
}

public enum WeaponMultiTarget
{
    Katana,
    Mace,
    Spear,
    Dagger,
    Shuriken,
    Crossbow,
    Shield
}

public enum ModelReplacementTarget
{
    Noctis,
    Gladiolus,
    Prompto,
    Ignis,
    Lunafrena,
    Ardyn,
    Ravus,
    Gentiana,
    Aranea,
    Cid,
    Iris,
    Cor,
    Cidney,
    NPC_MALE,
    NPC_MALE_CHILD,
    NPC_FEMALE,
    NPC_FEMALE_CHILD,
    Sword,
    LargeSword,
    Spear,
    Dagger,
    Gun,
    Shield,
    Crossbow,
    Shuriken,
    Mace,
    Katana
}

public static class BinmodTypeHelper
{
    private static Dictionary<BinmodType, string> TypeNames { get; set; }
    private static Dictionary<string, BinmodType> TypeNamesReversed { get; set; }
    private static Dictionary<BinmodType, Dictionary<int, string>> Targets { get; set; }
    private static Dictionary<BinmodType, Dictionary<int, string>> ModmetaTargets { get; set; }
    private static Dictionary<BinmodType, Dictionary<string, int>> ModmetaTargetsReversed { get; set; }
    private static Dictionary<BinmodType, string> ModmetaTypes { get; set; }
    private static Dictionary<string, BinmodType> ModmetaTypesReversed { get; set; }

    public static string GetModmetaTypeName(int type)
    {
        CreateModmetaTypes();
        return ModmetaTypes[(BinmodType)type];
    }

    public static int GetModmetaTypeFromName(string name)
    {
        CreateModmetaTypes();
        return (int)ModmetaTypesReversed[name];
    }

    public static string GetDisplayName(BinmodType binmodType)
    {
        CreateTypeNames();
        return TypeNames[binmodType];
    }

    public static int FromName(string typeName)
    {
        CreateTypeNames();
        return (int)TypeNamesReversed[typeName];
    }

    public static Dictionary<int, string> GetTargets(int type)
    {
        Targets ??= new Dictionary<BinmodType, Dictionary<int, string>>
        {
            {
                BinmodType.Character,
                Enum.GetValues<ModelReplacementTarget>().ToDictionary(t => (int)t, t => t.ToString())
            },
            {BinmodType.Cloth, Enum.GetValues<OutfitSoloTarget>().ToDictionary(t => (int)t, t => t.ToString())},
            {BinmodType.StyleEdit, Enum.GetValues<OutfitMultiTarget>().ToDictionary(t => (int)t, t => t.ToString())},
            {BinmodType.Weapon, Enum.GetValues<WeaponSoloTarget>().ToDictionary(t => (int)t, t => t.ToString())},
            {BinmodType.Multi_Weapon, Enum.GetValues<WeaponMultiTarget>().ToDictionary(t => (int)t, t => t.ToString())}
        };

        return Targets[(BinmodType)type];
    }

    public static string GetModmetaTargetName(int binmodType, int binmodTarget)
    {
        CreateModmetaTargets();
        return ModmetaTargets[(BinmodType)binmodType][binmodTarget];
    }

    public static int GetBinmodTarget(int binmodType, string targetName)
    {
        CreateModmetaTargets();
        return ModmetaTargetsReversed[(BinmodType)binmodType][targetName];
    }

    public static int GetModelCount(int binmodType, int binmodTarget)
    {
        var type = (BinmodType)binmodType;

        if (type == BinmodType.Weapon)
        {
            var target = (WeaponSoloTarget)binmodTarget;
            if (target == WeaponSoloTarget.Dagger)
            {
                return 2;
            }
        }
        else if (type == BinmodType.Multi_Weapon)
        {
            var target = (WeaponMultiTarget)binmodTarget;
            if (target == WeaponMultiTarget.Dagger)
            {
                return 2;
            }
        }
        else if (type == BinmodType.StyleEdit)
        {
            var target = (OutfitMultiTarget)binmodTarget;
            if (target != OutfitMultiTarget.Accessory)
            {
                return 2;
            }
        }

        return 1;
    }

    private static void CreateModmetaTypes()
    {
        if (ModmetaTypes != null)
        {
            return;
        }

        ModmetaTypes = Enum.GetValues<BinmodType>()
            .ToDictionary(e => e, e => e.ToString().ToLower());

        ModmetaTypesReversed = ModmetaTypes.ToDictionary(t => t.Value, t => t.Key);
    }

    private static void CreateTypeNames()
    {
        if (TypeNames != null)
        {
            return;
        }

        TypeNames = new Dictionary<BinmodType, string>
        {
            {BinmodType.Character, "Model Replacement"},
            {BinmodType.Cloth, "Outfit"},
            {BinmodType.Weapon, "Weapon"},
            {BinmodType.Multi_Weapon, "Comrades Weapon"},
            {BinmodType.StyleEdit, "Comrades Outfit"}
        };

        TypeNamesReversed = TypeNames.ToDictionary(t => t.Value, t => t.Key);
    }

    private static void CreateModmetaTargets()
    {
        if (ModmetaTargets != null)
        {
            return;
        }

        ModmetaTargets = new Dictionary<BinmodType, Dictionary<int, string>>
        {
            {
                BinmodType.Character,
                Enum.GetValues<ModelReplacementTarget>().ToDictionary(t => (int)t, t => t.ToString())
            },
            {
                BinmodType.Cloth,
                Enum.GetValues<OutfitSoloTarget>().ToDictionary(t => (int)t, t => t.ToString().ToLower())
            },
            {
                BinmodType.StyleEdit,
                Enum.GetValues<OutfitMultiTarget>().ToDictionary(t => (int)t, t => t.ToString().ToLower())
            },
            {
                BinmodType.Weapon,
                Enum.GetValues<WeaponSoloTarget>().ToDictionary(t => (int)t, t => t.ToString().ToLower())
            },
            {
                BinmodType.Multi_Weapon,
                Enum.GetValues<WeaponMultiTarget>().ToDictionary(t => (int)t, t => t.ToString().ToLower())
            }
        };

        ModmetaTargetsReversed =
            ModmetaTargets.ToDictionary(t => t.Key, t => t.Value
                .ToDictionary(v => v.Value, v => v.Key, StringComparer.OrdinalIgnoreCase));
    }
}