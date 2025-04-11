using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Flagrum.Core.Scripting.Ebex.Configuration;

[XmlRoot("ApplicationConfiguration")]
[Serializable]
public class JenovaApplicationConfiguration
{
    private static XmlSerializer serializer;

    private List<NamespaceAlias> aliases = new();

    //private string defaultCultureName = "ja-JP";
    private string defaultCultureName = "en-GB";
    private List<ExeInfo> processinfos = new();
    private string rootFolder = "..\\..\\..";
    private bool useAutoBuild = true;
    private bool useBuildTarget = true;
    private bool useFixidServer = true;
    private bool usePerforce = true;

    public bool UsePerforce
    {
        get => usePerforce;
        set => usePerforce = value;
    }

    public bool UseAutoBuild
    {
        get => useAutoBuild;
        set => useAutoBuild = value;
    }

    public bool UseBuildTarget
    {
        get => useBuildTarget;
        set => useBuildTarget = value;
    }

    public bool UseFixidServer
    {
        get => useFixidServer;
        set => useFixidServer = value;
    }

    public string RootFolder
    {
        get => rootFolder;
        set => rootFolder = value;
    }

    public string DefaultCultureName
    {
        get => defaultCultureName;
        set => defaultCultureName = value;
    }

    public ExeInfo[] ExeList
    {
        get => processinfos.ToArray();
        set => processinfos = new List<ExeInfo>(value);
    }

    public NamespaceAlias[] NamespaceAliases
    {
        get => aliases.ToArray();
        set => aliases = new List<NamespaceAlias>(value);
    }

    public static JenovaApplicationConfiguration LoadOrNew(string path)
    {
        JenovaApplicationConfiguration applicationConfiguration = null;
        if (File.Exists(path))
        {
            if (serializer == null)
            {
                serializer = new XmlSerializer(typeof(JenovaApplicationConfiguration));
            }

            try
            {
                using (var fileStream = File.OpenRead(path))
                {
                    applicationConfiguration = serializer.Deserialize(fileStream) as JenovaApplicationConfiguration;
                }
            }
            catch (IOException ex)
            {
                Console.Error.WriteLine(ex.ToString());
            }
            catch (InvalidOperationException ex)
            {
                Console.Error.WriteLine(ex.ToString());
            }
        }

        if (applicationConfiguration == null)
        {
            applicationConfiguration = new JenovaApplicationConfiguration();
        }

        return applicationConfiguration;
    }

    public bool Save(string path)
    {
        if (serializer == null)
        {
            serializer = new XmlSerializer(typeof(JenovaApplicationConfiguration));
        }

        try
        {
            using (var fileStream = File.OpenWrite(path))
            {
                serializer.Serialize(fileStream, this);
                return true;
            }
        }
        catch (IOException ex) { }

        return false;
    }
}