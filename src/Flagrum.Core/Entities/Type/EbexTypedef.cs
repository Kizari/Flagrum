using System.Collections.Generic;
using Flagrum.Core.Scripting.Ebex.Data;

namespace Flagrum.Core.Scripting.Ebex.Type;

public class Typedef : DataType
{
    private readonly string[] templateTypeNames;
    private IItem defaultValue;

    public Typedef(string name, string typeName, bool pointer = false, bool reference = false)
        : base(name)
    {
        var length = typeName.IndexOf('<');
        var num = typeName.LastIndexOf('>');
        if (length >= 0 && num > length)
        {
            var str1 = typeName.Substring(length + 1, num - length - 1).Trim();
            var strArray = str1.Split(',');
            var stringList = new List<string>();
            if (strArray != null)
            {
                foreach (var str2 in strArray)
                {
                    stringList.Add(str2.Trim());
                }
            }
            else
            {
                stringList.Add(str1);
            }

            templateTypeNames = stringList.ToArray();
            typeName = typeName.Substring(0, length).Trim();
        }

        TypeName = typeName;
    }

    public string TypeName { get; }

    public DataType DataType { get; private set; }

    public Class Class => DataType as Class;

    public string[] TemplateTypeNames => templateTypeNames ?? new string[0];

    public bool HasTemplate => templateTypeNames != null && (uint)templateTypeNames.Length > 0U;

    public override string Category => base.Category;

    public override bool Browsable => (DataType == null || DataType.Browsable) && base.Browsable;

    public override IItem DefaultValue
    {
        get
        {
            if (defaultValue == null)
            {
                defaultValue = ConstructValue(null);
            }

            return defaultValue;
        }
    }

    public override void matchTypes(ModuleContainer moduleContainer)
    {
        base.matchTypes(moduleContainer);
        DataType = moduleContainer[TypeName];
    }

    public IItem ConstructValue(DataItem parent)
    {
        return DocumentInterface.ModuleContainer.CreateObjectFromString(parent, DataType, TypeName,
            DefaultValueString,
            true, false);
    }
}