using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Flagrum.Generators.Utilities;

public static class Extensions
{
    public static IEnumerable<AttributeSyntax> GetAttributes(this ClassDeclarationSyntax syntax)
    {
        return syntax.AttributeLists.SelectMany(l => l.Attributes);
    }
}