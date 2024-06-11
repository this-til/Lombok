using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Til.Lombok;

public abstract class GeneratorBasics : IIncrementalGenerator {
    private static readonly string AttributeName = typeof(ILombokAttribute).FullName!;

    public void Initialize(IncrementalGeneratorInitializationContext context) {
        var sources = context.SyntaxProvider.ForAttributeWithMetadataName(AttributeName, IsCandidate, Transform);
        context.AddSources(sources);
    }

    private bool IsCandidate(SyntaxNode node, CancellationToken cancellationToken) => node is ClassDeclarationSyntax;

    private GeneratorResult Transform(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken) {
        ClassDeclarationSyntax contextTargetNode = (ClassDeclarationSyntax)context.TargetNode;

        if (!contextTargetNode.TryValidateType(out var @namespace, out var diagnostic)) {
            return new GeneratorResult(diagnostic);
        }
        List<MethodDeclarationSyntax> methodDeclarationSyntaxes = new List<MethodDeclarationSyntax>();

        // 遍历类的所有成员  
        foreach (var member in contextTargetNode.Members) {
            switch (member) {
                // 检查成员是否是字段或属性  
                case FieldDeclarationSyntax fieldDeclaration: {
                    foreach (MethodDeclarationSyntax methodDeclarationSyntax in onFieldDeclarationSyntax(fieldDeclaration)) {
                        try {
                            methodDeclarationSyntaxes.Add(methodDeclarationSyntax);
                        }
                        catch (Exception e) {
                            Console.WriteLine(e);
                        }
                    }
                    break;
                }
                case PropertyDeclarationSyntax propertyDeclaration: {
                    foreach (MethodDeclarationSyntax methodDeclarationSyntax in onPropertyDeclarationSyntax(propertyDeclaration)) {
                        try {
                            methodDeclarationSyntaxes.Add(methodDeclarationSyntax);
                        }
                        catch (Exception e) {
                            Console.WriteLine(e);
                        }
                    }
                    break;
                }
            }
        }

        if (methodDeclarationSyntaxes.Count == 0) {
            return GeneratorResult.Empty;
        }

        return new GeneratorResult(contextTargetNode.GetHintName(@namespace), CreateMethodUtil.CreatePartialClass(@namespace, contextTargetNode, methodDeclarationSyntaxes));
    }

    public abstract IEnumerable<MethodDeclarationSyntax> onFieldDeclarationSyntax(FieldDeclarationSyntax fieldDeclarationSyntax);

    public abstract IEnumerable<MethodDeclarationSyntax> onPropertyDeclarationSyntax(PropertyDeclarationSyntax propertyDeclarationSyntax);
}

[Generator]
public sealed class FreezeGenerator : IIncrementalGenerator {
    private static readonly string AttributeName = typeof(IFreezeAttribute).FullName!;

    public void Initialize(IncrementalGeneratorInitializationContext context) {
        var sources = context.SyntaxProvider.ForAttributeWithMetadataName(AttributeName, IsCandidate, Transform);
        context.AddSources(sources);
    }

    private bool IsCandidate(SyntaxNode node, CancellationToken cancellationToken) => node is ClassDeclarationSyntax;

    private GeneratorResult Transform(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken) {
        ClassDeclarationSyntax contextTargetNode = (ClassDeclarationSyntax)context.TargetNode;

        if (!contextTargetNode.TryValidateType(out var @namespace, out var diagnostic)) {
            return new GeneratorResult(diagnostic);
        }

        string model = $"""
                        using System.Collections.Generic;

                        namespace {@namespace.ToString()} {'{'}
                        
                            public partial class {contextTargetNode.Identifier.ToString()} {'{'}
                                protected Dictionary<string, bool> _frozen = new Dictionary<string, bool>();
                            
                                public bool isFrozen(string tag) {'{'}
                                    if (_frozen.ContainsKey(tag)) {'{'}
                                        return _frozen[tag];
                                    {'}'}
                                    _frozen.Add(tag, false);
                                    return false;
                                {'}'}
                            
                                public void frozen(string tag) {'{'}
                                    if (_frozen.ContainsKey(tag)) {'{'}
                                        _frozen[tag] = true;
                                        return;
                                    {'}'}
                                    _frozen.Add(tag, true);
                                {'}'}
                            
                                public void validateNonFrozen(string tag) {'{'}
                                    if (_frozen.ContainsKey(tag) && _frozen[tag]) {'{'}
                                        throw new InvalidOperationException("Cannot modify frozen property");
                                    {'}'}
                                {'}'}
                            {'}'}
                        {'}'}
                        """;

        return new GeneratorResult(
            contextTargetNode.GetHintName(@namespace),
            SourceText.From(model, Encoding.UTF8)
        );
    }
}

public abstract class AttributeGenerator : GeneratorBasics {
    public abstract string getAttributeName();
    public abstract IEnumerable<MethodDeclarationSyntax> onFieldDeclarationSyntax(FieldDeclarationSyntax fieldDeclarationSyntax, AttributeSyntax attributeSyntax, Dictionary<string, object> data);

    public abstract IEnumerable<MethodDeclarationSyntax> onPropertyDeclarationSyntax(PropertyDeclarationSyntax propertyDeclarationSyntax, AttributeSyntax attributeSyntax, Dictionary<string, object> data);

    public override IEnumerable<MethodDeclarationSyntax> onFieldDeclarationSyntax(FieldDeclarationSyntax fieldDeclarationSyntax) {
        AttributeSyntax? tryGetSpecifiedAttribute = fieldDeclarationSyntax.AttributeLists.tryGetSpecifiedAttribute(getAttributeName());

        if (tryGetSpecifiedAttribute is null) {
            return Array.Empty<MethodDeclarationSyntax>();
        }
        return onFieldDeclarationSyntax(fieldDeclarationSyntax, tryGetSpecifiedAttribute, tryGetSpecifiedAttribute.getAttributeArgumentsAsDictionary());
    }

    public override IEnumerable<MethodDeclarationSyntax> onPropertyDeclarationSyntax(PropertyDeclarationSyntax propertyDeclarationSyntax) {
        AttributeSyntax? tryGetSpecifiedAttribute = propertyDeclarationSyntax.AttributeLists.tryGetSpecifiedAttribute(getAttributeName());

        if (tryGetSpecifiedAttribute is null) {
            return Array.Empty<MethodDeclarationSyntax>();
        }

        return onPropertyDeclarationSyntax(propertyDeclarationSyntax, tryGetSpecifiedAttribute, tryGetSpecifiedAttribute.getAttributeArgumentsAsDictionary());
    }
}

public abstract class StandardAttributeGenerator<A> : AttributeGenerator {
    public abstract Func<string, string, string, A, MethodDeclarationSyntax> createMethodDeclarationSyntax();

    public abstract Func<Dictionary<string, object>, ClassDeclarationSyntax, TypeSyntax, A> of();

    public override IEnumerable<MethodDeclarationSyntax> onFieldDeclarationSyntax(FieldDeclarationSyntax fieldDeclarationSyntax, AttributeSyntax attributeSyntax, Dictionary<string, object> data) {
        A attribute = of()(data, ((ClassDeclarationSyntax)fieldDeclarationSyntax.Parent), fieldDeclarationSyntax.Declaration.Type);
        if (attribute is null) {
            yield break;
        }
        foreach (VariableDeclaratorSyntax variable in fieldDeclarationSyntax.Declaration.Variables) {
            yield return createMethodDeclarationSyntax()(
                variable.Identifier.ToString(),
                fieldDeclarationSyntax.Declaration.Type.ToString(),
                ((ClassDeclarationSyntax)fieldDeclarationSyntax.Parent).getHasGenericName(),
                attribute
            );
        }
    }

    public override IEnumerable<MethodDeclarationSyntax> onPropertyDeclarationSyntax(PropertyDeclarationSyntax propertyDeclarationSyntax, AttributeSyntax attributeSyntax, Dictionary<string, object> data) {
        A attribute = of()(data, ((ClassDeclarationSyntax)propertyDeclarationSyntax.Parent), propertyDeclarationSyntax.Type);
        if (attribute is null) {
            yield break;
        }
        yield return createMethodDeclarationSyntax()(
            propertyDeclarationSyntax.Identifier.ToString(),
            propertyDeclarationSyntax.Type.ToString(),
            ((ClassDeclarationSyntax)propertyDeclarationSyntax.Parent).getHasGenericName(),
            attribute
        );
    }
}

public abstract class MetadataAttributeGenerator : StandardAttributeGenerator<MetadataAttribute> {
    public override Func<Dictionary<string, object>, ClassDeclarationSyntax, TypeSyntax, MetadataAttribute> of() => (d, c, t) => MetadataAttribute.of(d);
}

public abstract class ListAttributeGenerator : StandardAttributeGenerator<ListMetadataAttribute> {
    public override Func<Dictionary<string, object>, ClassDeclarationSyntax, TypeSyntax, ListMetadataAttribute> of() => (d, c, t) => {
        ListMetadataAttribute listMetadataAttribute = ListMetadataAttribute.of(d);
        if (listMetadataAttribute.type is null && t is GenericNameSyntax genericNameSyntax) {
            TypeSyntax? firstOrDefault = genericNameSyntax.TypeArgumentList.Arguments.FirstOrDefault();
            listMetadataAttribute.type = firstOrDefault?.ToFullString();
        }
        if (listMetadataAttribute.type is null) {
            return null;
        }
        return listMetadataAttribute;
    };
}

public abstract class MapAttributeGenerator : StandardAttributeGenerator<MapMetadataAttribute> {
    public override Func<Dictionary<string, object>, ClassDeclarationSyntax, TypeSyntax, MapMetadataAttribute> of() => (d, c, t) => {
        MapMetadataAttribute mapMetadataAttribute = MapMetadataAttribute.of(d);
        if (t is GenericNameSyntax genericNameSyntax) {
            if (mapMetadataAttribute.keyType is null) {
                TypeSyntax? firstOrDefault = genericNameSyntax.TypeArgumentList.Arguments.FirstOrDefault();
                mapMetadataAttribute.keyType = firstOrDefault?.ToFullString();
            }
            if (mapMetadataAttribute.valueType is null) {
                TypeSyntax? firstOrDefault = genericNameSyntax.TypeArgumentList.Arguments.FirstOrDefault();
                mapMetadataAttribute.valueType = firstOrDefault?.ToFullString();
            }
        }
        if (mapMetadataAttribute.keyType is null || mapMetadataAttribute.valueType is null) {
            return null;
        }
        return mapMetadataAttribute;
    };
}

[Generator]
public sealed class GetGenerator : MetadataAttributeGenerator {
    public override string getAttributeName() => nameof(GetAttribute);

    public override Func<string, string, string, MetadataAttribute, MethodDeclarationSyntax> createMethodDeclarationSyntax() => CreateMethodUtil.CreateGetMethod;
}

[Generator]
public sealed class SetGenerator : MetadataAttributeGenerator {
    public override string getAttributeName() => nameof(SetAttribute);

    public override Func<string, string, string, MetadataAttribute, MethodDeclarationSyntax> createMethodDeclarationSyntax() => CreateMethodUtil.CreateSetMethod;
}

[Generator]
public sealed class IndexGenerator : ListAttributeGenerator {
    private static readonly string getAtName = nameof(IndexAttribute);

    public override string getAttributeName() => nameof(IndexAttribute);

    public override Func<string, string, string, ListMetadataAttribute, MethodDeclarationSyntax> createMethodDeclarationSyntax() => CreateMethodUtil.CreateGetAtMethod;
}

[Generator]
public sealed class AddGenerator : ListAttributeGenerator {
    public override string getAttributeName() => nameof(AddAttribute);

    public override Func<string, string, string, ListMetadataAttribute, MethodDeclarationSyntax> createMethodDeclarationSyntax() => CreateMethodUtil.CreateAddMethod;
}

[Generator]
public sealed class RemoveGenerator : ListAttributeGenerator {
    public override string getAttributeName() => nameof(RemoveAttribute);

    public override Func<string, string, string, ListMetadataAttribute, MethodDeclarationSyntax> createMethodDeclarationSyntax() => CreateMethodUtil.CreateRemoveMethod;
}

[Generator]
public sealed class ContainGenerator : ListAttributeGenerator {
    public override string getAttributeName() => nameof(ContainAttribute);

    public override Func<string, string, string, ListMetadataAttribute, MethodDeclarationSyntax> createMethodDeclarationSyntax() => CreateMethodUtil.CreateContainMethod;
}

[Generator]
public sealed class ForGenerator : ListAttributeGenerator {
    public override string getAttributeName() => nameof(ForAttribute);

    public override Func<string, string, string, ListMetadataAttribute, MethodDeclarationSyntax> createMethodDeclarationSyntax() => CreateMethodUtil.CreateForMethod;
}

[Generator]
public sealed class PutGenerator : MapAttributeGenerator {
    public override string getAttributeName() => nameof(PutAttribute);

    public override Func<string, string, string, MapMetadataAttribute, MethodDeclarationSyntax> createMethodDeclarationSyntax() => CreateMethodUtil.CreatePutMethod;
}

[Generator]
public sealed class MapGetGenerator : MapAttributeGenerator {
    public override string getAttributeName() => nameof(MapGetAttribute);

    public override Func<string, string, string, MapMetadataAttribute, MethodDeclarationSyntax> createMethodDeclarationSyntax() => CreateMethodUtil.CreateMapGetMethod;
}

[Generator]
public sealed class RemoveKeyGenerator : MapAttributeGenerator {
    public override string getAttributeName() => nameof(RemoveKeyAttribute);

    public override Func<string, string, string, MapMetadataAttribute, MethodDeclarationSyntax> createMethodDeclarationSyntax() => CreateMethodUtil.CreateRemoveKeyMethod;
}

[Generator]
public sealed class RemoveValueGenerator : MapAttributeGenerator {
    public override string getAttributeName() => nameof(RemoveValueAttribute);

    public override Func<string, string, string, MapMetadataAttribute, MethodDeclarationSyntax> createMethodDeclarationSyntax() => CreateMethodUtil.CreateRemoveValueMethod;
}

[Generator]
public sealed class ContainKeyGenerator : MapAttributeGenerator {
    public override string getAttributeName() => nameof(ContainKeyAttribute);

    public override Func<string, string, string, MapMetadataAttribute, MethodDeclarationSyntax> createMethodDeclarationSyntax() => CreateMethodUtil.CreateContainKeyMethod;
}

[Generator]
public sealed class ContainValueGenerator : MapAttributeGenerator {
    public override string getAttributeName() => nameof(ContainValueAttribute);

    public override Func<string, string, string, MapMetadataAttribute, MethodDeclarationSyntax> createMethodDeclarationSyntax() => CreateMethodUtil.CreateContainValueMethod;
}

[Generator]
public sealed class ForKeyGenerator : MapAttributeGenerator {
    public override string getAttributeName() => nameof(ForKeyAttribute);

    public override Func<string, string, string, MapMetadataAttribute, MethodDeclarationSyntax> createMethodDeclarationSyntax() => CreateMethodUtil.CreateForKeyMethod;
}

[Generator]
public sealed class ForValueGenerator : MapAttributeGenerator {
    public override string getAttributeName() => nameof(ForValueAttribute);

    public override Func<string, string, string, MapMetadataAttribute, MethodDeclarationSyntax> createMethodDeclarationSyntax() => CreateMethodUtil.CreateForValueMethod;
}

[Generator]
public sealed class ForAllGenerator : MapAttributeGenerator {
    public override string getAttributeName() => nameof(ForAllAttribute);

    public override Func<string, string, string, MapMetadataAttribute, MethodDeclarationSyntax> createMethodDeclarationSyntax() => CreateMethodUtil.CreateForAllMethod;
}

public static class CreateMethodUtil {
    public static SourceText CreatePartialClass(NameSyntax @namespace, ClassDeclarationSyntax classDeclaration, IEnumerable<MethodDeclarationSyntax> methods) {
        return @namespace.CreateNewNamespace(classDeclaration.GetUsings(),
                classDeclaration.CreateNewPartialClass()
                    .WithMembers(
                        List<MemberDeclarationSyntax>(methods)
                    )
            ).NormalizeWhitespace()
            .GetText(Encoding.UTF8);
    }

    public static ExpressionStatementSyntax validateNonFrozen(string freezeTag) {
        return
            string.IsNullOrEmpty(freezeTag)
                ? null!
                : ExpressionStatement(
                    InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression, // 使用点号访问  ,
                            ThisExpression(),
                            IdentifierName("validateNonFrozen")
                        ),
                        ArgumentList(
                            SingletonSeparatedList(
                                Argument(
                                    LiteralExpression(
                                        SyntaxKind.StringLiteralExpression,
                                        Literal(freezeTag)
                                    )
                                )
                            )
                        )
                    )
                );
    }

    public static ExpressionStatementSyntax noNull(string fieldName, string? message = null) {
        return ExpressionStatement(
            InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName(nameof(Util)),
                    IdentifierName(nameof(Util.noNull)) // 方法名  
                ),
                ArgumentList( // 创建参数列表  
                    SeparatedList(
                        new[] {
                            Argument( // 创建一个参数表达式  
                                IdentifierName(fieldName) // 引用参数i  
                            ),
                            message is null
                                ? Argument( // 创建一个参数表达式  
                                    IdentifierName($"\"{message}\"") // 引用参数i  
                                )
                                : null!
                        }.Where(v => v is not null)
                    )
                )
            )
        );
    }

    public static MethodDeclarationSyntax CreateGetMethod(string fieldName, string typeName, string parentName, MetadataAttribute metadataAttribute) {
        return MethodDeclaration(
                IdentifierName(typeName),
                "get" + fieldName.ToPascalCaseIdentifier()
            )
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .WithBody(
                Block(
                    new List<StatementSyntax>() {
                        validateNonFrozen(metadataAttribute.freezeTag)!,

                        ReturnStatement(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                ThisExpression(),
                                IdentifierName(fieldName)
                            )
                        )
                    }.Where(v => v is not null).ToList()
                )
            );
    }

    public static MethodDeclarationSyntax CreateSetMethod(string fieldName, string typeName, string parentName, MetadataAttribute metadataAttribute) {
        return MethodDeclaration(
                metadataAttribute.link
                    ? IdentifierName(parentName)
                    : IdentifierName("void"),
                "set" + fieldName.ToPascalCaseIdentifier()
            )
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddParameterListParameters(Parameter(Identifier(fieldName.ToCamelCaseIdentifier().genericEliminate())).WithType(ParseTypeName(typeName)))
            .WithBody(
                Block(
                    new StatementSyntax[] {
                        validateNonFrozen(metadataAttribute.freezeTag)!,
                        metadataAttribute.noNull
                            ? noNull(fieldName, $"{parentName}.{"set" + fieldName.ToPascalCaseIdentifier()}方法中传入参数为null")
                            : null!,
                        ExpressionStatement(
                            AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    ThisExpression(),
                                    IdentifierName(fieldName)
                                ),
                                IdentifierName(fieldName)
                            )
                        ),
                        metadataAttribute.link
                            ? ReturnStatement(
                                ThisExpression()
                            )
                            : null!
                    }.Where(v => v is not null)
                )
            );
    }

    public static MethodDeclarationSyntax CreateGetAtMethod(string fieldName, string typeName, string parentName, ListMetadataAttribute listMetadataAttribute) {
        return MethodDeclaration(
                IdentifierName(listMetadataAttribute.type),
                $"indexIn{fieldName.ToPascalCaseIdentifier()}"
            )
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddParameterListParameters(
                Parameter(Identifier("i")).WithType(ParseTypeName("int"))
            ).WithBody(Block(
                    new StatementSyntax[] {
                        validateNonFrozen(listMetadataAttribute.freezeTag)!,
                        ReturnStatement( // 创建一个 return 语句  
                            ElementAccessExpression( // 创建一个数组或列表的索引访问表达式  
                                IdentifierName(fieldName),
                                BracketedArgumentList( // 索引参数列表  
                                    SingletonSeparatedList( // 单个参数列表  
                                        Argument( // 创建一个参数表达式  
                                            IdentifierName("i") // 引用参数 i  
                                        )
                                    )
                                )
                            )
                        ),
                    }.Where(i => i is not null)
                )
            );
    }

    public static MethodDeclarationSyntax CreateAddMethod(string fieldName, string typeName, string parentName, ListMetadataAttribute listMetadataAttribute) {
        return MethodDeclaration(
                listMetadataAttribute.link
                    ? IdentifierName(parentName)
                    : IdentifierName("void"),
                $"addIn{fieldName.ToPascalCaseIdentifier().genericEliminate()}"
            )
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddParameterListParameters(Parameter(Identifier("a" + listMetadataAttribute.type.ToPascalCaseIdentifier().genericEliminate())).WithType(ParseTypeName(listMetadataAttribute.type)))
            .WithBody(
                Block(
                    new StatementSyntax[] {
                        validateNonFrozen(listMetadataAttribute.freezeTag)!,
                        listMetadataAttribute.noNull
                            ? noNull(fieldName, $"{parentName}.add{listMetadataAttribute.type.ToPascalCaseIdentifier().genericEliminate()}In{fieldName.ToPascalCaseIdentifier().genericEliminate()}方法中传入参数为null")
                            : null!,
                        ExpressionStatement(
                            InvocationExpression( // 创建一个方法调用表达式  
                                MemberAccessExpression( // 创建一个成员访问表达式（this.list.Add）  
                                    SyntaxKind.SimpleMemberAccessExpression, // 使用点号访问  
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression, // 使用点号访问  ,
                                        ThisExpression(),
                                        IdentifierName(fieldName)
                                    ),
                                    IdentifierName("Add") // 访问名为list的成员  
                                ),
                                ArgumentList( // 创建参数列表  
                                    SingletonSeparatedList( // 单个参数列表  
                                        Argument( // 创建一个参数表达式  
                                            IdentifierName("a" + listMetadataAttribute.type.ToPascalCaseIdentifier().genericEliminate()) // 引用参数i  
                                        )
                                    )
                                )
                            )
                        ),
                        listMetadataAttribute.link
                            ? ReturnStatement(
                                ThisExpression()
                            )
                            : null!
                    }.Where(v => v is not null)
                )
            );
    }

    public static MethodDeclarationSyntax CreateRemoveMethod(string fieldName, string typeName, string parentName, ListMetadataAttribute listMetadataAttribute) {
        return MethodDeclaration(
                listMetadataAttribute.link
                    ? IdentifierName(parentName)
                    : IdentifierName("void"),
                $"removeIn{fieldName.ToPascalCaseIdentifier()}"
            )
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddParameterListParameters(Parameter(Identifier("a" + listMetadataAttribute.type.ToPascalCaseIdentifier().genericEliminate())).WithType(ParseTypeName(listMetadataAttribute.type)))
            .WithBody(
                Block(
                    new StatementSyntax[] {
                        validateNonFrozen(listMetadataAttribute.freezeTag)!,
                        ExpressionStatement(
                            InvocationExpression( // 创建一个方法调用表达式  
                                MemberAccessExpression( // 创建一个成员访问表达式（this.list.Add）  
                                    SyntaxKind.SimpleMemberAccessExpression, // 使用点号访问  
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression, // 使用点号访问  ,
                                        ThisExpression(),
                                        IdentifierName(fieldName)
                                    ),
                                    IdentifierName("Remove") // 访问名为list的成员  
                                ),
                                ArgumentList( // 创建参数列表  
                                    SingletonSeparatedList( // 单个参数列表  
                                        Argument( // 创建一个参数表达式  
                                            IdentifierName("a" + listMetadataAttribute.type.ToPascalCaseIdentifier().genericEliminate()) // 引用参数i  
                                        )
                                    )
                                )
                            )
                        ),
                        listMetadataAttribute.link
                            ? ReturnStatement(
                                ThisExpression()
                            )
                            : null!
                    }.Where(v => v is not null).ToList()
                )
            );
    }

    public static MethodDeclarationSyntax CreateContainMethod(string fieldName, string typeName, string parentName, ListMetadataAttribute listMetadataAttribute) {
        return MethodDeclaration(
                IdentifierName("bool"),
                $"contaIn{fieldName.ToPascalCaseIdentifier()}"
            )
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddParameterListParameters(Parameter(Identifier("a" + listMetadataAttribute.type.ToPascalCaseIdentifier().genericEliminate())).WithType(ParseTypeName(listMetadataAttribute.type)))
            .WithBody(
                Block(
                    new StatementSyntax[] {
                        validateNonFrozen(listMetadataAttribute.freezeTag)!,
                        ReturnStatement(
                            InvocationExpression(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        ThisExpression(),
                                        IdentifierName(fieldName)
                                    ),
                                    IdentifierName("Contains")
                                ),
                                ArgumentList(
                                    SingletonSeparatedList(
                                        Argument(IdentifierName("a" + listMetadataAttribute.type.ToPascalCaseIdentifier().genericEliminate()))
                                    )
                                )
                            )
                        )
                    }.Where(v => v is not null)
                )
            );
    }

    public static MethodDeclarationSyntax CreateForMethod(string fieldName, string typeName, string parentName, ListMetadataAttribute listMetadataAttribute) {
        return MethodDeclaration(
                IdentifierName($"IEnumerable<{listMetadataAttribute.type}>"),
                $"for{fieldName.ToPascalCaseIdentifier()}"
            )
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .WithBody(
                Block(
                    new StatementSyntax[] {
                        validateNonFrozen(listMetadataAttribute.freezeTag),
                        listMetadataAttribute.useYield
                            ? ForEachStatement(
                                ParseTypeName("var"), // 声明变量类型 var  
                                Identifier("i"), // 变量名 i  
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    ThisExpression(),
                                    IdentifierName(fieldName)
                                ), // 假设 list 是一个字段或属性  
                                Block( // 循环体  
                                    SingletonList<StatementSyntax>(
                                        YieldStatement(
                                            SyntaxKind.YieldReturnStatement,
                                            IdentifierName("i")
                                        )
                                    ) // 包含 yield return 语句的列表  
                                )
                            )
                            : ReturnStatement(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    ThisExpression(),
                                    IdentifierName(fieldName)
                                )
                            )
                    }.Where(v => v is not null)
                )
            );
    }

    public static MethodDeclarationSyntax CreatePutMethod(string fieldName, string typeName, string parentName, MapMetadataAttribute mapMetadataAttribute) {
        return MethodDeclaration(
                mapMetadataAttribute.link
                    ? IdentifierName(parentName)
                    : IdentifierName("void"),
                $"putIn{fieldName.ToPascalCaseIdentifier()}"
            )
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddParameterListParameters(
                Parameter(Identifier("key")).WithType(ParseTypeName(mapMetadataAttribute.keyType)),
                Parameter(Identifier("value")).WithType(ParseTypeName(mapMetadataAttribute.valueType))
            )
            .WithBody(
                Block(
                    new StatementSyntax[] {
                        validateNonFrozen(mapMetadataAttribute.freezeTag),
                        mapMetadataAttribute.noNull
                            ? noNull("key", $"{parentName}.put{mapMetadataAttribute.keyType.ToPascalCaseIdentifier().genericEliminate()}And{mapMetadataAttribute.valueType.ToPascalCaseIdentifier().genericEliminate()}In{fieldName}中key为null")
                            : null!,
                        mapMetadataAttribute.noNull
                            ? noNull("value", $"{parentName}.put{mapMetadataAttribute.keyType.ToPascalCaseIdentifier().genericEliminate()}And{mapMetadataAttribute.valueType.ToPascalCaseIdentifier().genericEliminate()}In{fieldName}中value为null")
                            : null!,
                        IfStatement(
                            InvocationExpression(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        ThisExpression(),
                                        IdentifierName(fieldName)
                                    ),
                                    IdentifierName("ContainsKey")
                                ),
                                ArgumentList(
                                    SeparatedList(
                                        new[] {
                                            Argument(IdentifierName("key")),
                                        }
                                    )
                                )
                            ),
                            Block(
                                ExpressionStatement(
                                    AssignmentExpression(
                                        SyntaxKind.SimpleAssignmentExpression,
                                        ElementAccessExpression( // 创建一个数组或列表的索引访问表达式  
                                            IdentifierName(fieldName),
                                            BracketedArgumentList( // 索引参数列表  
                                                SingletonSeparatedList( // 单个参数列表  
                                                    Argument( // 创建一个参数表达式  
                                                        IdentifierName("key") // 引用参数 i  
                                                    )
                                                )
                                            )
                                        ),
                                        IdentifierName("value")
                                    )
                                )
                            )
                            ,
                            ElseClause(
                                Block(
                                    ExpressionStatement(
                                        InvocationExpression(
                                            MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    ThisExpression(),
                                                    IdentifierName(fieldName)
                                                ),
                                                IdentifierName("Add")
                                            ),
                                            ArgumentList(
                                                SeparatedList(
                                                    new[] {
                                                        Argument(IdentifierName("key")),
                                                        Argument(IdentifierName("value"))
                                                    }
                                                )
                                            )
                                        )
                                    )
                                )
                            )
                        ),
                        mapMetadataAttribute.link
                            ? ReturnStatement(
                                ThisExpression()
                            )
                            : null!
                    }.Where(v => v is not null)
                )
            );
    }

    public static MethodDeclarationSyntax CreateMapGetMethod(string fieldName, string typeName, string parentName, MapMetadataAttribute mapMetadataAttribute) {
        return MethodDeclaration(
                IdentifierName(mapMetadataAttribute.valueType),
                $"getIn{fieldName}"
            )
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddParameterListParameters(
                Parameter(Identifier("key")).WithType(ParseTypeName(mapMetadataAttribute.keyType))
            )
            .WithBody(Block(
                    new StatementSyntax[] {
                        validateNonFrozen(mapMetadataAttribute.freezeTag),
                        IfStatement(
                            InvocationExpression(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        ThisExpression(),
                                        IdentifierName(fieldName)
                                    ),
                                    IdentifierName("ContainsKey")
                                ),
                                ArgumentList(
                                    SeparatedList(
                                        new[] {
                                            Argument(IdentifierName("key")),
                                        }
                                    )
                                )
                            ),
                            Block(
                                ReturnStatement(
                                    ElementAccessExpression(
                                        IdentifierName(fieldName),
                                        BracketedArgumentList(
                                            SingletonSeparatedList(
                                                Argument(
                                                    IdentifierName("key")
                                                )
                                            )
                                        )
                                    )
                                )
                            )
                            ,
                            ElseClause(
                                Block(
                                    ReturnStatement(
                                        IdentifierName("default!")
                                    )
                                )
                            )
                        ),
                    }.Where(v => v is not null)
                )
            );
    }

    public static MethodDeclarationSyntax CreateRemoveKeyMethod(string fieldName, string typeName, string parentName, MapMetadataAttribute mapMetadataAttribute) {
        return MethodDeclaration(
                    mapMetadataAttribute.link
                        ? IdentifierName(parentName)
                        : IdentifierName("void"),
                    $"removeKeyIn{fieldName.ToPascalCaseIdentifier()}"
                )
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddParameterListParameters(
                    Parameter(Identifier("key")).WithType(ParseTypeName(mapMetadataAttribute.keyType))
                )
                .WithBody(
                    Block(
                        new StatementSyntax[] {
                            validateNonFrozen(mapMetadataAttribute.freezeTag),
                            ExpressionStatement(
                                InvocationExpression(
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            ThisExpression(),
                                            IdentifierName(fieldName)
                                        ),
                                        IdentifierName("Remove")
                                    ),
                                    ArgumentList(
                                        SeparatedList(
                                            new[] {
                                                Argument(IdentifierName("key")),
                                            }
                                        )
                                    )
                                )
                            ),
                            mapMetadataAttribute.link
                                ? ReturnStatement(ThisExpression())
                                : null!
                        }.Where(v => v is not null)
                    )
                )
            ;
    }

    public static MethodDeclarationSyntax CreateRemoveValueMethod(string fieldName, string typeName, string parentName, MapMetadataAttribute mapMetadataAttribute) {
        return MethodDeclaration(
                    mapMetadataAttribute.link
                        ? IdentifierName(parentName)
                        : IdentifierName("void"),
                    $"removeValueIn{fieldName.ToPascalCaseIdentifier()}"
                )
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddParameterListParameters(
                    Parameter(Identifier("value")).WithType(ParseTypeName(mapMetadataAttribute.valueType))
                )
                .WithBody(
                    Block(
                        new StatementSyntax[] {
                            validateNonFrozen(mapMetadataAttribute.freezeTag),
                            ExpressionStatement(
                                InvocationExpression(
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            ThisExpression(),
                                            IdentifierName(fieldName)
                                        ),
                                        IdentifierName("RemoveValue")
                                    ),
                                    ArgumentList(
                                        SeparatedList(
                                            new[] {
                                                Argument(IdentifierName("value")),
                                            }
                                        )
                                    )
                                )
                            ),
                            mapMetadataAttribute.link
                                ? ReturnStatement(ThisExpression())
                                : null!
                        }.Where(v => v is not null)
                    )
                )
            ;
    }

    public static MethodDeclarationSyntax CreateContainKeyMethod(string fieldName, string typeName, string parentName, MapMetadataAttribute mapMetadataAttribute) {
        return MethodDeclaration(
                IdentifierName("bool"),
                $"containKeyIn{fieldName.ToPascalCaseIdentifier()}"
            )
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddParameterListParameters(
                Parameter(Identifier("key")).WithType(ParseTypeName(mapMetadataAttribute.keyType))
            )
            .WithBody(Block(
                    new StatementSyntax[] {
                        validateNonFrozen(mapMetadataAttribute.freezeTag),
                        ReturnStatement(
                            InvocationExpression(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        ThisExpression(),
                                        IdentifierName(fieldName)
                                    ),
                                    IdentifierName("ContainsKey")
                                ),
                                ArgumentList(
                                    SeparatedList(
                                        new[] {
                                            Argument(IdentifierName("key")),
                                        }
                                    )
                                )
                            )
                        )
                    }.Where(v => v is not null)
                )
            );
    }

    public static MethodDeclarationSyntax CreateContainValueMethod(string fieldName, string typeName, string parentName, MapMetadataAttribute mapMetadataAttribute) {
        return MethodDeclaration(
                IdentifierName("bool"),
                $"containValueIn{fieldName.ToPascalCaseIdentifier()}"
            )
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddParameterListParameters(
                Parameter(Identifier("value")).WithType(ParseTypeName(mapMetadataAttribute.valueType))
            )
            .WithBody(
                Block(
                    new StatementSyntax[] {
                        validateNonFrozen(mapMetadataAttribute.freezeTag),
                        ReturnStatement(
                            InvocationExpression(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        ThisExpression(),
                                        IdentifierName(fieldName)
                                    ),
                                    IdentifierName("ContainsValue")
                                ),
                                ArgumentList(
                                    SeparatedList(
                                        new[] {
                                            Argument(IdentifierName("value")),
                                        }
                                    )
                                )
                            )
                        )
                    }.Where(v => v is not null)
                )
            );
    }

    public static MethodDeclarationSyntax CreateForKeyMethod(string fieldName, string typeName, string parentName, MapMetadataAttribute mapMetadataAttribute) {
        return MethodDeclaration(
                IdentifierName($"IEnumerable<{mapMetadataAttribute.keyType}>"),
                $"for{fieldName.ToPascalCaseIdentifier()}Key"
            )
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .WithBody(
                Block(
                    new StatementSyntax[] {
                        validateNonFrozen(mapMetadataAttribute.freezeTag),
                        mapMetadataAttribute.useYield
                            ? ForEachStatement(
                                ParseTypeName("var"), // 声明变量类型 var  
                                Identifier("i"), // 变量名 i  
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        ThisExpression(),
                                        IdentifierName(fieldName)
                                    ),
                                    IdentifierName("Keys")
                                ), // 假设 list 是一个字段或属性  
                                Block( // 循环体  
                                    SingletonList<StatementSyntax>(
                                        YieldStatement(
                                            SyntaxKind.YieldReturnStatement,
                                            IdentifierName("i")
                                        )
                                    ) // 包含 yield return 语句的列表  
                                )
                            )
                            : ReturnStatement(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        ThisExpression(),
                                        IdentifierName(fieldName)
                                    ),
                                    IdentifierName("Keys")
                                )
                            )
                    }.Where(v => v is not null)
                )
            );
    }

    public static MethodDeclarationSyntax CreateForValueMethod(string fieldName, string typeName, string parentName, MapMetadataAttribute mapMetadataAttribute) {
        return MethodDeclaration(
                IdentifierName($"IEnumerable<{mapMetadataAttribute.valueType}>"),
                $"for{fieldName.ToPascalCaseIdentifier()}Value"
            )
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .WithBody(
                Block(
                    new StatementSyntax[] {
                        validateNonFrozen(mapMetadataAttribute.freezeTag),
                        mapMetadataAttribute.useYield
                            ? ForEachStatement(
                                ParseTypeName("var"), // 声明变量类型 var  
                                Identifier("i"), // 变量名 i  
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        ThisExpression(),
                                        IdentifierName(fieldName)
                                    ),
                                    IdentifierName("Values")
                                ), // 假设 list 是一个字段或属性  
                                Block( // 循环体  
                                    SingletonList<StatementSyntax>(
                                        YieldStatement(
                                            SyntaxKind.YieldReturnStatement,
                                            IdentifierName("i")
                                        )
                                    ) // 包含 yield return 语句的列表  
                                )
                            )
                            : ReturnStatement(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        ThisExpression(),
                                        IdentifierName(fieldName)
                                    ),
                                    IdentifierName("Values")
                                )
                            )
                    }.Where(v => v is not null)
                )
            );
    }

    public static MethodDeclarationSyntax CreateForAllMethod(string fieldName, string typeName, string parentName, MapMetadataAttribute mapMetadataAttribute) {
        return MethodDeclaration(
                IdentifierName($"IEnumerable<KeyValuePair<{mapMetadataAttribute.keyType}, {mapMetadataAttribute.valueType}>>"),
                $"for{fieldName.ToPascalCaseIdentifier()}"
            )
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .WithBody(
                Block(
                    new StatementSyntax[] {
                        validateNonFrozen(mapMetadataAttribute.freezeTag),
                        mapMetadataAttribute.useYield
                            ? ForEachStatement(
                                ParseTypeName("var"), // 声明变量类型 var  
                                Identifier("i"), // 变量名 i  
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    ThisExpression(),
                                    IdentifierName(fieldName)
                                ),
                                Block( // 循环体  
                                    SingletonList<StatementSyntax>(
                                        YieldStatement(
                                            SyntaxKind.YieldReturnStatement,
                                            IdentifierName("i")
                                        )
                                    ) // 包含 yield return 语句的列表  
                                )
                            )
                            : ReturnStatement(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    ThisExpression(),
                                    IdentifierName(fieldName)
                                )
                            )
                    }.Where(v => v is not null)
                )
            );
    }
}