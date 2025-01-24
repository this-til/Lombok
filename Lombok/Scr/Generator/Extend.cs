using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Til.Lombok.Generator {

    public static class ClassDeclarationSyntaxExtend {

        public static string getName(this ClassDeclarationSyntax classDeclarationSyntax) => classDeclarationSyntax.Identifier.Text;

        public static string getGenericName(this ClassDeclarationSyntax classDeclarationSyntax) => classDeclarationSyntax.TypeParameterList is null
            ? classDeclarationSyntax.getName()
            : $"{classDeclarationSyntax.getName()}<{classDeclarationSyntax.TypeParameterList.Parameters.ToString()}>";

        public static string getFileName(this ClassDeclarationSyntax classDeclarationSyntax) => classDeclarationSyntax.getGenericName().Replace("<", "_").Replace(">", "_").Replace(",", "_");

        public static bool isGeneric(this ClassDeclarationSyntax classDeclarationSyntax) => classDeclarationSyntax.TypeParameterList is not null;

        public static A[] getAttribute<A>(this ClassDeclarationSyntax classDeclarationSyntax, SemanticModel semanticModel) where A : Attribute => classDeclarationSyntax.AttributeLists.getAttribute<A>(semanticModel);

    }

    public static class VariableDeclaratorSyntaxExtend {

    }

    public static class PropertyDeclarationSyntaxExtend {

    }

    public static class AttributeSyntaxExtend {

        public const string attribute = "Attribute";

        public static bool isAttribute<A>(this AttributeSyntax attributeListSyntax) where A : Attribute {
            string attributeTypeName = typeof(A).Name;
            string attributeName = attributeListSyntax.ToString();
            attributeName = !attributeName.EndsWith(attribute)
                ? attributeName + attribute
                : attributeName;
            return Equals(attributeName, attributeTypeName);
        }

        public static A asAttribute<A>(this AttributeSyntax attributeListSyntax, SemanticModel semanticModel) where A : Attribute {
            return (A)Activator.CreateInstance(typeof(A), attributeListSyntax.getAttributeArgumentsAsDictionary(semanticModel));
        }

        public static Dictionary<string, string> getAttributeArgumentsAsDictionary(this AttributeSyntax attributeSyntax, SemanticModel semanticModel) {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();

            if (attributeSyntax.ArgumentList == null) {
                return dictionary;
            }

            foreach (AttributeArgumentSyntax argument in attributeSyntax.ArgumentList.Arguments) {
                string? key = argument.NameEquals?.Name.Identifier.ToString() ?? null;
                if (key is null) {
                    continue;
                }

                string? value;
                try {
                    value = argument.Expression.extractValue(semanticModel);
                }
                catch (Exception _) {
                    value = argument.Expression.ToString();
                    int breakPoint = value.LastIndexOf('.');
                    if (breakPoint != -1) {
                        value = value.Substring(breakPoint + 1, value.Length - breakPoint - 1);
                    }
                }

                if (value is null) {
                    continue;
                }
                dictionary.Add(key, value);
            }

            return dictionary;
        }

    }

    public static class SyntaxListExtend {

        public static A[] getAttribute<A>(this IEnumerable<AttributeListSyntax> attributeListSyntax, SemanticModel semanticModel) where A : Attribute =>
            attributeListSyntax
                .SelectMany(listSyntax => listSyntax.Attributes)
                .Where(a => a.isAttribute<A>())
                .Select(a => a.asAttribute<A>(semanticModel))
                .ToArray();

    }

    public static class ExpressionSyntaxExtend {

        public static string? extractValue(this ExpressionSyntax expression, SemanticModel semanticModel) {
            switch (expression) {
                // 如果是成员访问表达式（如ConstString.metadata）    
                case MemberAccessExpressionSyntax memberAccess: {
                    // 获取成员访问的符号信息    
                    ISymbol? symbol = ModelExtensions.GetSymbolInfo(semanticModel, memberAccess).Symbol;

                    // 如果符号是常量，则尝试获取其值    
                    if (symbol is IFieldSymbol fieldSymbol) {
                        if (fieldSymbol.ContainingType.TypeKind == TypeKind.Enum) {
                            return fieldSymbol.Name;
                        }

                        if (fieldSymbol.HasConstantValue) {
                            return fieldSymbol.ConstantValue.ToString();
                        }
                    }
                    break;
                }
                case LiteralExpressionSyntax literal:
                    switch (literal.Kind()) {
                        case SyntaxKind.TrueLiteralExpression:
                            return "true";
                        case SyntaxKind.FalseLiteralExpression:
                            return "false";
                        case SyntaxKind.NumericLiteralExpression:
                        case SyntaxKind.StringLiteralExpression:
                            return literal.Token.Value?.ToString();
                    }
                    break;
                case IdentifierNameSyntax identifierName: {
                    // 获取nameof表达式指向的符号  
                    var symbolInfo = ModelExtensions.GetSymbolInfo(semanticModel, identifierName);

                    // 确保获取到的是有效的本地变量、参数或成员  
                    if (symbolInfo.Symbol is not null
                        && symbolInfo.Symbol.Kind == SymbolKind.Local
                        || symbolInfo.Symbol!.Kind == SymbolKind.Parameter
                        || symbolInfo.Symbol.Kind == SymbolKind.Property
                        || symbolInfo.Symbol.Kind == SymbolKind.Field
                        || symbolInfo.Symbol.Kind == SymbolKind.Method) {
                        // nameof表达式的结果就是符号的名称  
                        return symbolInfo.Symbol.Name;
                    }
                    break;
                }
                case InvocationExpressionSyntax invocationExpressionSyntax: {

                    ExpressionSyntax expressionSyntax = invocationExpressionSyntax.Expression;

                    if (expressionSyntax is not IdentifierNameSyntax identifierNameSyntax) {
                        break;
                    }
                    if (!identifierNameSyntax.Identifier.Text.Equals("nameof")) {
                        break;
                    }

                    ArgumentListSyntax argumentListSyntax = invocationExpressionSyntax.ArgumentList;

                    SeparatedSyntaxList<ArgumentSyntax> separatedSyntaxList = argumentListSyntax.Arguments;
                    if (separatedSyntaxList.Count == 0) {
                        break;
                    }

                    ArgumentSyntax separatedSyntax = separatedSyntaxList[0];

                    string s = separatedSyntax.ToString();
                    int lastIndexOf = s.LastIndexOf('.');
                    if (lastIndexOf == -1) {
                        return s;
                    }
                    return s.Substring(lastIndexOf + 1);

                }
            }

            return null;
        }

    }

    public static class StringExtend {

        public static string toPascalCaseIdentifier(this string identifier) {
            int tailor = -1;
            for (var i = 0; i < identifier.Length; i++) {
                if (identifier[i] != '_') {
                    break;
                }
                tailor = i;
            }
            if (tailor != -1) {
                identifier = identifier.Substring(tailor + 1);
            }
            return identifier.Capitalize().Replace('.', '_');
        }

        public static string toCamelCaseIdentifier(this string identifier) {
            int tailor = -1;
            for (var i = 0; i < identifier.Length; i++) {
                if (identifier[i] != '_') {
                    break;
                }
                tailor = i;
            }
            if (tailor != -1) {
                identifier = identifier.Substring(tailor + 1);
            }
            return identifier.Decapitalize().Replace('.', '_');
        }

        public static string escapeReservedKeyword(this string identifier) {
            if (ReservedKeywords.Contains(identifier)) {
                return "@" + identifier;
            }

            return identifier;
        }
        
        /// <summary>
        /// Lowercases the first character of a given string.
        /// </summary>
        /// <param name="s">The string whose first character to lowercase.</param>
        /// <returns>The string with its first character lowercased.</returns>
        public static string Decapitalize(this string? s) {
            if (s is null || char.IsLower(s[0])) {
                return s ?? String.Empty;
            }

            return char.ToLower(s[0]) + s.Substring(1);
        }

        /// <summary>
        /// Uppercases the first character of a given string.
        /// </summary>
        /// <param name="s">The string whose first character to uppercase.</param>
        /// <returns>The string with its first character uppercased.</returns>
        public static string Capitalize(this string? s) {
            if (s is null || char.IsUpper(s[0])) {
                return s ?? String.Empty;
            }

            return char.ToUpper(s[0]) + s.Substring(1);
        }

        public static string eliminateGeneric(this string identifier) {
            string pattern = @"<[^>]+>";
            return Regex.Replace(identifier, pattern, "");
        }

        public static string format(this string s, string format) {
            switch (format) {
                case nameof(StringExtend.toCamelCaseIdentifier):
                    s = s.toCamelCaseIdentifier();
                    break;
                case nameof(StringExtend.toPascalCaseIdentifier):
                    s = s.toPascalCaseIdentifier();
                    break;
            }
            return s;
        }

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

    }

}