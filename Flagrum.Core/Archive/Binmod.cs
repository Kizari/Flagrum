using System;
using System.Collections.Generic;
using System.Text;

namespace Flagrum.Core.Archive;

public enum ModVisibility
{
    Public,
    FriendsOnly,
    Private,
    Unlisted
}

public class Binmod
{
    public ulong ItemId { get; set; }
    public int Visibility { get; set; }
    public byte[] PreviewBytes { get; set; }
    public string GameMenuTitle { get; set; }
    public string WorkshopTitle { get; set; }
    public string Description { get; set; }
    public IList<string> Tags { get; set; }
    public string Uuid { get; set; }
    public string Path { get; set; }
    public bool IsUploaded { get; set; }
    public bool IsWorkshopMod { get; set; }
    public DateTime LastUpdated { get; set; }
    public int Index { get; set; }
    public string ModDirectoryName { get; set; }
    public string ModelName { get; set; }
    public bool IsApplyToGame { get; set; }
    public int Strength { get; set; }
    public int Vitality { get; set; }
    public int Magic { get; set; }
    public int Spirit { get; set; }
    public int MaxHp { get; set; }
    public int MaxMp { get; set; }
    public int Ballistic { get; set; }
    public int Fire { get; set; }
    public int Ice { get; set; }
    public int Thunder { get; set; }
    public int Dark { get; set; }
    public Boye Target { get; set; }

    public static Binmod FromModmetaBytes(byte[] buffer)
    {
        try
        {
            var lines = Encoding.UTF8.GetString(buffer).Split("\r\n");

            var modelString = lines[10]["modify_gmdl[0]=".Length..];
            var tokens = modelString.Split('/');

            var enumString = lines[5]["type=".Length..].ToCharArray();
            enumString[0] = enumString[0].ToString().ToUpper()[0];
            var target = Enum.Parse<Boye>(enumString);

            return new Binmod
            {
                WorkshopTitle = lines[2]["title=".Length..],
                ModDirectoryName = tokens[1],
                ModelName = tokens[2].Split('.')[0],
                Description = lines[3]["desc=".Length..],
                Uuid = lines[4]["uuid=".Length..],
                Target = target,
                ItemId = ulong.Parse(lines[6]["itemid=".Length..]),
                IsUploaded = bool.Parse(lines[7]["ischecked=".Length..]),
                IsApplyToGame = bool.Parse(lines[8]["isapplytogame=".Length..]),
                GameMenuTitle = lines[11]["name=".Length..],
                Strength = int.Parse(lines[13]["strength=".Length..]),
                Vitality = int.Parse(lines[14]["vitality=".Length..]),
                Magic = int.Parse(lines[15]["magic=".Length..]),
                Spirit = int.Parse(lines[16]["spirit=".Length..]),
                MaxHp = int.Parse(lines[17]["hp_max=".Length..]),
                MaxMp = int.Parse(lines[18]["mp_max=".Length..]),
                Ballistic = int.Parse(lines[19]["bullet=".Length..]),
                Fire = int.Parse(lines[20]["fire=".Length..]),
                Ice = int.Parse(lines[21]["ice=".Length..]),
                Thunder = int.Parse(lines[22]["thunder=".Length..]),
                Dark = int.Parse(lines[23]["dark=".Length..])
            };
        }
        catch (Exception e)
        {
            return null;
        }
    }

    public byte[] ToModmeta()
    {
        var builder = new StringBuilder();
        builder.AppendLine("[meta]");
        builder.AppendLine("modtype=cloth");
        builder.AppendLine($"title={WorkshopTitle}");
        // Can't append literal newlines to the modmeta or things will break
        builder.AppendLine($"desc={Description.Replace("\r\n", "\\n").Replace("\n", "\\n")}");
        builder.AppendLine($"uuid={Uuid}");
        builder.AppendLine($"type={Target.ToString().ToLower()}");
        builder.AppendLine($"itemid={ItemId}");
        builder.AppendLine($"ischecked={(IsWorkshopMod ? "True" : "False")}");
        builder.AppendLine($"isapplytogame={(IsApplyToGame ? "True" : "False")}");
        builder.AppendLine($"itemplace={(IsWorkshopMod ? "E_Local" : "E_SteamWorkshop")}");
        builder.AppendLine($"modify_gmdl[0]=mod/{ModDirectoryName}/{ModelName}.fbx");
        builder.AppendLine($"name={GameMenuTitle}");
        builder.AppendLine("help=");
        builder.AppendLine($"strength={Strength}");
        builder.AppendLine($"vitality={Vitality}");
        builder.AppendLine($"magic={Magic}");
        builder.AppendLine($"spirit={Spirit}");
        builder.AppendLine($"hp_max={MaxHp}");
        builder.AppendLine($"mp_max={MaxMp}");
        builder.AppendLine($"bullet={Ballistic}");
        builder.AppendLine($"fire={Fire}");
        builder.AppendLine($"ice={Ice}");
        builder.AppendLine($"thunder={Thunder}");
        builder.AppendLine($"dark={Dark}");
        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    public Binmod Clone()
    {
        return new()
        {
            ItemId = ItemId,
            Visibility = Visibility,
            PreviewBytes = PreviewBytes,
            GameMenuTitle = GameMenuTitle,
            WorkshopTitle = WorkshopTitle,
            Description = Description,
            Tags = Tags,
            Uuid = Uuid,
            Path = Path,
            IsUploaded = IsUploaded,
            IsWorkshopMod = IsWorkshopMod,
            LastUpdated = LastUpdated,
            Index = Index,
            ModDirectoryName = ModDirectoryName,
            ModelName = ModelName,
            IsApplyToGame = IsApplyToGame,
            Strength = Strength,
            Vitality = Vitality,
            Magic = Magic,
            Spirit = Spirit,
            MaxHp = MaxHp,
            MaxMp = MaxMp,
            Ballistic = Ballistic,
            Fire = Fire,
            Ice = Ice,
            Thunder = Thunder,
            Dark = Dark,
            Target = Target
        };
    }

    public void UpdateFrom(Binmod mod)
    {
        ItemId = mod.ItemId;
        Visibility = mod.Visibility;
        PreviewBytes = mod.PreviewBytes;
        GameMenuTitle = mod.GameMenuTitle;
        WorkshopTitle = mod.WorkshopTitle;
        Description = mod.Description;
        Tags = mod.Tags;
        Uuid = mod.Uuid;
        Path = mod.Path;
        IsUploaded = mod.IsUploaded;
        IsWorkshopMod = mod.IsWorkshopMod;
        LastUpdated = mod.LastUpdated;
        Index = mod.Index;
        ModDirectoryName = mod.ModDirectoryName;
        ModelName = mod.ModelName;
        IsApplyToGame = mod.IsApplyToGame;
        Strength = mod.Strength;
        Vitality = mod.Vitality;
        Magic = mod.Magic;
        Spirit = mod.Spirit;
        MaxHp = mod.MaxHp;
        MaxMp = mod.MaxMp;
        Ballistic = mod.Ballistic;
        Fire = mod.Fire;
        Ice = mod.Ice;
        Thunder = mod.Thunder;
        Dark = mod.Dark;
        Target = mod.Target;
    }
}