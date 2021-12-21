using System;
using Flagrum.Web.Services;

namespace Flagrum.Web.Features.ModLibrary.Data;

public class WeaponStatsModel
{
    private readonly Binmod _mod;
    private readonly Action _onChange;
    private int? _attack;
    private int? _ballistic;
    private int? _critical;

    private int? _dark;

    private int? _fire;

    private int? _ice;

    private int? _magic;

    private int? _maxHp;

    private int? _maxMp;

    private int? _spirit;

    private int? _strength;

    private int? _thunder;

    private int? _vitality;

    public WeaponStatsModel(Binmod mod, Action onChange)
    {
        _mod = mod;
        _onChange = onChange;

        _maxHp = mod.MaxHp;
        _maxMp = mod.MaxMp;
        _strength = mod.Strength;
        _vitality = mod.Vitality;
        _magic = mod.Magic;
        _spirit = mod.Spirit;
        _fire = mod.Fire;
        _ice = mod.Ice;
        _thunder = mod.Thunder;
        _dark = mod.Dark;
        _ballistic = mod.Ballistic;
        _attack = mod.Attack;
        _critical = mod.Critical;
    }

    public int? MaxHp
    {
        get => _maxHp;
        set
        {
            _maxHp = value;
            _mod.MaxHp = _maxHp ?? 0;
            _onChange();
        }
    }

    public int? MaxMp
    {
        get => _maxMp;
        set
        {
            _maxMp = value;
            _mod.MaxMp = value ?? 0;
            _onChange();
        }
    }

    public int? Strength
    {
        get => _strength;
        set
        {
            _strength = value;
            _mod.Strength = value ?? 0;
            _onChange();
        }
    }

    public int? Vitality
    {
        get => _vitality;
        set
        {
            _vitality = value;
            _mod.Vitality = value ?? 0;
            _onChange();
        }
    }

    public int? Magic
    {
        get => _magic;
        set
        {
            _magic = value;
            _mod.Magic = value ?? 0;
            _onChange();
        }
    }

    public int? Spirit
    {
        get => _spirit;
        set
        {
            _spirit = value;
            _mod.Spirit = value ?? 0;
            _onChange();
        }
    }

    public int? Fire
    {
        get => _fire;
        set
        {
            _fire = value;
            _mod.Fire = value ?? 0;
            _onChange();
        }
    }

    public int? Ice
    {
        get => _ice;
        set
        {
            _ice = value;
            _mod.Ice = value ?? 0;
            _onChange();
        }
    }

    public int? Thunder
    {
        get => _thunder;
        set
        {
            _thunder = value;
            _mod.Thunder = value ?? 0;
            _onChange();
        }
    }

    public int? Dark
    {
        get => _dark;
        set
        {
            _dark = value;
            _mod.Dark = value ?? 0;
            _onChange();
        }
    }

    public int? Ballistic
    {
        get => _ballistic;
        set
        {
            _ballistic = value;
            _mod.Ballistic = value ?? 0;
            _onChange();
        }
    }

    public int? Attack
    {
        get => _attack;
        set
        {
            _attack = value;
            _mod.Attack = value ?? 0;
            _onChange();
        }
    }

    public int? Critical
    {
        get => _critical;
        set
        {
            _critical = value;
            _mod.Critical = value ?? 0;
            _onChange();
        }
    }
}