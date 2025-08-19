using System.Text;
using Flagrum.Core.Archive.Mod;

namespace Flagrum.Application.Services;

public class Modmeta
{
    private readonly BinmodTypeHelper _binmodType;

    public Modmeta(BinmodTypeHelper binmodType)
    {
        _binmodType = binmodType;
    }

    public byte[] Build(Binmod mod)
    {
        var type = (WorkshopModType)mod.Type;

        var builder = new StringBuilder();
        builder.AppendLine("[meta]");
        builder.AppendLine($"modtype={_binmodType.GetModmetaTypeName(mod.Type)}");

        switch (type)
        {
            case WorkshopModType.Cloth:
                AddOutfitInfo(mod, builder);
                break;
            case WorkshopModType.Character:
                AddModelReplacementInfo(mod, builder);
                break;
            case WorkshopModType.Weapon or WorkshopModType.Multi_Weapon:
                AddWeaponInfo(mod, builder);
                break;
            case WorkshopModType.StyleEdit:
                AddMultiOutfitInfo(mod, builder);
                break;
        }

        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    private void AddModInfo(Binmod mod, StringBuilder builder)
    {
        builder.AppendLine($"title={mod.WorkshopTitle}");
        builder.AppendLine($"desc={mod.Description?.Replace("\r\n", "\\n")?.Replace("\n", "\\n")}");
        builder.AppendLine($"uuid={mod.Uuid}");
    }

    private void AddWorkshopInfo(Binmod mod, StringBuilder builder)
    {
        builder.AppendLine($"itemid={mod.ItemId}");
        builder.AppendLine("ischecked=False");
        builder.AppendLine($"isapplytogame={(mod.IsApplyToGame ? "True" : "False")}");
        builder.AppendLine($"itemplace={(mod.IsUploaded ? "E_SteamWorkshop" : "E_Local")}");
    }

    private void AddPrimaryStats(Binmod mod, StringBuilder builder)
    {
        builder.AppendLine($"strength={mod.Strength}");
        builder.AppendLine($"vitality={mod.Vitality}");
        builder.AppendLine($"magic={mod.Magic}");
        builder.AppendLine($"spirit={mod.Spirit}");
    }

    private void AddResistances(Binmod mod, StringBuilder builder)
    {
        builder.AppendLine($"bullet={mod.Ballistic * -1}");
        builder.AppendLine($"fire={mod.Fire * -1}");
        builder.AppendLine($"ice={mod.Ice * -1}");
        builder.AppendLine($"thunder={mod.Thunder * -1}");
        builder.AppendLine($"dark={mod.Dark * -1}");
    }

    private void AddWeaponInfo(Binmod mod, StringBuilder builder)
    {
        builder.AppendLine($"type={_binmodType.GetModmetaTargetName(mod.Type, mod.Target)}");

        if (mod.Model2Name == null)
        {
            if (mod.Type == (int)WorkshopModType.Weapon && mod.Target == (int)WeaponTarget.Shield)
            {
                builder.AppendLine($"modify_gmdl[1]=mod/{mod.ModDirectoryName}/{mod.ModelName}.{mod.ModelExtension}");
            }
            else
            {
                builder.AppendLine($"modify_gmdl[0]=mod/{mod.ModDirectoryName}/{mod.ModelName}.{mod.ModelExtension}");
            }
        }
        else
        {
            builder.AppendLine($"modify_gmdl[0]=mod/{mod.ModDirectoryName}/{mod.Model1Name}.{mod.ModelExtension}");
            builder.AppendLine($"modify_gmdl[1]=mod/{mod.ModDirectoryName}/{mod.Model2Name}.{mod.ModelExtension}");
        }

        builder.AppendLine($"name={mod.GameMenuTitle}");
        builder.AppendLine($"help={mod.GameMenuDescription?.Replace("\r\n", "\\n")?.Replace("\n", "\\n")}");

        AddModInfo(mod, builder);
        AddWorkshopInfo(mod, builder);

        builder.AppendLine($"attack={mod.Attack}");
        builder.AppendLine($"hp_max={mod.MaxHp}");
        builder.AppendLine($"mp_max={mod.MaxMp}");
        builder.AppendLine($"critical={mod.Critical}");

        AddPrimaryStats(mod, builder);
        AddResistances(mod, builder);
    }

    private void AddModelReplacementInfo(Binmod mod, StringBuilder builder)
    {
        for (var i = 0; i < mod.OriginalGmdls.Count; i++)
        {
            builder.AppendLine($"origin_gmdl[{i}]={mod.OriginalGmdls[i]}");
        }

        builder.AppendLine($"modify_gmdl[0]=mod/{mod.ModDirectoryName}/{mod.ModelName}.{mod.ModelExtension}");
        builder.AppendLine($"count_original_gmdls={mod.OriginalGmdls.Count}");
        builder.AppendLine($"type={_binmodType.GetModmetaTargetName(mod.Type, mod.Target)}");

        AddModInfo(mod, builder);
        AddWorkshopInfo(mod, builder);
    }

    private void AddMultiOutfitInfo(Binmod mod, StringBuilder builder)
    {
        builder.AppendLine($"type={_binmodType.GetModmetaTargetName(mod.Type, mod.Target)}");
        builder.AppendLine($"thumbnail1=mod/{mod.ModDirectoryName}/default.png");
        builder.AppendLine($"thumbnail2=mod/{mod.ModDirectoryName}/default.png");

        if (mod.Model2Name == null)
        {
            builder.AppendLine($"gmdl1=mod/{mod.ModDirectoryName}/{mod.ModelName}.{mod.ModelExtension}");
            builder.AppendLine($"gmdl2=mod/{mod.ModDirectoryName}/{mod.ModelName}.{mod.ModelExtension}");
        }
        else
        {
            builder.AppendLine($"gmdl1=mod/{mod.ModDirectoryName}/{mod.Model1Name}.{mod.ModelExtension}");
            builder.AppendLine($"gmdl2=mod/{mod.ModDirectoryName}/{mod.Model2Name}.{mod.ModelExtension}");
        }

        AddModInfo(mod, builder);
        AddWorkshopInfo(mod, builder);

        builder.AppendLine($"name={mod.GameMenuTitle}");
        builder.AppendLine($"gender={mod.Gender}");
    }

    private void AddOutfitInfo(Binmod mod, StringBuilder builder)
    {
        AddModInfo(mod, builder);

        builder.AppendLine($"type={_binmodType.GetModmetaTargetName(mod.Type, mod.Target)}");

        AddWorkshopInfo(mod, builder);

        builder.AppendLine($"modify_gmdl[0]=mod/{mod.ModDirectoryName}/{mod.ModelName}.{mod.ModelExtension}");
        builder.AppendLine($"name={mod.GameMenuTitle}");
        builder.AppendLine($"help={mod.GameMenuDescription?.Replace("\r\n", "\\n")?.Replace("\n", "\\n")}");

        AddPrimaryStats(mod, builder);

        builder.AppendLine($"hp_max={mod.MaxHp}");
        builder.AppendLine($"mp_max={mod.MaxMp}");

        AddResistances(mod, builder);
    }
}