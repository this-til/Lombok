using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Transactions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Til.Lombok {
    public abstract class GeneratorBasics : IIncrementalGenerator {
        private static readonly string AttributeName = typeof(ILombokAttribute).FullName!;

        public void Initialize(IncrementalGeneratorInitializationContext context) {
            var sources = context.SyntaxProvider.ForAttributeWithMetadataName(
                AttributeName,
                IsCandidate,
                Transform
            );
            context.AddSources(
                sources
            );
        }

        private bool IsCandidate(
            SyntaxNode node,
            CancellationToken cancellationToken) => node is ClassDeclarationSyntax;

        private GeneratorResult Transform(
            GeneratorAttributeSyntaxContext context,
            CancellationToken cancellationToken) {
            ClassDeclarationSyntax contextTargetNode = (ClassDeclarationSyntax)context.TargetNode;

            SemanticModel semanticModel = context.SemanticModel;

            if (!contextTargetNode.TryValidateType(
                    out var @namespace,
                    out var diagnostic
                )) {
                return new GeneratorResult(
                    diagnostic
                );
            }
            List<MethodDeclarationSyntax> methodDeclarationSyntaxes = new List<MethodDeclarationSyntax>();

            // 遍历类的所有成员  
            foreach (var member in contextTargetNode.Members) {
                switch (member) {
                    // 检查成员是否是字段或属性  
                    case FieldDeclarationSyntax fieldDeclaration: {
                        foreach (MethodDeclarationSyntax methodDeclarationSyntax in onFieldDeclarationSyntax(
                                     fieldDeclaration,
                                     semanticModel
                                 )) {
                            try {
                                methodDeclarationSyntaxes.Add(
                                    methodDeclarationSyntax
                                );
                            }
                            catch (Exception e) {
                                Console.WriteLine(
                                    e
                                );
                            }
                        }
                        break;
                    }
                    case PropertyDeclarationSyntax propertyDeclaration: {
                        foreach (MethodDeclarationSyntax methodDeclarationSyntax in onPropertyDeclarationSyntax(
                                     propertyDeclaration,
                                     semanticModel
                                 )) {
                            try {
                                methodDeclarationSyntaxes.Add(
                                    methodDeclarationSyntax
                                );
                            }
                            catch (Exception e) {
                                Console.WriteLine(
                                    e
                                );
                            }
                        }
                        break;
                    }
                }
            }

            if (methodDeclarationSyntaxes.Count == 0) {
                return GeneratorResult.Empty;
            }

            return new GeneratorResult(
                contextTargetNode.GetHintName(
                    @namespace
                ),
                CreateMethodUtil.CreatePartialClass(
                    @namespace,
                    contextTargetNode,
                    methodDeclarationSyntaxes
                )
            );
        }

        public abstract IEnumerable<MethodDeclarationSyntax> onFieldDeclarationSyntax(
            FieldDeclarationSyntax fieldDeclarationSyntax,
            SemanticModel semanticModel);

        public abstract IEnumerable<MethodDeclarationSyntax> onPropertyDeclarationSyntax(
            PropertyDeclarationSyntax propertyDeclarationSyntax,
            SemanticModel semanticModel);
    }


    [Generator]
    public sealed class SpecificationGenerator : IIncrementalGenerator {
        private static readonly string AttributeName = typeof(ILombokAttribute).FullName!;

        public void Initialize(IncrementalGeneratorInitializationContext context) {
            var sources = context.SyntaxProvider.ForAttributeWithMetadataName(
                AttributeName,
                IsCandidate,
                Transform
            );
            context.AddSources(
                sources
            );
        }

        private bool IsCandidate(
            SyntaxNode node,
            CancellationToken cancellationToken) => node is ClassDeclarationSyntax;

        private GeneratorResult Transform(
            GeneratorAttributeSyntaxContext context,
            CancellationToken cancellationToken) {
            ClassDeclarationSyntax contextTargetNode = (ClassDeclarationSyntax)context.TargetNode;

            SemanticModel semanticModel = context.SemanticModel;

            if (!contextTargetNode.TryValidateType(
                    out var @namespace,
                    out var diagnostic
                )) {
                return new GeneratorResult(
                    diagnostic
                );
            }

            List<CSharpSyntaxNode> toString = new List<CSharpSyntaxNode>();
            List<CSharpSyntaxNode> hashCode = new List<CSharpSyntaxNode>();
            List<CSharpSyntaxNode> equals = new List<CSharpSyntaxNode>();

            // 遍历类的所有成员  
            foreach (var member in contextTargetNode.Members) {
                switch (member) {
                    // 检查成员是否是字段或属性  
                    case FieldDeclarationSyntax fieldDeclaration: {
                        AttributeSyntax? tryGetSpecifiedAttribute = fieldDeclaration.AttributeLists.tryGetSpecifiedAttribute(
                            nameof(ToStringFieldAttribute)
                        );
                        if (tryGetSpecifiedAttribute is not null) {
                            foreach (VariableDeclaratorSyntax variableDeclaratorSyntax in fieldDeclaration.Declaration.Variables) {
                                toString.Add(
                                    variableDeclaratorSyntax
                                );
                            }
                        }
                        tryGetSpecifiedAttribute = fieldDeclaration.AttributeLists.tryGetSpecifiedAttribute(
                            nameof(HashCodeFieldAttribute)
                        );
                        if (tryGetSpecifiedAttribute is not null) {
                            foreach (VariableDeclaratorSyntax variableDeclaratorSyntax in fieldDeclaration.Declaration.Variables) {
                                hashCode.Add(
                                    variableDeclaratorSyntax
                                );
                            }
                        }
                        tryGetSpecifiedAttribute = fieldDeclaration.AttributeLists.tryGetSpecifiedAttribute(
                            nameof(EqualsFieldAttribute)
                        );
                        if (tryGetSpecifiedAttribute is not null) {
                            foreach (VariableDeclaratorSyntax variableDeclaratorSyntax in fieldDeclaration.Declaration.Variables) {
                                equals.Add(
                                    variableDeclaratorSyntax
                                );
                            }
                        }
                        break;
                    }
                    case PropertyDeclarationSyntax propertyDeclaration: {
                        AttributeSyntax? tryGetSpecifiedAttribute = propertyDeclaration.AttributeLists.tryGetSpecifiedAttribute(
                            nameof(ToStringFieldAttribute)
                        );
                        if (tryGetSpecifiedAttribute is not null) {
                            toString.Add(
                                propertyDeclaration
                            );
                        }
                        tryGetSpecifiedAttribute = propertyDeclaration.AttributeLists.tryGetSpecifiedAttribute(
                            nameof(HashCodeFieldAttribute)
                        );
                        if (tryGetSpecifiedAttribute is not null) {
                            toString.Add(
                                propertyDeclaration
                            );
                        }
                        tryGetSpecifiedAttribute = propertyDeclaration.AttributeLists.tryGetSpecifiedAttribute(
                            nameof(EqualsFieldAttribute)
                        );
                        if (tryGetSpecifiedAttribute is not null) {
                            toString.Add(
                                propertyDeclaration
                            );
                        }
                        break;
                    }
                }
            }

            if (toString.Count == 0 && hashCode.Count == 0 && equals.Count == 0) {
                return GeneratorResult.Empty;
            }

            List<MethodDeclarationSyntax> methodDeclarationSyntaxes = new List<MethodDeclarationSyntax>();

            if (toString.Count != 0) {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder
                    .Append(
                        '$'
                    )
                    .Append(
                        '"'
                    )
                    .Append(
                        contextTargetNode.toClassName()
                    )
                    .Append(
                        '('
                    )
                    .Append(
                        string.Join(
                            ",",
                            toString.Select(
                                s => $"{s}={{this.{s}}}"
                            )
                        )
                    )
                    .Append(
                        ','
                    )
                    .Append(
                        "base={base.ToString()}"
                    )
                    .Append(
                        ')'
                    )
                    .Append(
                        '"'
                    );

                methodDeclarationSyntaxes.Add(
                    MethodDeclaration(
                            IdentifierName(
                                "string"
                            ),
                            "ToString"
                        )
                        .AddModifiers(
                            Token(
                                SyntaxKind.PublicKeyword
                            ),
                            Token(
                                SyntaxKind.OverrideKeyword
                            )
                        )
                        .WithBody(
                            Block(
                                ReturnStatement(
                                    IdentifierName(
                                        stringBuilder.ToString()
                                    )
                                )
                            )
                        )
                );
            }
            if (hashCode.Count != 0) {
                List<StatementSyntax> list = new List<StatementSyntax>();

                list.Add(
                    LocalDeclarationStatement(
                        VariableDeclaration(
                            PredefinedType(
                                Token(
                                    SyntaxKind.IntKeyword
                                )
                            ),
                            SeparatedList(
                                new[] {
                                    VariableDeclarator(
                                        Identifier(
                                            "h"
                                        ),
                                        null,
                                        EqualsValueClause(
                                            LiteralExpression(
                                                SyntaxKind.NumericLiteralExpression,
                                                Literal(
                                                    17
                                                )
                                            )
                                        )
                                    )
                                }
                            )
                        )
                    )
                );

                foreach (var se in hashCode) {
                    bool isValueType = se is VariableDeclaratorSyntax variableDeclaratorSyntax
                        ? (semanticModel.GetSymbolInfo(
                                  ((VariableDeclarationSyntax)variableDeclaratorSyntax.Parent!).Type
                              )
                              .Symbol as ITypeSymbol)?.IsValueType
                          ?? false
                        : (semanticModel.GetSymbolInfo(
                                  ((PropertyDeclarationSyntax)se).Type
                              )
                              .Symbol as ITypeSymbol)?.IsValueType
                          ?? false;

                    list.Add(
                        ExpressionStatement(
                            AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                IdentifierName(
                                    "h"
                                ),
                                BinaryExpression(
                                    SyntaxKind.AddExpression,
                                    BinaryExpression(
                                        SyntaxKind.MultiplyExpression,
                                        IdentifierName(
                                            "h"
                                        ),
                                        LiteralExpression(
                                            SyntaxKind.NumericLiteralExpression,
                                            Literal(
                                                23
                                            )
                                        )
                                    ),
                                    isValueType
                                        ? InvocationExpression(
                                            MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    ThisExpression(),
                                                    IdentifierName(
                                                        se.ToString()
                                                    )
                                                ),
                                                IdentifierName(
                                                    nameof(GetHashCode)
                                                )
                                            )
                                        )
                                        : BinaryExpression(
                                            SyntaxKind.CoalesceExpression,
                                            ConditionalAccessExpression(
                                                MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    ThisExpression(),
                                                    IdentifierName(
                                                        se.ToString()
                                                    )
                                                ),
                                                InvocationExpression(
                                                    MemberBindingExpression(
                                                        Token(
                                                            SyntaxKind.DotToken
                                                        ),
                                                        IdentifierName(
                                                            nameof(GetHashCode)
                                                        )
                                                    )
                                                )
                                            ),
                                            LiteralExpression(
                                                SyntaxKind.NumericLiteralExpression,
                                                Literal(
                                                    0
                                                )
                                            )
                                        )
                                )
                            )
                        )
                    );
                }

                list.Add(
                    ReturnStatement(
                        IdentifierName(
                            "h"
                        )
                    )
                );

                methodDeclarationSyntaxes.Add(
                    MethodDeclaration(
                            IdentifierName(
                                "int"
                            ),
                            "GetHashCode"
                        )
                        .AddModifiers(
                            Token(
                                SyntaxKind.PublicKeyword
                            ),
                            Token(
                                SyntaxKind.OverrideKeyword
                            )
                        )
                        .WithBody(
                            Block(
                                CheckedStatement(
                                    SyntaxKind.UncheckedStatement,
                                    Block(
                                        list
                                    )
                                )
                            )
                        )
                );
            }
            if (equals.Count != 0) {
                IsPatternExpressionSyntax isPatternExpressionSyntax = IsPatternExpression(
                    ParseTypeName(
                        "obj"
                    ),
                    DeclarationPattern(
                        ParseTypeName(
                            contextTargetNode.toClassName()
                        ),
                        SingleVariableDesignation(
                            Identifier(
                                "_obj"
                            )
                        )
                    )
                );
                List<InvocationExpressionSyntax> list = new List<InvocationExpressionSyntax>();

                foreach (var equal in equals) {
                    list.Add(
                        InvocationExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName(
                                    "object"
                                ),
                                IdentifierName(
                                    "Equals"
                                )
                            ),
                            ArgumentList(
                                SeparatedList(
                                    new[] {
                                        Argument(
                                            IdentifierName(
                                                equal.ToString()
                                            )
                                        ),
                                        Argument(
                                            MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                IdentifierName(
                                                    "_obj"
                                                ),
                                                IdentifierName(
                                                    equal.ToString()
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
                    expression = BinaryExpression(
                        SyntaxKind.LogicalAndExpression,
                        expression,
                        list[i]
                    );
                }

                methodDeclarationSyntaxes.Add(
                    MethodDeclaration(
                            IdentifierName(
                                "bool"
                            ),
                            "Equals"
                        )
                        .AddModifiers(
                            Token(
                                SyntaxKind.PublicKeyword
                            ),
                            Token(
                                SyntaxKind.OverrideKeyword
                            )
                        )
                        .AddParameterListParameters(
                            Parameter(
                                    Identifier(
                                        "obj"
                                    )
                                )
                                .WithType(
                                    ParseTypeName(
                                        "object"
                                    )
                                )
                        )
                        .WithBody(
                            Block(
                                IfStatement(
                                    BinaryExpression(
                                        SyntaxKind.EqualsExpression,
                                        IdentifierName(
                                            "obj"
                                        ),
                                        ThisExpression()
                                    ),
                                    Block(
                                        ReturnStatement(
                                            LiteralExpression(
                                                SyntaxKind.TrueLiteralExpression
                                            )
                                        )
                                    )
                                ),
                                IfStatement(
                                    isPatternExpressionSyntax.WithPattern(
                                        UnaryPattern(
                                            Token(
                                                SyntaxKind.NotKeyword
                                            ),
                                            isPatternExpressionSyntax.Pattern
                                        )
                                    ),
                                    Block(
                                        ReturnStatement(
                                            LiteralExpression(
                                                SyntaxKind.FalseLiteralExpression
                                            )
                                        )
                                    )
                                ),
                                ReturnStatement(
                                    expression
                                )
                            )
                        )
                );
            }

            return new GeneratorResult(
                contextTargetNode.GetHintName(
                    @namespace
                ),
                CreateMethodUtil.CreatePartialClass(
                    @namespace,
                    contextTargetNode,
                    methodDeclarationSyntaxes
                )
            );
        }
    }

    [Generator]
    public sealed class FreezeGenerator : IIncrementalGenerator {
        private static readonly string AttributeName = typeof(IFreezeAttribute).FullName!;

        public void Initialize(IncrementalGeneratorInitializationContext context) {
            var sources = context.SyntaxProvider.ForAttributeWithMetadataName(
                AttributeName,
                IsCandidate,
                Transform
            );
            context.AddSources(
                sources
            );
        }

        private bool IsCandidate(
            SyntaxNode node,
            CancellationToken cancellationToken) => node is ClassDeclarationSyntax;

        private GeneratorResult Transform(
            GeneratorAttributeSyntaxContext context,
            CancellationToken cancellationToken) {
            ClassDeclarationSyntax contextTargetNode = (ClassDeclarationSyntax)context.TargetNode;

            if (!contextTargetNode.TryValidateType(
                    out var @namespace,
                    out var diagnostic
                )) {
                return new GeneratorResult(
                    diagnostic
                );
            }

            string model =
                $@"using System.Collections.Generic;
using Til.Lombok;
using System;

namespace {@namespace.ToString()} {'{'}

    public partial class {contextTargetNode.toClassName()} : IFreeze {'{'}
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
                throw new InvalidOperationException(""Cannot modify frozen property"");
            {'}'}
        {'}'}
    {'}'}
{'}'}";

            return new GeneratorResult(
                contextTargetNode.GetHintName(
                    @namespace
                ),
                SourceText.From(
                    model,
                    Encoding.UTF8
                )
            );
        }
    }

    [Generator]
    public class IPackGenerator : IIncrementalGenerator {
        private static readonly string AttributeName = typeof(IPackAttribute).FullName!;

        public void Initialize(IncrementalGeneratorInitializationContext context) {
            var sources = context.SyntaxProvider.ForAttributeWithMetadataName(
                AttributeName,
                IsCandidate,
                Transform
            );

            context.AddSources(
                sources
            );
        }

        private bool IsCandidate(
            SyntaxNode node,
            CancellationToken cancellationToken) => node is ClassDeclarationSyntax;

        private GeneratorResult Transform(
            GeneratorAttributeSyntaxContext context,
            CancellationToken cancellationToken) {
            ClassDeclarationSyntax contextTargetNode = (ClassDeclarationSyntax)context.TargetNode;

            SemanticModel semanticModel = context.SemanticModel;

            if (!contextTargetNode.TryValidateType(
                    out var @namespace,
                    out var diagnostic
                )) {
                return new GeneratorResult(
                    diagnostic!
                );
            }

            string model =
                $@"using System;
using System.Collections.Generic; 
using Til.Lombok;

namespace {@namespace!.ToString()} {'{'}

    public partial class {contextTargetNode.toClassName()} : IPack {'{'}
        protected IDictionary<string, object>? _pack;
        
        public IDictionary<string, object> pack() => pack(false);
        
        public IDictionary<string, object> pack(bool updates = false) {'{'}
            if (updates) {'{'}
                _pack = null;
            {'}'}
            _pack ??= Util.pack(this);
            return _pack!;
        {'}'}
    {'}'}
{'}'}";

            return new GeneratorResult(
                contextTargetNode.GetHintName(
                    @namespace
                ),
                SourceText.From(
                    model,
                    Encoding.UTF8
                )
            );
        }
    }

    [Generator]
    public class SelfGenerator : IIncrementalGenerator {
        private static readonly string AttributeName = typeof(ISelfAttribute).FullName!;

        public void Initialize(IncrementalGeneratorInitializationContext context) {
            var sources = context.SyntaxProvider.ForAttributeWithMetadataName(
                AttributeName,
                IsCandidate,
                Transform
            );

            context.AddSources(
                sources
            );
        }

        private bool IsCandidate(
            SyntaxNode node,
            CancellationToken cancellationToken) => node is ClassDeclarationSyntax;

        private GeneratorResult Transform(
            GeneratorAttributeSyntaxContext context,
            CancellationToken cancellationToken) {
            ClassDeclarationSyntax contextTargetNode = (ClassDeclarationSyntax)context.TargetNode;

            SemanticModel semanticModel = context.SemanticModel;

            if (!contextTargetNode.TryValidateType(
                    out var @namespace,
                    out var diagnostic
                )) {
                return new GeneratorResult(
                    diagnostic!
                );
            }

            AttributeSyntax? tryGetSpecifiedAttribute = contextTargetNode.AttributeLists.tryGetSpecifiedAttribute(
                nameof(ISelfAttribute)
            );
            if (tryGetSpecifiedAttribute is null) {
                return GeneratorResult.Empty;
            }

            ISelfAttribute selfAttribute = ISelfAttribute.of(
                tryGetSpecifiedAttribute.getAttributeArgumentsAsDictionary(
                    semanticModel
                )!
            );
            string instantiation = selfAttribute.instantiation is not null
                ? String.Format(
                    selfAttribute.instantiation,
                    contextTargetNode.toClassName()
                )
                : $"new {contextTargetNode.toClassName()}()";

            string model =
                $@"using System;
using Til.Lombok;

namespace {@namespace!.ToString()} {'{'}

    public partial class {contextTargetNode.toClassName()} {'{'}
        protected static {contextTargetNode.toClassName()} instance;
    
        public static {contextTargetNode.toClassName()} getInstance() {'{'}
            instance ??= ({contextTargetNode.toClassName()}) {instantiation}!;
            return instance;
        {'}'}
    {'}'}
{'}'}";

            return new GeneratorResult(
                contextTargetNode.GetHintName(
                    @namespace
                ),
                SourceText.From(
                    model,
                    Encoding.UTF8
                )
            );
        }
    }

    [Generator]
    public sealed class PartialGenerator : IIncrementalGenerator {
        private static readonly string AttributeName = typeof(IPartialAttribute).FullName!;

        public void Initialize(IncrementalGeneratorInitializationContext context) {
            var sources = context.SyntaxProvider.ForAttributeWithMetadataName(
                AttributeName,
                IsCandidate,
                Transform
            );
            context.AddSources(
                sources
            );
        }

        private bool IsCandidate(
            SyntaxNode node,
            CancellationToken cancellationToken) => node is ClassDeclarationSyntax;

        private GeneratorResult Transform(
            GeneratorAttributeSyntaxContext context,
            CancellationToken cancellationToken) {
            ClassDeclarationSyntax contextTargetNode = (ClassDeclarationSyntax)context.TargetNode;

            SemanticModel semanticModel = context.SemanticModel;

            if (!contextTargetNode.TryValidateType(
                    out var @namespace,
                    out var diagnostic
                )) {
                return new GeneratorResult(
                    diagnostic!
                );
            }

            string attributeName = nameof(IPartialAttribute);
            if (attributeName.EndsWith(
                    "Attribute"
                )) {
                attributeName = attributeName.Substring(
                    0,
                    attributeName.Length - 9
                );
            }

            StringBuilder model = new StringBuilder();

            Dictionary<string, string> fill = new Dictionary<string, string>() {
                { "type", contextTargetNode.toClassName() },
                { "namespace", @namespace!.ToString() }
            };

            foreach (AttributeListSyntax attributeList in contextTargetNode.AttributeLists) {
                foreach (AttributeSyntax attribute in attributeList.Attributes) {
                    if (attributeName.Equals(
                            attribute.Name.ToString()
                        )
                        || attributeName.Equals(
                            attribute.Name.ToString()
                        )) {
                        IPartialAttribute partialAttribute = IPartialAttribute.of(
                            attribute.getAttributeArgumentsAsDictionary(
                                semanticModel
                            )!
                        );
                        if (string.IsNullOrEmpty(
                                partialAttribute.model
                            )) {
                            continue;
                        }
                        partialAttribute.model!.format(
                            model,
                            k => model.Append(
                                fill[k]
                            )
                        );
                        model.Append(
                            '\n'
                        );
                    }
                }
            }

            return new GeneratorResult(
                contextTargetNode.GetHintName(
                    @namespace
                ),
                SourceText.From(
                    model.ToString(),
                    Encoding.UTF8
                )
            );
        }
    }

    public abstract class AttributeGenerator : GeneratorBasics {
        public abstract string getAttributeName();

        public abstract IEnumerable<MethodDeclarationSyntax> onFieldDeclarationSyntax(
            FieldDeclarationSyntax fieldDeclarationSyntax,
            AttributeSyntax attributeSyntax,
            Dictionary<string, object> data);

        public abstract IEnumerable<MethodDeclarationSyntax> onPropertyDeclarationSyntax(
            PropertyDeclarationSyntax propertyDeclarationSyntax,
            AttributeSyntax attributeSyntax,
            Dictionary<string, object> data);

        public override IEnumerable<MethodDeclarationSyntax> onFieldDeclarationSyntax(
            FieldDeclarationSyntax fieldDeclarationSyntax,
            SemanticModel semanticModel) {
            AttributeSyntax? tryGetSpecifiedAttribute = fieldDeclarationSyntax.AttributeLists.tryGetSpecifiedAttribute(
                getAttributeName()
            );

            if (tryGetSpecifiedAttribute is null) {
                return Array.Empty<MethodDeclarationSyntax>();
            }
            return onFieldDeclarationSyntax(
                fieldDeclarationSyntax,
                tryGetSpecifiedAttribute,
                tryGetSpecifiedAttribute.getAttributeArgumentsAsDictionary(
                    semanticModel
                )
            );
        }

        public override IEnumerable<MethodDeclarationSyntax> onPropertyDeclarationSyntax(
            PropertyDeclarationSyntax propertyDeclarationSyntax,
            SemanticModel semanticModel) {
            AttributeSyntax? tryGetSpecifiedAttribute = propertyDeclarationSyntax.AttributeLists.tryGetSpecifiedAttribute(
                getAttributeName()
            );

            if (tryGetSpecifiedAttribute is null) {
                return Array.Empty<MethodDeclarationSyntax>();
            }

            return onPropertyDeclarationSyntax(
                propertyDeclarationSyntax,
                tryGetSpecifiedAttribute,
                tryGetSpecifiedAttribute.getAttributeArgumentsAsDictionary(
                    semanticModel
                )
            );
        }
    }

    public abstract class StandardAttributeGenerator<A> : AttributeGenerator {
        public abstract Func<string, string, string, A, MethodDeclarationSyntax> createMethodDeclarationSyntax();

        public abstract Func<Dictionary<string, object>, ClassDeclarationSyntax, TypeSyntax, A> of();

        public override IEnumerable<MethodDeclarationSyntax> onFieldDeclarationSyntax(
            FieldDeclarationSyntax fieldDeclarationSyntax,
            AttributeSyntax attributeSyntax,
            Dictionary<string, object> data) {
            A attribute = of()(
                data,
                ((ClassDeclarationSyntax)fieldDeclarationSyntax.Parent),
                fieldDeclarationSyntax.Declaration.Type
            );
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

        public override IEnumerable<MethodDeclarationSyntax> onPropertyDeclarationSyntax(
            PropertyDeclarationSyntax propertyDeclarationSyntax,
            AttributeSyntax attributeSyntax,
            Dictionary<string, object> data) {
            A attribute = of()(
                data,
                ((ClassDeclarationSyntax)propertyDeclarationSyntax.Parent),
                propertyDeclarationSyntax.Type
            );
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
        public override Func<Dictionary<string, object>, ClassDeclarationSyntax, TypeSyntax, MetadataAttribute> of() => (
            d,
            c,
            t) => MetadataAttribute.of(
            d
        );
    }

    public abstract class ListAttributeGenerator : StandardAttributeGenerator<ListMetadataAttribute> {
        public override Func<Dictionary<string, object>, ClassDeclarationSyntax, TypeSyntax, ListMetadataAttribute> of() => (
            d,
            c,
            t) => {
            ListMetadataAttribute listMetadataAttribute = ListMetadataAttribute.of(
                d
            );
            if (string.IsNullOrEmpty(
                    listMetadataAttribute.type
                )
                && t is GenericNameSyntax genericNameSyntax) {
                TypeSyntax? firstOrDefault = genericNameSyntax.TypeArgumentList.Arguments.FirstOrDefault();
                listMetadataAttribute.type = firstOrDefault?.ToFullString();
            }
            if (string.IsNullOrEmpty(
                    listMetadataAttribute.type
                )) {
                return null;
            }
            return listMetadataAttribute;
        };
    }

    public abstract class MapAttributeGenerator : StandardAttributeGenerator<MapMetadataAttribute> {
        public override Func<Dictionary<string, object>, ClassDeclarationSyntax, TypeSyntax, MapMetadataAttribute> of() => (
            d,
            c,
            t) => {
            MapMetadataAttribute mapMetadataAttribute = MapMetadataAttribute.of(
                d
            );
            if (t is GenericNameSyntax genericNameSyntax) {
                if (string.IsNullOrEmpty(
                        mapMetadataAttribute.keyType
                    )
                    && genericNameSyntax.TypeArgumentList.Arguments.Count > 0) {
                    TypeSyntax? firstOrDefault = genericNameSyntax.TypeArgumentList.Arguments.FirstOrDefault();
                    mapMetadataAttribute.keyType = firstOrDefault?.ToFullString();
                }
                if (string.IsNullOrEmpty(
                        mapMetadataAttribute.valueType
                    )
                    && genericNameSyntax.TypeArgumentList.Arguments.Count > 1) {
                    TypeSyntax? firstOrDefault = genericNameSyntax.TypeArgumentList.Arguments[1];
                    mapMetadataAttribute.valueType = firstOrDefault?.ToFullString();
                }
            }
            if (string.IsNullOrEmpty(
                    mapMetadataAttribute.keyType
                )
                || string.IsNullOrEmpty(
                    mapMetadataAttribute.valueType
                )) {
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
    public sealed class IsGenerator : MetadataAttributeGenerator {
        public override string getAttributeName() => nameof(IsAttribute);

        public override Func<string, string, string, MetadataAttribute, MethodDeclarationSyntax> createMethodDeclarationSyntax() => CreateMethodUtil.CreateIsMethod;
    }

    [Generator]
    public sealed class OpenGenerator : MetadataAttributeGenerator {
        public override string getAttributeName() => nameof(OpenAttribute);

        public override Func<string, string, string, MetadataAttribute, MethodDeclarationSyntax> createMethodDeclarationSyntax() => CreateMethodUtil.CreateOpenMethod;
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
        public static SourceText CreatePartialClass(
            NameSyntax @namespace,
            ClassDeclarationSyntax classDeclaration,
            IEnumerable<MethodDeclarationSyntax> methods) {
            return SourceText.From(
                CompilationUnit()
                    .WithUsings(
                        classDeclaration.GetUsings()
                    )
                    .AddMembers(
                        NamespaceDeclaration(
                                @namespace
                            )
                            .AddMembers(
                                classDeclaration
                                    .CreateNewPartialClass()
                                    .WithMembers(
                                        List<MemberDeclarationSyntax>(
                                            methods
                                        )
                                    )
                            )
                    )
                    .NormalizeWhitespace()
                    .ToFullString(),
                Encoding.UTF8
            );
            /*@namespace.CreateNewNamespace(classDeclaration.GetUsings(),
                    classDeclaration.CreateNewPartialClass()
                        .WithMembers(
                            List<MemberDeclarationSyntax>(methods)
                        )
                ).NormalizeWhitespace()
                .GetText(Encoding.UTF8);

            context.AddSource("R.g.cs", SourceText.From(
                CompilationUnit()
                    .AddUsings(namespaceList.Select(x => UsingDirective(ParseName(x))).ToArray())
                    .AddMembers(
                        NamespaceDeclaration(ParseName(compilationAssemblyName!))
                            .AddMembers(
                                ClassDeclaration("R")
                                    .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword))
                                    .AddMembers(memberDeclarationSyntaxList.ToArray())
                            )
                    )
                    .NormalizeWhitespace().ToFullString(), Encoding.UTF8));*/
        }

        public static StatementSyntax validateNonFrozen(string freezeTag) {
            return
                string.IsNullOrEmpty(
                    freezeTag
                )
                    ? null!
                    : ExpressionStatement(
                        InvocationExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression, // 使用点号访问  ,
                                ThisExpression(),
                                IdentifierName(
                                    "validateNonFrozen"
                                )
                            ),
                            ArgumentList(
                                SingletonSeparatedList(
                                    Argument(
                                        LiteralExpression(
                                            SyntaxKind.StringLiteralExpression,
                                            Literal(
                                                freezeTag
                                            )
                                        )
                                    )
                                )
                            )
                        )
                    );
        }

        public static StatementSyntax validateUpdateField(string fieldName) {
            return IfStatement(
                InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(
                            "object"
                        ),
                        IdentifierName(
                            nameof(Equals)
                        )
                    ),
                    ArgumentList(
                        SeparatedList(
                            new[] {
                                Argument(
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        ThisExpression(),
                                        IdentifierName(
                                            fieldName
                                        )
                                    )
                                ),
                                Argument(
                                    IdentifierName(
                                        fieldName
                                    )
                                ),
                            }
                        )
                    )
                ),
                Block(
                    ExpressionStatement(
                        InvocationExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression, // 使用点号访问  ,
                                ThisExpression(),
                                IdentifierName(
                                    "update" + fieldName.ToPascalCaseIdentifier()
                                )
                            ),
                            ArgumentList(
                                SeparatedList(
                                    new[] {
                                        Argument(
                                            MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                ThisExpression(),
                                                IdentifierName(
                                                    fieldName
                                                )
                                            )
                                        ),
                                        Argument(
                                            IdentifierName(
                                                fieldName
                                            )
                                        ),
                                    }
                                )
                            )
                        )
                    )
                )
            );
        }

        public static StatementSyntax updateField(string fieldName) {
            return ExpressionStatement(
                InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression, // 使用点号访问  ,
                        ThisExpression(),
                        IdentifierName(
                            "update" + fieldName.ToPascalCaseIdentifier()
                        )
                    )
                )
            );
        }

        public static StatementSyntax noNull(
            string fieldName,
            string? message = null) {
            return ExpressionStatement(
                InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName(
                                    nameof(Til)
                                ),
                                IdentifierName(
                                    nameof(Lombok)
                                )
                            ),
                            IdentifierName(
                                nameof(Util)
                            )
                        ),
                        IdentifierName(
                            nameof(Util.noNull)
                        ) // 方法名  
                    ),
                    ArgumentList( // 创建参数列表  
                        SeparatedList(
                            new[] {
                                Argument( // 创建一个参数表达式  
                                    IdentifierName(
                                        fieldName
                                    ) // 引用参数i  
                                ),
                                message is not null
                                    ? Argument( // 创建一个参数表达式  
                                        IdentifierName(
                                            $"\"{message}\""
                                        ) // 引用参数i  
                                    )
                                    : null!
                            }.Where(
                                v => v is not null
                            )
                        )
                    )
                )
            );
        }

        public static MethodDeclarationSyntax CreateGetMethod(
            string fieldName,
            string typeName,
            string parentName,
            MetadataAttribute metadataAttribute) {
            return MethodDeclaration(
                    IdentifierName(
                        typeName
                    ),
                    "get" + fieldName.ToPascalCaseIdentifier()
                )
                .AddModifiers(
                    Token(
                        SyntaxKind.PublicKeyword
                    )
                )
                .WithBody(
                    Block(
                        new List<StatementSyntax>() {
                                validateNonFrozen(
                                    metadataAttribute.freezeTag
                                )!,

                                ReturnStatement(
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        ThisExpression(),
                                        IdentifierName(
                                            fieldName
                                        )
                                    )
                                )
                            }
                            .Where(
                                v => v is not null
                            )
                            .ToList()
                    )
                );
        }

        public static MethodDeclarationSyntax CreateIsMethod(
            string fieldName,
            string typeName,
            string parentName,
            MetadataAttribute metadataAttribute) {
            return MethodDeclaration(
                    IdentifierName(
                        typeName
                    ),
                    "is" + fieldName.ToPascalCaseIdentifier()
                )
                .AddModifiers(
                    Token(
                        SyntaxKind.PublicKeyword
                    )
                )
                .WithBody(
                    Block(
                        new List<StatementSyntax>() {
                                validateNonFrozen(
                                    metadataAttribute.freezeTag
                                )!,

                                ReturnStatement(
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        ThisExpression(),
                                        IdentifierName(
                                            fieldName
                                        )
                                    )
                                )
                            }
                            .Where(
                                v => v is not null
                            )
                            .ToList()
                    )
                );
        }

        public static MethodDeclarationSyntax CreateOpenMethod(
            string fieldName,
            string typeName,
            string parentName,
            MetadataAttribute metadataAttribute) {
            return MethodDeclaration(
                    metadataAttribute.link
                        ? IdentifierName(
                            parentName
                        )
                        : IdentifierName(
                            "void"
                        ),
                    "open" + fieldName.ToPascalCaseIdentifier()
                )
                .AddModifiers(
                    Token(
                        SyntaxKind.PublicKeyword
                    )
                )
                .AddParameterListParameters(
                    Parameter(
                            Identifier(
                                "action"
                            )
                        )
                        .WithType(
                            ParseTypeName(
                                $"Action<{typeName}>"
                            )
                        )
                )
                .WithBody(
                    Block(
                        new List<StatementSyntax>() {
                                validateNonFrozen(
                                    metadataAttribute.freezeTag
                                )!,
                                ExpressionStatement(
                                    InvocationExpression(
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression, // 使用点号访问  ,
                                            IdentifierName(
                                                "action"
                                            ),
                                            IdentifierName(
                                                "Invoke"
                                            )
                                        ),
                                        ArgumentList( // 创建参数列表  
                                            SingletonSeparatedList( // 单个参数列表  
                                                Argument( // 创建一个参数表达式  
                                                    MemberAccessExpression(
                                                        SyntaxKind.SimpleMemberAccessExpression, // 使用点号访问  ,
                                                        ThisExpression(),
                                                        IdentifierName(
                                                            fieldName
                                                        )
                                                    )
                                                )
                                            )
                                        )
                                    )
                                ),
                                metadataAttribute.link
                                    ? ReturnStatement(
                                        ThisExpression()
                                    )
                                    : null!

                                /*ReturnStatement(
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        ThisExpression(),
                                        IdentifierName(fieldName)
                                    )
                                )*/
                            }
                            .Where(
                                v => v is not null
                            )
                            .ToList()
                    )
                );
        }

        public static MethodDeclarationSyntax CreateSetMethod(
            string fieldName,
            string typeName,
            string parentName,
            MetadataAttribute metadataAttribute) {
            return MethodDeclaration(
                    metadataAttribute.link
                        ? IdentifierName(
                            parentName
                        )
                        : IdentifierName(
                            "void"
                        ),
                    "set" + fieldName.ToPascalCaseIdentifier()
                )
                .AddModifiers(
                    Token(
                        SyntaxKind.PublicKeyword
                    )
                )
                .AddParameterListParameters(
                    Parameter(
                            Identifier(
                                fieldName
                                    .ToCamelCaseIdentifier()
                                    .genericEliminate()
                            )
                        )
                        .WithType(
                            ParseTypeName(
                                typeName
                            )
                        )
                )
                .WithBody(
                    Block(
                        new[] {
                            validateNonFrozen(
                                metadataAttribute.freezeTag
                            ),
                            metadataAttribute.noNull
                                ? noNull(
                                    fieldName
                                        .ToCamelCaseIdentifier()
                                        .genericEliminate(),
                                    $"{parentName}.{"set" + fieldName.ToPascalCaseIdentifier()}方法中传入参数为null"
                                )
                                : null!,
                            metadataAttribute.updateField
                                ? validateUpdateField(
                                    fieldName
                                )
                                : null!,
                            ExpressionStatement(
                                AssignmentExpression(
                                    SyntaxKind.SimpleAssignmentExpression,
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        ThisExpression(),
                                        IdentifierName(
                                            fieldName
                                        )
                                    ),
                                    IdentifierName(
                                        fieldName
                                            .ToCamelCaseIdentifier()
                                            .genericEliminate()
                                    )
                                )
                            ),
                            metadataAttribute.link
                                ? ReturnStatement(
                                    ThisExpression()
                                )
                                : null!
                        }.Where(
                            v => v is not null
                        )
                    )
                );
        }

        public static MethodDeclarationSyntax CreateGetAtMethod(
            string fieldName,
            string typeName,
            string parentName,
            ListMetadataAttribute listMetadataAttribute) {
            return MethodDeclaration(
                    IdentifierName(
                        listMetadataAttribute.type
                    ),
                    $"indexIn{fieldName.ToPascalCaseIdentifier()}"
                )
                .AddModifiers(
                    Token(
                        SyntaxKind.PublicKeyword
                    )
                )
                .AddParameterListParameters(
                    Parameter(
                            Identifier(
                                "i"
                            )
                        )
                        .WithType(
                            ParseTypeName(
                                "int"
                            )
                        )
                )
                .WithBody(
                    Block(
                        new StatementSyntax[] {
                            validateNonFrozen(
                                listMetadataAttribute.freezeTag
                            )!,
                            ReturnStatement( // 创建一个 return 语句  
                                ElementAccessExpression( // 创建一个数组或列表的索引访问表达式  
                                    IdentifierName(
                                        fieldName
                                    ),
                                    BracketedArgumentList( // 索引参数列表  
                                        SingletonSeparatedList( // 单个参数列表  
                                            Argument( // 创建一个参数表达式  
                                                IdentifierName(
                                                    "i"
                                                ) // 引用参数 i  
                                            )
                                        )
                                    )
                                )
                            ),
                        }.Where(
                            i => i is not null
                        )
                    )
                );
        }

        public static MethodDeclarationSyntax CreateAddMethod(
            string fieldName,
            string typeName,
            string parentName,
            ListMetadataAttribute listMetadataAttribute) {
            return MethodDeclaration(
                    listMetadataAttribute.link
                        ? IdentifierName(
                            parentName
                        )
                        : IdentifierName(
                            "void"
                        ),
                    $"addIn{fieldName.ToPascalCaseIdentifier().genericEliminate()}"
                )
                .AddModifiers(
                    Token(
                        SyntaxKind.PublicKeyword
                    )
                )
                .AddParameterListParameters(
                    Parameter(
                            Identifier(
                                "a"
                                + listMetadataAttribute
                                    .type.ToPascalCaseIdentifier()
                                    .genericEliminate()
                            )
                        )
                        .WithType(
                            ParseTypeName(
                                listMetadataAttribute.type
                            )
                        )
                )
                .WithBody(
                    Block(
                        new[] {
                            validateNonFrozen(
                                listMetadataAttribute.freezeTag
                            )!,
                            listMetadataAttribute.noNull
                                ? noNull(
                                    "a"
                                    + listMetadataAttribute
                                        .type.ToPascalCaseIdentifier()
                                        .genericEliminate(),
                                    $"{parentName}.add{listMetadataAttribute.type.ToPascalCaseIdentifier().genericEliminate()}In{fieldName.ToPascalCaseIdentifier().genericEliminate()}方法中传入参数为null"
                                )
                                : null!,
                            ExpressionStatement(
                                InvocationExpression( // 创建一个方法调用表达式  
                                    MemberAccessExpression( // 创建一个成员访问表达式（this.list.Add）  
                                        SyntaxKind.SimpleMemberAccessExpression, // 使用点号访问  
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression, // 使用点号访问  ,
                                            ThisExpression(),
                                            IdentifierName(
                                                fieldName
                                            )
                                        ),
                                        IdentifierName(
                                            "Add"
                                        ) // 访问名为list的成员  
                                    ),
                                    ArgumentList( // 创建参数列表  
                                        SingletonSeparatedList( // 单个参数列表  
                                            Argument( // 创建一个参数表达式  
                                                IdentifierName(
                                                    "a"
                                                    + listMetadataAttribute
                                                        .type.ToPascalCaseIdentifier()
                                                        .genericEliminate()
                                                ) // 引用参数i  
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
                        }.Where(
                            v => v is not null
                        )
                    )
                );
        }

        public static MethodDeclarationSyntax CreateRemoveMethod(
            string fieldName,
            string typeName,
            string parentName,
            ListMetadataAttribute listMetadataAttribute) {
            return MethodDeclaration(
                    listMetadataAttribute.link
                        ? IdentifierName(
                            parentName
                        )
                        : IdentifierName(
                            "void"
                        ),
                    $"removeIn{fieldName.ToPascalCaseIdentifier()}"
                )
                .AddModifiers(
                    Token(
                        SyntaxKind.PublicKeyword
                    )
                )
                .AddParameterListParameters(
                    Parameter(
                            Identifier(
                                "a"
                                + listMetadataAttribute
                                    .type.ToPascalCaseIdentifier()
                                    .genericEliminate()
                            )
                        )
                        .WithType(
                            ParseTypeName(
                                listMetadataAttribute.type
                            )
                        )
                )
                .WithBody(
                    Block(
                        new[] {
                                validateNonFrozen(
                                    listMetadataAttribute.freezeTag
                                )!,
                                ExpressionStatement(
                                    InvocationExpression( // 创建一个方法调用表达式  
                                        MemberAccessExpression( // 创建一个成员访问表达式（this.list.Add）  
                                            SyntaxKind.SimpleMemberAccessExpression, // 使用点号访问  
                                            MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression, // 使用点号访问  ,
                                                ThisExpression(),
                                                IdentifierName(
                                                    fieldName
                                                )
                                            ),
                                            IdentifierName(
                                                "Remove"
                                            ) // 访问名为list的成员  
                                        ),
                                        ArgumentList( // 创建参数列表  
                                            SingletonSeparatedList( // 单个参数列表  
                                                Argument( // 创建一个参数表达式  
                                                    IdentifierName(
                                                        "a"
                                                        + listMetadataAttribute
                                                            .type.ToPascalCaseIdentifier()
                                                            .genericEliminate()
                                                    ) // 引用参数i  
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
                            }
                            .Where(
                                v => v is not null
                            )
                            .ToList()
                    )
                );
        }

        public static MethodDeclarationSyntax CreateContainMethod(
            string fieldName,
            string typeName,
            string parentName,
            ListMetadataAttribute listMetadataAttribute) {
            return MethodDeclaration(
                    IdentifierName(
                        "bool"
                    ),
                    $"contaIn{fieldName.ToPascalCaseIdentifier()}"
                )
                .AddModifiers(
                    Token(
                        SyntaxKind.PublicKeyword
                    )
                )
                .AddParameterListParameters(
                    Parameter(
                            Identifier(
                                "a"
                                + listMetadataAttribute
                                    .type.ToPascalCaseIdentifier()
                                    .genericEliminate()
                            )
                        )
                        .WithType(
                            ParseTypeName(
                                listMetadataAttribute.type
                            )
                        )
                )
                .WithBody(
                    Block(
                        new StatementSyntax[] {
                            validateNonFrozen(
                                listMetadataAttribute.freezeTag
                            )!,
                            ReturnStatement(
                                InvocationExpression(
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            ThisExpression(),
                                            IdentifierName(
                                                fieldName
                                            )
                                        ),
                                        IdentifierName(
                                            "Contains"
                                        )
                                    ),
                                    ArgumentList(
                                        SingletonSeparatedList(
                                            Argument(
                                                IdentifierName(
                                                    "a"
                                                    + listMetadataAttribute
                                                        .type.ToPascalCaseIdentifier()
                                                        .genericEliminate()
                                                )
                                            )
                                        )
                                    )
                                )
                            )
                        }.Where(
                            v => v is not null
                        )
                    )
                );
        }

        public static MethodDeclarationSyntax CreateForMethod(
            string fieldName,
            string typeName,
            string parentName,
            ListMetadataAttribute listMetadataAttribute) {
            return MethodDeclaration(
                    IdentifierName(
                        $"IEnumerable<{listMetadataAttribute.type}>"
                    ),
                    $"for{fieldName.ToPascalCaseIdentifier()}"
                )
                .AddModifiers(
                    Token(
                        SyntaxKind.PublicKeyword
                    )
                )
                .WithBody(
                    Block(
                        new StatementSyntax[] {
                            validateNonFrozen(
                                listMetadataAttribute.freezeTag
                            ),
                            listMetadataAttribute.useYield
                                ? ForEachStatement(
                                    ParseTypeName(
                                        "var"
                                    ), // 声明变量类型 var  
                                    Identifier(
                                        "i"
                                    ), // 变量名 i  
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        ThisExpression(),
                                        IdentifierName(
                                            fieldName
                                        )
                                    ), // 假设 list 是一个字段或属性  
                                    Block( // 循环体  
                                        SingletonList<StatementSyntax>(
                                            YieldStatement(
                                                SyntaxKind.YieldReturnStatement,
                                                IdentifierName(
                                                    "i"
                                                )
                                            )
                                        ) // 包含 yield return 语句的列表  
                                    )
                                )
                                : ReturnStatement(
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        ThisExpression(),
                                        IdentifierName(
                                            fieldName
                                        )
                                    )
                                )
                        }.Where(
                            v => v is not null
                        )
                    )
                );
        }

        public static MethodDeclarationSyntax CreatePutMethod(
            string fieldName,
            string typeName,
            string parentName,
            MapMetadataAttribute mapMetadataAttribute) {
            return MethodDeclaration(
                    mapMetadataAttribute.link
                        ? IdentifierName(
                            parentName
                        )
                        : IdentifierName(
                            "void"
                        ),
                    $"putIn{fieldName.ToPascalCaseIdentifier()}"
                )
                .AddModifiers(
                    Token(
                        SyntaxKind.PublicKeyword
                    )
                )
                .AddParameterListParameters(
                    Parameter(
                            Identifier(
                                "key"
                            )
                        )
                        .WithType(
                            ParseTypeName(
                                mapMetadataAttribute.keyType
                            )
                        ),
                    Parameter(
                            Identifier(
                                "value"
                            )
                        )
                        .WithType(
                            ParseTypeName(
                                mapMetadataAttribute.valueType
                            )
                        )
                )
                .WithBody(
                    Block(
                        new[] {
                            validateNonFrozen(
                                mapMetadataAttribute.freezeTag
                            ),
                            mapMetadataAttribute.noNull
                                ? noNull(
                                    "key",
                                    $"{parentName}.put{mapMetadataAttribute.keyType.ToPascalCaseIdentifier().genericEliminate()}And{mapMetadataAttribute.valueType.ToPascalCaseIdentifier().genericEliminate()}In{fieldName}中key为null"
                                )
                                : null!,
                            mapMetadataAttribute.noNull
                                ? noNull(
                                    "value",
                                    $"{parentName}.put{mapMetadataAttribute.keyType.ToPascalCaseIdentifier().genericEliminate()}And{mapMetadataAttribute.valueType.ToPascalCaseIdentifier().genericEliminate()}In{fieldName}中value为null"
                                )
                                : null!,
                            IfStatement(
                                InvocationExpression(
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            ThisExpression(),
                                            IdentifierName(
                                                fieldName
                                            )
                                        ),
                                        IdentifierName(
                                            "ContainsKey"
                                        )
                                    ),
                                    ArgumentList(
                                        SeparatedList(
                                            new[] {
                                                Argument(
                                                    IdentifierName(
                                                        "key"
                                                    )
                                                ),
                                            }
                                        )
                                    )
                                ),
                                Block(
                                    ExpressionStatement(
                                        AssignmentExpression(
                                            SyntaxKind.SimpleAssignmentExpression,
                                            ElementAccessExpression( // 创建一个数组或列表的索引访问表达式  
                                                IdentifierName(
                                                    fieldName
                                                ),
                                                BracketedArgumentList( // 索引参数列表  
                                                    SingletonSeparatedList( // 单个参数列表  
                                                        Argument( // 创建一个参数表达式  
                                                            IdentifierName(
                                                                "key"
                                                            ) // 引用参数 i  
                                                        )
                                                    )
                                                )
                                            ),
                                            IdentifierName(
                                                "value"
                                            )
                                        )
                                    )
                                ),
                                ElseClause(
                                    Block(
                                        ExpressionStatement(
                                            InvocationExpression(
                                                MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    MemberAccessExpression(
                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                        ThisExpression(),
                                                        IdentifierName(
                                                            fieldName
                                                        )
                                                    ),
                                                    IdentifierName(
                                                        "Add"
                                                    )
                                                ),
                                                ArgumentList(
                                                    SeparatedList(
                                                        new[] {
                                                            Argument(
                                                                IdentifierName(
                                                                    "key"
                                                                )
                                                            ),
                                                            Argument(
                                                                IdentifierName(
                                                                    "value"
                                                                )
                                                            )
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
                        }.Where(
                            v => v is not null
                        )
                    )
                );
        }

        public static MethodDeclarationSyntax CreateMapGetMethod(
            string fieldName,
            string typeName,
            string parentName,
            MapMetadataAttribute mapMetadataAttribute) {
            return MethodDeclaration(
                    IdentifierName(
                        mapMetadataAttribute.valueType
                    ),
                    $"getIn{fieldName.ToPascalCaseIdentifier()}"
                )
                .AddModifiers(
                    Token(
                        SyntaxKind.PublicKeyword
                    )
                )
                .AddParameterListParameters(
                    Parameter(
                            Identifier(
                                "key"
                            )
                        )
                        .WithType(
                            ParseTypeName(
                                mapMetadataAttribute.keyType
                            )
                        )
                )
                .WithBody(
                    Block(
                        new StatementSyntax[] {
                            validateNonFrozen(
                                mapMetadataAttribute.freezeTag
                            ),
                            IfStatement(
                                InvocationExpression(
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            ThisExpression(),
                                            IdentifierName(
                                                fieldName
                                            )
                                        ),
                                        IdentifierName(
                                            "ContainsKey"
                                        )
                                    ),
                                    ArgumentList(
                                        SeparatedList(
                                            new[] {
                                                Argument(
                                                    IdentifierName(
                                                        "key"
                                                    )
                                                ),
                                            }
                                        )
                                    )
                                ),
                                Block(
                                    ReturnStatement(
                                        ElementAccessExpression(
                                            IdentifierName(
                                                fieldName
                                            ),
                                            BracketedArgumentList(
                                                SingletonSeparatedList(
                                                    Argument(
                                                        IdentifierName(
                                                            "key"
                                                        )
                                                    )
                                                )
                                            )
                                        )
                                    )
                                ),
                                ElseClause(
                                    Block(
                                        ReturnStatement(
                                            IdentifierName(
                                                "default!"
                                            )
                                        )
                                    )
                                )
                            ),
                        }.Where(
                            v => v is not null
                        )
                    )
                );
        }

        public static MethodDeclarationSyntax CreateRemoveKeyMethod(
            string fieldName,
            string typeName,
            string parentName,
            MapMetadataAttribute mapMetadataAttribute) {
            return MethodDeclaration(
                        mapMetadataAttribute.link
                            ? IdentifierName(
                                parentName
                            )
                            : IdentifierName(
                                "void"
                            ),
                        $"removeKeyIn{fieldName.ToPascalCaseIdentifier()}"
                    )
                    .AddModifiers(
                        Token(
                            SyntaxKind.PublicKeyword
                        )
                    )
                    .AddParameterListParameters(
                        Parameter(
                                Identifier(
                                    "key"
                                )
                            )
                            .WithType(
                                ParseTypeName(
                                    mapMetadataAttribute.keyType
                                )
                            )
                    )
                    .WithBody(
                        Block(
                            new StatementSyntax[] {
                                validateNonFrozen(
                                    mapMetadataAttribute.freezeTag
                                ),
                                ExpressionStatement(
                                    InvocationExpression(
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                ThisExpression(),
                                                IdentifierName(
                                                    fieldName
                                                )
                                            ),
                                            IdentifierName(
                                                "Remove"
                                            )
                                        ),
                                        ArgumentList(
                                            SeparatedList(
                                                new[] {
                                                    Argument(
                                                        IdentifierName(
                                                            "key"
                                                        )
                                                    ),
                                                }
                                            )
                                        )
                                    )
                                ),
                                mapMetadataAttribute.link
                                    ? ReturnStatement(
                                        ThisExpression()
                                    )
                                    : null!
                            }.Where(
                                v => v is not null
                            )
                        )
                    )
                ;
        }

        public static MethodDeclarationSyntax CreateRemoveValueMethod(
            string fieldName,
            string typeName,
            string parentName,
            MapMetadataAttribute mapMetadataAttribute) {
            return MethodDeclaration(
                        mapMetadataAttribute.link
                            ? IdentifierName(
                                parentName
                            )
                            : IdentifierName(
                                "void"
                            ),
                        $"removeValueIn{fieldName.ToPascalCaseIdentifier()}"
                    )
                    .AddModifiers(
                        Token(
                            SyntaxKind.PublicKeyword
                        )
                    )
                    .AddParameterListParameters(
                        Parameter(
                                Identifier(
                                    "value"
                                )
                            )
                            .WithType(
                                ParseTypeName(
                                    mapMetadataAttribute.valueType
                                )
                            )
                    )
                    .WithBody(
                        Block(
                            new StatementSyntax[] {
                                validateNonFrozen(
                                    mapMetadataAttribute.freezeTag
                                ),
                                ExpressionStatement(
                                    InvocationExpression(
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                ThisExpression(),
                                                IdentifierName(
                                                    fieldName
                                                )
                                            ),
                                            IdentifierName(
                                                "RemoveValue"
                                            )
                                        ),
                                        ArgumentList(
                                            SeparatedList(
                                                new[] {
                                                    Argument(
                                                        IdentifierName(
                                                            "value"
                                                        )
                                                    ),
                                                }
                                            )
                                        )
                                    )
                                ),
                                mapMetadataAttribute.link
                                    ? ReturnStatement(
                                        ThisExpression()
                                    )
                                    : null!
                            }.Where(
                                v => v is not null
                            )
                        )
                    )
                ;
        }

        public static MethodDeclarationSyntax CreateContainKeyMethod(
            string fieldName,
            string typeName,
            string parentName,
            MapMetadataAttribute mapMetadataAttribute) {
            return MethodDeclaration(
                    IdentifierName(
                        "bool"
                    ),
                    $"containKeyIn{fieldName.ToPascalCaseIdentifier()}"
                )
                .AddModifiers(
                    Token(
                        SyntaxKind.PublicKeyword
                    )
                )
                .AddParameterListParameters(
                    Parameter(
                            Identifier(
                                "key"
                            )
                        )
                        .WithType(
                            ParseTypeName(
                                mapMetadataAttribute.keyType
                            )
                        )
                )
                .WithBody(
                    Block(
                        new StatementSyntax[] {
                            validateNonFrozen(
                                mapMetadataAttribute.freezeTag
                            ),
                            ReturnStatement(
                                InvocationExpression(
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            ThisExpression(),
                                            IdentifierName(
                                                fieldName
                                            )
                                        ),
                                        IdentifierName(
                                            "ContainsKey"
                                        )
                                    ),
                                    ArgumentList(
                                        SeparatedList(
                                            new[] {
                                                Argument(
                                                    IdentifierName(
                                                        "key"
                                                    )
                                                ),
                                            }
                                        )
                                    )
                                )
                            )
                        }.Where(
                            v => v is not null
                        )
                    )
                );
        }

        public static MethodDeclarationSyntax CreateContainValueMethod(
            string fieldName,
            string typeName,
            string parentName,
            MapMetadataAttribute mapMetadataAttribute) {
            return MethodDeclaration(
                    IdentifierName(
                        "bool"
                    ),
                    $"containValueIn{fieldName.ToPascalCaseIdentifier()}"
                )
                .AddModifiers(
                    Token(
                        SyntaxKind.PublicKeyword
                    )
                )
                .AddParameterListParameters(
                    Parameter(
                            Identifier(
                                "value"
                            )
                        )
                        .WithType(
                            ParseTypeName(
                                mapMetadataAttribute.valueType
                            )
                        )
                )
                .WithBody(
                    Block(
                        new StatementSyntax[] {
                            validateNonFrozen(
                                mapMetadataAttribute.freezeTag
                            ),
                            ReturnStatement(
                                InvocationExpression(
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            ThisExpression(),
                                            IdentifierName(
                                                fieldName
                                            )
                                        ),
                                        IdentifierName(
                                            "ContainsValue"
                                        )
                                    ),
                                    ArgumentList(
                                        SeparatedList(
                                            new[] {
                                                Argument(
                                                    IdentifierName(
                                                        "value"
                                                    )
                                                ),
                                            }
                                        )
                                    )
                                )
                            )
                        }.Where(
                            v => v is not null
                        )
                    )
                );
        }

        public static MethodDeclarationSyntax CreateForKeyMethod(
            string fieldName,
            string typeName,
            string parentName,
            MapMetadataAttribute mapMetadataAttribute) {
            return MethodDeclaration(
                    IdentifierName(
                        $"IEnumerable<{mapMetadataAttribute.keyType}>"
                    ),
                    $"for{fieldName.ToPascalCaseIdentifier()}Key"
                )
                .AddModifiers(
                    Token(
                        SyntaxKind.PublicKeyword
                    )
                )
                .WithBody(
                    Block(
                        new StatementSyntax[] {
                            validateNonFrozen(
                                mapMetadataAttribute.freezeTag
                            ),
                            mapMetadataAttribute.useYield
                                ? ForEachStatement(
                                    ParseTypeName(
                                        "var"
                                    ), // 声明变量类型 var  
                                    Identifier(
                                        "i"
                                    ), // 变量名 i  
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            ThisExpression(),
                                            IdentifierName(
                                                fieldName
                                            )
                                        ),
                                        IdentifierName(
                                            "Keys"
                                        )
                                    ), // 假设 list 是一个字段或属性  
                                    Block( // 循环体  
                                        SingletonList<StatementSyntax>(
                                            YieldStatement(
                                                SyntaxKind.YieldReturnStatement,
                                                IdentifierName(
                                                    "i"
                                                )
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
                                            IdentifierName(
                                                fieldName
                                            )
                                        ),
                                        IdentifierName(
                                            "Keys"
                                        )
                                    )
                                )
                        }.Where(
                            v => v is not null
                        )
                    )
                );
        }

        public static MethodDeclarationSyntax CreateForValueMethod(
            string fieldName,
            string typeName,
            string parentName,
            MapMetadataAttribute mapMetadataAttribute) {
            return MethodDeclaration(
                    IdentifierName(
                        $"IEnumerable<{mapMetadataAttribute.valueType}>"
                    ),
                    $"for{fieldName.ToPascalCaseIdentifier()}Value"
                )
                .AddModifiers(
                    Token(
                        SyntaxKind.PublicKeyword
                    )
                )
                .WithBody(
                    Block(
                        new StatementSyntax[] {
                            validateNonFrozen(
                                mapMetadataAttribute.freezeTag
                            ),
                            mapMetadataAttribute.useYield
                                ? ForEachStatement(
                                    ParseTypeName(
                                        "var"
                                    ), // 声明变量类型 var  
                                    Identifier(
                                        "i"
                                    ), // 变量名 i  
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            ThisExpression(),
                                            IdentifierName(
                                                fieldName
                                            )
                                        ),
                                        IdentifierName(
                                            "Values"
                                        )
                                    ), // 假设 list 是一个字段或属性  
                                    Block( // 循环体  
                                        SingletonList<StatementSyntax>(
                                            YieldStatement(
                                                SyntaxKind.YieldReturnStatement,
                                                IdentifierName(
                                                    "i"
                                                )
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
                                            IdentifierName(
                                                fieldName
                                            )
                                        ),
                                        IdentifierName(
                                            "Values"
                                        )
                                    )
                                )
                        }.Where(
                            v => v is not null
                        )
                    )
                );
        }

        public static MethodDeclarationSyntax CreateForAllMethod(
            string fieldName,
            string typeName,
            string parentName,
            MapMetadataAttribute mapMetadataAttribute) {
            return MethodDeclaration(
                    IdentifierName(
                        $"IEnumerable<KeyValuePair<{mapMetadataAttribute.keyType}, {mapMetadataAttribute.valueType}>>"
                    ),
                    $"for{fieldName.ToPascalCaseIdentifier()}"
                )
                .AddModifiers(
                    Token(
                        SyntaxKind.PublicKeyword
                    )
                )
                .WithBody(
                    Block(
                        new StatementSyntax[] {
                            validateNonFrozen(
                                mapMetadataAttribute.freezeTag
                            ),
                            mapMetadataAttribute.useYield
                                ? ForEachStatement(
                                    ParseTypeName(
                                        "var"
                                    ), // 声明变量类型 var  
                                    Identifier(
                                        "i"
                                    ), // 变量名 i  
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        ThisExpression(),
                                        IdentifierName(
                                            fieldName
                                        )
                                    ),
                                    Block( // 循环体  
                                        SingletonList<StatementSyntax>(
                                            YieldStatement(
                                                SyntaxKind.YieldReturnStatement,
                                                IdentifierName(
                                                    "i"
                                                )
                                            )
                                        ) // 包含 yield return 语句的列表  
                                    )
                                )
                                : ReturnStatement(
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        ThisExpression(),
                                        IdentifierName(
                                            fieldName
                                        )
                                    )
                                )
                        }.Where(
                            v => v is not null
                        )
                    )
                );
        }
    }
}