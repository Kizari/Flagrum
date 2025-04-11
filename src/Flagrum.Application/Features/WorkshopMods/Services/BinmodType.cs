using System;
using System.Collections.Generic;
using System.Linq;
using Flagrum.Core.Archive.Mod;

namespace Flagrum.Application.Services;

public class BinmodTypeHelper
{
    private readonly ModelReplacementPresets _modelReplacementPresets;

    public BinmodTypeHelper(
        ModelReplacementPresets presets)
    {
        _modelReplacementPresets = presets;
    }

    private Dictionary<WorkshopModType, string> TypeNames { get; set; }
    private Dictionary<string, WorkshopModType> TypeNamesReversed { get; set; }
    private Dictionary<WorkshopModType, Dictionary<int, string>> Targets { get; set; }
    private Dictionary<WorkshopModType, Dictionary<int, string>> ModmetaTargets { get; set; }
    private Dictionary<WorkshopModType, Dictionary<string, int>> ModmetaTargetsReversed { get; set; }
    private Dictionary<WorkshopModType, string> ModmetaTypes { get; set; }
    private Dictionary<string, WorkshopModType> ModmetaTypesReversed { get; set; }

    private Dictionary<int, string> TemporaryModmetaTargets { get; } = new();

    public string GetModmetaTypeName(int type)
    {
        CreateModmetaTypes();
        return ModmetaTypes[(WorkshopModType)type];
    }

    public int GetModmetaTypeFromName(string name)
    {
        CreateModmetaTypes();
        return (int)ModmetaTypesReversed[name];
    }

    public string GetDisplayName(WorkshopModType workshopModType)
    {
        CreateTypeNames();
        return TypeNames[workshopModType];
    }

    public Dictionary<int, string> GetTargets(int type)
    {
        Targets ??= new Dictionary<WorkshopModType, Dictionary<int, string>>
        {
            {
                WorkshopModType.Character,
                _modelReplacementPresets.GetReplacementDictionary()
            },
            {WorkshopModType.Cloth, Enum.GetValues<OutfitTarget>().ToDictionary(t => (int)t, t => t.ToString())},
            {
                WorkshopModType.StyleEdit,
                Enum.GetValues<ComradesStyleTarget>().ToDictionary(t => (int)t, t => t.ToString())
            },
            {WorkshopModType.Weapon, Enum.GetValues<WeaponTarget>().ToDictionary(t => (int)t, t => t.ToString())},
            {
                WorkshopModType.Multi_Weapon,
                Enum.GetValues<ComradesWeaponTarget>().ToDictionary(t => (int)t, t => t.ToString())
            }
        };

        return Targets[(WorkshopModType)type];
    }

    public string GetTargetName(int binmodType, int binmodTarget)
    {
        var targets = GetTargets(binmodType);
        return targets.TryGetValue(binmodTarget, out var value)
            ? value
            : _modelReplacementPresets.Repository.Presets
                .Where(p => p.Id == binmodTarget - 100)
                .Select(p => p.Name)
                .FirstOrDefault() ?? "Custom Model Replacement";
    }

    public string GetTargetTag(int binmodType, int binmodTarget)
    {
        Dictionary<int, string> targets;
        if (binmodType == (int)WorkshopModType.Character)
        {
            targets = _modelReplacementPresets.GetDefaultReplacements()
                .ToDictionary(t => t.Index, t => t.Name);
        }
        else
        {
            targets = GetTargets(binmodType);
        }

        return targets.TryGetValue(binmodTarget, out var value) ? value : "Other";
    }

    public string GetModmetaTargetName(int binmodType, int binmodTarget)
    {
        CreateModmetaTargets();
        var dictionary = ModmetaTargets[(WorkshopModType)binmodType];
        return dictionary.TryGetValue(binmodTarget, out var value) ? value : $"Custom_{binmodTarget}";
    }

    public int GetBinmodTarget(int binmodType, string targetName)
    {
        CreateModmetaTargets();
        var dictionary = ModmetaTargetsReversed[(WorkshopModType)binmodType];
        if (dictionary.TryGetValue(targetName, out var value))
        {
            return value;
        }

        var id = 100000;
        if (TemporaryModmetaTargets.Count > 0)
        {
            id = TemporaryModmetaTargets.Max(t => t.Key) + 1;
        }

        TemporaryModmetaTargets.Add(id, targetName);
        return id;
    }

    public int GetModelCount(int binmodType, int binmodTarget)
    {
        var type = (WorkshopModType)binmodType;

        switch (type)
        {
            case WorkshopModType.Weapon:
            {
                var target = (WeaponTarget)binmodTarget;
                if (target is WeaponTarget.Dagger or WeaponTarget.Gun)
                {
                    return 2;
                }

                break;
            }
            case WorkshopModType.Multi_Weapon:
            {
                var target = (ComradesWeaponTarget)binmodTarget;
                if (target is ComradesWeaponTarget.Dagger or ComradesWeaponTarget.Katana
                    or ComradesWeaponTarget.Shuriken)
                {
                    return 2;
                }

                break;
            }
            case WorkshopModType.StyleEdit:
            {
                var target = (ComradesStyleTarget)binmodTarget;
                if (target is not ComradesStyleTarget.Accessory and not ComradesStyleTarget.Hair)
                {
                    return 2;
                }

                break;
            }
        }

        return 1;
    }

    public Dictionary<int, string> GetModelNames(int binmodType, int binmodTarget)
    {
        var type = (WorkshopModType)binmodType;

        switch (type)
        {
            case WorkshopModType.Weapon:
            {
                var target = (WeaponTarget)binmodTarget;
                if (target is WeaponTarget.Dagger or WeaponTarget.Gun)
                {
                    return new Dictionary<int, string>
                    {
                        {0, "Right"},
                        {1, "Left"}
                    };
                }

                break;
            }
            case WorkshopModType.Multi_Weapon:
            {
                var target = (ComradesWeaponTarget)binmodTarget;
                if (target is ComradesWeaponTarget.Dagger or ComradesWeaponTarget.Katana
                    or ComradesWeaponTarget.Shuriken)
                {
                    return new Dictionary<int, string>
                    {
                        {0, "Right"},
                        {1, "Left"}
                    };
                }

                break;
            }
            case WorkshopModType.StyleEdit:
            {
                var target = (ComradesStyleTarget)binmodTarget;
                if (target != ComradesStyleTarget.Accessory)
                {
                    return new Dictionary<int, string>
                    {
                        {0, "Slim"},
                        {1, "Chubby"}
                    };
                }

                break;
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

        ModmetaTypes = Enum.GetValues<WorkshopModType>()
            .ToDictionary(e => e, e => e.ToString().ToLower());

        ModmetaTypesReversed = ModmetaTypes.ToDictionary(t => t.Value, t => t.Key);
    }

    private void CreateTypeNames()
    {
        if (TypeNames != null)
        {
            return;
        }

        TypeNames = new Dictionary<WorkshopModType, string>
        {
            {WorkshopModType.Character, "Model Replacement"},
            {WorkshopModType.Cloth, "Outfit"},
            {WorkshopModType.Weapon, "Weapon"},
            {WorkshopModType.Multi_Weapon, "Comrades Weapon"},
            {WorkshopModType.StyleEdit, "Comrades Outfit"}
        };

        TypeNamesReversed = TypeNames.ToDictionary(t => t.Value, t => t.Key);
    }

    private void CreateModmetaTargets()
    {
        if (ModmetaTargets != null)
        {
            return;
        }

        ModmetaTargets = new Dictionary<WorkshopModType, Dictionary<int, string>>
        {
            {
                WorkshopModType.Character,
                _modelReplacementPresets.GetReplacementModmetaDictionary()
            },
            {
                WorkshopModType.Cloth,
                Enum.GetValues<OutfitTarget>().ToDictionary(t => (int)t, t => t.ToString().ToLower())
            },
            {
                WorkshopModType.StyleEdit,
                Enum.GetValues<ComradesStyleTarget>().ToDictionary(t => (int)t, t => t.ToString().ToLower())
            },
            {
                WorkshopModType.Weapon,
                Enum.GetValues<WeaponTarget>().ToDictionary(t => (int)t, t => t.ToString().ToLower())
            },
            {
                WorkshopModType.Multi_Weapon,
                Enum.GetValues<ComradesWeaponTarget>().ToDictionary(t => (int)t, t => t.ToString().ToLower())
            }
        };

        ModmetaTargetsReversed =
            ModmetaTargets.ToDictionary(t => t.Key, t => t.Value
                .ToDictionary(v => v.Value, v => v.Key, StringComparer.OrdinalIgnoreCase));
    }
}