using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Til.Lombok {

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ILombokAttribute : Attribute {

    }

    public abstract class MetadataAttribute : Attribute {

        #region customName

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

        public SyntaxToken changeIdentifier(SyntaxToken tag) {
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

        #endregion

        #region customType

        /// <summary>
        /// 自定义类型
        /// </summary>
        public string customType = String.Empty;

        /// <summary>
        /// 自定义泛型类型
        /// 以','分隔
        /// <A,B,C> "int,string,List<string>"
        /// 如果不希望设置某个位置的泛型请留空 "int,,List<string>"
        /// </summary>
        public string[]? customGenericType;

        public virtual TypeSyntax changeTypeSyntax(TypeSyntax typeSyntax) {
            if (customType != null) {
                return ParseTypeName(customType);
            }
            if (customGenericType != null) {
                List<TypeSyntax> genericTypeList = new List<TypeSyntax>();
                if (typeSyntax is GenericNameSyntax genericNameSyntax) {
                    genericTypeList.AddRange(genericNameSyntax.TypeArgumentList.Arguments);
                }
                int max = Math.Max(customGenericType.Length, genericTypeList.Count);
                for (int i = genericTypeList.Count; i < max; i++) {
                    genericTypeList.Add(ParseTypeName("object"));
                }
                for (var i = 0; i < customGenericType.Length; i++) {
                    string s = customGenericType[i];
                    if (string.IsNullOrWhiteSpace(s)) {
                        continue;
                    }
                    genericTypeList[i] = ParseTypeName(s);
                }
                return GenericName
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
        }

        #endregion

        #region methodModification

        /// <summary>
        /// 可见性
        /// </summary>
        public AccessLevel accessLevel = AccessLevel.Public;

        /// <summary>
        /// 方法类型
        /// </summary>
        public MethodType methodType = MethodType.def;

        /// <summary>
        /// 添加new关键字
        /// </summary>
        public bool hasNew = false;

        public virtual MethodDeclarationSyntax changeMethodDeclarationSyntax(MethodDeclarationSyntax methodDeclarationSyntax) {
            switch (accessLevel) {
                case AccessLevel.Private:
                    methodDeclarationSyntax = methodDeclarationSyntax.AddModifiers
                    (
                        Token
                        (
                            SyntaxKind.PrivateKeyword
                        )
                    );
                    break;
                case AccessLevel.Protected:
                    methodDeclarationSyntax = methodDeclarationSyntax.AddModifiers
                    (
                        Token
                        (
                            SyntaxKind.ProtectedKeyword
                        )
                    );
                    break;
                case AccessLevel.ProtectedInternal:
                    methodDeclarationSyntax = methodDeclarationSyntax.AddModifiers
                    (
                        Token
                        (
                            SyntaxKind.ProtectedKeyword
                        ),
                        Token
                        (
                            SyntaxKind.InternalKeyword
                        )
                    );
                    break;
                case AccessLevel.Internal:
                    methodDeclarationSyntax = methodDeclarationSyntax.AddModifiers
                    (
                        Token
                        (
                            SyntaxKind.InternalKeyword
                        )
                    );
                    break;
                case AccessLevel.Public:
                    methodDeclarationSyntax = methodDeclarationSyntax.AddModifiers
                    (
                        Token
                        (
                            SyntaxKind.PublicKeyword
                        )
                    );
                    break;
            }

            switch (methodType) {
                case MethodType.def:
                    break;
                case MethodType.Abstract:
                    methodDeclarationSyntax = methodDeclarationSyntax.WithBody(null)
                        .AddModifiers
                        (
                            Token
                            (
                                SyntaxKind.AbstractKeyword
                            )
                        )
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
                    break;
                case MethodType.Partial:
                    methodDeclarationSyntax = methodDeclarationSyntax.WithBody(null)
                        .AddModifiers
                        (
                            Token
                            (
                                SyntaxKind.PartialKeyword
                            )
                        )
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
                    break;
                case MethodType.Override:
                    methodDeclarationSyntax = methodDeclarationSyntax.AddModifiers
                    (
                        Token
                        (
                            SyntaxKind.OverrideKeyword
                        )
                    );
                    break;
                case MethodType.Virtual:
                    methodDeclarationSyntax = methodDeclarationSyntax.AddModifiers
                    (
                        Token
                        (
                            SyntaxKind.VirtualKeyword
                        )
                    );
                    break;
            }

            if (hasNew) {
                methodDeclarationSyntax = methodDeclarationSyntax.AddModifiers
                (
                    Token
                    (
                        SyntaxKind.NewKeyword
                    )
                );
            }

            return methodDeclarationSyntax;
        }

        #endregion

        #region methodContent

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

        public MethodDeclarationSyntax changeMethodContent(MethodDeclarationSyntax methodDeclarationSyntax, Context context, Dictionary<string, string> dictionaryFill) {
            if (methodDeclarationSyntax.Body is null) {
                return methodDeclarationSyntax;
            }
            List<StatementSyntax> statementSyntaxes = new List<StatementSyntax>();

            if (noNull) {
                statementSyntaxes.AddRange
                (
                    methodDeclarationSyntax.ParameterList.Parameters.Select
                    (
                        parameterListParameter => IfStatement
                        (
                            BinaryExpression
                            (
                                SyntaxKind.EqualsExpression,
                                IdentifierName(parameterListParameter.Identifier.Text),
                                LiteralExpression(SyntaxKind.NullLiteralExpression)
                            ),
                            Block
                            (
                                ThrowStatement
                                (
                                    ObjectCreationExpression
                                    (
                                        ParseTypeName("System.NullReferenceException"),
                                        ArgumentList
                                        (
                                            SeparatedList
                                            (
                                                new[] {
                                                    Argument
                                                    (
                                                        LiteralExpression
                                                        (
                                                            SyntaxKind.StringLiteralExpression,
                                                            Literal
                                                            (
                                                                $"{methodDeclarationSyntax.Identifier.Text}.{parameterListParameter.Identifier.Text} is null"
                                                            )
                                                        )
                                                    )
                                                }
                                            )
                                        ),
                                        null
                                    )
                                )
                            )
                        )
                    )
                );
            }

            if (freezeTag is not null) {
                statementSyntaxes.Add
                (
                    ExpressionStatement
                    (
                        InvocationExpression
                        (
                            MemberAccessExpression
                            (
                                SyntaxKind.SimpleMemberAccessExpression, // 使用点号访问  ,
                                ThisExpression(),
                                IdentifierName
                                (
                                    "validateNonFrozen"
                                )
                            ),
                            ArgumentList
                            (
                                SingletonSeparatedList
                                (
                                    Argument
                                    (
                                        LiteralExpression
                                        (
                                            SyntaxKind.StringLiteralExpression,
                                            Literal
                                            (
                                                freezeTag
                                            )
                                        )
                                    )
                                )
                            )
                        )
                    )
                );
            }

            if (beforeOperation is not null) {
                statementSyntaxes.AddRange
                (
                    CSharpSyntaxTree.ParseText(beforeOperation.format(dictionaryFill))
                        .GetRoot()
                        .ChildNodes()
                        .OfType<StatementSyntax>()
                );
            }

            statementSyntaxes.AddRange(methodDeclarationSyntax.Body!.Statements);

            if (afterOperation is not null) {
                statementSyntaxes.AddRange
                (
                    CSharpSyntaxTree.ParseText(afterOperation.format(dictionaryFill))
                        .GetRoot()
                        .ChildNodes()
                        .OfType<StatementSyntax>()
                );
            }

            if (link && methodDeclarationSyntax.ReturnType.ToString().Equals("void")) {
                methodDeclarationSyntax = methodDeclarationSyntax
                    .WithReturnType
                    (
                        changeTypeSyntax
                        (
                            ParseTypeName
                            (
                                context.contextTargetNode.getGenericName()
                            )
                        )
                    );
                statementSyntaxes.Add
                (
                    ReturnStatement
                    (
                        ThisExpression()
                    )
                );
            }

            return methodDeclarationSyntax.WithBody(Block(statementSyntaxes));
        }

        #endregion

        /// <summary>
        /// 在执行固定逻辑之
        /// </summary>
        public string? beforeOperation;

        public string? afterOperation;

        public virtual Dictionary<string, string> generateDictionaryFill(Context context, FieldOrPropertyPack? fieldOrPropertyPack) {
            Dictionary<string, string> fill = new Dictionary<string, string>();
            fill["type"] = context.contextTargetNode.Identifier.ToString();
            fill["namespace"] = context.contextNamespaceNameSyntax.ToString();
            if (fieldOrPropertyPack is not null) {
                fill["field"] = fieldOrPropertyPack.name.ToString();
                fill["fieldType"] = changeTypeSyntax(fieldOrPropertyPack.typeSyntax).ToString();
            }
            return fill;
        }

        public virtual void onField(Context context, FieldOrPropertyPack fieldOrPropertyPack) {

        }

        public virtual void onMethod(Context context, MethodDeclarationSyntax methodDeclarationSyntax) {

        }

        public virtual void onClass(Context context, ClassDeclarationSyntax classDeclarationSyntax) {

        }

    }

    //------------------------------------

    public abstract class ContainerMetadataAttribute : MetadataAttribute {

        /// <summary>
        /// 在for中指定是否使用yield
        /// </summary>
        public bool useYield;

    }

    public abstract class ListMetadataAttribute : ContainerMetadataAttribute {

        /// <summary>
        /// 在list方法中指定泛型类型
        /// </summary>
        public string? listCellType;

        public TypeSyntax? searchListCellType(TypeSyntax baseType) {
            if (listCellType is not null) {
                return ParseTypeName(listCellType);
            }
            return baseType is not GenericNameSyntax genericNameSyntax
                ? null
                : genericNameSyntax.TypeArgumentList.Arguments[0];
        }

    }

    public abstract class MapMetadataAttribute : ContainerMetadataAttribute {

        /// <summary>
        /// 在map方法中指定泛型Key类型
        /// </summary>
        public string? keyType;

        /// <summary>
        /// 在map方法中指定泛型Value类型
        /// </summary>
        public string? valueType;

        public TypeSyntax? searchKeyType(TypeSyntax baseType) {
            if (keyType is not null) {
                return ParseTypeName(keyType);
            }
            return baseType is not GenericNameSyntax genericNameSyntax
                ? null
                : genericNameSyntax.TypeArgumentList.Arguments[0];
        }

        public TypeSyntax? searchValueType(TypeSyntax baseType) {
            if (valueType is not null) {
                return ParseTypeName(valueType);
            }
            if (baseType is not GenericNameSyntax genericNameSyntax) {
                return null;
            }
            SeparatedSyntaxList<TypeSyntax> separatedSyntaxList = genericNameSyntax.TypeArgumentList.Arguments;
            if (separatedSyntaxList.Count < 2) {
                return null;
            }
            return separatedSyntaxList[2];
        }

    }

    //--------------------------------------

    public class GetAttribute : MetadataAttribute {

        public GetAttribute() {
            customPrefix = "get";
        }

        public override void onField(Context context, FieldOrPropertyPack fieldOrPropertyPack) {
            base.onField(context, fieldOrPropertyPack);
            context.addInPartialClassMembers
            (
                changeMethodContent
                (
                    MethodDeclaration
                        (
                            changeTypeSyntax(fieldOrPropertyPack.typeSyntax),
                            changeIdentifier(fieldOrPropertyPack.name)
                        )
                        .AddBodyStatements
                        (
                            ReturnStatement
                            (
                                MemberAccessExpression
                                (
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    ThisExpression(),
                                    IdentifierName
                                    (
                                        fieldOrPropertyPack.name
                                    )
                                )
                            )
                        ),
                    context,
                    generateDictionaryFill(context, fieldOrPropertyPack)
                )
            );
        }

    }
    

}