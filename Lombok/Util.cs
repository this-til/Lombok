﻿using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Til.Lombok;

/// <summary>
/// Extensions for <see cref="IncrementalGeneratorInitializationContext"/>.
/// </summary>
internal static class IncrementalGeneratorInitializationContextExtensions {
    /// <summary>
    /// Checks if the result is erroneous and if a diagnostic needs to be raised. If not, it adds the source to the compilation.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="provider"></param>
    public static void AddSources(this IncrementalGeneratorInitializationContext context, IncrementalValuesProvider<GeneratorResult> provider) {
        context.RegisterSourceOutput(provider, AddSources);
    }

    private static void AddSources(SourceProductionContext context, GeneratorResult result) {
        if (result.IsValid) {
            context.AddSource($"{result.TypeName}.g.cs", result.Source);
        }
        else if (result.Diagnostic is not null) {
            context.ReportDiagnostic(result.Diagnostic);
        }
    }
}

/// <summary>
/// The result class for incremental generators. Either contains source code or a diagnostic which should be raised.
/// </summary>
internal sealed class GeneratorResult {
    /// <summary>
    /// The name of the generated type.
    /// </summary>
    public string? TypeName { get; }

    /// <summary>
    /// The SourceText of the generated code.
    /// </summary>
    public SourceText? Source { get; }

    /// <summary>
    /// The diagnostic to be raised if something went wrong.
    /// </summary>
    public Diagnostic? Diagnostic { get; }

    /// <summary>
    /// Determines if the result is valid and can be added to the compilation or if the diagnostic needs to be raised.
    /// </summary>
    public bool IsValid => TypeName is not null && Source is not null && Diagnostic is null;

    /// <summary>
    /// An empty result. Something went wrong, however no diagnostic should be reported
    /// </summary>
    public static GeneratorResult Empty { get; } = new();

    /// <summary>
    /// Constructor to be used in case of success.
    /// </summary>
    /// <param name="typeName">The name of the generated type.</param>
    /// <param name="source">The source of the generated code.</param>
    public GeneratorResult(string typeName, SourceText source) {
        TypeName = typeName;
        Source = source;
    }

    /// <summary>
    /// Constructor to be used in case of failure.
    /// </summary>
    /// <param name="diagnostic">The diagnostic to be raised.</param>
    public GeneratorResult(Diagnostic diagnostic) {
        Diagnostic = diagnostic;
    }

    private GeneratorResult() {
    }
}

internal static class SyntaxNodeExtensions {
    private static readonly IDictionary<AccessTypes, SyntaxKind> SyntaxKindsByAccessType = new Dictionary<AccessTypes, SyntaxKind>(4) {
        [AccessTypes.Private] = SyntaxKind.PrivateKeyword,
        [AccessTypes.Protected] = SyntaxKind.ProtectedKeyword,
        [AccessTypes.Internal] = SyntaxKind.InternalKeyword,
        [AccessTypes.Public] = SyntaxKind.PublicKeyword
    };

    internal static readonly SyntaxTriviaList NullableTrivia = TriviaList(
        Trivia(
            NullableDirectiveTrivia(
                Token(SyntaxKind.EnableKeyword),
                true
            )
        )
    );

    private static readonly SyntaxTrivia AutoGeneratedComment = Comment("// <auto-generated/>");

    /// <summary>
    /// Traverses a syntax node upwards until it reaches a <code>BaseNamespaceDeclarationSyntax</code>.
    /// </summary>
    /// <param name="node">The syntax node to traverse.</param>
    /// <returns>The namespace this syntax node is in. <code>null</code> if a namespace cannot be found.</returns>
    public static NameSyntax? GetNamespace(this SyntaxNode node) {
        var parent = node.Parent;
        while (parent != null) {
            if (parent is BaseNamespaceDeclarationSyntax ns) {
                return ns.Name;
            }

            parent = parent.Parent;
        }

        return null;
    }

    /// <summary>
    /// Gets the using directives from a SyntaxNode. Traverses the tree upwards until it finds using directives.
    /// </summary>
    /// <param name="node">The staring point.</param>
    /// <returns>A list of using directives.</returns>
    public static SyntaxList<UsingDirectiveSyntax> GetUsings(this SyntaxNode node) {
        var parent = node.Parent;
        while (parent is not null) {
            if (parent is BaseNamespaceDeclarationSyntax ns && ns.Usings.Any()) {
                return ns.Usings;
            }

            if (parent is CompilationUnitSyntax compilationUnit && compilationUnit.Usings.Any()) {
                return compilationUnit.Usings;
            }

            parent = parent.Parent;
        }

        return default;
    }

    /// <summary>
    /// Gets the accessibility modifier for a type declaration.
    /// </summary>
    /// <param name="typeDeclaration">The type declaration's accessibility modifier to find.</param>
    /// <returns>The types accessibility modifier.</returns>
    public static SyntaxKind GetAccessibilityModifier(this BaseTypeDeclarationSyntax typeDeclaration) {
        if (typeDeclaration.Modifiers.Any(SyntaxKind.PublicKeyword)) {
            return SyntaxKind.PublicKeyword;
        }

        return SyntaxKind.InternalKeyword;
    }

    /// <summary>
    /// Constructs a new partial type from the original type's name, accessibility and type arguments.
    /// </summary>
    /// <param name="typeDeclaration">The type to clone.</param>
    /// <returns>A new partial type with a few of the original types traits.</returns>
    public static TypeDeclarationSyntax CreateNewPartialType(this TypeDeclarationSyntax typeDeclaration) {
        return typeDeclaration switch {
            ClassDeclarationSyntax => typeDeclaration.CreateNewPartialClass(),
            StructDeclarationSyntax => typeDeclaration.CreateNewPartialStruct(),
            InterfaceDeclarationSyntax => typeDeclaration.CreateNewPartialInterface(),
            _ => typeDeclaration
        };
    }

    /// <summary>
    /// Constructs a new partial class from the original type's name, accessibility and type arguments.
    /// </summary>
    /// <param name="type">The type to clone.</param>
    /// <returns>A new partial class with a few of the original types traits.</returns>
    public static ClassDeclarationSyntax CreateNewPartialClass(this TypeDeclarationSyntax type) {
        var declaration = ClassDeclaration(type.Identifier.Text)
            .WithModifiers(
                TokenList(
                    Token(type.GetAccessibilityModifier()),
                    Token(SyntaxKind.PartialKeyword)
                )
            ).WithTypeParameterList(type.TypeParameterList);
        if (type.ShouldEmitNrtTrivia()) {
            declaration = declaration.WithLeadingTrivia(NullableTrivia);
        }

        return declaration;
    }

    /// <summary>
    /// Constructs a new partial struct from the original type's name, accessibility and type arguments.
    /// </summary>
    /// <param name="type">The type to clone.</param>
    /// <returns>A new partial struct with a few of the original types traits.</returns>
    public static StructDeclarationSyntax CreateNewPartialStruct(this TypeDeclarationSyntax type) {
        var declaration = StructDeclaration(type.Identifier.Text)
            .WithModifiers(
                TokenList(
                    Token(type.GetAccessibilityModifier()),
                    Token(SyntaxKind.PartialKeyword)
                )
            ).WithTypeParameterList(type.TypeParameterList);
        if (type.ShouldEmitNrtTrivia()) {
            declaration = declaration.WithLeadingTrivia(NullableTrivia);
        }

        return declaration;
    }

    /// <summary>
    /// Constructs a new partial interface from the original type's name, accessibility and type arguments.
    /// </summary>
    /// <param name="type">The type to clone.</param>
    /// <returns>A new partial interface with a few of the original types traits.</returns>
    public static InterfaceDeclarationSyntax CreateNewPartialInterface(this TypeDeclarationSyntax type) {
        var declaration = InterfaceDeclaration(type.Identifier.Text)
            .WithModifiers(
                TokenList(
                    Token(type.GetAccessibilityModifier()),
                    Token(SyntaxKind.PartialKeyword)
                )
            ).WithTypeParameterList(type.TypeParameterList);
        if (type.ShouldEmitNrtTrivia()) {
            declaration = declaration.WithLeadingTrivia(NullableTrivia);
        }

        return declaration;
    }

    public static CompilationUnitSyntax CreateNewNamespace(this NameSyntax @namespace, MemberDeclarationSyntax innerMember) {
        return CreateNewNamespace(@namespace, default, innerMember);
    }

    public static CompilationUnitSyntax CreateNewNamespace(this NameSyntax @namespace, SyntaxList<UsingDirectiveSyntax> usings, MemberDeclarationSyntax innerMember) {
        var newNamespace = FileScopedNamespaceDeclaration(@namespace)
            .WithMembers(
                SingletonList(innerMember)
            );
        if (usings.Any()) {
            var newUsing = usings[0].WithUsingKeyword(
                Token(
                    TriviaList(
                        AutoGeneratedComment
                    ),
                    SyntaxKind.UsingKeyword,
                    TriviaList()
                )
            );
            usings = usings.Replace(usings[0], newUsing);
        }
        else {
            newNamespace = newNamespace.WithNamespaceKeyword(
                Token(
                    TriviaList(
                        AutoGeneratedComment
                    ),
                    SyntaxKind.NamespaceKeyword,
                    TriviaList()
                )
            );
        }

        return CompilationUnit()
            .WithUsings(usings)
            .WithMembers(
                SingletonList<MemberDeclarationSyntax>(newNamespace)
            );
    }

    /// <summary>
    /// Checks if a TypeSyntax represents void.
    /// </summary>
    /// <param name="typeSyntax">The TypeSyntax to check.</param>
    /// <returns>True, if the type represents void.</returns>
    public static bool IsVoid(this TypeSyntax typeSyntax) {
        return typeSyntax is PredefinedTypeSyntax predefinedType && predefinedType.Keyword.IsKind(SyntaxKind.VoidKeyword);
    }

    /// <summary>
    /// Checks if a type is declared as a nested type.
    /// </summary>
    /// <param name="typeDeclaration">The type to check.</param>
    /// <returns>True, if the type is declared within another type.</returns>
    public static bool IsNestedType(this BaseTypeDeclarationSyntax typeDeclaration) {
        return typeDeclaration.Parent is TypeDeclarationSyntax;
    }

    /// <summary>
    /// Determines if the type is eligible for code generation.
    /// </summary>
    /// <param name="typeDeclaration">The type to check for.</param>
    /// <param name="namespace">The type's namespace. Will be set in this method.</param>
    /// <param name="diagnostic">A diagnostic to be emitted if the type is not valid.</param>
    /// <returns>True, if code can be generated for this type.</returns>
    public static bool TryValidateType(this TypeDeclarationSyntax typeDeclaration, out NameSyntax? @namespace, out Diagnostic? diagnostic) {
        @namespace = null;
        diagnostic = null;

        if (!typeDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword)) {
            diagnostic = Diagnostic.Create(DiagnosticDescriptors.TypeMustBePartial, typeDeclaration.Identifier.GetLocation(), typeDeclaration.Identifier.Text);

            return false;
        }

        if (typeDeclaration.Modifiers.Any(static token => token.Text == "file")) {
            diagnostic = Diagnostic.Create(DiagnosticDescriptors.TypeCannotBeFileLocal, typeDeclaration.Identifier.GetLocation(), typeDeclaration.Identifier.Text);

            return false;
        }

        if (typeDeclaration.IsNestedType()) {
            diagnostic = Diagnostic.Create(DiagnosticDescriptors.TypeMustBeNonNested, typeDeclaration.Identifier.GetLocation(), typeDeclaration.Identifier.Text);

            return false;
        }

        @namespace = typeDeclaration.GetNamespace();
        if (@namespace is null) {
            diagnostic = Diagnostic.Create(DiagnosticDescriptors.TypeMustHaveNamespace, typeDeclaration.Identifier.GetLocation(), typeDeclaration.Identifier.Text);

            return false;
        }

        return true;
    }

    /// <summary>
    /// Removes all the members which do not have the desired access modifier.
    /// </summary>
    /// <param name="members">The members to filter</param>
    /// <param name="accessType">The access modifer to look out for.</param>
    /// <typeparam name="T">The type of the members (<code>PropertyDeclarationSyntax</code>/<code>FieldDeclarationSyntax</code>).</typeparam>
    /// <returns>The members which have the desired access modifier.</returns>
    /// <exception cref="ArgumentOutOfRangeException">If an access modifier is supplied which is not supported.</exception>
    public static IEnumerable<T> Where<T>(this IEnumerable<T> members, AccessTypes accessType)
        where T : MemberDeclarationSyntax {
        var predicateBuilder = PredicateBuilder.False<T>();
        foreach (AccessTypes t in typeof(AccessTypes).GetEnumValues()) {
            if (accessType.HasFlag(t)) {
                predicateBuilder = predicateBuilder.Or(m => m.Modifiers.Any(SyntaxKindsByAccessType[t]));
            }
        }

        return members.Where(predicateBuilder.Compile());
    }

    /// <summary>
    /// Creates a unique name for a type which can be used as the hint name in Source Generator output.
    /// </summary>
    /// <param name="type">The type to get the name for</param>
    /// <param name="namespace">The namespace which will be prepended to the type using underscores.</param>
    /// <returns>A unique name for the type inside a generator context.</returns>
    public static string GetHintName(this BaseTypeDeclarationSyntax type, NameSyntax @namespace) {
        return string.Concat(@namespace.ToString().Replace('.', '_'), '_', type.Identifier.Text);
    }

    /// <summary>
    /// Determines if the <code>#nullable enable</code> preprocessor directive should be emitted in generated code.
    /// </summary>
    /// <param name="node">The node to determine the nullability context in.</param>
    /// <returns><code>true</code> if the preprocessor directive should be emitted, <code>false</code> otherwise.</returns>
    public static bool ShouldEmitNrtTrivia(this SyntaxNode node) {
        return node.SyntaxTree.Options is CSharpParseOptions opt && (int)opt.LanguageVersion >= (int)LanguageVersion.CSharp8;
    }

    public static SyntaxTriviaList GetLeadingTriviaFromMultipleLocations(this FieldDeclarationSyntax field) {
        var typeTrivia = field.Declaration.Type.GetCommentTrivia();
        if (typeTrivia.Any()) {
            return typeTrivia;
        }

        var modifierTrivia = field.Modifiers.First().GetCommentTrivia();
        if (modifierTrivia.Any()) {
            return modifierTrivia;
        }

        var attributeList = field.AttributeLists[0];

        return attributeList.OpenBracketToken.GetCommentTrivia();
    }

    public static SyntaxTriviaList GetCommentTrivia(this SyntaxToken token) {
        if (token.LeadingTrivia.Any(SyntaxKind.SingleLineCommentTrivia) || token.LeadingTrivia.Any(SyntaxKind.MultiLineCommentTrivia)) {
            return token.LeadingTrivia;
        }

        return default;
    }

    public static SyntaxTriviaList GetCommentTrivia(this TypeSyntax type) {
        if (type is PredefinedTypeSyntax predefinedType) {
            return predefinedType.Keyword.GetCommentTrivia();
        }

        if (type is IdentifierNameSyntax identifier) {
            return identifier.Identifier.GetCommentTrivia();
        }

        return default;
    }

    public static IEnumerable<INamedTypeSymbol> GetAllTypeSymbol(this INamespaceSymbol namespaceSymbol) {
        var typeMemberList = namespaceSymbol.GetTypeMembers();

        foreach (var typeSymbol in typeMemberList) {
            yield return typeSymbol;
        }

        foreach (var namespaceMember in namespaceSymbol.GetNamespaceMembers()) {
            foreach (var typeSymbol in GetAllTypeSymbol(namespaceMember)) {
                yield return typeSymbol;
            }
        }
    }

    public static AttributeSyntax? tryGetSpecifiedAttribute(this IEnumerable<AttributeListSyntax> attributeLists, string attributeName) {
        foreach (AttributeListSyntax attributeList in attributeLists) {
            foreach (AttributeSyntax attribute in attributeList.Attributes)
                if (attributeName.StartsWith(attribute.Name.ToString())) {
                    return attribute;
                }
        }
        return null; // 没有找到指定的注解  
    }

    public static Dictionary<string, object> getAttributeArgumentsAsDictionary(this AttributeSyntax attribute) {
        var dictionary = new Dictionary<string, object>();

        if (attribute.ArgumentList != null) {
            foreach (var argument in attribute.ArgumentList.Arguments) {
                var key = argument.NameEquals?.Name.Identifier.ToString() ?? default;
                var value = ExtractAttributeValue(argument.Expression);
                dictionary.Add(key, value);
            }
        }

        return dictionary;
    }

    public static string getHasGenericName(this ClassDeclarationSyntax classDeclarationSyntax) {
        string className = classDeclarationSyntax.Identifier.ValueText;

        if (classDeclarationSyntax.TypeParameterList is null) {
            return className;
        }

        SeparatedSyntaxList<TypeParameterSyntax> typeParameters = classDeclarationSyntax.TypeParameterList.Parameters;

        // 你可以遍历这些参数，或者将它们格式化为字符串  
        string genericParameters = string.Join(", ", typeParameters.Select(tp => tp.Identifier.ValueText));

        // 输出包含泛型参数的类名  
        return $"{className}<{genericParameters}>";
    }

    static object ExtractAttributeValue(ExpressionSyntax expression) {
        // 根据不同的 ExpressionSyntax 类型提取值  
        switch (expression.Kind()) {
            case SyntaxKind.TrueLiteralExpression:
                return true;
            case SyntaxKind.FalseLiteralExpression:
                return false;
            case SyntaxKind.StringLiteralExpression:
                return ((LiteralExpressionSyntax)expression).Token.ValueText;
            default:
                return null;
        }
    }
}

/// <summary>
/// Contains definitions of diagnostics which can be raised by Lombok.NET.
/// </summary>
public static class DiagnosticDescriptors {
    /// <summary>
    /// Raised when a type is not partial although it should be.
    /// </summary>
    public static readonly DiagnosticDescriptor TypeMustBePartial = new(
        "LOM001",
        "Type must be partial",
        "The type '{0}' must be partial in order to generate code for it",
        "Usage",
        DiagnosticSeverity.Error,
        true
    );

    /// <summary>
    /// Raised when a type is within another type although it should not be.
    /// </summary>
    public static readonly DiagnosticDescriptor TypeMustBeNonNested = new(
        "LOM002",
        "Type must be non-nested",
        "The type '{0}' must be non-nested in order to generate code for it",
        "Usage",
        DiagnosticSeverity.Error,
        true
    );

    /// <summary>
    /// Raised when a type is not within a namespace although it should be.
    /// </summary>
    public static readonly DiagnosticDescriptor TypeMustHaveNamespace = new(
        "LOM003",
        "Type must have namespace",
        "The type '{0}' must be in a namespace in order to generate code for it",
        "Usage",
        DiagnosticSeverity.Error,
        true
    );

    /// <summary>
    /// Raised when a method is not within a class or a struct although it should be, or if it is a local function.
    /// </summary>
    public static readonly DiagnosticDescriptor MethodMustBeInPartialClassOrStruct = new(
        "LOM004",
        "Method must be inside partial class or struct",
        "The method '{0}' must be inside a partial class or a struct and cannot be a local function",
        "Usage",
        DiagnosticSeverity.Error,
        true
    );

    /// <summary>
    /// Raised when a field is not within a class or a struct although it should be.
    /// </summary>
    public static readonly DiagnosticDescriptor PropertyFieldMustBeInClassOrStruct = new(
        "LOM005",
        "Field must be inside class or struct",
        "The field '{0}' must be inside a class or a struct",
        "Usage",
        DiagnosticSeverity.Error,
        true
    );

    /// <summary>
    /// Raised when invalid JSON is encountered.
    /// </summary>
    public static readonly DiagnosticDescriptor InvalidJson = new(
        "LOM006",
        "Invalid JSON",
        "Unable to generate code, since the JSON input is invalid.",
        "Usage",
        DiagnosticSeverity.Error,
        true
    );

    /// <summary>
    /// Raised when a type is file-local.
    /// </summary>
    public static readonly DiagnosticDescriptor TypeCannotBeFileLocal = new(
        "LOM007",
        "Type cannot be file-local",
        "The type '{0}' must not be file-local in order to generate code for it.",
        "Usage",
        DiagnosticSeverity.Error,
        true
    );
}

/// <summary>
/// Builds a predicate by chaining multiple conditions.
/// </summary>
internal static class PredicateBuilder {
    /// <summary>
    /// Returns an expression which is always true.
    /// </summary>
    /// <typeparam name="T">The type this predicate targets.</typeparam>
    /// <returns>An always-true predicate.</returns>
    public static Expression<Func<T, bool>> True<T>() {
        return static f => true;
    }

    /// <summary>
    /// Returns an expression which is always false.
    /// </summary>
    /// <typeparam name="T">The type this predicate targets.</typeparam>
    /// <returns>An always-false predicate.</returns>
    public static Expression<Func<T, bool>> False<T>() {
        return static f => false;
    }

    /// <summary>
    /// Adds a new condition to the chain and combines it using an OR expression.
    /// </summary>
    /// <param name="expr1">The existing predicate chain.</param>
    /// <param name="expr2">The predicate to add.</param>
    /// <typeparam name="T">The type this predicate targets.</typeparam>
    /// <returns>A new predicate with an additional OR predicate.</returns>
    public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2) {
        var invokedExpr = Expression.Invoke(expr2, expr1.Parameters);

        return Expression.Lambda<Func<T, bool>>(Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);
    }

    /// <summary>
    /// Adds a new condition to the chain and combines it using an AND expression.
    /// </summary>
    /// <param name="expr1">The existing predicate chain.</param>
    /// <param name="expr2">The predicate to add.</param>
    /// <typeparam name="T">The type this predicate targets.</typeparam>
    /// <returns>A new predicate with an additional AND predicate.</returns>
    public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2) {
        var invokedExpr = Expression.Invoke(expr2, expr1.Parameters);

        return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters);
    }
}

internal static class StringExtensions {
    // Taken from https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/
    private static readonly ISet<string> ReservedKeywords = new HashSet<string> {
        "abstract",
        "as",
        "base",
        "bool",
        "break",
        "byte",
        "case",
        "catch",
        "char",
        "checked",
        "class",
        "const",
        "continue",
        "decimal",
        "default",
        "delegate",
        "do",
        "double",
        "else",
        "enum",
        "event",
        "explicit",
        "extern",
        "false",
        "finally",
        "fixed",
        "float",
        "for",
        "foreach",
        "goto",
        "if",
        "implicit",
        "in",
        "int",
        "interface",
        "internal",
        "is",
        "lock",
        "long",
        "namespace",
        "new",
        "null",
        "object",
        "operator",
        "out",
        "override",
        "params",
        "private",
        "protected",
        "public",
        "readonly",
        "ref",
        "return",
        "sbyte",
        "sealed",
        "short",
        "sizeof",
        "stackalloc",
        "static",
        "string",
        "struct",
        "switch",
        "this",
        "throw",
        "true",
        "try",
        "typeof",
        "uint",
        "ulong",
        "unchecked",
        "unsafe",
        "ushort",
        "using",
        "virtual",
        "void",
        "volatile",
        "while"
    };

    /// <summary>
    /// Lowercases the first character of a given string.
    /// </summary>
    /// <param name="s">The string whose first character to lowercase.</param>
    /// <returns>The string with its first character lowercased.</returns>
    public static string? Decapitalize(this string? s) {
        if (s is null || char.IsLower(s[0])) {
            return s;
        }

        return char.ToLower(s[0]) + s.Substring(1);
    }

    /// <summary>
    /// Uppercases the first character of a given string.
    /// </summary>
    /// <param name="s">The string whose first character to uppercase.</param>
    /// <returns>The string with its first character uppercased.</returns>
    public static string? Capitalize(this string? s) {
        if (s is null || char.IsUpper(s[0])) {
            return s;
        }

        return char.ToUpper(s[0]) + s.Substring(1);
    }

    /// <summary>
    /// Escapes a reserved keyword which should be used as an identifier.
    /// </summary>
    /// <param name="identifier">The identifier to be used.</param>
    /// <returns>A valid identifier.</returns>
    public static string EscapeReservedKeyword(this string identifier) {
        if (ReservedKeywords.Contains(identifier)) {
            return "@" + identifier;
        }

        return identifier;
    }

    public static string genericEliminate(this string identifier) {
        string pattern = @"<[^>]+>"; // 匹配 < 和 > 之间的任何内容（不包括这两个尖括号）  
        return Regex.Replace(identifier, pattern, "");
    }

    /// <summary>
    /// Ensures normal PascalCase for an identifier. (e.g. "_age" becomes "Age").
    /// </summary>
    /// <param name="identifier">The identifier to get the property name for.</param>
    /// <returns>A PascalCase identifier.</returns>
    public static string ToPascalCaseIdentifier(this string identifier) {
        if (identifier.StartsWith("_")) {
            identifier = identifier.Substring(1);
        }

        return identifier.Capitalize()!;
    }

    /// <summary>
    /// Transforms an identifier to camelCase. (e.g. "_myAge" -> "myAge", "MyAge" -> "myAge").
    /// </summary>
    /// <param name="identifier">The identifier to transform.</param>
    /// <returns>A camelCase identifier.</returns>
    public static string ToCamelCaseIdentifier(this string identifier) {
        if (identifier.StartsWith("_")) {
            return identifier.Substring(1).Decapitalize()!;
        }

        return identifier.Decapitalize()!;
    }
}

public static class Util {
    public static void noNull(object? obj, string? message = null) {
        if (obj is null) {
            throw new NullReferenceException(message);
        }
    }
}