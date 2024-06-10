using System;
using System.Collections.Generic;

namespace Til.Lombok;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
public class ILombokAttribute : Attribute {
}

[AttributeUsage(AttributeTargets.Class)]
public class IFreezeAttribute : Attribute {
}

public class MetadataAttribute : Attribute {
    /// <summary>
    /// 在 set 方法中，如果属性被标记为 true，将生成链式调用
    /// </summary>
    public bool link;

    public bool noNull;

    /// <summary>
    /// 在 get set 方法中，如果不为空将进行冻结验证，验证不通过将抛出异常
    /// </summary>
    public string freezeTag;

    public string type;

    public static MetadataAttribute of(Dictionary<string, object?> data) {
        var attribute = new MetadataAttribute();
        if (data.TryGetValue("link", out var link)) {
            attribute.link = link is not null && (bool)link;
        }
        if (data.TryGetValue("link", out var noNull)) {
            attribute.noNull = noNull is not null && (bool)noNull;
        }
        if (data.TryGetValue("freezeTag", out var freezeTag)) {
            attribute.freezeTag = freezeTag?.ToString() ?? string.Empty;
        }
        return attribute;
    }
}

public class ListMetadataAttribute : MetadataAttribute {
    /// <summary>
    /// 在list方法中指定泛型类型
    /// </summary>
    public string type;

    public static ListMetadataAttribute of(Dictionary<string, object?> data) {
        var attribute = new ListMetadataAttribute();
        if (data.TryGetValue("link", out var link)) {
            attribute.link = link is not null && (bool)link;
        }
        if (data.TryGetValue("freezeTag", out var freezeTag)) {
            attribute.freezeTag = freezeTag?.ToString() ?? string.Empty;
        }
        if (data.TryGetValue("type", out var type)) {
            attribute.type = type?.ToString() ?? string.Empty;
        }
        return attribute;
    }
}

public class MapMetadataAttribute : MetadataAttribute {
    /// <summary>
    /// 在map方法中指定泛型Key类型
    /// </summary>
    public string keyType;

    /// <summary>
    /// 在map方法中指定泛型Value类型
    /// </summary>
    public string valueType;

    public static MapMetadataAttribute of(Dictionary<string, object?> data) {
        var attribute = new MapMetadataAttribute();
        if (data.TryGetValue("link", out var link)) {
            attribute.link = link is not null && (bool)link;
        }
        if (data.TryGetValue("freezeTag", out var freezeTag)) {
            attribute.freezeTag = freezeTag?.ToString() ?? string.Empty;
        }
        if (data.TryGetValue("keyType", out var keyType)) {
            attribute.keyType = keyType?.ToString() ?? string.Empty;
        }
        if (data.TryGetValue("valueType", out var valueType)) {
            attribute.keyType = keyType?.ToString() ?? string.Empty;
        }
        return attribute;
    }
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class GetAttribute : MetadataAttribute {
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class SetAttribute : MetadataAttribute {
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class AddAttribute : ListMetadataAttribute {
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class RemoveAttribute : ListMetadataAttribute {
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class IndexAttribute : ListMetadataAttribute {
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ContainAttribute : ListMetadataAttribute {
}