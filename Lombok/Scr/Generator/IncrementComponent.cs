using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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

            if (caAttributeSyntax.Count == 0) {
                return;
            }

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

            foreach (AttributeContext<CA> attributeContext in incrementContext.attributeContextList) {
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

        }

        public abstract void fill(ClassFieldAttributeIncrementContext<FA, CA> classFieldAttributeIncrementContext);

    }

    public abstract class TranslationClassFieldAttributeIncrementComponent<FA, CA> : ClassFieldAttributeIncrementComponent<FA, CA> where FA : IncrementFieldAttribute where CA : IncrementClassAttribute {

        public override void fill(ClassFieldAttributeIncrementContext<FA, CA> classFieldAttributeIncrementContext) {
            List<FieldAttributeIncrementContext<FA>> fieldsAttributeContexts = new List<FieldAttributeIncrementContext<FA>>();
            foreach (FieldsAttributeContext<FA> fieldsAttributeContext in classFieldAttributeIncrementContext.fieldsAttributeContextList) {
                foreach (AttributeContext<FA> attributeContext in fieldsAttributeContext.attributeContext) {

                    string getInvoke = fieldsAttributeContext.fieldsContext.fieldName;
                    Func<string, string> setInvoke = v => fieldsAttributeContext.fieldsContext.fieldName + '=';

                    if (!attributeContext.attribute.directAccess) {
                        getInvoke = "get" + getInvoke.toPascalCaseIdentifier();
                        setInvoke = v => $"set{fieldsAttributeContext.fieldsContext.fieldName.toPascalCaseIdentifier()}({v})";
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
                            attributeContext
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

        public abstract void fill(TranslationClassFieldAttributeIncrementContext<FA, CA> translationClassFieldAttributeIncrementContext);

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

    }

    public class ToStringGenerator : TranslationClassFieldAttributeIncrementComponent<ToStringFieldAttribute, ToStringClassAttribute> {

        public override void fill(TranslationClassFieldAttributeIncrementContext<ToStringFieldAttribute, ToStringClassAttribute> translationClassFieldAttributeIncrementContext) {
            StringBuilder stringBuilder = new StringBuilder();
            CodeBuilder codeBuilder = new CodeBuilder(stringBuilder);

            using (codeBuilder.appendBlock("public static string ToString(string value)")) {

                
                
            }

        }

    }

}

/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Til.Lombok.Generator {

    public abstract class ClassMethodAttributeGeneratorComponent<MA, CA> : GeneratorComponent where CA : ClassAttribute where MA : MethodAttribute {

        protected virtual CA restoreCa(Dictionary<string, string> data) {
            return (CA)Activator.CreateInstance(typeof(CA), data);
        }

        protected virtual MA restoreMa(Dictionary<string, string> data) {
            return (MA)Activator.CreateInstance(typeof(MA), data);
        }

        public override void fill(BasicsContext basicsContext) {
            List<AttributeSyntax> caAttributeSyntax = basicsContext.contextTargetNode.AttributeLists.tryGetSpecifiedAttribute(typeof(CA).Name).ToList();
            List<CA> caList = caAttributeSyntax.Select(a => restoreCa(a.getAttributeArgumentsAsDictionary(basicsContext.semanticModel))).ToList();

            AttributeContext<CA>? caAttributeContext = null;

            if (caList.Count != 0) {
                caAttributeContext = new AttributeContext<CA>
                (
                    caList.First(),
                    caAttributeSyntax.First(),
                    caList,
                    caAttributeSyntax
                );
            }

            List<FieldsAttributeContext<MA>> methodAttributeContextList = new List<FieldsAttributeContext<MA>>();

            if (methodAttributeContextList.Count == 0) {
                return;
            }

            fill(new ClassFieldAttributeContext<MA, CA>(basicsContext, caAttributeContext, methodAttributeContextList));
        }

        public abstract void fill(ClassFieldAttributeContext<MA, CA> context);

    }

    [GeneratorComponent]
    public sealed class ToStringGenerator : ClassFieldAttributeGeneratorComponent<ToStringFieldAttribute, ToStringClassAttribute> {

        public override void fill(ClassFieldAttributeContext<ToStringFieldAttribute, ToStringClassAttribute> context) {

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder
                .Append
                (
                    '$'
                )
                .Append
                (
                    '"'
                )
                .Append
                (
                    context.basicsContext.contextTargetNode.toClassName()
                )
                .Append
                (
                    '('
                )
                .Append
                (
                    string.Join
                    (
                        ",",
                        context.fieldsAttributeContextList.Select
                        (
                            s => $"{s.fieldsContext.fieldName}={{this.{s.fieldsContext.fieldName}}}"
                        )
                    )
                );
            if (context.caAttributeContext?.firstAttribute.hasBase ?? false) {

                stringBuilder.Append
                    (
                        ','
                    )
                    .Append
                    (
                        "base={base.ToString()}"
                    );

            }

            stringBuilder.Append
                (
                    ')'
                )
                .Append
                (
                    '"'
                );

            context.partialClassMemberDeclarationSyntaxList.Add
            (
                MethodDeclaration
                    (
                        IdentifierName
                        (
                            "string"
                        ),
                        "ToString"
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
                    .WithBody
                    (
                        Block
                        (
                            ReturnStatement
                            (
                                IdentifierName
                                (
                                    stringBuilder.ToString()
                                )
                            )
                        )
                    )
            );

        }

    }

    [GeneratorComponent]
    public sealed class EqualsGenerator : ClassFieldAttributeGeneratorComponent<EqualsFieldAttribute, EqualsClassAttribute> {

        public override void fill(ClassFieldAttributeContext<EqualsFieldAttribute, EqualsClassAttribute> context) {

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

            if (context.caAttributeContext?.firstAttribute.hasBase ?? false) {
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

            foreach (FieldsAttributeContext<EqualsFieldAttribute>? equal in context.fieldsAttributeContextList) {

                string fieldName = equal.fieldsContext.fieldName;

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
                                            fieldName
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
                                                fieldName
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

            context.partialClassMemberDeclarationSyntaxList.Add
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

    [GeneratorComponent]
    public sealed class HashCodeGenerator : ClassFieldAttributeGeneratorComponent<HashCodeFieldAttribute, HashCodeClassAttribute> {

        public override void fill(ClassFieldAttributeContext<HashCodeFieldAttribute, HashCodeClassAttribute> context) {
            List<StatementSyntax> list = new List<StatementSyntax>();

            if (context.caAttributeContext?.firstAttribute.hasBase ?? false) {
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

            foreach (FieldsAttributeContext<HashCodeFieldAttribute>? se in context.fieldsAttributeContextList) {
                bool isValueType = (se.basicsContext.semanticModel.GetSymbolInfo(se.fieldsContext.typeContext.typeSyntax).Symbol as ITypeSymbol)?.IsValueType ?? false;
                string fieldName = se.fieldsContext.fieldName;

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
                                                    fieldName
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
                                                    fieldName
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

            context.partialClassMemberDeclarationSyntaxList.Add
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

}
*/
