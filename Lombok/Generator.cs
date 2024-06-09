using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Til.Lombok;

[Generator]
public sealed class GetGenerator : IIncrementalGenerator {
    private static readonly string AttributeName = typeof(ILombokAttribute).FullName!;

    private static readonly string getName = nameof(GetAttribute);

    public void Initialize(IncrementalGeneratorInitializationContext context) {
        var sources = context.SyntaxProvider.ForAttributeWithMetadataName(AttributeName, IsCandidate, Transform);
        context.AddSources(sources);
    }

    private static bool IsCandidate(SyntaxNode node, CancellationToken cancellationToken) => node is ClassDeclarationSyntax;

    private static GeneratorResult Transform(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken) {
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
                    AttributeSyntax? tryGetSpecifiedAttribute = fieldDeclaration.AttributeLists.tryGetSpecifiedAttribute(getName);

                    if (tryGetSpecifiedAttribute is null) {
                        continue;
                    }

                    // 处理字段  
                    foreach (VariableDeclaratorSyntax variable in fieldDeclaration.Declaration.Variables) {
                        methodDeclarationSyntaxes.Add(CreateMethodFromField(variable));
                    }
                    break;
                }
                case PropertyDeclarationSyntax propertyDeclaration: {
                    AttributeSyntax? tryGetSpecifiedAttribute = propertyDeclaration.AttributeLists.tryGetSpecifiedAttribute(getName);

                    if (tryGetSpecifiedAttribute is null) {
                        continue;
                    }
                    methodDeclarationSyntaxes.Add(CreateMethodFromProperty(propertyDeclaration));

                    break;
                }
            }
        }

        return new GeneratorResult(contextTargetNode.GetHintName(@namespace), CreatePartialClass(@namespace, contextTargetNode, methodDeclarationSyntaxes));
    }

    private static MethodDeclarationSyntax CreateMethodFromProperty(PropertyDeclarationSyntax propertyDeclarationSyntax) {
        return CreateMethod(
            MethodDeclaration(IdentifierName(propertyDeclarationSyntax.Type.ToString()), "get" + propertyDeclarationSyntax.Identifier.Text.ToPascalCaseIdentifier()),
            Parameter(Identifier(propertyDeclarationSyntax.Identifier.Text.ToCamelCaseIdentifier())).WithType(propertyDeclarationSyntax.Type),
            propertyDeclarationSyntax.Identifier.Text,
            propertyDeclarationSyntax.Modifiers.Any(SyntaxKind.StaticKeyword)
        );
    }

    private static MethodDeclarationSyntax CreateMethodFromField(VariableDeclaratorSyntax variableDeclaratorSyntax) {
        VariableDeclarationSyntax variableDeclarationSyntax = (VariableDeclarationSyntax)variableDeclaratorSyntax.Parent!;
        FieldDeclarationSyntax fieldDeclarationSyntax = (FieldDeclarationSyntax)variableDeclarationSyntax.Parent!;
        return CreateMethod(
            MethodDeclaration(IdentifierName(variableDeclarationSyntax.Type.ToString()), "get" + variableDeclaratorSyntax.Identifier.Text.ToPascalCaseIdentifier()),
            Parameter(Identifier(variableDeclaratorSyntax.Identifier.Text.ToCamelCaseIdentifier())).WithType(variableDeclarationSyntax.Type),
            variableDeclaratorSyntax.Identifier.Text,
            fieldDeclarationSyntax.Modifiers.Any(SyntaxKind.StaticKeyword)
        );
    }

    private static IEnumerable<MethodDeclarationSyntax> CreateMethodFromField(FieldDeclarationSyntax fieldDeclarationSyntax) {
        return fieldDeclarationSyntax.Declaration.Variables.Select(CreateMethodFromField);
    }

    private static MethodDeclarationSyntax CreateMethod(MethodDeclarationSyntax method, ParameterSyntax parameter, string memberName, bool isStatic) {
        method = method.AddModifiers(Token(SyntaxKind.PublicKeyword));
        if (isStatic) {
            method = method.AddModifiers(Token(SyntaxKind.StaticKeyword));
        }
        method = method.AddParameterListParameters(
            parameter
        );
        if (isStatic) {
            method = method.WithBody(
                Block(
                    ReturnStatement(
                        IdentifierName(memberName)
                    )
                )
            );
        }
        else {
            method = method.WithBody(
                Block(
                    ReturnStatement(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            ThisExpression(),
                            IdentifierName(memberName)
                        )
                    )
                )
            );
        }

        return method;
    }

    private static SourceText CreatePartialClass(NameSyntax @namespace, ClassDeclarationSyntax classDeclaration, IEnumerable<MethodDeclarationSyntax> methods) {
        return @namespace.CreateNewNamespace(classDeclaration.GetUsings(),
                classDeclaration.CreateNewPartialClass()
                    .WithMembers(
                        List<MemberDeclarationSyntax>(methods)
                    )
            ).NormalizeWhitespace()
            .GetText(Encoding.UTF8);
    }
}

public static class CreateMethodUtil {
    public static MethodDeclarationSyntax CreateGetMethod(PropertyDeclarationSyntax propertyDeclarationSyntax) {
        return CreateGetMethod(
            MethodDeclaration(IdentifierName(propertyDeclarationSyntax.Type.ToString()), "get" + propertyDeclarationSyntax.Identifier.Text.ToPascalCaseIdentifier()),
            Parameter(Identifier(propertyDeclarationSyntax.Identifier.Text.ToCamelCaseIdentifier())).WithType(propertyDeclarationSyntax.Type),
            propertyDeclarationSyntax.Identifier.Text,
            propertyDeclarationSyntax.Modifiers.Any(SyntaxKind.StaticKeyword)
        );
    }

    public static MethodDeclarationSyntax CreateGetMethod(VariableDeclaratorSyntax variableDeclaratorSyntax) {
        VariableDeclarationSyntax variableDeclarationSyntax = (VariableDeclarationSyntax)variableDeclaratorSyntax.Parent!;
        FieldDeclarationSyntax fieldDeclarationSyntax = (FieldDeclarationSyntax)variableDeclarationSyntax.Parent!;
        return CreateGetMethod(
            MethodDeclaration(IdentifierName(variableDeclarationSyntax.Type.ToString()), "get" + variableDeclaratorSyntax.Identifier.Text.ToPascalCaseIdentifier()),
            Parameter(Identifier(variableDeclaratorSyntax.Identifier.Text.ToCamelCaseIdentifier())).WithType(variableDeclarationSyntax.Type),
            variableDeclaratorSyntax.Identifier.Text,
            fieldDeclarationSyntax.Modifiers.Any(SyntaxKind.StaticKeyword)
        );
    }

    public static MethodDeclarationSyntax CreateGetMethod(MethodDeclarationSyntax method, ParameterSyntax parameter, string memberName, bool isStatic) {
        method = method.AddModifiers(Token(SyntaxKind.PublicKeyword));
        if (isStatic) {
            method = method.AddModifiers(Token(SyntaxKind.StaticKeyword));
        }
        method = method.AddParameterListParameters(
            parameter
        );
        method = method.WithBody(
            Block(
                ReturnStatement(
                    isStatic
                        ? IdentifierName(memberName)
                        : MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            ThisExpression(),
                            IdentifierName(memberName)
                        )
                )
            )
        );

        return method;
    }

    public static MethodDeclarationSyntax CreateSetMethod(MethodDeclarationSyntax method, ParameterSyntax parameter, string memberName, bool isStatic, string freezeTag) {
        return method.AddModifiers(isStatic ? new[] { Token(SyntaxKind.PublicKeyword) } : new[] { Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword) });
    }
}