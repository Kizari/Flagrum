using System;
using Flagrum.Core.Archive.Mod;

namespace Flagrum.Application.Features.WorkshopMods.Data;

public enum WorkshopModStat
{
    Attack,
    Critical,
    MaxHp,
    MaxMp,
    Strength,
    Vitality,
    Magic,
    Spirit,
    Fire,
    Ice,
    Thunder,
    Dark,
    Ballistic
}

public class WorkshopModStats
{
    private readonly Action<int> _set;
    private int? _value;

    private WorkshopModStats(int minimum, double multiplier, Func<int?> get, Action<int> set)
    {
        _set = set;

        Minimum = minimum;
        Maximum = (minimum > 0 ? minimum : 0) + (int)(multiplier * 100);
        Multiplier = multiplier;

        var value = get();
        if (value < Minimum)
        {
            Value = Minimum;
        }
        else
        {
            _value = value;
        }
    }

    public int? Value
    {
        get => _value;
        set
        {
            _value = value < Minimum ? Minimum : value > Maximum ? Maximum : value;
            _set(_value ?? 0);
        }
    }

    private int Minimum { get; }
    private int Maximum { get; }
    private double Multiplier { get; }

    public int Cost => (int)((Math.Abs(Value ?? 0) - (Minimum > 0 ? Minimum : 0)) / Multiplier);

    public static WorkshopModStats Create(int modType, int target, WorkshopModStat workshopModStat, Func<int?> get,
        Action<int> set)
    {
        var type = (WorkshopModType)modType;
        var data = (type, target, stat: workshopModStat);
        return new WorkshopModStats(GetMin(data), GetMulti(data), get, set);
    }

    private static double GetMulti((WorkshopModType type, int target, WorkshopModStat stat) data)
    {
        var (type, target, stat) = data;
        return type switch
        {
            WorkshopModType.Weapon => stat switch
            {
                WorkshopModStat.Attack => 3.0,
                WorkshopModStat.MaxHp => 3.0,
                WorkshopModStat.MaxMp => 0.2,
                _ => 1.0
            },

            WorkshopModType.Multi_Weapon => stat switch
            {
                WorkshopModStat.Attack => 2.0,
                WorkshopModStat.MaxHp => 5.0,
                _ => 1.0
            },

            WorkshopModType.Cloth => stat switch
            {
                WorkshopModStat.MaxHp => 5.0,
                WorkshopModStat.MaxMp => 5.0,
                WorkshopModStat.Strength => 5.0,
                WorkshopModStat.Vitality => 5.0,
                WorkshopModStat.Magic => 5.0,
                WorkshopModStat.Spirit => 5.0,
                WorkshopModStat.Fire => 5.0,
                WorkshopModStat.Ice => 5.0,
                WorkshopModStat.Thunder => 5.0,
                WorkshopModStat.Dark => 5.0,
                WorkshopModStat.Ballistic => 5.0,
                _ => 1.0
            },

            _ => 1.0
        };
    }

    private static int GetMin((WorkshopModType type, int target, WorkshopModStat stat) data)
    {
        var (type, target, stat) = data;

        switch (stat)
        {
            case WorkshopModStat.Fire:
                return type == WorkshopModType.Cloth ? -500 : -100;
            case WorkshopModStat.Ice:
                return type == WorkshopModType.Cloth ? -500 : -100;
            case WorkshopModStat.Thunder:
                return type == WorkshopModType.Cloth ? -500 : -100;
            case WorkshopModStat.Dark:
                return type == WorkshopModType.Cloth ? -500 : -100;
            case WorkshopModStat.MaxHp:
                if (type == WorkshopModType.Weapon && target == (int)WeaponTarget.Large_Sword)
                {
                    return 30;
                }

                break;
            case WorkshopModStat.MaxMp:
                if (type == WorkshopModType.Weapon)
                {
                    var modTarget = (WeaponTarget)target;
                    return modTarget switch
                    {
                        WeaponTarget.Sword => 8,
                        WeaponTarget.Dagger => 5,
                        _ => 0
                    };
                }

                break;
            case WorkshopModStat.Magic:
                if (type == WorkshopModType.Weapon && target == (int)WeaponTarget.Dagger)
                {
                    return 5;
                }

                break;
            case WorkshopModStat.Spirit:
                if (type == WorkshopModType.Weapon && target == (int)WeaponTarget.Gun)
                {
                    return 5;
                }

                break;
            case WorkshopModStat.Vitality:
                if (type == WorkshopModType.Weapon && target == (int)WeaponTarget.Shield)
                {
                    return 10;
                }

                break;
            case WorkshopModStat.Ballistic:
                if (type == WorkshopModType.Weapon && target == (int)WeaponTarget.Shield)
                {
                    return 5;
                }

                return type == WorkshopModType.Cloth ? -500 : -100;
            case WorkshopModStat.Attack:
                if (type == WorkshopModType.Weapon)
                {
                    var modTarget = (WeaponTarget)target;
                    return modTarget switch
                    {
                        WeaponTarget.Sword => 30,
                        WeaponTarget.Large_Sword => 38,
                        WeaponTarget.Spear => 8,
                        WeaponTarget.Dagger => 10,
                        WeaponTarget.Gun => 22,
                        WeaponTarget.Shield => 42,
                        _ => 0
                    };
                }

                if (type == WorkshopModType.Multi_Weapon)
                {
                    var modTarget = (ComradesWeaponTarget)target;
                    return modTarget switch
                    {
                        ComradesWeaponTarget.Katana => 60,
                        ComradesWeaponTarget.Mace => 94,
                        ComradesWeaponTarget.Spear => 56,
                        ComradesWeaponTarget.Dagger => 40,
                        ComradesWeaponTarget.Shuriken => 34,
                        ComradesWeaponTarget.Crossbow => 47,
                        ComradesWeaponTarget.Shield => 70,
                        _ => 0
                    };
                }

                break;
            case WorkshopModStat.Critical:
                if (type == WorkshopModType.Weapon)
                {
                    var modTarget = (WeaponTarget)target;
                    return modTarget switch
                    {
                        WeaponTarget.Sword => 2,
                        WeaponTarget.Large_Sword => 1,
                        WeaponTarget.Spear => 3,
                        WeaponTarget.Dagger => 7,
                        WeaponTarget.Gun => 1,
                        _ => 0
                    };
                }

                if (type == WorkshopModType.Multi_Weapon)
                {
                    var modTarget = (ComradesWeaponTarget)target;
                    return modTarget switch
                    {
                        ComradesWeaponTarget.Katana => 4,
                        ComradesWeaponTarget.Mace => 2,
                        ComradesWeaponTarget.Spear => 3,
                        ComradesWeaponTarget.Dagger => 2,
                        ComradesWeaponTarget.Shuriken => 1,
                        ComradesWeaponTarget.Crossbow => 1,
                        ComradesWeaponTarget.Shield => 2,
                        _ => 0
                    };
                }

                break;
        }

        return 0;
    }
}