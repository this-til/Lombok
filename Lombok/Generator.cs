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
                    methodDeclarationSyntaxes.AddRange(onFieldDeclarationSyntax(fieldDeclaration));
                    break;
                }
                case PropertyDeclarationSyntax propertyDeclaration: {
                    methodDeclarationSyntaxes.AddRange(onPropertyDeclarationSyntax(propertyDeclaration));
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

public abstract class AttributeGeneratorBasics : GeneratorBasics {
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

[Generator]
public sealed class GetGenerator : AttributeGeneratorBasics {
    private static readonly string getName = nameof(GetAttribute);

    public override string getAttributeName() => getName;

    public override IEnumerable<MethodDeclarationSyntax> onFieldDeclarationSyntax(FieldDeclarationSyntax fieldDeclarationSyntax, AttributeSyntax attributeSyntax, Dictionary<string, object> data) {
        MetadataAttribute metadataAttribute = MetadataAttribute.of(data);
        foreach (VariableDeclaratorSyntax variable in fieldDeclarationSyntax.Declaration.Variables) {
            yield return CreateMethodUtil.CreateGetMethod(
                variable.Identifier.Text,
                fieldDeclarationSyntax.Declaration.Type.ToString(),
                metadataAttribute
            );
        }
    }

    public override IEnumerable<MethodDeclarationSyntax> onPropertyDeclarationSyntax(PropertyDeclarationSyntax propertyDeclarationSyntax, AttributeSyntax attributeSyntax, Dictionary<string, object> data) {
        MetadataAttribute metadataAttribute = MetadataAttribute.of(data);
        yield return CreateMethodUtil.CreateGetMethod(
            propertyDeclarationSyntax.Identifier.ToString(),
            propertyDeclarationSyntax.Type.ToString(),
            metadataAttribute
        );
    }
}

[Generator]
public sealed class SetGenerator : AttributeGeneratorBasics {
    private static readonly string setName = nameof(SetAttribute);

    public override string getAttributeName() => setName;

    public override IEnumerable<MethodDeclarationSyntax> onFieldDeclarationSyntax(FieldDeclarationSyntax fieldDeclarationSyntax, AttributeSyntax attributeSyntax, Dictionary<string, object> data) {
        MetadataAttribute metadataAttribute = MetadataAttribute.of(data);

        foreach (VariableDeclaratorSyntax variable in fieldDeclarationSyntax.Declaration.Variables) {
            //yield return CreateMethodUtil.CreateSetMethod(variable, _freezeTag as string ?? string.Empty, _link is not null && (bool)_link);
            yield return CreateMethodUtil.CreateSetMethod(
                variable.ToString(),
                fieldDeclarationSyntax.Declaration.Type.ToString(),
                ((ClassDeclarationSyntax)fieldDeclarationSyntax.Parent).getHasGenericName(),
                metadataAttribute
            );
        }
    }

    public override IEnumerable<MethodDeclarationSyntax> onPropertyDeclarationSyntax(PropertyDeclarationSyntax propertyDeclarationSyntax, AttributeSyntax attributeSyntax, Dictionary<string, object> data) {
        MetadataAttribute metadataAttribute = MetadataAttribute.of(data);
        yield return CreateMethodUtil.CreateSetMethod(
            propertyDeclarationSyntax.Identifier.ToString(),
            propertyDeclarationSyntax.Type.ToString(),
            ((ClassDeclarationSyntax)propertyDeclarationSyntax.Parent).getHasGenericName(),
            metadataAttribute
        );
    }
}

[Generator]
public sealed class IndexGenerator : AttributeGeneratorBasics {
    private static readonly string getAtName = nameof(IndexAttribute);

    public override string getAttributeName() => getAtName;

    public override IEnumerable<MethodDeclarationSyntax> onFieldDeclarationSyntax(FieldDeclarationSyntax fieldDeclarationSyntax, AttributeSyntax attributeSyntax, Dictionary<string, object> data) {
        VariableDeclarationSyntax variableDeclarationSyntax = fieldDeclarationSyntax.Declaration;
        TypeSyntax typeSyntax = variableDeclarationSyntax.Type;
        ListMetadataAttribute listMetadataAttribute = ListMetadataAttribute.of(data);
        if (listMetadataAttribute.type is null && typeSyntax is GenericNameSyntax genericNameSyntax) {
            TypeSyntax? firstOrDefault = genericNameSyntax.TypeArgumentList.Arguments.FirstOrDefault();
            listMetadataAttribute.type = firstOrDefault?.ToFullString();
        }
        if (listMetadataAttribute.type == null) {
            yield break;
        }
        foreach (VariableDeclaratorSyntax variable in fieldDeclarationSyntax.Declaration.Variables) {
            yield return CreateMethodUtil.CreateGetAtMethod(variable.ToString(), listMetadataAttribute);
        }
    }

    public override IEnumerable<MethodDeclarationSyntax> onPropertyDeclarationSyntax(PropertyDeclarationSyntax propertyDeclarationSyntax, AttributeSyntax attributeSyntax, Dictionary<string, object> data) {
        TypeSyntax typeSyntax = propertyDeclarationSyntax.Type;
        ListMetadataAttribute listMetadataAttribute = ListMetadataAttribute.of(data);
        if (listMetadataAttribute.type is null && typeSyntax is GenericNameSyntax genericNameSyntax) {
            TypeSyntax? firstOrDefault = genericNameSyntax.TypeArgumentList.Arguments.FirstOrDefault();
            listMetadataAttribute.type = firstOrDefault?.ToFullString();
        }
        if (listMetadataAttribute.type == null) {
            yield break;
        }
        yield return CreateMethodUtil.CreateGetAtMethod(propertyDeclarationSyntax.Initializer.ToString(), listMetadataAttribute);
    }
}

[Generator]
public sealed class AddGenerator : AttributeGeneratorBasics {
    public override string getAttributeName() => nameof(AddAttribute);

    public override IEnumerable<MethodDeclarationSyntax> onFieldDeclarationSyntax(FieldDeclarationSyntax fieldDeclarationSyntax, AttributeSyntax attributeSyntax, Dictionary<string, object> data) {
        TypeSyntax typeSyntax = fieldDeclarationSyntax.Declaration.Type;
        ListMetadataAttribute listMetadataAttribute = ListMetadataAttribute.of(data);
        if (listMetadataAttribute.type is null && typeSyntax is GenericNameSyntax genericNameSyntax) {
            TypeSyntax? firstOrDefault = genericNameSyntax.TypeArgumentList.Arguments.FirstOrDefault();
            listMetadataAttribute.type = firstOrDefault?.ToFullString();
        }
        if (listMetadataAttribute.type == null) {
            yield break;
        }
        foreach (VariableDeclaratorSyntax variable in fieldDeclarationSyntax.Declaration.Variables) {
            yield return CreateMethodUtil.CreateAddMethod(
                variable.ToString(),
                ((ClassDeclarationSyntax)fieldDeclarationSyntax.Parent).getHasGenericName(),
                listMetadataAttribute
            );
        }
    }

    public override IEnumerable<MethodDeclarationSyntax> onPropertyDeclarationSyntax(PropertyDeclarationSyntax propertyDeclarationSyntax, AttributeSyntax attributeSyntax, Dictionary<string, object> data) {
        TypeSyntax typeSyntax = propertyDeclarationSyntax.Type;
        ListMetadataAttribute listMetadataAttribute = ListMetadataAttribute.of(data);
        if (listMetadataAttribute.type is null && typeSyntax is GenericNameSyntax genericNameSyntax) {
            TypeSyntax? firstOrDefault = genericNameSyntax.TypeArgumentList.Arguments.FirstOrDefault();
            listMetadataAttribute.type = firstOrDefault?.ToFullString();
        }
        if (listMetadataAttribute.type == null) {
            yield break;
        }
        yield return CreateMethodUtil.CreateAddMethod(
            propertyDeclarationSyntax.Initializer.ToString(),
            ((ClassDeclarationSyntax)propertyDeclarationSyntax.Parent).getHasGenericName(),
            listMetadataAttribute
        );
    }
}

[Generator]
public sealed class RemoveGenerator : AttributeGeneratorBasics {
    public override string getAttributeName() => nameof(AddAttribute);

    public override IEnumerable<MethodDeclarationSyntax> onFieldDeclarationSyntax(FieldDeclarationSyntax fieldDeclarationSyntax, AttributeSyntax attributeSyntax, Dictionary<string, object> data) {
        TypeSyntax typeSyntax = fieldDeclarationSyntax.Declaration.Type;
        ListMetadataAttribute listMetadataAttribute = ListMetadataAttribute.of(data);
        if (listMetadataAttribute.type is null && typeSyntax is GenericNameSyntax genericNameSyntax) {
            TypeSyntax? firstOrDefault = genericNameSyntax.TypeArgumentList.Arguments.FirstOrDefault();
            listMetadataAttribute.type = firstOrDefault?.ToFullString();
        }
        if (listMetadataAttribute.type == null) {
            yield break;
        }
        foreach (VariableDeclaratorSyntax variable in fieldDeclarationSyntax.Declaration.Variables) {
            yield return CreateMethodUtil.CreateRemoveMethod(
                variable.ToString(),
                ((ClassDeclarationSyntax)fieldDeclarationSyntax.Parent).getHasGenericName(),
                listMetadataAttribute
            );
        }
    }

    public override IEnumerable<MethodDeclarationSyntax> onPropertyDeclarationSyntax(PropertyDeclarationSyntax propertyDeclarationSyntax, AttributeSyntax attributeSyntax, Dictionary<string, object> data) {
        TypeSyntax typeSyntax = propertyDeclarationSyntax.Type;
        ListMetadataAttribute listMetadataAttribute = ListMetadataAttribute.of(data);
        if (listMetadataAttribute.type is null && typeSyntax is GenericNameSyntax genericNameSyntax) {
            TypeSyntax? firstOrDefault = genericNameSyntax.TypeArgumentList.Arguments.FirstOrDefault();
            listMetadataAttribute.type = firstOrDefault?.ToFullString();
        }
        if (listMetadataAttribute.type == null) {
            yield break;
        }
        yield return CreateMethodUtil.CreateRemoveMethod(
            propertyDeclarationSyntax.Initializer.ToString(),
            ((ClassDeclarationSyntax)propertyDeclarationSyntax.Parent).getHasGenericName(),
            listMetadataAttribute
        );
    }
}

[Generator]
public sealed class ContainGenerator : AttributeGeneratorBasics {
    public override string getAttributeName() => nameof(ContainAttribute);

    public override IEnumerable<MethodDeclarationSyntax> onFieldDeclarationSyntax(FieldDeclarationSyntax fieldDeclarationSyntax, AttributeSyntax attributeSyntax, Dictionary<string, object> data) {
        TypeSyntax typeSyntax = fieldDeclarationSyntax.Declaration.Type;
        ListMetadataAttribute listMetadataAttribute = ListMetadataAttribute.of(data);
        if (listMetadataAttribute.type is null && typeSyntax is GenericNameSyntax genericNameSyntax) {
            TypeSyntax? firstOrDefault = genericNameSyntax.TypeArgumentList.Arguments.FirstOrDefault();
            listMetadataAttribute.type = firstOrDefault?.ToFullString();
        }
        if (listMetadataAttribute.type == null) {
            yield break;
        }
        foreach (VariableDeclaratorSyntax variable in fieldDeclarationSyntax.Declaration.Variables) {
            yield return CreateMethodUtil.CreateContainRemoveMethod(
                variable.ToString(),
                ((ClassDeclarationSyntax)fieldDeclarationSyntax.Parent).getHasGenericName(),
                listMetadataAttribute
            );
        }
    }

    public override IEnumerable<MethodDeclarationSyntax> onPropertyDeclarationSyntax(PropertyDeclarationSyntax propertyDeclarationSyntax, AttributeSyntax attributeSyntax, Dictionary<string, object> data) {
        TypeSyntax typeSyntax = propertyDeclarationSyntax.Type;
        ListMetadataAttribute listMetadataAttribute = ListMetadataAttribute.of(data);
        if (listMetadataAttribute.type is null && typeSyntax is GenericNameSyntax genericNameSyntax) {
            TypeSyntax? firstOrDefault = genericNameSyntax.TypeArgumentList.Arguments.FirstOrDefault();
            listMetadataAttribute.type = firstOrDefault?.ToFullString();
        }
        if (listMetadataAttribute.type == null) {
            yield break;
        }
        yield return CreateMethodUtil.CreateContainRemoveMethod(
            propertyDeclarationSyntax.Initializer.ToString(),
            ((ClassDeclarationSyntax)propertyDeclarationSyntax.Parent).getHasGenericName(),
            listMetadataAttribute
        );
    }
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
                        IdentifierName("this.validateNonFrozen"),
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
                                    IdentifierName("\"{message}\"") // 引用参数i  
                                )
                                : null!
                        }.Where(v => v is not null)
                    )
                )
            )
        );
    }

    public static MethodDeclarationSyntax CreateGetMethod(string fieldName, string typeName, MetadataAttribute metadataAttribute) {
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
            .AddParameterListParameters(Parameter(Identifier(fieldName.ToCamelCaseIdentifier())).WithType(ParseTypeName(typeName)))
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

    public static MethodDeclarationSyntax CreateGetAtMethod(string fieldName, ListMetadataAttribute listMetadataAttribute) {
        return MethodDeclaration(
                IdentifierName(listMetadataAttribute.type),
                $"get{listMetadataAttribute.type.ToPascalCaseIdentifier().genericEliminate()}In{fieldName.ToPascalCaseIdentifier().genericEliminate()}ByIndex"
            )
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddParameterListParameters(
                Parameter(Identifier("i")).WithType(ParseTypeName("int"))
            ).WithBody(Block(
                    new StatementSyntax[] {
                        validateNonFrozen(listMetadataAttribute.freezeTag)!,
                        ReturnStatement( // 创建一个 return 语句  
                            ElementAccessExpression( // 创建一个数组或列表的索引访问表达式  
                                IdentifierName(fieldName), // 假设 list 是当前类或结构体的一个字段  
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

    public static MethodDeclarationSyntax CreateAddMethod(string fieldName, string parentName, ListMetadataAttribute listMetadataAttribute) {
        return MethodDeclaration(
                listMetadataAttribute.link
                    ? IdentifierName(parentName)
                    : IdentifierName("void"),
                $"add{listMetadataAttribute.type.ToPascalCaseIdentifier().genericEliminate()}In{fieldName.ToPascalCaseIdentifier().genericEliminate()}"
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
                        listMetadataAttribute.noNull
                            ? noNull(fieldName, $"{parentName}.{"set" + fieldName.ToPascalCaseIdentifier()}方法中传入参数为null")
                            : null!,
                        ExpressionStatement(
                            InvocationExpression( // 创建一个方法调用表达式  
                                MemberAccessExpression( // 创建一个成员访问表达式（this.list.Add）  
                                    SyntaxKind.SimpleMemberAccessExpression, // 使用点号访问  
                                    ThisExpression(), // 访问当前实例的this  
                                    IdentifierName($"{fieldName}.Add") // 访问名为list的成员  
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

    public static MethodDeclarationSyntax CreateRemoveMethod(string fieldName, string parentName, ListMetadataAttribute listMetadataAttribute) {
        return MethodDeclaration(
                listMetadataAttribute.link
                    ? IdentifierName(parentName)
                    : IdentifierName("void"),
                $"remove{listMetadataAttribute.type.ToPascalCaseIdentifier().genericEliminate()}In{fieldName.ToPascalCaseIdentifier().genericEliminate()}"
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
                                    ThisExpression(), // 访问当前实例的this  
                                    IdentifierName($"{fieldName}.Remove") // 访问名为list的成员  
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

    public static MethodDeclarationSyntax CreateContainRemoveMethod(string fieldName, string parentName, ListMetadataAttribute listMetadataAttribute) {
        return MethodDeclaration(
                IdentifierName("bool"),
                $"contain{listMetadataAttribute.type.ToPascalCaseIdentifier().genericEliminate()}In{fieldName.ToPascalCaseIdentifier().genericEliminate()}"
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
                                    IdentifierName(fieldName),
                                    IdentifierName("Contains")
                                ),
                                ArgumentList(
                                    SingletonSeparatedList(
                                        Argument(IdentifierName("a" + listMetadataAttribute.type.ToPascalCaseIdentifier().genericEliminate()))
                                    )
                                )
                            )
                        )
                    }.Where(v => v is not null).ToList()
                )
            );
    }
}