using System;
using System.Collections.Generic;
using System.Linq;
using Flagrum.Core.Archive;

namespace Flagrum.Web.Services;

public class BinmodTypeHelper
{
    private readonly ModelReplacementPresets _modelReplacementPresets;
    private readonly SettingsService _settings;

    public BinmodTypeHelper(
        SettingsService settings,
        ModelReplacementPresets presets)
    {
        _settings = settings;
        _modelReplacementPresets = presets;
    }

    private Dictionary<BinmodType, string> TypeNames { get; set; }
    private Dictionary<string, BinmodType> TypeNamesReversed { get; set; }
    private Dictionary<BinmodType, Dictionary<int, string>> Targets { get; set; }
    private Dictionary<BinmodType, Dictionary<int, string>> ModmetaTargets { get; set; }
    private Dictionary<BinmodType, Dictionary<string, int>> ModmetaTargetsReversed { get; set; }
    private Dictionary<BinmodType, string> ModmetaTypes { get; set; }
    private Dictionary<string, BinmodType> ModmetaTypesReversed { get; set; }

    public string GetModmetaTypeName(int type)
    {
        CreateModmetaTypes();
        return ModmetaTypes[(BinmodType)type];
    }

    public int GetModmetaTypeFromName(string name)
    {
        CreateModmetaTypes();
        return (int)ModmetaTypesReversed[name];
    }

    public string GetDisplayName(BinmodType binmodType)
    {
        CreateTypeNames();
        return TypeNames[binmodType];
    }

    public int FromName(string typeName)
    {
        CreateTypeNames();
        return (int)TypeNamesReversed[typeName];
    }

    public Dictionary<int, string> GetTargets(int type)
    {
        Targets ??= new Dictionary<BinmodType, Dictionary<int, string>>
        {
            {
                BinmodType.Character,
                _modelReplacementPresets.GetReplacementDictionary()
            },
            {BinmodType.Cloth, Enum.GetValues<OutfitSoloTarget>().ToDictionary(t => (int)t, t => t.ToString())},
            {BinmodType.StyleEdit, Enum.GetValues<OutfitMultiTarget>().ToDictionary(t => (int)t, t => t.ToString())},
            {BinmodType.Weapon, Enum.GetValues<WeaponSoloTarget>().ToDictionary(t => (int)t, t => t.ToString())},
            {BinmodType.Multi_Weapon, Enum.GetValues<WeaponMultiTarget>().ToDictionary(t => (int)t, t => t.ToString())}
        };

        return Targets[(BinmodType)type];
    }

    public string GetModmetaTargetName(int binmodType, int binmodTarget)
    {
        CreateModmetaTargets();
        return ModmetaTargets[(BinmodType)binmodType][binmodTarget];
    }

    public int GetBinmodTarget(int binmodType, string targetName)
    {
        CreateModmetaTargets();
        return ModmetaTargetsReversed[(BinmodType)binmodType][targetName];
    }

    public int GetModelCount(int binmodType, int binmodTarget)
    {
        var type = (BinmodType)binmodType;

        if (type == BinmodType.Weapon)
        {
            var target = (WeaponSoloTarget)binmodTarget;
            if (target is WeaponSoloTarget.Dagger or WeaponSoloTarget.Gun)
            {
                return 2;
            }
        }
        else if (type == BinmodType.Multi_Weapon)
        {
            var target = (WeaponMultiTarget)binmodTarget;
            if (target is WeaponMultiTarget.Dagger or WeaponMultiTarget.Katana or WeaponMultiTarget.Shuriken)
            {
                return 2;
            }
        }
        else if (type == BinmodType.StyleEdit)
        {
            var target = (OutfitMultiTarget)binmodTarget;
            if (target is not OutfitMultiTarget.Accessory and not OutfitMultiTarget.Hair)
            {
                return 2;
            }
        }

        return 1;
    }

    public Dictionary<int, string> GetModelNames(int binmodType, int binmodTarget)
    {
        var type = (BinmodType)binmodType;

        if (type == BinmodType.Weapon)
        {
            var target = (WeaponSoloTarget)binmodTarget;
            if (target is WeaponSoloTarget.Dagger or WeaponSoloTarget.Gun)
            {
                return new Dictionary<int, string>
                {
                    {0, "Right"},
                    {1, "Left"}
                };
            }
        }
        else if (type == BinmodType.Multi_Weapon)
        {
            var target = (WeaponMultiTarget)binmodTarget;
            if (target is WeaponMultiTarget.Dagger or WeaponMultiTarget.Katana or WeaponMultiTarget.Shuriken)
            {
                return new Dictionary<int, string>
                {
                    {0, "Right"},
                    {1, "Left"}
                };
            }
        }
        else if (type == BinmodType.StyleEdit)
        {
            var target = (OutfitMultiTarget)binmodTarget;
            if (target != OutfitMultiTarget.Accessory)
            {
                return new Dictionary<int, string>
                {
                    {0, "Slim"},
                    {1, "Chubby"}
                };
            }
        }

        return new Dictionary<int, string>
        {
            {0, "Model"}
        };
    }

    private void CreateModmetaTypes()
    {
        if (ModmetaTypes != null)
        {
            return;
        }

        ModmetaTypes = Enum.GetValues<BinmodType>()
            .ToDictionary(e => e, e => e.ToString().ToLower());

        ModmetaTypesReversed = ModmetaTypes.ToDictionary(t => t.Value, t => t.Key);
    }

    private void CreateTypeNames()
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

    private void CreateModmetaTargets()
    {
        if (ModmetaTargets != null)
        {
            return;
        }

        ModmetaTargets = new Dictionary<BinmodType, Dictionary<int, string>>
        {
            {
                BinmodType.Character,
                _modelReplacementPresets.GetReplacementModmetaDictionary()
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