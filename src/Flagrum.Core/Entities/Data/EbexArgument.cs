using System.Collections.Generic;
using Flagrum.Core.Scripting.Ebex.Type;

namespace Flagrum.Core.Scripting.Ebex.Data;

public class Argument
{
    private readonly string typeName_;
    private Dictionary<string, string> attributes = new();
    private string displayName_;
    private string displayNameJp_;

    public Argument(string name, string typeName, bool isPointer)
    {
        typeName_ = typeName;
        ArgType = null;
        PrimitiveArgType = PrimitiveType.None;
        ArgName = name;
        IsPointer = isPointer;
        displayName_ = displayNameJp_ = name;
    }

    public string DisplayName => DocumentInterface.Configuration.JapaneseLanguage ? displayNameJp_ : displayName_;

    public DataType ArgType { get; private set; }

    public bool IsPrimitive => (uint)PrimitiveArgType > 0U;

    public PrimitiveType PrimitiveArgType { get; private set; }

    public string ArgName { get; }

    public bool IsPointer { get; }

    public bool ResolveType(ModuleContainer container)
    {
        ArgType = container[typeName_];
        if (ArgType == null)
        {
            PrimitiveArgType = PrimitiveTypeUtility.FromName(typeName_);
        }

        return false;
    }

    public void SetDisplayName(string displayName, string displayNameJp)
    {
        if (!string.IsNullOrEmpty(displayName))
        {
            displayName_ = displayName;
        }

        if (string.IsNullOrEmpty(displayNameJp))
        {
            return;
        }

        displayNameJp_ = displayNameJp;
    }
}