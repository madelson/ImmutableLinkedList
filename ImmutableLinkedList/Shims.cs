using System;
using System.Collections.Generic;
using System.Text;

#if !NET8_0_OR_GREATER
namespace System.Runtime.CompilerServices;

internal sealed class CollectionBuilderAttribute(Type builderType, string methodName) : Attribute
{
    public Type BuilderType { get; } = builderType;
    public string MethodName { get; } = methodName;
}
#endif
