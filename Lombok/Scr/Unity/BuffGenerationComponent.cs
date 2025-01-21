using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Til.Lombok.Generator;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Til.Lombok.Unity.Generator {

    [IncrementComponent]
    public class BuffGenerationComponent : TranslationClassFieldAttributeIncrementComponent<NetworkSerializationFieldAttribute, NetworkSerializationClassAttribute> {

        public override bool onlyOne() => true;

        public override void fill(TranslationClassFieldAttributeIncrementContext<NetworkSerializationFieldAttribute, NetworkSerializationClassAttribute> context) {

            string? baseTypeName = null;

            if (context.classFieldAttributeIncrementContext.caAttributeContext?.attribute.hasBase ?? false) {
                baseTypeName = context.basicsContext.semanticModel.GetDeclaredSymbol(context.basicsContext.contextTargetNode)?.BaseType?.Name;
            }

            StringBuilder stringBuilder = new StringBuilder();
            CodeBuilder codeBuilder = new CodeBuilder(stringBuilder);

            using (codeBuilder.appendBlock($"public new static void read(Unity.Netcode.FastBufferReader reader, out {context.basicsContext.className} value)")) {
                codeBuilder.appendLine("Unity.Netcode.ByteUnpacker.ReadValuePacked(reader, out bool isNull);");
                using (codeBuilder.appendBlock("if(isNull)")) {
                    codeBuilder.appendLine("value = null!;");
                    codeBuilder.appendLine("return;");
                }
                codeBuilder.appendLine($"value = new {context.basicsContext.className}();");

                codeBuilder.appendLine($"readField(reader, value);");
                if (baseTypeName is not null) {
                    codeBuilder.appendLine($"{baseTypeName}.readField(reader, value);");
                }
            }

            using (codeBuilder.appendBlock($"public new static void readField(Unity.Netcode.FastBufferReader reader, {context.basicsContext.className} value)")) {
                foreach (FieldAttributeIncrementContext<NetworkSerializationFieldAttribute> fieldAttributeIncrementContext in context.fieldsAttributeContextList) {
                    using (codeBuilder.appendBlock()) {
                        codeBuilder.appendLine($"{fieldAttributeIncrementContext.typeContext.typeName} a = default;");
                        codeBuilder.appendLine($"Unity.Netcode.NetworkVariableSerialization<{fieldAttributeIncrementContext.typeContext.typeName}>.Read(reader, ref a);");
                        codeBuilder.appendLine($"value.{fieldAttributeIncrementContext.setInvoke("a")};");
                    }
                }
            }

            using (codeBuilder.appendBlock($"public new static void readDelta(Unity.Netcode.FastBufferReader reader, ref {context.basicsContext.className} value)")) {
                codeBuilder.appendLine("Unity.Netcode.ByteUnpacker.ReadValuePacked(reader, out bool useRead);");
                using (codeBuilder.appendBlock("if(useRead)")) {
                    codeBuilder.appendLine("read(reader, out value);");
                    codeBuilder.appendLine("return;");
                }
                codeBuilder.appendLine("Unity.Netcode.ByteUnpacker.ReadValuePacked(reader, out bool isNull);");
                using (codeBuilder.appendBlock("if(isNull)")) {
                    codeBuilder.appendLine("value = null!;");
                    codeBuilder.appendLine("return;");
                }
                codeBuilder.appendLine($"value ??= new {context.basicsContext.className}();");

                codeBuilder.appendLine($"readDeltaField(reader, ref value);");
                if (baseTypeName is not null) {
                    codeBuilder.appendLine($"{baseTypeName} _value = value;");
                    codeBuilder.appendLine($"{baseTypeName}.readDeltaField(reader, ref _value);");
                }
            }

            using (codeBuilder.appendBlock($"public new static void readDeltaField(Unity.Netcode.FastBufferReader reader, ref {context.basicsContext.className} value)")) {
                codeBuilder.appendLine("Unity.Netcode.ByteUnpacker.ReadValuePacked(reader, out int tag);");
                for (int index = 0; index < context.fieldsAttributeContextList.Count; index++) {
                    FieldAttributeIncrementContext<NetworkSerializationFieldAttribute> fieldAttributeIncrementContext = context.fieldsAttributeContextList[index];
                    using (codeBuilder.appendBlock($"if ((tag & (1 << {index})) != 0)")) {
                        codeBuilder.appendLine($"{fieldAttributeIncrementContext.typeContext.typeName} a = value.{fieldAttributeIncrementContext.getInvoke};");
                        codeBuilder.appendLine($"Unity.Netcode.NetworkVariableSerialization<{fieldAttributeIncrementContext.typeContext.typeName}>.ReadDelta(reader, ref a);");
                        codeBuilder.appendLine($"value.{fieldAttributeIncrementContext.setInvoke("a")};");
                    }
                }
            }

            using (codeBuilder.appendBlock($"public new static void write(Unity.Netcode.FastBufferWriter writer, in {context.basicsContext.className} value)")) {
                using (codeBuilder.appendBlock("if(value == null)")) {
                    codeBuilder.appendLine("Unity.Netcode.BytePacker.WriteValuePacked(writer, true);");
                    codeBuilder.appendLine("return;");
                }
                codeBuilder.appendLine($"writeField(writer, value);");
                if (baseTypeName is not null) {
                    codeBuilder.appendLine($"{baseTypeName}.writeField(writer, value);");
                }
            }

            using (codeBuilder.appendBlock($"public new static void writeField(Unity.Netcode.FastBufferWriter writer, {context.basicsContext.className} value)")) {
                foreach (FieldAttributeIncrementContext<NetworkSerializationFieldAttribute> fieldAttributeIncrementContext in context.fieldsAttributeContextList) {
                    using (codeBuilder.appendBlock()) {
                        codeBuilder.appendLine($"{fieldAttributeIncrementContext.typeContext.typeName} a = value.{fieldAttributeIncrementContext.getInvoke};");
                        codeBuilder.appendLine($"Unity.Netcode.NetworkVariableSerialization<{fieldAttributeIncrementContext.typeContext.typeName}>.Write(writer, ref a);");
                    }
                }
            }

            using (codeBuilder.appendBlock($"public new static void writeDelta(Unity.Netcode.FastBufferWriter writer, in {context.basicsContext.className} value, in {context.basicsContext.className} previousValue)")) {
                using (codeBuilder.appendBlock("if(previousValue == null)")) {
                    codeBuilder.appendLine("Unity.Netcode.BytePacker.WriteValuePacked(writer, true);");
                    codeBuilder.appendLine("write(writer, in value);");
                    codeBuilder.appendLine("return;");
                }
                codeBuilder.appendLine("Unity.Netcode.BytePacker.WriteValuePacked(writer, false);");

                using (codeBuilder.appendBlock("if (value == null)")) {
                    codeBuilder.appendLine("Unity.Netcode.BytePacker.WriteValuePacked(writer, true);");
                    codeBuilder.appendLine("return;");
                }
                codeBuilder.appendLine("Unity.Netcode.BytePacker.WriteValuePacked(writer, false);");

                codeBuilder.appendLine($"writeDeltaField(writer, value, previousValue);");
                if (baseTypeName is not null) {
                    codeBuilder.appendLine($"{baseTypeName}.writeDeltaField(writer, value, previousValue);");
                }
            }

            using (codeBuilder.appendBlock($"public new static void writeDeltaField(Unity.Netcode.FastBufferWriter writer, {context.basicsContext.className} value, {context.basicsContext.className} previousValue)")) {
                codeBuilder.appendLine("int tag = 0;");

                for (int index = 0; index < context.fieldsAttributeContextList.Count; index++) {
                    FieldAttributeIncrementContext<NetworkSerializationFieldAttribute> fieldAttributeIncrementContext = context.fieldsAttributeContextList[index];
                    using (codeBuilder.appendBlock()) {
                        codeBuilder.appendLine($"{fieldAttributeIncrementContext.typeContext.typeName} a = previousValue.{fieldAttributeIncrementContext.getInvoke};");
                        codeBuilder.appendLine($"{fieldAttributeIncrementContext.typeContext.typeName} b = value.{fieldAttributeIncrementContext.getInvoke};");
                        using (codeBuilder.appendBlock($"if(!Unity.Netcode.NetworkVariableSerialization<{fieldAttributeIncrementContext.typeContext.typeName}>.AreEqual(ref a, ref b))")) {
                            codeBuilder.appendLine($"tag |= (1 << {index});");
                        }
                    }
                }

                codeBuilder.appendLine("Unity.Netcode.BytePacker.WriteValuePacked(writer, tag);");

                for (int index = 0; index < context.fieldsAttributeContextList.Count; index++) {
                    FieldAttributeIncrementContext<NetworkSerializationFieldAttribute> fieldAttributeIncrementContext = context.fieldsAttributeContextList[index];
                    using (codeBuilder.appendBlock("if ((tag & (1 << 0)) != 0)")) {
                        codeBuilder.appendLine($"{fieldAttributeIncrementContext.typeContext.typeName} a = previousValue.{fieldAttributeIncrementContext.getInvoke};");
                        codeBuilder.appendLine($"{fieldAttributeIncrementContext.typeContext.typeName} b = value.{fieldAttributeIncrementContext.getInvoke};");
                        codeBuilder.appendLine($"Unity.Netcode.NetworkVariableSerialization<{fieldAttributeIncrementContext.typeContext.typeName}>.WriteDelta(writer, ref b, ref a);");
                        codeBuilder.appendLine($"previousValue.{fieldAttributeIncrementContext.setInvoke("a")};");
                    }
                }
            }

            using (codeBuilder.appendBlock($"public new static void duplicateValue(in {context.basicsContext.className} value, ref {context.basicsContext.className} duplicatedValue)")) {
                using (codeBuilder.appendBlock("if (value == null)")) {
                    codeBuilder.appendLine("duplicatedValue = null!;");
                    codeBuilder.appendLine("return;");
                }
                using (codeBuilder.appendBlock("if (duplicatedValue == null)")) {
                    codeBuilder.appendLine($"duplicatedValue = new {context.basicsContext.className}();");
                }

                foreach (FieldAttributeIncrementContext<NetworkSerializationFieldAttribute> fieldAttributeIncrementContext in context.fieldsAttributeContextList) {
                    using (codeBuilder.appendBlock()) {
                        codeBuilder.appendLine($"{fieldAttributeIncrementContext.typeContext.typeName} a = duplicatedValue.{fieldAttributeIncrementContext.getInvoke};");
                        codeBuilder.appendLine($"{fieldAttributeIncrementContext.typeContext.typeName} b = value.{fieldAttributeIncrementContext.getInvoke};");
                        using (codeBuilder.appendBlock($"if(!Unity.Netcode.NetworkVariableSerialization<{fieldAttributeIncrementContext.typeContext.typeName}>.AreEqual(ref a, ref b))")) {
                            codeBuilder.appendLine($"Unity.Netcode.NetworkVariableSerialization<{fieldAttributeIncrementContext.typeContext.typeName}>.Duplicate(b, ref a);");
                            codeBuilder.appendLine($"duplicatedValue.{fieldAttributeIncrementContext.setInvoke("a")};");
                        }
                    }
                }

                if (baseTypeName is not null) {
                    codeBuilder.appendLine($"{baseTypeName} _duplicatedValue = duplicatedValue;");
                    codeBuilder.appendLine($"{baseTypeName}.duplicateValue(value, ref _duplicatedValue);");
                }
            }

            using (codeBuilder.appendBlock($"public new static bool equals(ref {context.basicsContext.className} a, ref {context.basicsContext.className} b)")) {
                using (codeBuilder.appendBlock("if(a == b)")) {
                    codeBuilder.appendLine("return true;");
                }
                using (codeBuilder.appendBlock("if(a == null)")) {
                    codeBuilder.appendLine("return false;");
                }
                using (codeBuilder.appendBlock("if(b == null)")) {
                    codeBuilder.appendLine("return false;");
                }

                foreach (FieldAttributeIncrementContext<NetworkSerializationFieldAttribute> fieldAttributeIncrementContext in context.fieldsAttributeContextList) {
                    using (codeBuilder.appendBlock()) {
                        codeBuilder.appendLine($"{fieldAttributeIncrementContext.typeContext.typeName} _a = a.{fieldAttributeIncrementContext.getInvoke};");
                        codeBuilder.appendLine($"{fieldAttributeIncrementContext.typeContext.typeName} _b = b.{fieldAttributeIncrementContext.getInvoke};");
                        using (codeBuilder.appendBlock($"if(!Unity.Netcode.NetworkVariableSerialization<{fieldAttributeIncrementContext.typeContext.typeName}>.AreEqual(ref _a, ref _b))")) {
                            codeBuilder.appendLine("return false;");
                        }
                    }
                }

                if (baseTypeName is not null) {
                    codeBuilder.appendLine($"{baseTypeName} __a = a;");
                    codeBuilder.appendLine($"{baseTypeName} __b = b;");
                    using (codeBuilder.appendBlock($"if(!{baseTypeName}.equals(ref __a, ref __b))")) {
                        codeBuilder.appendLine("return false;");
                    }
                }

                codeBuilder.appendLine("return true;");
            }

            codeBuilder.appendLine("[UnityEngine.RuntimeInitializeOnLoadMethodAttribute(UnityEngine.RuntimeInitializeLoadType.AfterAssembliesLoaded)]");
            if (context.basicsContext.context.SemanticModel.Compilation.Options.SpecificDiagnosticOptions.ContainsKey("UNITY_EDITOR")) {
                codeBuilder.appendLine("[UnityEditor.InitializeOnLoadMethodAttribute]");
            }
            using (codeBuilder.appendBlock("protected new static void InitializeOnLoad()")) {
                codeBuilder.appendLine($"Unity.Netcode.UserNetworkVariableSerialization<{context.basicsContext.className}>.ReadValue = read;");
                codeBuilder.appendLine($"Unity.Netcode.UserNetworkVariableSerialization<{context.basicsContext.className}>.WriteValue = write;");
                codeBuilder.appendLine($"Unity.Netcode.UserNetworkVariableSerialization<{context.basicsContext.className}>.WriteDelta = writeDelta;");
                codeBuilder.appendLine($"Unity.Netcode.UserNetworkVariableSerialization<{context.basicsContext.className}>.ReadDelta = readDelta;");
                codeBuilder.appendLine($"Unity.Netcode.UserNetworkVariableSerialization<{context.basicsContext.className}>.DuplicateValue = duplicateValue;");
                codeBuilder.appendLine
                (
                    $"typeof(Unity.Netcode.NetworkVariableSerialization<{context.basicsContext.className}>).GetProperty(\"AreEqual\", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)?.SetValue(null, new Unity.Netcode.NetworkVariableSerialization<{context.basicsContext.className}>.EqualsDelegate(equals));"
                );
            }

            MemberDeclarationSyntax[] memberDeclarationSyntaxes = CSharpSyntaxTree.ParseText(stringBuilder.ToString())
                .GetRoot()
                .ChildNodes()
                .OfType<MemberDeclarationSyntax>()
                .ToArray();

            context.incrementContext.partialClassMemberDeclarationSyntaxList.AddRange(memberDeclarationSyntaxes);
        }

    }

}

/*
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Til.Lombok.Generator;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Til.Lombok.Unity.Generator {

    [GeneratorComponent]
    public class BuffGenerationComponent : ClassFieldAttributeGeneratorComponent<NetworkSerializationFieldAttribute, NetworkSerializationClassAttribute> {

        public override void fill(ClassFieldAttributeContext<NetworkSerializationFieldAttribute, NetworkSerializationClassAttribute> context) {

            bool isNotValueType = !((context.basicsContext.semanticModel.GetSymbolInfo(context.basicsContext.contextTargetNode).Symbol as ITypeSymbol)?.IsValueType ?? false);

            #region readField

            MethodDeclarationSyntax readField = MethodDeclaration
                (
                    IdentifierName
                    (
                        "void"
                    ),
                    "read"
                )
                .AddModifiers
                (
                    Token
                    (
                        SyntaxKind.PublicKeyword
                    ),
                    Token
                    (
                        SyntaxKind.StaticKeyword
                    )
                )
                .AddParameterListParameters
                (
                    Parameter
                        (
                            Identifier
                            (
                                "reader"
                            )
                        )
                        .WithType
                        (
                            IdentifierName
                            (
                                "Unity.Netcode.FastBufferReader"
                            )
                        ),
                    Parameter
                        (
                            Identifier
                            (
                                "value"
                            )
                        )
                        .WithType
                        (
                            IdentifierName
                            (
                                context.basicsContext.contextTargetNode.Identifier.Text
                            )
                        )
                        .WithModifiers
                        (
                            TokenList
                            (
                                Token
                                (
                                    SyntaxKind.OutKeyword
                                )
                            )
                        )
                )
                .AddBodyStatements
                (
                    isNotValueType
                        ? new StatementSyntax[] {
                            LocalDeclarationStatement
                            (
                                VariableDeclaration
                                (
                                    ParseTypeName
                                    (
                                        "bool"
                                    ),
                                    SeparatedList
                                    (
                                        new[] {
                                            VariableDeclarator
                                            (
                                                "isNull"
                                            )
                                        }
                                    )
                                )
                            ),
                            ExpressionStatement
                            (
                                InvocationExpression
                                (
                                    IdentifierName
                                    (
                                        "Unity.Netcode.ByteUnpacker.ReadValuePacked"
                                    ),
                                    ArgumentList
                                    (
                                        SeparatedList
                                        (
                                            new[] {
                                                Argument
                                                (
                                                    IdentifierName
                                                    (
                                                        "reader"
                                                    )
                                                ),
                                                Argument
                                                    (
                                                        IdentifierName
                                                        (
                                                            "isNull"
                                                        )
                                                    )
                                                    .WithRefKindKeyword
                                                    (
                                                        Token
                                                        (
                                                            SyntaxKind.OutKeyword
                                                        )
                                                    )
                                            }
                                        )
                                    )
                                )
                            ),
                            IfStatement
                            (
                                IdentifierName
                                (
                                    "isNull"
                                ),
                                Block
                                (
                                    ExpressionStatement
                                    (
                                        AssignmentExpression
                                        (
                                            SyntaxKind.SimpleAssignmentExpression,
                                            IdentifierName
                                            (
                                                "value"
                                            ),
                                            IdentifierName
                                            (
                                                "null!"
                                            )
                                        )
                                    ),
                                    ReturnStatement(
                                    )
                                )
                            ),
                            ExpressionStatement
                            (
                                AssignmentExpression
                                (
                                    SyntaxKind.SimpleAssignmentExpression,
                                    IdentifierName
                                    (
                                        "value"
                                    ),
                                    ObjectCreationExpression
                                        (
                                            ParseTypeName
                                            (
                                                context.basicsContext.contextTargetNode.Identifier.Text
                                            )
                                        )
                                        .WithArgumentList
                                        (
                                            ArgumentList()
                                        )
                                )
                            )
                        }
                        : Array.Empty<StatementSyntax>()
                )
                .AddBodyStatements
                (
                    context.fieldsAttributeContextList.Select
                        (
                            field => {
                                string type = field.fieldsContext.typeContext.typeName;
                                string fieldName = field.fieldsContext.fieldName;

                                return (StatementSyntax)ExpressionStatement
                                (
                                    InvocationExpression
                                    (
                                        MemberAccessExpression
                                        (
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            IdentifierName
                                            (
                                                $"Unity.Netcode.NetworkVariableSerialization<{type}>"
                                            ),
                                            IdentifierName
                                            (
                                                "Read"
                                            )
                                        ),
                                        ArgumentList
                                        (
                                            SeparatedList
                                            (
                                                new List<ArgumentSyntax>() {
                                                    Argument
                                                    (
                                                        IdentifierName
                                                        (
                                                            "reader"
                                                        )
                                                    ),
                                                    Argument
                                                        (
                                                            MemberAccessExpression
                                                            (
                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                IdentifierName
                                                                (
                                                                    "value"
                                                                ),
                                                                IdentifierName
                                                                (
                                                                    fieldName
                                                                )
                                                            )
                                                        )
                                                        .WithRefKindKeyword
                                                        (
                                                            Token
                                                            (
                                                                SyntaxKind.RefKeyword
                                                            )
                                                        )
                                                }
                                            )
                                        )
                                    )
                                );
                            }
                        )
                        .ToArray()
                );

            #endregion

            #region readDeltaField

            MethodDeclarationSyntax readDeltaField = MethodDeclaration
                (
                    IdentifierName
                    (
                        "void"
                    ),
                    "readDelta"
                )
                .AddModifiers
                (
                    Token
                    (
                        SyntaxKind.PublicKeyword
                    ),
                    Token
                    (
                        SyntaxKind.StaticKeyword
                    )
                )
                .AddParameterListParameters
                (
                    Parameter
                        (
                            Identifier
                            (
                                "reader"
                            )
                        )
                        .WithType
                        (
                            IdentifierName
                            (
                                "Unity.Netcode.FastBufferReader"
                            )
                        ),
                    Parameter
                        (
                            Identifier
                            (
                                "value"
                            )
                        )
                        .WithType
                        (
                            IdentifierName
                            (
                                context.basicsContext.contextTargetNode.Identifier.Text
                            )
                        )
                        .WithModifiers
                        (
                            TokenList
                            (
                                Token
                                (
                                    SyntaxKind.RefKeyword
                                )
                            )
                        )
                )
                .AddBodyStatements
                (
                    isNotValueType
                        ? new StatementSyntax[] {
                            LocalDeclarationStatement
                            (
                                VariableDeclaration
                                (
                                    ParseTypeName
                                    (
                                        "bool"
                                    ),
                                    SeparatedList
                                    (
                                        new[] {
                                            VariableDeclarator
                                            (
                                                "useRead"
                                            )
                                        }
                                    )
                                )
                            ),
                            ExpressionStatement
                            (
                                InvocationExpression
                                (
                                    IdentifierName
                                    (
                                        "Unity.Netcode.ByteUnpacker.ReadValuePacked"
                                    ),
                                    ArgumentList
                                    (
                                        SeparatedList
                                        (
                                            new[] {
                                                Argument
                                                (
                                                    IdentifierName
                                                    (
                                                        "reader"
                                                    )
                                                ),
                                                Argument
                                                    (
                                                        IdentifierName
                                                        (
                                                            "useRead"
                                                        )
                                                    )
                                                    .WithRefKindKeyword
                                                    (
                                                        Token
                                                        (
                                                            SyntaxKind.OutKeyword
                                                        )
                                                    )
                                            }
                                        )
                                    )
                                )
                            ),
                            IfStatement
                            (
                                IdentifierName("useRead"),
                                Block
                                (
                                    ExpressionStatement
                                    (
                                        InvocationExpression
                                        (
                                            IdentifierName
                                            (
                                                "read"
                                            ),
                                            ArgumentList
                                            (
                                                SeparatedList
                                                (
                                                    new[] {
                                                        Argument
                                                        (
                                                            IdentifierName
                                                            (
                                                                "reader"
                                                            )
                                                        ),
                                                        Argument
                                                            (
                                                                IdentifierName
                                                                (
                                                                    "value"
                                                                )
                                                            )
                                                            .WithRefKindKeyword
                                                            (
                                                                Token
                                                                (
                                                                    SyntaxKind.OutKeyword
                                                                )
                                                            ),
                                                    }
                                                )
                                            )
                                        )
                                    ),
                                    ReturnStatement()
                                )
                            ),
                            LocalDeclarationStatement
                            (
                                VariableDeclaration
                                (
                                    ParseTypeName
                                    (
                                        "bool"
                                    ),
                                    SeparatedList
                                    (
                                        new[] {
                                            VariableDeclarator
                                            (
                                                "isNull"
                                            )
                                        }
                                    )
                                )
                            ),
                            ExpressionStatement
                            (
                                InvocationExpression
                                (
                                    IdentifierName
                                    (
                                        "Unity.Netcode.ByteUnpacker.ReadValuePacked"
                                    ),
                                    ArgumentList
                                    (
                                        SeparatedList
                                        (
                                            new[] {
                                                Argument
                                                (
                                                    IdentifierName
                                                    (
                                                        "reader"
                                                    )
                                                ),
                                                Argument
                                                    (
                                                        IdentifierName
                                                        (
                                                            "isNull"
                                                        )
                                                    )
                                                    .WithRefKindKeyword
                                                    (
                                                        Token
                                                        (
                                                            SyntaxKind.OutKeyword
                                                        )
                                                    )
                                            }
                                        )
                                    )
                                )
                            ),
                            IfStatement
                            (
                                IdentifierName
                                (
                                    "isNull"
                                ),
                                Block
                                (
                                    ExpressionStatement
                                    (
                                        AssignmentExpression
                                        (
                                            SyntaxKind.SimpleAssignmentExpression,
                                            IdentifierName
                                            (
                                                "value"
                                            ),
                                            IdentifierName
                                            (
                                                "null!"
                                            )
                                        )
                                    ),
                                    ReturnStatement(
                                    )
                                )
                            ),
                            IfStatement
                            (
                                BinaryExpression
                                (
                                    SyntaxKind.EqualsExpression,
                                    IdentifierName
                                    (
                                        "value"
                                    ),
                                    LiteralExpression
                                    (
                                        SyntaxKind.NullLiteralExpression
                                    )
                                ),
                                Block
                                (
                                    ExpressionStatement
                                    (
                                        AssignmentExpression
                                        (
                                            SyntaxKind.SimpleAssignmentExpression,
                                            IdentifierName
                                            (
                                                "value"
                                            ),
                                            ObjectCreationExpression
                                                (
                                                    ParseTypeName
                                                    (
                                                        context.basicsContext.contextTargetNode.Identifier.Text
                                                    )
                                                )
                                                .WithArgumentList
                                                (
                                                    ArgumentList()
                                                )
                                        )
                                    )
                                )
                            )
                        }
                        : Array.Empty<StatementSyntax>()
                )
                .AddBodyStatements
                (
                    LocalDeclarationStatement
                    (
                        VariableDeclaration
                        (
                            PredefinedType
                            (
                                Token
                                (
                                    SyntaxKind.IntKeyword
                                )
                            ),
                            SeparatedList
                            (
                                new[] {
                                    VariableDeclarator
                                    (
                                        Identifier
                                        (
                                            "tag"
                                        )
                                    )
                                }
                            )
                        )
                    ),
                    ExpressionStatement
                    (
                        InvocationExpression
                        (
                            IdentifierName
                            (
                                "Unity.Netcode.ByteUnpacker.ReadValuePacked"
                            ),
                            ArgumentList
                            (
                                SeparatedList
                                (
                                    new[] {
                                        Argument
                                        (
                                            IdentifierName
                                            (
                                                "reader"
                                            )
                                        ),
                                        Argument
                                            (
                                                IdentifierName
                                                (
                                                    "tag"
                                                )
                                            )
                                            .WithRefKindKeyword
                                            (
                                                Token
                                                (
                                                    SyntaxKind.OutKeyword
                                                )
                                            )
                                    }
                                )
                            )
                        )
                    )
                )
                .AddBodyStatements
                (
                    context.fieldsAttributeContextList.Select
                        (
                            (field, id) => {
                                string type = field.fieldsContext.typeContext.typeName;
                                string fieldName = field.fieldsContext.fieldName;

                                return (StatementSyntax)IfStatement
                                (
                                    BinaryExpression
                                    (
                                        SyntaxKind.NotEqualsExpression,
                                        ParenthesizedExpression
                                        (
                                            BinaryExpression
                                            (
                                                SyntaxKind.BitwiseAndExpression,
                                                IdentifierName("tag"),
                                                ParenthesizedExpression
                                                (
                                                    BinaryExpression
                                                    (
                                                        SyntaxKind.LeftShiftExpression,
                                                        LiteralExpression
                                                        (
                                                            SyntaxKind.NumericLiteralExpression,
                                                            Literal(1)
                                                        ),
                                                        IdentifierName(id.ToString())
                                                    )
                                                )
                                            )
                                        ),
                                        LiteralExpression
                                        (
                                            SyntaxKind.NumericLiteralExpression,
                                            Literal(0)
                                        )
                                    ),
                                    Block
                                    (
                                        ExpressionStatement
                                        (
                                            InvocationExpression
                                            (
                                                MemberAccessExpression
                                                (
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    IdentifierName
                                                    (
                                                        $"Unity.Netcode.NetworkVariableSerialization<{type}>"
                                                    ),
                                                    IdentifierName
                                                    (
                                                        "ReadDelta"
                                                    )
                                                ),
                                                ArgumentList
                                                (
                                                    SeparatedList
                                                    (
                                                        new List<ArgumentSyntax>() {
                                                            Argument
                                                            (
                                                                IdentifierName
                                                                (
                                                                    "reader"
                                                                )
                                                            ),
                                                            Argument
                                                                (
                                                                    MemberAccessExpression
                                                                    (
                                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                                        IdentifierName
                                                                        (
                                                                            "value"
                                                                        ),
                                                                        IdentifierName
                                                                        (
                                                                            fieldName
                                                                        )
                                                                    )
                                                                )
                                                                .WithRefKindKeyword
                                                                (
                                                                    Token
                                                                    (
                                                                        SyntaxKind.RefKeyword
                                                                    )
                                                                )
                                                        }
                                                    )
                                                )
                                            )
                                        )
                                    )
                                );
                            }
                        )
                        .ToArray()
                );

            #endregion

            #region writeField

            MethodDeclarationSyntax writeField = MethodDeclaration
                (
                    IdentifierName
                    (
                        "void"
                    ),
                    "write"
                )
                .AddModifiers
                (
                    Token
                    (
                        SyntaxKind.PublicKeyword
                    ),
                    Token
                    (
                        SyntaxKind.StaticKeyword
                    )
                )
                .AddParameterListParameters
                (
                    Parameter
                        (
                            Identifier
                            (
                                "writer"
                            )
                        )
                        .WithType
                        (
                            IdentifierName
                            (
                                "Unity.Netcode.FastBufferWriter"
                            )
                        ),
                    Parameter
                        (
                            Identifier
                            (
                                "value"
                            )
                        )
                        .WithType
                        (
                            IdentifierName
                            (
                                context.basicsContext.contextTargetNode.Identifier.Text
                            )
                        )
                        .WithModifiers
                        (
                            TokenList
                            (
                                Token
                                (
                                    SyntaxKind.InKeyword
                                )
                            )
                        )
                )
                .AddBodyStatements
                (
                    isNotValueType
                        ? new StatementSyntax[] {
                            IfStatement
                            (
                                BinaryExpression
                                (
                                    SyntaxKind.EqualsExpression,
                                    IdentifierName
                                    (
                                        "value"
                                    ),
                                    LiteralExpression
                                    (
                                        SyntaxKind.NullLiteralExpression
                                    )
                                ),
                                Block
                                (
                                    ExpressionStatement
                                    (
                                        InvocationExpression
                                        (
                                            IdentifierName
                                            (
                                                "Unity.Netcode.BytePacker.WriteValuePacked"
                                            ),
                                            ArgumentList
                                            (
                                                SeparatedList
                                                (
                                                    new[] {
                                                        Argument
                                                        (
                                                            IdentifierName
                                                            (
                                                                "writer"
                                                            )
                                                        ),
                                                        Argument
                                                        (
                                                            IdentifierName
                                                            (
                                                                "true"
                                                            )
                                                        ),
                                                    }
                                                )
                                            )
                                        )
                                    ),
                                    ReturnStatement()
                                )
                            ),
                            ExpressionStatement
                            (
                                InvocationExpression
                                (
                                    IdentifierName
                                    (
                                        "Unity.Netcode.BytePacker.WriteValuePacked"
                                    ),
                                    ArgumentList
                                    (
                                        SeparatedList
                                        (
                                            new[] {
                                                Argument
                                                (
                                                    IdentifierName
                                                    (
                                                        "writer"
                                                    )
                                                ),
                                                Argument
                                                (
                                                    IdentifierName
                                                    (
                                                        "false"
                                                    )
                                                ),
                                            }
                                        )
                                    )
                                )
                            )
                        }
                        : Array.Empty<StatementSyntax>()
                )
                .AddBodyStatements(
                )
                .AddBodyStatements
                (
                    context.fieldsAttributeContextList.Select
                        (
                            field => {
                                string type = field.fieldsContext.typeContext.typeName;
                                string fieldName = field.fieldsContext.fieldName;

                                return (StatementSyntax)ExpressionStatement
                                    (
                                        InvocationExpression
                                        (
                                            MemberAccessExpression
                                            (
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                IdentifierName
                                                (
                                                    $"Unity.Netcode.NetworkVariableSerialization<{type}>"
                                                ),
                                                IdentifierName
                                                (
                                                    "Write"
                                                )
                                            ),
                                            ArgumentList
                                            (
                                                SeparatedList
                                                (
                                                    new List<ArgumentSyntax>() {
                                                        Argument
                                                        (
                                                            IdentifierName
                                                            (
                                                                "writer"
                                                            )
                                                        ),
                                                        Argument
                                                            (
                                                                MemberAccessExpression
                                                                (
                                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                                    IdentifierName
                                                                    (
                                                                        "value"
                                                                    ),
                                                                    IdentifierName
                                                                    (
                                                                        fieldName
                                                                    )
                                                                )
                                                            )
                                                            .WithRefKindKeyword
                                                            (
                                                                Token
                                                                (
                                                                    SyntaxKind.RefKeyword
                                                                )
                                                            )
                                                    }
                                                )
                                            )
                                        )
                                    )
                                    ;
                            }
                        )
                        .ToArray()
                );

            #endregion

            #region writeDeltaField

            MethodDeclarationSyntax writeDeltaField = MethodDeclaration
                (
                    IdentifierName
                    (
                        "void"
                    ),
                    "writeDelta"
                )
                .AddModifiers
                (
                    Token
                    (
                        SyntaxKind.PublicKeyword
                    ),
                    Token
                    (
                        SyntaxKind.StaticKeyword
                    )
                )
                .AddParameterListParameters
                (
                    Parameter
                        (
                            Identifier
                            (
                                "writer"
                            )
                        )
                        .WithType
                        (
                            IdentifierName
                            (
                                "Unity.Netcode.FastBufferWriter"
                            )
                        ),
                    Parameter
                        (
                            Identifier
                            (
                                "value"
                            )
                        )
                        .WithType
                        (
                            IdentifierName
                            (
                                context.basicsContext.contextTargetNode.Identifier.Text
                            )
                        )
                        .WithModifiers
                        (
                            TokenList
                            (
                                Token
                                (
                                    SyntaxKind.InKeyword
                                )
                            )
                        ),
                    Parameter
                        (
                            Identifier
                            (
                                "previousValue"
                            )
                        )
                        .WithType
                        (
                            IdentifierName
                            (
                                context.basicsContext.contextTargetNode.Identifier.Text
                            )
                        )
                        .WithModifiers
                        (
                            TokenList
                            (
                                Token
                                (
                                    SyntaxKind.InKeyword
                                )
                            )
                        )
                )
                .AddBodyStatements
                (
                    isNotValueType
                        ? new StatementSyntax[] {
                            IfStatement
                            (
                                BinaryExpression
                                (
                                    SyntaxKind.EqualsExpression,
                                    IdentifierName
                                    (
                                        "previousValue"
                                    ),
                                    LiteralExpression
                                    (
                                        SyntaxKind.NullLiteralExpression
                                    )
                                ),
                                Block
                                (
                                    ExpressionStatement
                                    (
                                        InvocationExpression
                                        (
                                            IdentifierName
                                            (
                                                "Unity.Netcode.BytePacker.WriteValuePacked"
                                            ),
                                            ArgumentList
                                            (
                                                SeparatedList
                                                (
                                                    new[] {
                                                        Argument
                                                        (
                                                            IdentifierName
                                                            (
                                                                "writer"
                                                            )
                                                        ),
                                                        Argument
                                                        (
                                                            IdentifierName
                                                            (
                                                                "true"
                                                            )
                                                        ),
                                                    }
                                                )
                                            )
                                        )
                                    ),
                                    ExpressionStatement
                                    (
                                        InvocationExpression
                                        (
                                            IdentifierName
                                            (
                                                "write"
                                            ),
                                            ArgumentList
                                            (
                                                SeparatedList
                                                (
                                                    new[] {
                                                        Argument
                                                        (
                                                            IdentifierName
                                                            (
                                                                "writer"
                                                            )
                                                        ),
                                                        Argument
                                                            (
                                                                IdentifierName
                                                                (
                                                                    "value"
                                                                )
                                                            )
                                                            .WithRefKindKeyword
                                                            (
                                                                Token
                                                                (
                                                                    SyntaxKind.InKeyword
                                                                )
                                                            ),
                                                    }
                                                )
                                            )
                                        )
                                    ),
                                    ReturnStatement()
                                )
                            ),
                            ExpressionStatement
                            (
                                InvocationExpression
                                (
                                    IdentifierName
                                    (
                                        "Unity.Netcode.BytePacker.WriteValuePacked"
                                    ),
                                    ArgumentList
                                    (
                                        SeparatedList
                                        (
                                            new[] {
                                                Argument
                                                (
                                                    IdentifierName
                                                    (
                                                        "writer"
                                                    )
                                                ),
                                                Argument
                                                (
                                                    IdentifierName
                                                    (
                                                        "false"
                                                    )
                                                ),
                                            }
                                        )
                                    )
                                )
                            ),
                            IfStatement
                            (
                                BinaryExpression
                                (
                                    SyntaxKind.EqualsExpression,
                                    IdentifierName
                                    (
                                        "value"
                                    ),
                                    LiteralExpression
                                    (
                                        SyntaxKind.NullLiteralExpression
                                    )
                                ),
                                Block
                                (
                                    ExpressionStatement
                                    (
                                        InvocationExpression
                                        (
                                            IdentifierName
                                            (
                                                "Unity.Netcode.BytePacker.WriteValuePacked"
                                            ),
                                            ArgumentList
                                            (
                                                SeparatedList
                                                (
                                                    new[] {
                                                        Argument
                                                        (
                                                            IdentifierName
                                                            (
                                                                "writer"
                                                            )
                                                        ),
                                                        Argument
                                                        (
                                                            IdentifierName
                                                            (
                                                                "true"
                                                            )
                                                        ),
                                                    }
                                                )
                                            )
                                        )
                                    ),
                                    ReturnStatement()
                                )
                            ),
                            ExpressionStatement
                            (
                                InvocationExpression
                                (
                                    IdentifierName
                                    (
                                        "Unity.Netcode.BytePacker.WriteValuePacked"
                                    ),
                                    ArgumentList
                                    (
                                        SeparatedList
                                        (
                                            new[] {
                                                Argument
                                                (
                                                    IdentifierName
                                                    (
                                                        "writer"
                                                    )
                                                ),
                                                Argument
                                                (
                                                    IdentifierName
                                                    (
                                                        "false"
                                                    )
                                                ),
                                            }
                                        )
                                    )
                                )
                            )
                        }
                        : Array.Empty<StatementSyntax>()
                )
                .AddBodyStatements
                (
                    LocalDeclarationStatement
                    (
                        VariableDeclaration
                        (
                            PredefinedType
                            (
                                Token
                                (
                                    SyntaxKind.IntKeyword
                                )
                            ),
                            SeparatedList
                            (
                                new[] {
                                    VariableDeclarator
                                    (
                                        Identifier
                                        (
                                            "tag"
                                        ),
                                        null,
                                        EqualsValueClause
                                        (
                                            LiteralExpression
                                            (
                                                SyntaxKind.NumericLiteralExpression,
                                                Literal
                                                (
                                                    0
                                                )
                                            )
                                        )
                                    )
                                }
                            )
                        )
                    )
                )
                .AddBodyStatements
                (
                    context.fieldsAttributeContextList.Select
                        (
                            (field, id) => {
                                string type = field.fieldsContext.typeContext.typeName;
                                string fieldName = field.fieldsContext.fieldName;

                                return (StatementSyntax)IfStatement
                                (
                                    PrefixUnaryExpression
                                    (
                                        SyntaxKind.LogicalNotExpression,
                                        InvocationExpression
                                        (
                                            IdentifierName
                                            (
                                                $"Unity.Netcode.NetworkVariableSerialization<{type}>.AreEqual"
                                            ),
                                            ArgumentList
                                            (
                                                SeparatedList
                                                (
                                                    new[] {
                                                        Argument
                                                            (
                                                                MemberAccessExpression
                                                                (
                                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                                    IdentifierName
                                                                    (
                                                                        "previousValue"
                                                                    ),
                                                                    IdentifierName
                                                                    (
                                                                        fieldName
                                                                    )
                                                                )
                                                            )
                                                            .WithRefKindKeyword
                                                            (
                                                                Token
                                                                (
                                                                    SyntaxKind.RefKeyword
                                                                )
                                                            ),
                                                        Argument
                                                            (
                                                                MemberAccessExpression
                                                                (
                                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                                    IdentifierName
                                                                    (
                                                                        "value"
                                                                    ),
                                                                    IdentifierName
                                                                    (
                                                                        fieldName
                                                                    )
                                                                )
                                                            )
                                                            .WithRefKindKeyword
                                                            (
                                                                Token
                                                                (
                                                                    SyntaxKind.RefKeyword
                                                                )
                                                            )
                                                    }
                                                )
                                            )
                                        )
                                    ),
                                    Block
                                    (
                                        ExpressionStatement
                                        (
                                            AssignmentExpression
                                            (
                                                SyntaxKind.SimpleAssignmentExpression,
                                                IdentifierName("tag"),
                                                BinaryExpression
                                                (
                                                    SyntaxKind.BitwiseOrExpression,
                                                    IdentifierName("tag"),
                                                    ParenthesizedExpression
                                                    (
                                                        BinaryExpression
                                                        (
                                                            SyntaxKind.LeftShiftExpression,
                                                            LiteralExpression
                                                            (
                                                                SyntaxKind.NumericLiteralExpression,
                                                                Literal(1)
                                                            ),
                                                            IdentifierName(id.ToString())
                                                        )
                                                    )
                                                )
                                            )
                                        )
                                    )
                                );
                            }
                        )
                        .ToArray()
                )
                .AddBodyStatements
                (
                    ExpressionStatement
                    (
                        InvocationExpression
                        (
                            IdentifierName
                            (
                                "Unity.Netcode.BytePacker.WriteValuePacked"
                            ),
                            ArgumentList
                            (
                                SeparatedList
                                (
                                    new[] {
                                        Argument
                                        (
                                            IdentifierName
                                            (
                                                "writer"
                                            )
                                        ),
                                        Argument
                                        (
                                            IdentifierName
                                            (
                                                "tag"
                                            )
                                        ),
                                    }
                                )
                            )
                        )
                    )
                )
                .AddBodyStatements
                (
                    context.fieldsAttributeContextList.Select
                        (
                            (field, id) => {
                                string type = field.fieldsContext.typeContext.typeName;
                                string fieldName = field.fieldsContext.fieldName;

                                return (StatementSyntax)IfStatement
                                (
                                    BinaryExpression
                                    (
                                        SyntaxKind.NotEqualsExpression,
                                        ParenthesizedExpression
                                        (
                                            BinaryExpression
                                            (
                                                SyntaxKind.BitwiseAndExpression,
                                                IdentifierName("tag"),
                                                ParenthesizedExpression
                                                (
                                                    BinaryExpression
                                                    (
                                                        SyntaxKind.LeftShiftExpression,
                                                        LiteralExpression
                                                        (
                                                            SyntaxKind.NumericLiteralExpression,
                                                            Literal(1)
                                                        ),
                                                        IdentifierName(id.ToString())
                                                    )
                                                )
                                            )
                                        ),
                                        LiteralExpression
                                        (
                                            SyntaxKind.NumericLiteralExpression,
                                            Literal(0)
                                        )
                                    ),
                                    Block
                                    (
                                        ExpressionStatement
                                        (
                                            InvocationExpression
                                            (
                                                IdentifierName
                                                (
                                                    $"Unity.Netcode.NetworkVariableSerialization<{type}>.WriteDelta"
                                                ),
                                                ArgumentList
                                                (
                                                    SeparatedList
                                                    (
                                                        new List<ArgumentSyntax>() {
                                                            Argument
                                                            (
                                                                IdentifierName
                                                                (
                                                                    "writer"
                                                                )
                                                            ),
                                                            Argument
                                                                (
                                                                    MemberAccessExpression
                                                                    (
                                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                                        IdentifierName
                                                                        (
                                                                            "value"
                                                                        ),
                                                                        IdentifierName
                                                                        (
                                                                            fieldName
                                                                        )
                                                                    )
                                                                )
                                                                .WithRefKindKeyword
                                                                (
                                                                    Token
                                                                    (
                                                                        SyntaxKind.RefKeyword
                                                                    )
                                                                ),
                                                            Argument
                                                                (
                                                                    MemberAccessExpression
                                                                    (
                                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                                        IdentifierName
                                                                        (
                                                                            "previousValue"
                                                                        ),
                                                                        IdentifierName
                                                                        (
                                                                            fieldName
                                                                        )
                                                                    )
                                                                )
                                                                .WithRefKindKeyword
                                                                (
                                                                    Token
                                                                    (
                                                                        SyntaxKind.RefKeyword
                                                                    )
                                                                )
                                                        }
                                                    )
                                                )
                                            )
                                        )
                                    )
                                );
                            }
                        )
                        .ToArray()
                );

            #endregion

            #region duplicateValue

            MethodDeclarationSyntax duplicateValue = MethodDeclaration
                (
                    IdentifierName
                    (
                        "void"
                    ),
                    "duplicateValue"
                )
                .AddModifiers
                (
                    Token
                    (
                        SyntaxKind.PublicKeyword
                    ),
                    Token
                    (
                        SyntaxKind.StaticKeyword
                    )
                )
                .AddParameterListParameters
                (
                    Parameter
                        (
                            Identifier
                            (
                                "value"
                            )
                        )
                        .WithType
                        (
                            IdentifierName
                            (
                                context.basicsContext.contextTargetNode.Identifier.Text
                            )
                        )
                        .WithModifiers
                        (
                            TokenList
                            (
                                Token
                                (
                                    SyntaxKind.InKeyword
                                )
                            )
                        ),
                    Parameter
                        (
                            Identifier
                            (
                                "duplicatedValue"
                            )
                        )
                        .WithType
                        (
                            IdentifierName
                            (
                                context.basicsContext.contextTargetNode.Identifier.Text
                            )
                        )
                        .WithModifiers
                        (
                            TokenList
                            (
                                Token
                                (
                                    SyntaxKind.RefKeyword
                                )
                            )
                        )
                )
                .AddBodyStatements
                (
                    isNotValueType
                        ? new StatementSyntax[] {
                            IfStatement
                            (
                                BinaryExpression
                                (
                                    SyntaxKind.EqualsExpression,
                                    IdentifierName
                                    (
                                        "value"
                                    ),
                                    LiteralExpression
                                    (
                                        SyntaxKind.NullLiteralExpression
                                    )
                                ),
                                Block
                                (
                                    ExpressionStatement
                                    (
                                        AssignmentExpression
                                        (
                                            SyntaxKind.SimpleAssignmentExpression,
                                            IdentifierName
                                            (
                                                "duplicatedValue"
                                            ),
                                            IdentifierName
                                            (
                                                "null!"
                                            )
                                        )
                                    ),
                                    ReturnStatement()
                                )
                            ),
                            IfStatement
                            (
                                BinaryExpression
                                (
                                    SyntaxKind.EqualsExpression,
                                    IdentifierName
                                    (
                                        "duplicatedValue"
                                    ),
                                    LiteralExpression
                                    (
                                        SyntaxKind.NullLiteralExpression
                                    )
                                ),
                                Block
                                (
                                    ExpressionStatement
                                    (
                                        AssignmentExpression
                                        (
                                            SyntaxKind.SimpleAssignmentExpression,
                                            IdentifierName
                                            (
                                                "duplicatedValue"
                                            ),
                                            ObjectCreationExpression
                                                (
                                                    ParseTypeName
                                                    (
                                                        context.basicsContext.contextTargetNode.Identifier.Text
                                                    )
                                                )
                                                .WithArgumentList
                                                (
                                                    ArgumentList()
                                                )
                                        )
                                    )
                                )
                            )
                        }
                        : Array.Empty<StatementSyntax>()
                )
                .AddBodyStatements
                (
                    context.fieldsAttributeContextList.Select
                        (
                            (field, id) => {
                                string type = field.fieldsContext.typeContext.typeName;
                                string fieldName = field.fieldsContext.fieldName;

                                return (StatementSyntax)IfStatement
                                    (
                                        PrefixUnaryExpression
                                        (
                                            SyntaxKind.LogicalNotExpression,
                                            InvocationExpression
                                            (
                                                IdentifierName
                                                (
                                                    $"Unity.Netcode.NetworkVariableSerialization<{type}>.AreEqual"
                                                ),
                                                ArgumentList
                                                (
                                                    SeparatedList
                                                    (
                                                        new[] {
                                                            Argument
                                                                (
                                                                    MemberAccessExpression
                                                                    (
                                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                                        IdentifierName
                                                                        (
                                                                            "duplicatedValue"
                                                                        ),
                                                                        IdentifierName
                                                                        (
                                                                            fieldName
                                                                        )
                                                                    )
                                                                )
                                                                .WithRefKindKeyword
                                                                (
                                                                    Token
                                                                    (
                                                                        SyntaxKind.RefKeyword
                                                                    )
                                                                ),
                                                            Argument
                                                                (
                                                                    MemberAccessExpression
                                                                    (
                                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                                        IdentifierName
                                                                        (
                                                                            "value"
                                                                        ),
                                                                        IdentifierName
                                                                        (
                                                                            fieldName
                                                                        )
                                                                    )
                                                                )
                                                                .WithRefKindKeyword
                                                                (
                                                                    Token
                                                                    (
                                                                        SyntaxKind.RefKeyword
                                                                    )
                                                                )
                                                        }
                                                    )
                                                )
                                            )
                                        ),
                                        Block
                                        (
                                            ExpressionStatement
                                            (
                                                InvocationExpression
                                                (
                                                    IdentifierName
                                                    (
                                                        $"Unity.Netcode.NetworkVariableSerialization<{type}>.Duplicate"
                                                    ),
                                                    ArgumentList
                                                    (
                                                        SeparatedList
                                                        (
                                                            new List<ArgumentSyntax>() {
                                                                Argument
                                                                (
                                                                    MemberAccessExpression
                                                                    (
                                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                                        IdentifierName
                                                                        (
                                                                            "value"
                                                                        ),
                                                                        IdentifierName
                                                                        (
                                                                            fieldName
                                                                        )
                                                                    )
                                                                ),
                                                                Argument
                                                                    (
                                                                        MemberAccessExpression
                                                                        (
                                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                                            IdentifierName
                                                                            (
                                                                                "duplicatedValue"
                                                                            ),
                                                                            IdentifierName
                                                                            (
                                                                                fieldName
                                                                            )
                                                                        )
                                                                    )
                                                                    .WithRefKindKeyword
                                                                    (
                                                                        Token
                                                                        (
                                                                            SyntaxKind.RefKeyword
                                                                        )
                                                                    )
                                                            }
                                                        )
                                                    )
                                                )
                                            )
                                        )
                                    )
                                    ;
                            }
                        )
                        .ToArray()
                );

            #endregion

            #region equals

            MethodDeclarationSyntax equals = MethodDeclaration
                (
                    IdentifierName
                    (
                        "bool"
                    ),
                    "equals"
                )
                .AddModifiers
                (
                    Token
                    (
                        SyntaxKind.PublicKeyword
                    ),
                    Token
                    (
                        SyntaxKind.StaticKeyword
                    )
                )
                .AddParameterListParameters
                (
                    Parameter
                        (
                            Identifier
                            (
                                "a"
                            )
                        )
                        .WithType
                        (
                            IdentifierName
                            (
                                context.basicsContext.contextTargetNode.Identifier.Text
                            )
                        )
                        .WithModifiers
                        (
                            TokenList
                            (
                                Token
                                (
                                    SyntaxKind.RefKeyword
                                )
                            )
                        ),
                    Parameter
                        (
                            Identifier
                            (
                                "b"
                            )
                        )
                        .WithType
                        (
                            IdentifierName
                            (
                                context.basicsContext.contextTargetNode.Identifier.Text
                            )
                        )
                        .WithModifiers
                        (
                            TokenList
                            (
                                Token
                                (
                                    SyntaxKind.RefKeyword
                                )
                            )
                        )
                )
                .AddBodyStatements
                (
                    IfStatement
                    (
                        BinaryExpression
                        (
                            SyntaxKind.EqualsExpression,
                            IdentifierName
                            (
                                "a"
                            ),
                            IdentifierName
                            (
                                "b"
                            )
                        ),
                        Block
                        (
                            ReturnStatement
                            (
                                LiteralExpression
                                (
                                    SyntaxKind.TrueLiteralExpression
                                )
                            )
                        )
                    ),
                    IfStatement
                    (
                        BinaryExpression
                        (
                            SyntaxKind.EqualsExpression,
                            IdentifierName
                            (
                                "a"
                            ),
                            IdentifierName
                            (
                                "null"
                            )
                        ),
                        Block
                        (
                            ReturnStatement
                            (
                                LiteralExpression
                                (
                                    SyntaxKind.FalseLiteralExpression
                                )
                            )
                        )
                    ),
                    IfStatement
                    (
                        BinaryExpression
                        (
                            SyntaxKind.EqualsExpression,
                            IdentifierName
                            (
                                "b"
                            ),
                            IdentifierName
                            (
                                "null"
                            )
                        ),
                        Block
                        (
                            ReturnStatement
                            (
                                LiteralExpression
                                (
                                    SyntaxKind.FalseLiteralExpression
                                )
                            )
                        )
                    )
                )
                .AddBodyStatements
                (
                    context.fieldsAttributeContextList.Select
                        (
                            (field, id) => {
                                string type = field.fieldsContext.typeContext.typeName;
                                string fieldName = field.fieldsContext.fieldName;

                                return IfStatement
                                (
                                    PrefixUnaryExpression
                                    (
                                        SyntaxKind.LogicalNotExpression,
                                        InvocationExpression
                                        (
                                            IdentifierName
                                            (
                                                $"Unity.Netcode.NetworkVariableSerialization<{type}>.AreEqual"
                                            ),
                                            ArgumentList
                                            (
                                                SeparatedList
                                                (
                                                    new[] {
                                                        Argument
                                                            (
                                                                MemberAccessExpression
                                                                (
                                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                                    IdentifierName
                                                                    (
                                                                        "a"
                                                                    ),
                                                                    IdentifierName
                                                                    (
                                                                        fieldName
                                                                    )
                                                                )
                                                            )
                                                            .WithRefKindKeyword
                                                            (
                                                                Token
                                                                (
                                                                    SyntaxKind.RefKeyword
                                                                )
                                                            ),
                                                        Argument
                                                            (
                                                                MemberAccessExpression
                                                                (
                                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                                    IdentifierName
                                                                    (
                                                                        "b"
                                                                    ),
                                                                    IdentifierName
                                                                    (
                                                                        fieldName
                                                                    )
                                                                )
                                                            )
                                                            .WithRefKindKeyword
                                                            (
                                                                Token
                                                                (
                                                                    SyntaxKind.RefKeyword
                                                                )
                                                            )
                                                    }
                                                )
                                            )
                                        )
                                    ),
                                    Block
                                    (
                                        ReturnStatement
                                        (
                                            LiteralExpression
                                            (
                                                SyntaxKind.FalseLiteralExpression
                                            )
                                        )
                                    )
                                );

                            }
                        )
                        .OfType<StatementSyntax>()
                        .ToArray()
                )
                .AddBodyStatements
                (
                    ReturnStatement
                    (
                        LiteralExpression
                        (
                            SyntaxKind.TrueLiteralExpression
                        )
                    )
                );

            #endregion

            #region initializeOnLoad

            MethodDeclarationSyntax initializeOnLoad = MethodDeclaration
                (
                    IdentifierName
                    (
                        "void"
                    ),
                    context.basicsContext.contextTargetNode.Identifier.Text + "InitializeOnLoad"
                )
                .AddModifiers
                (
                    Token
                    (
                        SyntaxKind.ProtectedKeyword
                    ),
                    Token
                    (
                        SyntaxKind.StaticKeyword
                    )
                )
                /*.AddAttributeLists
                (
                    AttributeList
                    (
                        SeparatedList
                        (
                            new[] {
                                Attribute
                                (
                                    ParseName("UnityEngine.RuntimeInitializeOnLoadMethodAttribute"),
                                    AttributeArgumentList
                                    (
                                        SeparatedList
                                        (
                                            new[] {
                                                AttributeArgument
                                                (
                                                    ParseTypeName("UnityEngine.RuntimeInitializeLoadType.AfterAssembliesLoaded")
                                                )
                                            }
                                        )
                                    )
                                )
                            }
                        )
                    )
                )#1#
                .AddAttributeLists
                (
                    AttributeList
                    (
                        SeparatedList
                        (
                            new[] {
                                Attribute
                                (
                                    ParseName
                                    (
                                        "UnityEditor.InitializeOnLoadMethodAttribute"
                                    )
                                )
                            }
                        )
                    )
                )
                .AddBodyStatements
                (
                    ExpressionStatement
                    (
                        AssignmentExpression
                        (
                            SyntaxKind.SimpleAssignmentExpression,
                            MemberAccessExpression
                            (
                                SyntaxKind.SimpleMemberAccessExpression,
                                ParseTypeName
                                (
                                    $"Unity.Netcode.UserNetworkVariableSerialization<{context.basicsContext.contextTargetNode.Identifier.Text}>"
                                ),
                                IdentifierName
                                (
                                    "ReadValue"
                                )
                            ),
                            IdentifierName
                            (
                                "read"
                            )
                        )
                    ),
                    ExpressionStatement
                    (
                        AssignmentExpression
                        (
                            SyntaxKind.SimpleAssignmentExpression,
                            MemberAccessExpression
                            (
                                SyntaxKind.SimpleMemberAccessExpression,
                                ParseTypeName
                                (
                                    $"Unity.Netcode.UserNetworkVariableSerialization<{context.basicsContext.contextTargetNode.Identifier.Text}>"
                                ),
                                IdentifierName
                                (
                                    "WriteValue"
                                )
                            ),
                            IdentifierName
                            (
                                "write"
                            )
                        )
                    ),
                    ExpressionStatement
                    (
                        AssignmentExpression
                        (
                            SyntaxKind.SimpleAssignmentExpression,
                            MemberAccessExpression
                            (
                                SyntaxKind.SimpleMemberAccessExpression,
                                ParseTypeName
                                (
                                    $"Unity.Netcode.UserNetworkVariableSerialization<{context.basicsContext.contextTargetNode.Identifier.Text}>"
                                ),
                                IdentifierName
                                (
                                    "WriteDelta"
                                )
                            ),
                            IdentifierName
                            (
                                "writeDelta"
                            )
                        )
                    ),
                    ExpressionStatement
                    (
                        AssignmentExpression
                        (
                            SyntaxKind.SimpleAssignmentExpression,
                            MemberAccessExpression
                            (
                                SyntaxKind.SimpleMemberAccessExpression,
                                ParseTypeName
                                (
                                    $"Unity.Netcode.UserNetworkVariableSerialization<{context.basicsContext.contextTargetNode.Identifier.Text}>"
                                ),
                                IdentifierName
                                (
                                    "ReadDelta"
                                )
                            ),
                            IdentifierName
                            (
                                "readDelta"
                            )
                        )
                    ),
                    ExpressionStatement
                    (
                        AssignmentExpression
                        (
                            SyntaxKind.SimpleAssignmentExpression,
                            MemberAccessExpression
                            (
                                SyntaxKind.SimpleMemberAccessExpression,
                                ParseTypeName
                                (
                                    $"Unity.Netcode.UserNetworkVariableSerialization<{context.basicsContext.contextTargetNode.Identifier.Text}>"
                                ),
                                IdentifierName
                                (
                                    "DuplicateValue"
                                )
                            ),
                            IdentifierName
                            (
                                "duplicateValue"
                            )
                        )
                    ),
                    ExpressionStatement
                    (
                        InvocationExpression
                        (
                            IdentifierName
                            (
                                $"typeof(Unity.Netcode.NetworkVariableSerialization<{context.basicsContext.contextTargetNode.Identifier.Text}>).GetProperty(\"AreEqual\", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)?.SetValue"
                            ),
                            ArgumentList
                            (
                                SeparatedList
                                (
                                    new[] {
                                        Argument(IdentifierName("null")),
                                        Argument(IdentifierName($"new Unity.Netcode.NetworkVariableSerialization<{context.basicsContext.contextTargetNode.Identifier.Text}>.EqualsDelegate(equals)"))
                                    }
                                )
                            )
                        )
                    )
                );
            initializeOnLoad = initializeOnLoad.AddAttributeLists
            (
                AttributeList
                (
                    SeparatedList
                    (
                        new[] {
                            Attribute
                            (
                                ParseName("UnityEngine.RuntimeInitializeOnLoadMethodAttribute"),
                                AttributeArgumentList
                                (
                                    SeparatedList
                                    (
                                        new[] {
                                            AttributeArgument
                                            (
                                                ParseTypeName("UnityEngine.RuntimeInitializeLoadType.AfterAssembliesLoaded")
                                            )
                                        }
                                    )
                                )
                            )
                        }
                    )
                )
            );

            if (context.basicsContext.context.SemanticModel.Compilation.Options.SpecificDiagnosticOptions.ContainsKey("UNITY_EDITOR")) {

                initializeOnLoad = initializeOnLoad.AddAttributeLists
                (
                    AttributeList
                    (
                        SeparatedList
                        (
                            new[] {
                                Attribute
                                (
                                    ParseName("UnityEditor.InitializeOnLoadMethodAttribute")
                                )
                            }
                        )
                    )
                );
            }

            #endregion

            context.partialClassMemberDeclarationSyntaxList.AddRange
            (
                new[] {
                    readField,
                    readDeltaField,
                    writeField,
                    writeDeltaField,
                    duplicateValue,
                    equals,
                    initializeOnLoad
                }
            );

        }

    }

}
*/
