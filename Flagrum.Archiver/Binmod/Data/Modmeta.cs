using System;
using System.Text;

namespace Flagrum.Archiver.Binmod.Data;

public class Modmeta
{
    private Modmeta() { }

    public Modmeta(string modDirectoryName, string modelName)
    {
        ModDirectoryName = modDirectoryName;
        ModelName = modelName;
    }

    public string Title { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Uuid { get; set; }
    public string ModDirectoryName { get; set; }
    public string ModelName { get; set; }
    public bool IsChecked { get; set; }
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

    public static Modmeta FromBytes(byte[] buffer)
    {
        try
        {
            var lines = Encoding.ASCII.GetString(buffer).Split("\r\n");

            var modelString = lines[10]["modify_gmdl[0]=".Length..];
            var tokens = modelString.Split('/');

            var enumString = lines[5]["type=".Length..].ToCharArray();
            enumString[0] = enumString[0].ToString().ToUpper()[0];
            var target = Enum.Parse<Boye>(enumString);
            
            return new Modmeta
            {
                Title = lines[2]["title=".Length..],
                ModDirectoryName = tokens[1],
                ModelName = tokens[2].Split('.')[0],
                Description = lines[3]["desc=".Length..],
                Uuid = lines[4]["uuid=".Length..],
                Target = target,
                IsChecked = bool.Parse(lines[7]["ischecked=".Length..]),
                IsApplyToGame = bool.Parse(lines[8]["isapplytogame=".Length..]),
                Name = lines[11]["name=".Length..],
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

    public byte[] ToBytes()
    {
        var builder = new StringBuilder();
        builder.AppendLine("[meta]");
        builder.AppendLine("modtype=cloth");
        builder.AppendLine($"title={Title}");
        builder.AppendLine($"desc={Description}");
        builder.AppendLine($"uuid={Uuid}");
        builder.AppendLine($"type={Target.ToString().ToLower()}");
        builder.AppendLine("itemid=0");
        builder.AppendLine($"ischecked={(IsChecked ? "True" : "False")}");
        builder.AppendLine($"isapplytogame={(IsApplyToGame ? "True" : "False")}");
        builder.AppendLine("itemplace=E_Local");
        builder.AppendLine($"modify_gmdl[0]=mod/{ModDirectoryName}/{ModelName}.fbx");
        builder.AppendLine($"name={Title}");
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
        return Encoding.ASCII.GetBytes(builder.ToString());
    }
}