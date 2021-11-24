using System.Text;

namespace Flagrum.Archiver.Binmod
{
    public class Modmeta
    {
        public enum Boye
        {
            Noctis,
            Prompto,
            Ignis,
            Gladiolus
        }

        public static byte[] Build(string name, string description, string uuid, Boye boye)
        {
            var builder = new StringBuilder();
            builder.AppendLine("[meta]");
            builder.AppendLine("modtype=cloth");
            builder.AppendLine($"title={name}");
            builder.AppendLine($"desc={description}");
            builder.AppendLine($"uuid={uuid}");
            builder.AppendLine($"type={boye.ToString().ToLower()}");
            builder.AppendLine("itemid=0");
            builder.AppendLine("ischecked=False");
            builder.AppendLine("isapplytogame=True");
            builder.AppendLine("itemplace=E_Local");
            builder.AppendLine($"modify_gmdl[0]=mod/{name}/main.fbx");
            builder.AppendLine($"name={name}");
            builder.AppendLine("help=");
            builder.AppendLine("strength=0");
            builder.AppendLine("vitality=0");
            builder.AppendLine("magic=0");
            builder.AppendLine("spirit=0");
            builder.AppendLine("hp_max=0");
            builder.AppendLine("mp_max=0");
            builder.AppendLine("bullet=0");
            builder.AppendLine("fire=0");
            builder.AppendLine("ice=0");
            builder.AppendLine("thunder=0");
            builder.AppendLine("dark=0");
            return Encoding.ASCII.GetBytes(builder.ToString());
        }
    }
}