using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Til.Lombok.Generator;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Til.Lombok {

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ILombokAttribute : Attribute {

    }

    [AttributeUsage(AttributeTargets.Class)]
    public class IFreezeAttribute : IncrementClassAttribute {

        public IFreezeAttribute() {
        }

        public IFreezeAttribute(Dictionary<string, string> data) {
        }

    }

    public class MetadataAttribute : Attribute {

        /// <summary>
        /// 自定义前缀
        /// </summary>
        public string? customPrefix;

        /// <summary>
        /// 自定义后缀
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

        /*/// <summary>
        /// 自定义泛型类型
        /// 以','分隔
        /// <A,B,C> "int,string,List<string>"
        /// 如果不希望设置某个位置的泛型请留空 "int,,List<string>"
        /// </summary>
        public string? customGenericType;*/

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

        public bool isCustomName() => customPrefix != null || customSuffix != null || customName != null;

        /*public SyntaxToken changeIdentifier(SyntaxToken tag) {
            if (customName != null) {
                return Identifier(customName);
            }
            switch (customName, customSuffix) {
                case (not null, null):
                    return Identifier($"{customSuffix}{tag.ToString().toPascalCaseIdentifier()}");
                case (null, not null):
                    return Identifier($"{tag}{customSuffix}");
                case (not null, not null):
                    return Identifier($"{customSuffix}{tag.ToString().toPascalCaseIdentifier()}{customSuffix}");
                default:
                    return tag;
            }
        }

        public TypeSyntax changeTypeSyntax(TypeSyntax typeSyntax) {
            if (customType != null) {
                return ParseTypeName(customType);
            }
            if (customGenericType != null) {
                string[] strings = customGenericType.Split(',');
                List<TypeSyntax> genericTypeList = new List<TypeSyntax>();
                if (typeSyntax is GenericNameSyntax genericNameSyntax) {
                    genericTypeList.AddRange(genericNameSyntax.TypeArgumentList.Arguments);
                }
                int max = Math.Max(strings.Length, genericTypeList.Count);
                for (int i = genericTypeList.Count; i < max; i++) {
                for (var i = 0; i < strings.Length; i++) {
                    genericTypeList.Add(ParseTypeName("object"));

                }
                    string s = strings[i];
                    if (string.IsNullOrWhiteSpace(s)) {
                        continue;
                    }
                    genericTypeList[i] = ParseTypeName(s);
                }
                return typeSyntax is GenericNameSyntax _genericNameSyntax
                    ? _genericNameSyntax.WithTypeArgumentList
                    (
                        TypeArgumentList
                        (
                            SeparatedList
                            (
                                genericTypeList
                            )
                        )
                    )
                    : GenericName
                    (
                        Identifier(typeSyntax.ToString().eliminateGeneric()),
                        TypeArgumentList
                        (
                            SeparatedList
                            (
                                genericTypeList
                            )
                        )
                    );
            }
            return typeSyntax;
        }*/

    }

    public class ContainerMetadataAttribute : MetadataAttribute {

        /// <summary>
        /// 在for中指定是否使用yield
        /// </summary>
        public bool useYield;

        public ContainerMetadataAttribute() {
        }

        public ContainerMetadataAttribute(Dictionary<string, string> data) : base(data) {
            // ReSharper disable once InlineOutVariableDeclaration
            string value;

            if (data.TryGetValue(nameof(this.useYield), out value)) {
                this.useYield = bool.TryParse(value, out _);
            }

        }

    }

    public class ListMetadataAttribute : ContainerMetadataAttribute {

        /// <summary>
        /// 在list方法中指定泛型类型
        /// </summary>
        public string? listCellType;

        public ListMetadataAttribute() {
        }

        public ListMetadataAttribute(Dictionary<string, string> data) : base(data) {
            data.TryGetValue(nameof(listCellType), out listCellType);
        }

    }

    public class MapMetadataAttribute : ContainerMetadataAttribute {

        /// <summary>
        /// 在map方法中指定泛型Key类型
        /// </summary>
        public string? keyType;

        /// <summary>
        /// 在map方法中指定泛型Value类型
        /// </summary>
        public string? valueType;

        public MapMetadataAttribute() {
        }

        public MapMetadataAttribute(Dictionary<string, string> data) : base(data) {
            data.TryGetValue(nameof(keyType), out keyType);
            data.TryGetValue(nameof(valueType), out valueType);
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

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class IPartialAttribute : MetadataAttribute {

        public string? model;

        public string? customFill;

        public Dictionary<string, string>? _customFill;

        public PartialPos partialPos;

        public IPartialAttribute() {
        }

        public IPartialAttribute(Dictionary<string, string> data) : base(data) {
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
    public class FieldAttribute : MetadataAttribute {

        public FieldAttribute() {
        }

        public FieldAttribute(Dictionary<string, string> data) : base(data) {
        }

    }

    public abstract class IncrementFieldAttribute : FieldAttribute {

        public bool directAccess;

        public IncrementFieldAttribute() {
        }

        public IncrementFieldAttribute(Dictionary<string, string> data) : base(data) {
            if (data.TryGetValue(nameof(this.directAccess), out string value)) {
                this.directAccess = bool.TryParse(value, out _);
            }
        }

    }

    public class ToStringFieldAttribute : IncrementFieldAttribute {

        public ToStringFieldAttribute() {
        }

        public ToStringFieldAttribute(Dictionary<string, string> data) : base(data) {
        }

    }

    public class HashCodeFieldAttribute : IncrementFieldAttribute {

        public HashCodeFieldAttribute() {
        }

        public HashCodeFieldAttribute(Dictionary<string, string> data) : base(data) {
        }

    }

    public class EqualsFieldAttribute : IncrementFieldAttribute {

        public EqualsFieldAttribute() {
        }

        public EqualsFieldAttribute(Dictionary<string, string> data) : base(data) {
        }

    }

    public class ExportFieldAttribute : IncrementFieldAttribute {

        public ExportFieldAttribute() {
        }

        public ExportFieldAttribute(Dictionary<string, string> data) : base(data) {
        }

    }

    [AttributeUsage(AttributeTargets.Method)]
    public class MethodAttribute : MetadataAttribute {

        public MethodAttribute() {
        }

        public MethodAttribute(Dictionary<string, string> data) : base(data) {
        }

    }

    public class ExtendMethodAttribute : MethodAttribute {

        public string? injectMethod;

        public ExtendMethodAttribute() {
        }

        public ExtendMethodAttribute(Dictionary<string, string> data) : base(data) {
            data.TryGetValue(nameof(injectMethod), out injectMethod);
        }

    }

    [AttributeUsage(AttributeTargets.Class)]
    public abstract class ClassAttribute : MetadataAttribute {

        public bool hasBase;

        public ClassAttribute() {
        }

        public ClassAttribute(Dictionary<string, string> data) : base(data) {
            string value;
            if (data.TryGetValue(nameof(this.hasBase), out value)) {
                this.hasBase = bool.TryParse(value, out _);
            }
        }

    }

    [AttributeUsage(AttributeTargets.Class)]
    public abstract class IncrementClassAttribute : ClassAttribute {

        protected IncrementClassAttribute() {
        }

        protected IncrementClassAttribute(Dictionary<string, string> data) : base(data) {
        }

    }

    public class ToStringClassAttribute : IncrementClassAttribute {

        public ToStringClassAttribute() {
        }

        public ToStringClassAttribute(Dictionary<string, string> data) : base(data) {
        }

    }

    public class HashCodeClassAttribute : IncrementClassAttribute {

        public HashCodeClassAttribute() {
        }

        public HashCodeClassAttribute(Dictionary<string, string> data) : base(data) {
        }

    }

    public class EqualsClassAttribute : IncrementClassAttribute {

        public EqualsClassAttribute() {
        }

        public EqualsClassAttribute(Dictionary<string, string> data) : base(data) {
        }

    }

    public class ExportClassAttribute : IncrementClassAttribute {

        public ExportClassAttribute() {
        }

        public ExportClassAttribute(Dictionary<string, string> data) : base(data) {
        }

    }

    public class ExtendMethodClassAttribute : ClassAttribute {

    }

}