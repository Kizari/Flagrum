using System;

namespace Flagrum.Core.Utilities;

[AttributeUsage(AttributeTargets.Method)]
public class RunStaticInitializerOnStartup : Attribute
{
}