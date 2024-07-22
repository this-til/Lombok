using System;
using System.Collections.Generic;

namespace Til.Lombok {
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

        /// <summary>
        /// 设置非空
        /// </summary>
        public bool noNull;

        /// <summary>
        /// 在 get set 方法中，如果不为空将进行冻结验证，验证不通过将抛出异常
        /// </summary>
        public string freezeTag;

        /// <summary>
        /// 调用更新方法
        /// </summary>
        public bool updateField;

        public MetadataAttribute() {
        }

        public MetadataAttribute(MetadataAttribute metadataAttribute) {
            link = metadataAttribute.link;
            noNull = metadataAttribute.noNull;
            freezeTag = metadataAttribute.freezeTag;
            updateField = metadataAttribute.updateField;
        }

        public static MetadataAttribute of(Dictionary<string, object?> data) {
            var attribute = new MetadataAttribute();
            data.TryGetValue("link", out var link);
            attribute.link = link is not null && (bool)link;
            data.TryGetValue("noNull", out var noNull);
            attribute.noNull = noNull is not null && (bool)noNull;
            data.TryGetValue("freezeTag", out var freezeTag);
            attribute.freezeTag = freezeTag?.ToString() ?? string.Empty;
            data.TryGetValue("updateField", out var updateField);
            attribute.updateField = updateField is not null && (bool)updateField;
            return attribute;
        }
    }

    public class ListMetadataAttribute : MetadataAttribute {
        /// <summary>
        /// 在list方法中指定泛型类型
        /// </summary>
        public string type;

        /// <summary>
        /// 在for中指定是否使用yield
        /// </summary>
        public bool useYield;

        public ListMetadataAttribute() {
        }

        public ListMetadataAttribute(MetadataAttribute metadataAttribute) : base(metadataAttribute) {
        }

        public new static ListMetadataAttribute of(Dictionary<string, object?> data) {
            var attribute = new ListMetadataAttribute(MetadataAttribute.of(data));
            data.TryGetValue("useYield", out var useYield);
            attribute.useYield = useYield is not null && (bool)useYield;
            data.TryGetValue("type", out var type);
            attribute.type = type?.ToString() ?? string.Empty;
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

        /// <summary>
        /// 在for中指定是否使用yield
        /// </summary>
        public bool useYield;

        public MapMetadataAttribute() {
        }

        public MapMetadataAttribute(MetadataAttribute metadataAttribute) : base(metadataAttribute) {
        }

        public new static MapMetadataAttribute of(Dictionary<string, object?> data) {
            var attribute = new MapMetadataAttribute(MetadataAttribute.of(data));
            data.TryGetValue("useYield", out var useYield);
            attribute.useYield = useYield is not null && (bool)useYield;
            data.TryGetValue("keyType", out var keyType);
            attribute.keyType = keyType?.ToString() ?? string.Empty;
            data.TryGetValue("valueType", out var valueType);
            attribute.valueType = valueType?.ToString() ?? string.Empty;
            return attribute;
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class GetAttribute : MetadataAttribute {
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class IsAttribute : MetadataAttribute {
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class OpenAttribute : MetadataAttribute {
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

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ForAttribute : ListMetadataAttribute {
    }

    //------------------------------------------------------------------------------------

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class PutAttribute : MapMetadataAttribute {
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class MapGetAttribute : MapMetadataAttribute {
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class RemoveKeyAttribute : MapMetadataAttribute {
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class RemoveValueAttribute : MapMetadataAttribute {
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ContainKeyAttribute : MapMetadataAttribute {
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ContainValueAttribute : MapMetadataAttribute {
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ForKeyAttribute : MapMetadataAttribute {
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ForValueAttribute : MapMetadataAttribute {
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ForAllAttribute : MapMetadataAttribute {
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ISelfAttribute : Attribute {
        public string? instantiation;

        public static ISelfAttribute of(Dictionary<string, object?> data) {
            var attribute = new ISelfAttribute();

            data.TryGetValue("instantiation", out var instantiation);
            attribute.instantiation = instantiation!.ToString();

            return attribute;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class IPackAttribute : Attribute {
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class IPartialAttribute : Attribute {
        public string? model;

        public static IPartialAttribute of(Dictionary<string, object?> data) {
            var attribute = new IPartialAttribute();

            data.TryGetValue("model", out var model);
            attribute.model = model!.ToString();

            return attribute;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class MultipleExtendsAttribute : Attribute {
        public string? className;

        public static MultipleExtendsAttribute of(Dictionary<string, object?> data) {
            var attribute = new MultipleExtendsAttribute();

            data.TryGetValue("className", out var model);
            attribute.className = model!.ToString();

            return attribute;
        }
    }
    
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class PackFieldAttribute : Attribute {
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ToStringFieldAttribute : Attribute {
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class HashCodeFieldAttribute : Attribute {
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class EqualsFieldAttribute : Attribute {
    }
}