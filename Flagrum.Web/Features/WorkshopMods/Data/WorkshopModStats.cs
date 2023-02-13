using System;
using Flagrum.Core.Archive;

namespace Flagrum.Web.Features.WorkshopMods.Data;

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

    public static WorkshopModStats Create(int modType, int target, WorkshopModStat workshopModStat, Func<int?> get, Action<int> set)
    {
        var type = (BinmodType)modType;
        var data = (type, target, stat: workshopModStat);
        return new WorkshopModStats(GetMin(data), GetMulti(data), get, set);
    }

    private static double GetMulti((BinmodType type, int target, WorkshopModStat stat) data)
    {
        var (type, target, stat) = data;
        return type switch
        {
            BinmodType.Weapon => stat switch
            {
                WorkshopModStat.Attack => 3.0,
                WorkshopModStat.MaxHp => 3.0,
                WorkshopModStat.MaxMp => 0.2,
                _ => 1.0
            },

            BinmodType.Multi_Weapon => stat switch
            {
                WorkshopModStat.Attack => 2.0,
                WorkshopModStat.MaxHp => 5.0,
                _ => 1.0
            },

            BinmodType.Cloth => stat switch
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

    private static int GetMin((BinmodType type, int target, WorkshopModStat stat) data)
    {
        var (type, target, stat) = data;

        switch (stat)
        {
            case WorkshopModStat.Fire:
                return type == BinmodType.Cloth ? -500 : -100;
            case WorkshopModStat.Ice:
                return type == BinmodType.Cloth ? -500 : -100;
            case WorkshopModStat.Thunder:
                return type == BinmodType.Cloth ? -500 : -100;
            case WorkshopModStat.Dark:
                return type == BinmodType.Cloth ? -500 : -100;
            case WorkshopModStat.MaxHp:
                if (type == BinmodType.Weapon && target == (int)WeaponSoloTarget.Large_Sword)
                {
                    return 30;
                }

                break;
            case WorkshopModStat.MaxMp:
                if (type == BinmodType.Weapon)
                {
                    var modTarget = (WeaponSoloTarget)target;
                    return modTarget switch
                    {
                        WeaponSoloTarget.Sword => 8,
                        WeaponSoloTarget.Dagger => 5,
                        _ => 0
                    };
                }

                break;
            case WorkshopModStat.Magic:
                if (type == BinmodType.Weapon && target == (int)WeaponSoloTarget.Dagger)
                {
                    return 5;
                }

                break;
            case WorkshopModStat.Spirit:
                if (type == BinmodType.Weapon && target == (int)WeaponSoloTarget.Gun)
                {
                    return 5;
                }

                break;
            case WorkshopModStat.Vitality:
                if (type == BinmodType.Weapon && target == (int)WeaponSoloTarget.Shield)
                {
                    return 10;
                }

                break;
            case WorkshopModStat.Ballistic:
                if (type == BinmodType.Weapon && target == (int)WeaponSoloTarget.Shield)
                {
                    return 5;
                }

                return type == BinmodType.Cloth ? -500 : -100;
            case WorkshopModStat.Attack:
                if (type == BinmodType.Weapon)
                {
                    var modTarget = (WeaponSoloTarget)target;
                    return modTarget switch
                    {
                        WeaponSoloTarget.Sword => 30,
                        WeaponSoloTarget.Large_Sword => 38,
                        WeaponSoloTarget.Spear => 8,
                        WeaponSoloTarget.Dagger => 10,
                        WeaponSoloTarget.Gun => 22,
                        WeaponSoloTarget.Shield => 42,
                        _ => 0
                    };
                }
                else if (type == BinmodType.Multi_Weapon)
                {
                    var modTarget = (WeaponMultiTarget)target;
                    return modTarget switch
                    {
                        WeaponMultiTarget.Katana => 60,
                        WeaponMultiTarget.Mace => 94,
                        WeaponMultiTarget.Spear => 56,
                        WeaponMultiTarget.Dagger => 40,
                        WeaponMultiTarget.Shuriken => 34,
                        WeaponMultiTarget.Crossbow => 47,
                        WeaponMultiTarget.Shield => 70,
                        _ => 0
                    };
                }

                break;
            case WorkshopModStat.Critical:
                if (type == BinmodType.Weapon)
                {
                    var modTarget = (WeaponSoloTarget)target;
                    return modTarget switch
                    {
                        WeaponSoloTarget.Sword => 2,
                        WeaponSoloTarget.Large_Sword => 1,
                        WeaponSoloTarget.Spear => 3,
                        WeaponSoloTarget.Dagger => 7,
                        WeaponSoloTarget.Gun => 1,
                        _ => 0
                    };
                }
                else if (type == BinmodType.Multi_Weapon)
                {
                    var modTarget = (WeaponMultiTarget)target;
                    return modTarget switch
                    {
                        WeaponMultiTarget.Katana => 4,
                        WeaponMultiTarget.Mace => 2,
                        WeaponMultiTarget.Spear => 3,
                        WeaponMultiTarget.Dagger => 2,
                        WeaponMultiTarget.Shuriken => 1,
                        WeaponMultiTarget.Crossbow => 1,
                        WeaponMultiTarget.Shield => 2,
                        _ => 0
                    };
                }

                break;
        }

        return 0;
    }
}