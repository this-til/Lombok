using System;
using System.Collections.Generic;
using System.IO;

namespace Til.Lombok {

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ILombokAttribute : Attribute {

    }

    [AttributeUsage(AttributeTargets.Class)]
    public class IFreezeAttribute : Attribute {

        public IFreezeAttribute() {
        }

        public IFreezeAttribute(Dictionary<string, string> data) {
        }

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
        public string? freezeTag;

        /// <summary>
        /// 自定义前缀
        /// 默认 get、set 等，根据注解类型设定
        /// </summary>
        public string? customPrefix;

        /// <summary>
        /// 自定义后缀
        /// 默认为空
        /// </summary>
        public string? customSuffix;

        /// <summary>
        /// 自定义名称
        /// </summary>
        public string? customName;

        /// <summary>
        /// 自定义类型
        /// </summary>
        public string? customType;

        /// <summary>
        /// 调用更新方法
        /// </summary>
        public bool updateField;

        /// <summary>
        /// 可见性
        /// </summary>
        public AccessLevel accessLevel = AccessLevel.Public;

        /// <summary>
        /// 方法类型
        /// </summary>
        public MethodType methodType = MethodType.def;

        public MetadataAttribute() {
        }

        public MetadataAttribute(Dictionary<string, string> data) {
            // ReSharper disable once InlineOutVariableDeclaration
            string value;
            if (data.TryGetValue(nameof(this.link), out value)) {
                this.link = bool.TryParse(value, out _);
            }
            if (data.TryGetValue(nameof(this.noNull), out value)) {
                this.noNull = bool.TryParse(value, out _);
            }
            if (data.TryGetValue(nameof(this.freezeTag), out value)) {
                this.freezeTag = value;
            }
            if (data.TryGetValue(nameof(this.customPrefix), out value)) {
                this.customPrefix = value;
            }
            if (data.TryGetValue(nameof(this.customSuffix), out value)) {
                this.customSuffix = value;
            }
            if (data.TryGetValue(nameof(this.customName), out value)) {
                this.customName = value;
            }
            if (data.TryGetValue(nameof(this.customType), out value)) {
                this.customType = value;
            }
            if (data.TryGetValue(nameof(this.updateField), out value)) {
                this.updateField = bool.TryParse(value, out _);
            }
            if (data.TryGetValue(nameof(this.accessLevel), out value)) {
                Enum.TryParse(value, out accessLevel);
            }
            if (data.TryGetValue(nameof(this.methodType), out value)) {
                Enum.TryParse(value, out methodType);
            }
        }

    }

    public class ContainerMetadataAttribute : MetadataAttribute {

        /// <summary>
        /// 直接的
        /// </summary>
        public bool direct;

        /// <summary>
        /// 在for中指定是否使用yield
        /// </summary>
        public bool useYield;

        /// <summary>
        /// 在list方法中指定泛型类型
        /// </summary>
        public string? listCellType;

        /// <summary>
        /// 在map方法中指定泛型Key类型
        /// </summary>
        public string? keyType;

        /// <summary>
        /// 在map方法中指定泛型Value类型
        /// </summary>
        public string? valueType;

        public ContainerMetadataAttribute() {
        }

        public ContainerMetadataAttribute(Dictionary<string, string> data) : base(data) {
            // ReSharper disable once InlineOutVariableDeclaration
            string value;
            if (data.TryGetValue(nameof(this.direct), out value)) {
                this.direct = bool.TryParse(value, out _);
            }
            if (data.TryGetValue(nameof(this.useYield), out value)) {
                this.useYield = bool.TryParse(value, out _);
            }
            if (data.TryGetValue(nameof(this.listCellType), out value)) {
                this.listCellType = value;
            }
            if (data.TryGetValue(nameof(this.keyType), out value)) {
                this.keyType = value;
            }
            if (data.TryGetValue(nameof(this.valueType), out value)) {
                this.valueType = value;
            }
        }

    }

    public class ListMetadataAttribute : ContainerMetadataAttribute {

        public ListMetadataAttribute() {
        }

        public ListMetadataAttribute(Dictionary<string, string> data) : base(data) {
        }

    }

    public class MapMetadataAttribute : ContainerMetadataAttribute {

        public MapMetadataAttribute() {
        }

        public MapMetadataAttribute(Dictionary<string, string> data) : base(data) {
        }

    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class GetAttribute : MetadataAttribute {

        public GetAttribute() {
        }

        public GetAttribute(Dictionary<string, string> data) : base(data) {
        }

    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class SetAttribute : MetadataAttribute {

        public SetAttribute() {
        }

        public SetAttribute(Dictionary<string, string> data) : base(data) {
        }

    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class OpenAttribute : MetadataAttribute {

        public OpenAttribute() {
        }

        public OpenAttribute(Dictionary<string, string> data) : base(data) {
        }

    }

    //------------------------------------------------------------------------------------

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class CountAttribute : ListMetadataAttribute {

        public CountAttribute() {
        }

        public CountAttribute(Dictionary<string, string> data) : base(data) {
        }

    }

    //------------------------------------------------------------------------------------

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class AddAttribute : ListMetadataAttribute {

        public AddAttribute() {
        }

        public AddAttribute(Dictionary<string, string> data) : base(data) {
        }

    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class RemoveAttribute : ListMetadataAttribute {

        public RemoveAttribute() {
        }

        public RemoveAttribute(Dictionary<string, string> data) : base(data) {
        }

    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class IndexAttribute : ListMetadataAttribute {

        public IndexAttribute() {
        }

        public IndexAttribute(Dictionary<string, string> data) : base(data) {
        }

    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class ContainAttribute : ListMetadataAttribute {

        public ContainAttribute() {
        }

        public ContainAttribute(Dictionary<string, string> data) : base(data) {
        }

    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class ForAttribute : ListMetadataAttribute {

        public ForAttribute() {
        }

        public ForAttribute(Dictionary<string, string> data) : base(data) {
        }

    }

    //------------------------------------------------------------------------------------

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class PutAttribute : MapMetadataAttribute {

        public PutAttribute() {
        }

        public PutAttribute(Dictionary<string, string> data) : base(data) {
        }

    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class MapGetAttribute : MapMetadataAttribute {

        public MapGetAttribute() {
        }

        public MapGetAttribute(Dictionary<string, string> data) : base(data) {
        }

    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class RemoveKeyAttribute : MapMetadataAttribute {

        public RemoveKeyAttribute() {
        }

        public RemoveKeyAttribute(Dictionary<string, string> data) : base(data) {
        }

    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class RemoveValueAttribute : MapMetadataAttribute {

        public RemoveValueAttribute() {
        }

        public RemoveValueAttribute(Dictionary<string, string> data) : base(data) {
        }

    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class ContainKeyAttribute : MapMetadataAttribute {

        public ContainKeyAttribute() {
        }

        public ContainKeyAttribute(Dictionary<string, string> data) : base(data) {
        }

    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class ContainValueAttribute : MapMetadataAttribute {

        public ContainValueAttribute() {
        }

        public ContainValueAttribute(Dictionary<string, string> data) : base(data) {
        }

    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class ForKeyAttribute : MapMetadataAttribute {

        public ForKeyAttribute() {
        }

        public ForKeyAttribute(Dictionary<string, string> data) : base(data) {
        }

    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class ForValueAttribute : MapMetadataAttribute {

        public ForValueAttribute() {
        }

        public ForValueAttribute(Dictionary<string, string> data) : base(data) {
        }

    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class ForAllAttribute : MapMetadataAttribute {

        public ForAllAttribute() {
        }

        public ForAllAttribute(Dictionary<string, string> data) : base(data) {
        }

    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field, AllowMultiple = true)]
    public class IPartialAttribute : Attribute {

        public string? model;

        public string? customFill;

        public Dictionary<string, string>? _customFill;

        public PartialPos partialPos;

        public IPartialAttribute() {
        }

        public IPartialAttribute(Dictionary<string, string> data) {
            string value;
            if (data.TryGetValue(nameof(this.model), out value)) {
                this.model = value;
            }
            if (data.TryGetValue(nameof(this.partialPos), out value)) {
                Enum.TryParse(value, out partialPos);
            }
            if (data.TryGetValue(nameof(this.customFill), out value)) {
                this.customFill = value;
                _customFill = new Dictionary<string, string>();
                StringReader stringReader = new StringReader(customFill);
                while (stringReader.ReadLine() is { } line) {
                    string[] kv = line.Split(':');
                    if (kv.Length != 2) {
                        continue;
                    }
                    string k = kv[0].Trim();
                    string v = kv[1].Trim();

                    _customFill[k] = v;
                }
            }
        }

    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ToStringFieldAttribute : Attribute {

        public ToStringFieldAttribute() {
        }

        public ToStringFieldAttribute(Dictionary<string, string> data) {
        }

    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class HashCodeFieldAttribute : Attribute {

        public HashCodeFieldAttribute() {
        }

        public HashCodeFieldAttribute(Dictionary<string, string> data) {
        }

    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class EqualsFieldAttribute : Attribute {

        public EqualsFieldAttribute() {
        }

        public EqualsFieldAttribute(Dictionary<string, string> data) {
        }

    }

    public abstract class StringClassAttribute : Attribute {

        public bool hasBase;

        public StringClassAttribute() {
        }

        public StringClassAttribute(Dictionary<string, string> data) {
            string value;
            if (data.TryGetValue(nameof(this.hasBase), out value)) {
                this.hasBase = bool.TryParse(value, out _);
            }
        }
        
        
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ToStringClassAttribute : StringClassAttribute {

        public ToStringClassAttribute() {
        }

        public ToStringClassAttribute(Dictionary<string, string> data) : base(data) {
        }

    }

    [AttributeUsage(AttributeTargets.Class)]
    public class HashCodeClassAttribute : StringClassAttribute {

        public HashCodeClassAttribute() {
        }

        public HashCodeClassAttribute(Dictionary<string, string> data) : base(data) {
        }

    }

    [AttributeUsage(AttributeTargets.Class)]
    public class EqualsClassAttribute : StringClassAttribute {

        public EqualsClassAttribute() {
        }

        public EqualsClassAttribute(Dictionary<string, string> data) : base(data) {
        }

    }

}
