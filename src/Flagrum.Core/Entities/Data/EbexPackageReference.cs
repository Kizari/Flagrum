using System;
using System.IO;
using Flagrum.Core.Scripting.Ebex.Configuration;
using Flagrum.Core.Scripting.Ebex.Type;

namespace Flagrum.Core.Scripting.Ebex.Data;

public class EntityPackageReference : EntityGroup
{
    public new const string ClassFullName = "SQEX.Ebony.Framework.Entity.EntityPackageReference";
    public const string EXTENSION = ".ebex";
    public const string FILEDESCRIPTION = "Ebony Entity XML";
    private bool isLoaded = true;

    public EntityPackageReference(DataItem parent, DataType dataType)
        : base(parent, dataType)
    {
        FullFilePath = "";
    }

    public bool IsLoaded
    {
        get => isLoaded;
        set
        {
            if (isLoaded == value)
            {
                return;
            }

            isLoaded = value;
        }
    }

    public bool IsShared
    {
        get => GetBool("isShared_");
        set => SetBool("isShared_", value);
    }

    public virtual string Extension => ".ebex";

    public virtual string FileDescription => "Ebony Entity XML";

    public virtual string FullFilePath { get; set; }

    public string Guid => GetString("name_") ?? "(none)";

    public void SetSourcePath(bool isRelative = true)
    {
        if (isRelative && ParentPackage != null && !string.IsNullOrEmpty(ParentPackage.FullFilePath) &&
            !string.IsNullOrEmpty(FullFilePath))
        {
            var str = new Uri(Path.GetDirectoryName(ParentPackage.FullFilePath) + "/")
                .MakeRelativeUri(new Uri(FullFilePath)).ToString().Replace('\\', '/');
            if (!str.StartsWith("."))
            {
                str = "./" + str;
            }

            this["sourcePath_"].Value = new Value(str);
        }
        else
        {
            this["sourcePath_"].Value = new Value(Project.GetDataRelativePath(FullFilePath));
        }
    }

    public bool IsReferenceMyself()
    {
        return isReference(this, this);
    }

    private bool isReference(EntityPackageReference package, EntityGroup group)
    {
        foreach (var entity in group.Entities)
        {
            if ((entity is EntityPackageReference packageReference1 &&
                 packageReference1.FullFilePath == package.FullFilePath &&
                 !string.IsNullOrEmpty(packageReference1.FullFilePath)) ||
                (entity is EntityGroup && isReference(package, (EntityGroup)entity)))
            {
                return true;
            }
        }

        return false;
    }

    public override DataItem GetChild(ItemPath relativePath, bool isAllowLazyLoading = true)
    {
        if (!IsLoaded & isAllowLazyLoading && DocumentInterface.DocumentContainer != null)
        {
            DocumentInterface.DocumentContainer.LazyLoading(this);
        }

        return base.GetChild(relativePath, isAllowLazyLoading);
    }
}