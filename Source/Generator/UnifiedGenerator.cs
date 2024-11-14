using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Til.Lombok.Generator {

    [Generator]
    public sealed class UnifiedGenerator : IIncrementalGenerator {

        private static readonly string AttributeName = typeof(ILombokAttribute).FullName!;

        private static List<GeneratorComponent> generatorComponentList = null!;

        public void Initialize(IncrementalGeneratorInitializationContext context) {
            generatorComponentList ??= typeof(UnifiedGenerator).Assembly.GetTypes()
                .Where(t => t.GetCustomAttribute<GeneratorComponentAttribute>() != null)
                .Select(Activator.CreateInstance)
                .OfType<GeneratorComponent>()
                .ToList();

            IncrementalValuesProvider<GeneratorResult> sources = context.SyntaxProvider.ForAttributeWithMetadataName(AttributeName, IsCandidate, Transform);
            context.AddSources(sources);
        }

        private bool IsCandidate(SyntaxNode node, CancellationToken cancellationToken) => node is ClassDeclarationSyntax classDeclarationSyntax && !classDeclarationSyntax.IsNestedType();

        private GeneratorResult Transform(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken) {
            ClassDeclarationSyntax contextTargetNode = (ClassDeclarationSyntax)context.TargetNode;

            SemanticModel semanticModel = context.SemanticModel;

            if (!contextTargetNode.TryValidateType(out NameSyntax? @namespace, out Diagnostic? diagnostic)) {
                return new GeneratorResult(diagnostic!);
            }

            List<MemberDeclarationSyntax> partialClassMemberDeclarationSyntaxList = new List<MemberDeclarationSyntax>();
            List<MemberDeclarationSyntax> namespaceMemberDeclarationSyntaxList = new List<MemberDeclarationSyntax>();
            List<MemberDeclarationSyntax> compilationMemberDeclarationSyntaxList = new List<MemberDeclarationSyntax>();

            BasicsContext basicsContext = new BasicsContext
            (
                contextTargetNode,
                @namespace,
                semanticModel,
                context,
                cancellationToken,
                partialClassMemberDeclarationSyntaxList,
                namespaceMemberDeclarationSyntaxList,
                compilationMemberDeclarationSyntaxList
            );

            generatedPartialClass(basicsContext);

            ClassDeclarationSyntax partialClass = contextTargetNode.CreateNewPartialClass().AddMembers(partialClassMemberDeclarationSyntaxList.ToArray());
            NamespaceDeclarationSyntax namespaceDeclarationSyntax = NamespaceDeclaration(@namespace).AddMembers(namespaceMemberDeclarationSyntaxList.Concat(new[] { partialClass }).ToArray());
            CompilationUnitSyntax compilationUnitSyntax = CompilationUnit()
                .WithUsings(contextTargetNode.GetUsings())
                .AddMembers(compilationMemberDeclarationSyntaxList.Concat(new[] { namespaceDeclarationSyntax }).ToArray())
                .NormalizeWhitespace();

            return new GeneratorResult
            (
                contextTargetNode.GetHintName
                (
                    @namespace!
                ),
                SourceText.From
                (
                    compilationUnitSyntax.ToFullString(),
                    Encoding.UTF8
                )
            );
        }

        public static void generatedPartialClass(BasicsContext basicsContext) {

            foreach (GeneratorComponent generatorComponent in generatorComponentList) {
                try {
                    generatorComponent.fill(basicsContext);
                }
                catch (Exception e) {
                    e.PrintExceptionSummaryAndStackTrace();
                }
            }

            basicsContext.partialClassMemberDeclarationSyntaxList.AddRange
            (
                basicsContext.contextTargetNode.Members
                    .OfType<ClassDeclarationSyntax>()
                    .Where(c => c.AttributeLists.tryGetSpecifiedAttribute(nameof(ILombokAttribute)).Any())
                    .Select
                    (
                        c => {
                            List<MemberDeclarationSyntax> partialClassMemberDeclarationSyntaxList = new List<MemberDeclarationSyntax>();
                            BasicsContext context = new BasicsContext
                            (
                                c,
                                basicsContext.contextNamespaceNameSyntax,
                                basicsContext.semanticModel,
                                basicsContext.context,
                                basicsContext.cancellationToken,
                                partialClassMemberDeclarationSyntaxList,
                                basicsContext.namespaceMemberDeclarationSyntaxList,
                                basicsContext.compilationMemberDeclarationSyntaxList
                            ) {
                                nestContext = basicsContext
                            };
                            generatedPartialClass
                            (
                                context
                            );
                            return c.CreateNewPartialClass().AddMembers(partialClassMemberDeclarationSyntaxList.ToArray());
                        }
                    )
                    .OfType<MemberDeclarationSyntax>()
                    .ToArray()
            );
        }

    }

}
