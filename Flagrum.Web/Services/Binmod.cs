using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace Flagrum.Web.Services;

public enum ModVisibility
{
    Public,
    FriendsOnly,
    Private,
    Unlisted
}

public class Binmod
{
    public int Type { get; set; }
    public int Target { get; set; } = -1;
    public ulong ItemId { get; set; }
    public byte[] PreviewBytes { get; set; }
    public byte[] ThumbnailBytes { get; set; }
    public string GameMenuTitle { get; set; }
    public string GameMenuDescription { get; set; }
    public string WorkshopTitle { get; set; }
    public string Description { get; set; }
    public string Uuid { get; set; }
    public string Path { get; set; }
    public bool IsUploaded { get; set; }
    public bool IsWorkshopMod { get; set; }
    public int Index { get; set; }
    public string ModDirectoryName { get; set; }
    public string ModelName { get; set; }
    public string Model1Name { get; set; }
    public string Model2Name { get; set; }
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
    public int Attack { get; set; }
    public int Critical { get; set; }
    public List<string> OriginalGmdls { get; set; }
    public int OriginalGmdlCount { get; set; }
    public string Gender { get; set; }
    public string ModelExtension { get; set; } = "gmdl";

    public static Binmod FromModmetaBytes(byte[] buffer, BinmodTypeHelper binmodType, ILogger logger)
    {
        try
        {
            var mod = new Binmod();
            var lines = Encoding.UTF8.GetString(buffer).Split("\r\n");

            var properties = lines
                .Select(line => line.Split('='))
                .Where(pair => pair.Length >= 2)
                .ToDictionary(pair => pair[0], pair => pair[1]);

            foreach (var (property, value) in properties)
            {
                switch (property)
                {
                    case "title":
                        mod.WorkshopTitle = value;
                        break;
                    case "desc":
                        mod.Description = value;
                        break;
                    case "uuid":
                        mod.Uuid = value;
                        mod.ModDirectoryName = value;
                        break;
                    case "itemid":
                        mod.ItemId = ulong.Parse(value);
                        break;
                    case "itemplace":
                        mod.IsUploaded = value == "E_SteamWorkshop";
                        break;
                    case "isapplytogame":
                        mod.IsApplyToGame = bool.Parse(value);
                        break;
                    case "name":
                        mod.GameMenuTitle = value;
                        break;
                    case "help":
                        mod.GameMenuDescription = value;
                        break;
                    case "strength":
                        mod.Strength = int.Parse(value);
                        break;
                    case "vitality":
                        mod.Vitality = int.Parse(value);
                        break;
                    case "magic":
                        mod.Magic = int.Parse(value);
                        break;
                    case "spirit":
                        mod.Spirit = int.Parse(value);
                        break;
                    case "hp_max":
                        mod.MaxHp = int.Parse(value);
                        break;
                    case "mp_max":
                        mod.MaxMp = int.Parse(value);
                        break;
                    case "bullet":
                        mod.Ballistic = int.Parse(value) * -1;
                        break;
                    case "fire":
                        mod.Fire = int.Parse(value) * -1;
                        break;
                    case "ice":
                        mod.Ice = int.Parse(value) * -1;
                        break;
                    case "thunder":
                        mod.Thunder = int.Parse(value) * -1;
                        break;
                    case "dark":
                        mod.Dark = int.Parse(value) * -1;
                        break;
                    case "attack":
                        mod.Attack = int.Parse(value);
                        break;
                    case "critical":
                        mod.Critical = int.Parse(value);
                        break;
                    case "modtype":
                        mod.Type = binmodType.GetModmetaTypeFromName(value);
                        break;
                    case "gmdl1":
                        mod.ModelExtension = value.EndsWith(".fbx") ? "fbx" : "gmdl";
                        mod.Model1Name = value.Replace(".fbx", "").Replace(".gmdl", "");
                        break;
                    case "gmdl2":
                        mod.ModelExtension = value.EndsWith(".fbx") ? "fbx" : "gmdl";
                        mod.Model2Name = value.Replace(".fbx", "").Replace(".gmdl", "");
                        break;
                    case "count_original_gmdls":
                        mod.OriginalGmdlCount = int.Parse(value);
                        break;
                    case "gender":
                        mod.Gender = value;
                        break;
                }
            }

            foreach (var (property, value) in properties)
            {
                switch (property)
                {
                    case "type":
                        mod.Target = binmodType.GetBinmodTarget(mod.Type, value);
                        break;
                }
            }

            var type = (BinmodType)mod.Type;
            if (type == BinmodType.Character)
            {
                var strings = new string[mod.OriginalGmdlCount];

                foreach (var (property, value) in properties)
                {
                    if (property.Contains("origin_gmdl["))
                    {
                        var matches = Regex.Matches(
                            property,
                            @".+?\[(\d+)\]");

                        var indexString = matches[0].Groups[1].Value;
                        var index = int.Parse(indexString);
                        strings[index] = value;
                    }
                }

                mod.OriginalGmdls = new List<string>(strings);
            }

            var modelCount = binmodType.GetModelCount(mod.Type, mod.Target);
            foreach (var (property, value) in properties)
            {
                if (modelCount > 1)
                {
                    if (property == "modify_gmdl[0]")
                    {
                        mod.ModelExtension = value.EndsWith(".fbx") ? "fbx" : "gmdl";
                        mod.ModDirectoryName = value.Split('/')[1];
                        mod.Model1Name = value.Split('/').Last().Replace(".fbx", "").Replace(".gmdl", "");
                    }
                    else if (property == "modify_gmdl[1]")
                    {
                        mod.ModelExtension = value.EndsWith(".fbx") ? "fbx" : "gmdl";
                        mod.ModDirectoryName = value.Split('/')[1];
                        mod.Model2Name = value.Split('/').Last().Replace(".fbx", "").Replace(".gmdl", "");
                    }
                }
                else
                {
                    if (property == "modify_gmdl[0]")
                    {
                        mod.ModelExtension = value.EndsWith(".fbx") ? "fbx" : "gmdl";
                        mod.ModDirectoryName = value.Split('/')[1];
                        mod.ModelName = value.Split('/').Last().Replace(".fbx", "").Replace(".gmdl", "");
                    }
                }
            }

            return mod;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to read binmod!");
            return null;
        }
    }

    public Binmod Clone()
    {
        return new Binmod
        {
            Type = Type,
            ItemId = ItemId,
            PreviewBytes = PreviewBytes,
            GameMenuTitle = GameMenuTitle,
            WorkshopTitle = WorkshopTitle,
            Description = Description,
            Uuid = Uuid,
            Path = Path,
            IsUploaded = IsUploaded,
            IsWorkshopMod = IsWorkshopMod,
            Index = Index,
            ModDirectoryName = ModDirectoryName,
            ModelName = ModelName,
            ModelExtension = ModelExtension,
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
            Target = Target,
            Attack = Attack,
            Critical = Critical,
            Model1Name = Model1Name,
            Model2Name = Model2Name,
            OriginalGmdls = OriginalGmdls,
            OriginalGmdlCount = OriginalGmdlCount,
            Gender = Gender
        };
    }

    public void UpdateFrom(Binmod mod)
    {
        Type = mod.Type;
        ItemId = mod.ItemId;
        PreviewBytes = mod.PreviewBytes;
        GameMenuTitle = mod.GameMenuTitle;
        WorkshopTitle = mod.WorkshopTitle;
        Description = mod.Description;
        Uuid = mod.Uuid;
        Path = mod.Path;
        IsUploaded = mod.IsUploaded;
        IsWorkshopMod = mod.IsWorkshopMod;
        Index = mod.Index;
        ModDirectoryName = mod.ModDirectoryName;
        ModelName = mod.ModelName;
        ModelExtension = mod.ModelExtension;
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
        Attack = mod.Attack;
        Critical = mod.Critical;
        Model1Name = mod.Model1Name;
        Model2Name = mod.Model2Name;
        OriginalGmdls = mod.OriginalGmdls;
        OriginalGmdlCount = mod.OriginalGmdlCount;
        Gender = mod.Gender;
    }
}