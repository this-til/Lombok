using System;

namespace Til.Lombok;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
public class ILombokAttribute : Attribute {
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class GetAttribute : Attribute {
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class SetAttribute : Attribute {
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class WithAttribute : Attribute {
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class FreezeAttribute : Attribute {
    public readonly string tag;

    public FreezeAttribute(string tag) {
        this.tag = tag;
    }
}