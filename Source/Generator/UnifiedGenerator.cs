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

            Ptr<ClassDeclarationSyntax> partialClass = new Ptr<ClassDeclarationSyntax>
            (
                contextTargetNode.CreateNewPartialClass()
            );
            Ptr<NamespaceDeclarationSyntax> namespaceDeclarationSyntax = new Ptr<NamespaceDeclarationSyntax>
            (
                NamespaceDeclaration
                (
                    @namespace!
                )
            );
            Ptr<CompilationUnitSyntax> compilationUnitSyntax = new Ptr<CompilationUnitSyntax>
            (
                CompilationUnit()
                    .WithUsings
                    (
                        contextTargetNode.GetUsings()
                    )
            );

            BasicsContext basicsContext = new BasicsContext
            (
                contextTargetNode,
                semanticModel,
                context,
                cancellationToken,
                partialClass,
                namespaceDeclarationSyntax,
                compilationUnitSyntax
            );

            generatedPartialClass(basicsContext);

            namespaceDeclarationSyntax.value = basicsContext.namespaceDeclarationSyntax.value.AddMembers
            (
                basicsContext.partialClass
            );

            compilationUnitSyntax.value = basicsContext.compilationUnitSyntax.value.AddMembers
                (
                    basicsContext.namespaceDeclarationSyntax
                )
                .NormalizeWhitespace();

            return new GeneratorResult
            (
                contextTargetNode.GetHintName
                (
                    @namespace!
                ),
                SourceText.From
                (
                    basicsContext.compilationUnitSyntax.value.ToFullString(),
                    Encoding.UTF8
                )
            );
        }

        private void generatedPartialClass(BasicsContext basicsContext) {

            foreach (GeneratorComponent generatorComponent in generatorComponentList) {
                try {
                    generatorComponent.fill(basicsContext);
                }
                catch (Exception e) {
                    e.PrintExceptionSummaryAndStackTrace();
                }
            }

            basicsContext.partialClass.value = basicsContext.partialClass.value.AddMembers
            (
                basicsContext.contextTargetNode.Members
                    .OfType<ClassDeclarationSyntax>()
                    .Where(c => c.AttributeLists.tryGetSpecifiedAttribute(nameof(ILombokAttribute)).Any())
                    .Select
                    (
                        c => {
                            Ptr<ClassDeclarationSyntax> partialClass = new Ptr<ClassDeclarationSyntax>(c.CreateNewPartialClass());
                            BasicsContext context = new BasicsContext
                            (
                                c,
                                basicsContext.semanticModel,
                                basicsContext.context,
                                basicsContext.cancellationToken,
                                partialClass,
                                basicsContext.namespaceDeclarationSyntax,
                                basicsContext.compilationUnitSyntax
                            ) {
                                nestContext = basicsContext
                            };
                            generatedPartialClass
                            (
                                context
                            );
                            return context.partialClass.value;
                        }
                    )
                    .OfType<MemberDeclarationSyntax>()
                    .ToArray()
            );
        }

    }

}
