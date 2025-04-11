using System.Collections.Generic;
using Flagrum.Core.Scripting.Ebex.Type;
using Flagrum.Core.Scripting.Ebex.Utility;

namespace Flagrum.Core.Scripting.Ebex.Data;

public class Function : DataType
{
    private readonly List<Argument> arguments = new();
    private readonly string returnTypeName_ = "";

    public Function(string name, string returnTypeName, bool isStatic, bool isReturnTypePointer)
        : base(name)
    {
        IsStatic = isStatic;
        returnTypeName_ = returnTypeName;
        IsReturnTypePointer = isReturnTypePointer;
        Hash32 = Fnv1A32.Hash(name);
    }

    public IEnumerable<Argument> Arguments => arguments;

    public bool HasReturnValue => ReturnType != null || (uint)PrimitiveReturnType > 0U;

    public bool IsReturnTypePrimitive => (uint)PrimitiveReturnType > 0U;

    public bool IsReturnTypePointer { get; }

    public DataType ReturnType { get; private set; }

    public PrimitiveType PrimitiveReturnType { get; private set; }

    public bool IsStatic { get; }

    public uint Hash32 { get; }

    public void AddArgument(Argument arg)
    {
        arguments.Add(arg);
    }

    public override void matchTypes(ModuleContainer moduleContainer)
    {
        base.matchTypes(moduleContainer);
        ReturnType = moduleContainer[returnTypeName_];
        if (ReturnType == null)
        {
            PrimitiveReturnType = PrimitiveTypeUtility.FromName(returnTypeName_);
        }

        foreach (var obj in arguments)
        {
            obj.ResolveType(moduleContainer);
        }
    }
}