using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Til.Lombok.Generator {

    public abstract class IncrementComponent {

        public abstract void fill(IncrementContext incrementContext);

    }

    public abstract class ClassAttributeIncrementComponent<CA> : IncrementComponent where CA : IncrementClassAttribute {

        protected virtual CA restore(Dictionary<string, string> data) {
            return (CA)Activator.CreateInstance(typeof(CA), data);
        }

        public sealed override void fill(IncrementContext incrementContext) {
            List<AttributeSyntax> caAttributeSyntax = incrementContext.basicsContext.contextTargetNode.AttributeLists.tryGetSpecifiedAttribute(typeof(CA).Name).ToList();

            IReadOnlyList<AttributeContext<CA>> attributeContexts = caAttributeSyntax.Select
                (
                    syntax => {
                        CA attribute = restore
                        (
                            syntax.getAttributeArgumentsAsDictionary
                            (
                                incrementContext.basicsContext.semanticModel
                            )
                        );

                        return new AttributeContext<CA>
                        (
                            attribute,
                            syntax
                        );
                    }
                )
                .Where(a => a != null)
                .ToList()!;

            if (!enforce() && attributeContexts.Count == 0) {
                return;
            }

            if (onlyOne()) {
                attributeContexts = new List<AttributeContext<CA>>() {
                    attributeContexts.FirstOrDefault()
                };
            }

            fill
            (
                new ClassAttributeIncrementContext<CA>
                (
                    incrementContext,
                    attributeContexts
                )
            );

        }

        public abstract void fill(ClassAttributeIncrementContext<CA> incrementContext);

        public virtual bool enforce() => true;

        public virtual bool onlyOne() => false;

    }

    public abstract class ClassFieldAttributeIncrementComponent<FA, CA> : ClassAttributeIncrementComponent<CA> where FA : IncrementFieldAttribute where CA : IncrementClassAttribute {

        protected virtual FA restoreFa(Dictionary<string, string> data) {
            return (FA)Activator.CreateInstance(typeof(FA), data);
        }

        public sealed override void fill(ClassAttributeIncrementContext<CA> incrementContext) {
            List<FieldsAttributeContext<FA>> fieldsAttributeContextList = new List<FieldsAttributeContext<FA>>();

            foreach (MemberDeclarationSyntax member in incrementContext.incrementContext.source) {
                switch (member) {
                    case FieldDeclarationSyntax fieldDeclaration: {
                        List<AttributeSyntax> attributeSyntaxeList = member.AttributeLists.tryGetSpecifiedAttribute(typeof(FA).Name).ToList();
                        if (attributeSyntaxeList.Count == 0) {
                            break;
                        }

                        IReadOnlyList<AttributeContext<FA>> attributeContexts = attributeSyntaxeList.Select
                            (
                                syntax => {
                                    FA attribute = restoreFa
                                    (
                                        syntax.getAttributeArgumentsAsDictionary
                                        (
                                            incrementContext.basicsContext.semanticModel
                                        )
                                    );
                                    return new AttributeContext<FA>
                                    (
                                        attribute,
                                        syntax
                                    );
                                }
                            )
                            .Where(a => a != null)
                            .ToList()!;

                        foreach (VariableDeclaratorSyntax variableDeclaratorSyntax in fieldDeclaration.Declaration.Variables) {

                            fieldsAttributeContextList.Add
                            (
                                new FieldsAttributeContext<FA>
                                (
                                    incrementContext.basicsContext,
                                    new FieldsContext
                                    (
                                        incrementContext.basicsContext,
                                        variableDeclaratorSyntax.Identifier.ToString(),
                                        new TypeContext
                                        (
                                            fieldDeclaration.Declaration.Type.ToString(),
                                            fieldDeclaration.Declaration.Type
                                        ),
                                        fieldDeclaration.AttributeLists,
                                        fieldDeclaration,
                                        variableDeclaratorSyntax,
                                        null
                                    ),
                                    attributeContexts
                                )
                            );

                        }
                        break;
                    }
                    case PropertyDeclarationSyntax propertyDeclaration: {

                        List<AttributeSyntax> attributeSyntaxeList = member.AttributeLists.tryGetSpecifiedAttribute(typeof(FA).Name).ToList();
                        if (attributeSyntaxeList.Count == 0) {
                            break;
                        }

                        IReadOnlyList<AttributeContext<FA>> attributeContexts = attributeSyntaxeList.Select
                            (
                                syntax => {
                                    FA attribute = restoreFa
                                    (
                                        syntax.getAttributeArgumentsAsDictionary
                                        (
                                            incrementContext.basicsContext.semanticModel
                                        )
                                    );
                                    return new AttributeContext<FA>
                                    (
                                        attribute,
                                        syntax
                                    );
                                }
                            )
                            .Where(a => a != null)
                            .ToList()!;

                        fieldsAttributeContextList.Add
                        (
                            new FieldsAttributeContext<FA>
                            (
                                incrementContext.basicsContext,
                                new FieldsContext
                                (
                                    incrementContext.basicsContext,
                                    propertyDeclaration.Identifier.ToString(),
                                    new TypeContext
                                    (
                                        propertyDeclaration.Type.ToString(),
                                        propertyDeclaration.Type
                                    ),
                                    propertyDeclaration.AttributeLists,
                                    null,
                                    null,
                                    propertyDeclaration
                                ),
                                attributeContexts
                            )
                        );
                        break;
                    }
                }
            }

            if (fieldsAttributeContextList.Count == 0) {
                return;
            }

            if (enforce() && incrementContext.attributeContextList.Count == 0) {
                fill
                (
                    new ClassFieldAttributeIncrementContext<FA, CA>
                    (
                        incrementContext.incrementContext,
                        null,
                        fieldsAttributeContextList
                    )
                );
            }

            foreach (AttributeContext<CA> attributeContext in incrementContext.attributeContextList) {
                try {
                    fill
                    (
                        new ClassFieldAttributeIncrementContext<FA, CA>
                        (
                            incrementContext.incrementContext,
                            attributeContext,
                            fieldsAttributeContextList
                        )
                    );
                }
                catch (Exception e) {
                    e.PrintExceptionSummaryAndStackTrace();
                }
            }

        }

        public abstract void fill(ClassFieldAttributeIncrementContext<FA, CA> classFieldAttributeIncrementContext);

    }

    public abstract class TranslationClassFieldAttributeIncrementComponent<FA, CA> : ClassFieldAttributeIncrementComponent<FA, CA> where FA : IncrementFieldAttribute where CA : IncrementClassAttribute {

        public sealed override void fill(ClassFieldAttributeIncrementContext<FA, CA> classFieldAttributeIncrementContext) {
            List<FieldAttributeIncrementContext<FA>> fieldsAttributeContexts = new List<FieldAttributeIncrementContext<FA>>();
            foreach (FieldsAttributeContext<FA> fieldsAttributeContext in classFieldAttributeIncrementContext.fieldsAttributeContextList) {
                foreach (AttributeContext<FA> attributeContext in fieldsAttributeContext.attributeContext) {

                    string getInvoke = fieldsAttributeContext.fieldsContext.fieldName;
                    Func<string, string> setInvoke = v => fieldsAttributeContext.fieldsContext.fieldName + '=';

                    if (!attributeContext.attribute.directAccess) {

                        getInvoke = Util.mackCustomName(getInvoke, "get" + getInvoke.toPascalCaseIdentifier(), attributeContext.attribute) + "()";
                        setInvoke = v => attributeContext.attribute.isCustomName()
                            ? $"{Util.mackCustomName(getInvoke, "set" + getInvoke.toPascalCaseIdentifier(), attributeContext.attribute)}({v})"
                            : $"set{fieldsAttributeContext.fieldsContext.fieldName.toPascalCaseIdentifier()}({v})";

                    }

                    TypeContext typeContext = new TypeContext
                    (
                        fieldsAttributeContext.fieldsContext.typeContext.typeName,
                        fieldsAttributeContext.fieldsContext.typeContext.typeSyntax,
                        fieldsAttributeContext.fieldsContext.typeContext.genericTypeContexts
                    );
                    typeContext.receive(attributeContext.attribute);

                    fieldsAttributeContexts.Add
                    (
                        new FieldAttributeIncrementContext<FA>
                        (
                            getInvoke,
                            setInvoke,
                            typeContext,
                            attributeContext,
                            fieldsAttributeContext
                        )
                    );
                }
            }
            fill
            (
                new TranslationClassFieldAttributeIncrementContext<FA, CA>
                (
                    classFieldAttributeIncrementContext,
                    fieldsAttributeContexts
                )
            );
        }

        public abstract void fill(TranslationClassFieldAttributeIncrementContext<FA, CA> context);

    }

    [IncrementComponent]
    public class FreezeGenerator : ClassAttributeIncrementComponent<IFreezeAttribute> {

        public override void fill(ClassAttributeIncrementContext<IFreezeAttribute> fieldsAttributeContext) {

            Dictionary<string, string> fillMap = new Dictionary<string, string>();
            fillMap["type"] = fieldsAttributeContext.basicsContext.contextTargetNode.toClassName();

            fillMap["namespace"] = fieldsAttributeContext.basicsContext.contextNamespaceNameSyntax.ToString();
            string model = @"

public partial class {type} : Til.Lombok.IFreeze {{

    protected System.Collections.Generic.HashSet<string> _frozen = new System.Collections.Generic.HashSet<string>();

    public bool isFrozen(string tag) => _frozen.Contains(tag);

    public void frozen(string tag) => _frozen.Add(tag);

    protected void unFrozen(string tag) => _frozen.Remove(tag);

    public void validateNonFrozen(string tag) {{
        if (isFrozen(tag)) {{
            throw new System.InvalidOperationException(""Cannot modify frozen property"");
        }}
    }}

}}

";

            StringBuilder stringBuilder = new StringBuilder();

            model.format
            (
                stringBuilder,
                k => stringBuilder.Append
                (
                    fillMap[k]
                )
            );

            MemberDeclarationSyntax[] memberDeclarationSyntaxes = CSharpSyntaxTree.ParseText(stringBuilder.ToString())
                .GetRoot()
                .ChildNodes()
                .OfType<MemberDeclarationSyntax>()
                .ToArray();

            if (fieldsAttributeContext.basicsContext.nestContext is not null) {
                fieldsAttributeContext.basicsContext.nestContext.partialClassMemberDeclarationSyntaxList.AddRange(memberDeclarationSyntaxes);
            }
            else {
                fieldsAttributeContext.basicsContext.namespaceMemberDeclarationSyntaxList.AddRange(memberDeclarationSyntaxes);
            }
        }

        public override bool enforce() => false;

    }

    [IncrementComponent]
    public class ToStringGenerator : TranslationClassFieldAttributeIncrementComponent<ToStringFieldAttribute, ToStringClassAttribute> {

        public override void fill(TranslationClassFieldAttributeIncrementContext<ToStringFieldAttribute, ToStringClassAttribute> context) {
            StringBuilder stringBuilder = new StringBuilder();
            CodeBuilder codeBuilder = new CodeBuilder(stringBuilder);

            /*using (codeBuilder.appendBlock($"public {(context.classFieldAttributeIncrementContext.caAttributeContext?.attribute.hasBase ?? false ? "new" : string.Empty)} string toFieldString(Til.Lombok.ProfundityStringBuilder profundityStringBuilder)")) {
                for (int index = 0; index < context.fieldsAttributeContextList.Count; index++) {
                    FieldAttributeIncrementContext<ToStringFieldAttribute> fieldAttributeIncrementContext = context.fieldsAttributeContextList[index];
                    string fieldName = fieldAttributeIncrementContext.attributeContext.attribute.customName ?? fieldAttributeIncrementContext.fieldsAttributeContext.fieldsContext.fieldName;
                    codeBuilder.appendLine($"profundityStringBuilder.Append(\"{fieldName}\");");
                    codeBuilder.appendLine($"profundityStringBuilder.Append(\"=\");");

                    bool isValueType = (context.basicsContext.semanticModel.GetSymbolInfo(fieldAttributeIncrementContext.fieldsAttributeContext.fieldsContext.typeContext.typeSyntax).Symbol as ITypeSymbol)?.IsValueType ?? false;

                    if (isValueType) {
                        codeBuilder.appendLine($"profundityStringBuilder");
                    }

                }


            }*/

            using (codeBuilder.appendBlock($"public {(context.classFieldAttributeIncrementContext.caAttributeContext?.attribute.hasBase ?? false ? "new" : string.Empty)} string toFieldString()")) {
                codeBuilder.appendLine("System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();");
                for (int index = 0; index < context.fieldsAttributeContextList.Count; index++) {
                    FieldAttributeIncrementContext<ToStringFieldAttribute> fieldAttributeIncrementContext = context.fieldsAttributeContextList[index];
                    string fieldName = fieldAttributeIncrementContext.attributeContext.attribute.customName ?? fieldAttributeIncrementContext.fieldsAttributeContext.fieldsContext.fieldName;
                    codeBuilder.appendLine($"stringBuilder.Append(\"{fieldName}\").Append(\"=\").Append({fieldAttributeIncrementContext.getInvoke}){(index + 1 < context.fieldsAttributeContextList.Count ? ".Append(\"\\n\")" : String.Empty)};");
                }
                codeBuilder.appendLine("return stringBuilder.ToString();");
            }

            using (codeBuilder.appendBlock("public override string ToString()")) {
                codeBuilder.appendLine("System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();");
                codeBuilder.appendLine($"stringBuilder.Append(\"{context.basicsContext.className} {{\").Append(\"\\n\");");
                codeBuilder.appendLine("stringBuilder.Append(\" \").Append(toFieldString().Replace(\"\\n\",\"\\n \")).Append(\"\\n\");");
                if (context.classFieldAttributeIncrementContext.caAttributeContext?.attribute.hasBase ?? false) {
                    codeBuilder.appendLine("stringBuilder.Append(\" \").Append(base.toFieldString().Replace(\"\\n\",\"\\n \")).Append(\"\\n\");");
                }
                codeBuilder.appendLine("stringBuilder.Append(\"}\");");
                codeBuilder.appendLine("return stringBuilder.ToString();");
            }

            MemberDeclarationSyntax[] memberDeclarationSyntaxes = CSharpSyntaxTree.ParseText(stringBuilder.ToString())
                .GetRoot()
                .ChildNodes()
                .OfType<MemberDeclarationSyntax>()
                .ToArray();

            context.incrementContext.partialClassMemberDeclarationSyntaxList.AddRange(memberDeclarationSyntaxes);
        }

    }

    [IncrementComponent]
    public sealed class HashCodeGenerator : TranslationClassFieldAttributeIncrementComponent<HashCodeFieldAttribute, HashCodeClassAttribute> {

        public override void fill(TranslationClassFieldAttributeIncrementContext<HashCodeFieldAttribute, HashCodeClassAttribute> context) {
            List<StatementSyntax> list = new List<StatementSyntax>();

            if (context.classFieldAttributeIncrementContext.caAttributeContext?.attribute.hasBase ?? false) {
                list.Add
                (
                    ExpressionStatement
                    (
                        AssignmentExpression
                        (
                            SyntaxKind.SimpleAssignmentExpression,
                            IdentifierName
                            (
                                "h"
                            ),
                            BinaryExpression
                            (
                                SyntaxKind.AddExpression,
                                BinaryExpression
                                (
                                    SyntaxKind.MultiplyExpression,
                                    IdentifierName
                                    (
                                        "h"
                                    ),
                                    LiteralExpression
                                    (
                                        SyntaxKind.NumericLiteralExpression,
                                        Literal
                                        (
                                            23
                                        )
                                    )
                                ),
                                InvocationExpression
                                (
                                    MemberAccessExpression
                                    (
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName
                                        (
                                            "base"
                                        ),
                                        IdentifierName
                                        (
                                            nameof(GetHashCode)
                                        )
                                    )
                                )
                            )
                        )
                    )
                );
            }

            foreach (FieldAttributeIncrementContext<HashCodeFieldAttribute>? se in context.fieldsAttributeContextList) {
                bool isValueType = (context.basicsContext.semanticModel.GetSymbolInfo(se.fieldsAttributeContext.fieldsContext.typeContext.typeSyntax).Symbol as ITypeSymbol)?.IsValueType ?? false;

                list.Add
                (
                    ExpressionStatement
                    (
                        AssignmentExpression
                        (
                            SyntaxKind.SimpleAssignmentExpression,
                            IdentifierName
                            (
                                "h"
                            ),
                            BinaryExpression
                            (
                                SyntaxKind.AddExpression,
                                BinaryExpression
                                (
                                    SyntaxKind.MultiplyExpression,
                                    IdentifierName
                                    (
                                        "h"
                                    ),
                                    LiteralExpression
                                    (
                                        SyntaxKind.NumericLiteralExpression,
                                        Literal
                                        (
                                            23
                                        )
                                    )
                                ),
                                isValueType
                                    ? InvocationExpression
                                    (
                                        MemberAccessExpression
                                        (
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            MemberAccessExpression
                                            (
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                ThisExpression(),
                                                IdentifierName
                                                (
                                                    se.getInvoke
                                                )
                                            ),
                                            IdentifierName
                                            (
                                                nameof(GetHashCode)
                                            )
                                        )
                                    )
                                    : BinaryExpression
                                    (
                                        SyntaxKind.CoalesceExpression,
                                        ConditionalAccessExpression
                                        (
                                            MemberAccessExpression
                                            (
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                ThisExpression(),
                                                IdentifierName
                                                (
                                                    se.getInvoke
                                                )
                                            ),
                                            InvocationExpression
                                            (
                                                MemberBindingExpression
                                                (
                                                    Token
                                                    (
                                                        SyntaxKind.DotToken
                                                    ),
                                                    IdentifierName
                                                    (
                                                        nameof(GetHashCode)
                                                    )
                                                )
                                            )
                                        ),
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
                        )
                    )
                );
            }

            list.Add
            (
                ReturnStatement
                (
                    IdentifierName
                    (
                        "h"
                    )
                )
            );

            context.incrementContext.partialClassMemberDeclarationSyntaxList.Add
            (
                MethodDeclaration
                    (
                        IdentifierName
                        (
                            "int"
                        ),
                        "GetHashCode"
                    )
                    .AddModifiers
                    (
                        Token
                        (
                            SyntaxKind.PublicKeyword
                        ),
                        Token
                        (
                            SyntaxKind.OverrideKeyword
                        )
                    )
                    .AddBodyStatements
                    (
                        Block
                        (
                            CheckedStatement
                            (
                                SyntaxKind.UncheckedStatement,
                                Block()
                                    .AddStatements
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
                                                                "h"
                                                            ),
                                                            null,
                                                            EqualsValueClause
                                                            (
                                                                LiteralExpression
                                                                (
                                                                    SyntaxKind.NumericLiteralExpression,
                                                                    Literal
                                                                    (
                                                                        17
                                                                    )
                                                                )
                                                            )
                                                        )
                                                    }
                                                )
                                            )
                                        )
                                    )
                                    .AddStatements
                                    (
                                        list.ToArray()
                                    )
                            )
                        )
                    )
            );
        }

    }

    [IncrementComponent]
    public sealed class EqualsGenerator : TranslationClassFieldAttributeIncrementComponent<EqualsFieldAttribute, EqualsClassAttribute> {

        public override void fill(TranslationClassFieldAttributeIncrementContext<EqualsFieldAttribute, EqualsClassAttribute> context) {

            IsPatternExpressionSyntax isPatternExpressionSyntax = IsPatternExpression
            (
                ParseTypeName
                (
                    "obj"
                ),
                DeclarationPattern
                (
                    ParseTypeName
                    (
                        context.basicsContext.contextTargetNode.toClassName()
                    ),
                    SingleVariableDesignation
                    (
                        Identifier
                        (
                            "_obj"
                        )
                    )
                )
            );

            List<InvocationExpressionSyntax> list = new List<InvocationExpressionSyntax>();

            if (context.classFieldAttributeIncrementContext.caAttributeContext?.attribute.hasBase ?? false) {
                list.Add
                (
                    InvocationExpression
                    (
                        MemberAccessExpression
                        (
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName
                            (
                                "base"
                            ),
                            IdentifierName
                            (
                                "Equals"
                            )
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
                                            "_obj"
                                        )
                                    )
                                }
                            )
                        )
                    )
                );
            }

            foreach (FieldAttributeIncrementContext<EqualsFieldAttribute>? equal in context.fieldsAttributeContextList) {

                list.Add
                (
                    InvocationExpression
                    (
                        MemberAccessExpression
                        (
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName
                            (
                                "object"
                            ),
                            IdentifierName
                            (
                                "Equals"
                            )
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
                                            equal.getInvoke
                                        )
                                    ),
                                    Argument
                                    (
                                        MemberAccessExpression
                                        (
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            IdentifierName
                                            (
                                                "_obj"
                                            ),
                                            IdentifierName
                                            (
                                                equal.getInvoke
                                            )
                                        )
                                    )
                                }
                            )
                        )
                    )
                );
            }

            ExpressionSyntax expression = null!;

            for (var i = 0; i < list.Count; i++) {
                if (i == 0) {
                    expression = list[0];
                    continue;
                }
                expression = BinaryExpression
                (
                    SyntaxKind.LogicalAndExpression,
                    expression,
                    list[i]
                );
            }

            context.incrementContext.partialClassMemberDeclarationSyntaxList.Add
            (
                MethodDeclaration
                    (
                        IdentifierName
                        (
                            "bool"
                        ),
                        "Equals"
                    )
                    .AddModifiers
                    (
                        Token
                        (
                            SyntaxKind.PublicKeyword
                        ),
                        Token
                        (
                            SyntaxKind.OverrideKeyword
                        )
                    )
                    .AddParameterListParameters
                    (
                        Parameter
                            (
                                Identifier
                                (
                                    "obj"
                                )
                            )
                            .WithType
                            (
                                ParseTypeName
                                (
                                    "object"
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
                                    "obj"
                                ),
                                ThisExpression()
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
                            isPatternExpressionSyntax.WithPattern
                            (
                                UnaryPattern
                                (
                                    Token
                                    (
                                        SyntaxKind.NotKeyword
                                    ),
                                    isPatternExpressionSyntax.Pattern
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
                        ReturnStatement
                        (
                            expression
                        )
                    )
            );

        }

    }

    [IncrementComponent]
    public sealed class ExportGenerator : TranslationClassFieldAttributeIncrementComponent<EqualsFieldAttribute, ExportClassAttribute> {

        public override bool onlyOne() => true;

        public override void fill(TranslationClassFieldAttributeIncrementContext<EqualsFieldAttribute, ExportClassAttribute> context) {

        }

    }

}
