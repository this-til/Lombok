using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Scripting;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Til.Lombok.Generator {

    [Generator]
    public sealed class UnifiedGenerator : IIncrementalGenerator {

        private static readonly string AttributeName = typeof(ILombokAttribute).FullName!;

        public static List<GeneratorComponent> generatorComponentList = new List<GeneratorComponent>();

        public static List<IncrementComponent> incrementComponentList = new List<IncrementComponent>();

        public void Initialize(IncrementalGeneratorInitializationContext context) {
            generatorComponentList.Clear();
            generatorComponentList.AddRange
            (
                typeof(UnifiedGenerator).Assembly.GetTypes()
                    .Where(t => t.GetCustomAttribute<GeneratorComponentAttribute>() != null)
                    .Select(Activator.CreateInstance)
                    .OfType<GeneratorComponent>()
                    .ToList()
            );
            incrementComponentList.Clear();
            incrementComponentList.AddRange
            (
                typeof(UnifiedGenerator).Assembly.GetTypes()
                    .Where(t => t.GetCustomAttribute<IncrementComponentAttribute>() != null)
                    .Select(Activator.CreateInstance)
                    .OfType<IncrementComponent>()
                    .ToList()
            );

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
                @namespace!,
                semanticModel,
                context,
                cancellationToken,
                partialClassMemberDeclarationSyntaxList,
                namespaceMemberDeclarationSyntaxList,
                compilationMemberDeclarationSyntaxList
            );

            generatedPartialClass(basicsContext, true);

            ClassDeclarationSyntax partialClass = contextTargetNode.CreateNewPartialClass().AddMembers(partialClassMemberDeclarationSyntaxList.ToArray());
            NamespaceDeclarationSyntax namespaceDeclarationSyntax = NamespaceDeclaration(@namespace!).AddMembers(namespaceMemberDeclarationSyntaxList.Concat(new[] { partialClass }).ToArray());
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

        public static void generatedPartialClass(BasicsContext basicsContext, bool protogenetic) {

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
                                context,
                                protogenetic
                            );
                            return c.CreateNewPartialClass().AddMembers(partialClassMemberDeclarationSyntaxList.ToArray());
                        }
                    )
                    .OfType<MemberDeclarationSyntax>()
                    .ToArray()
            );

            if (protogenetic) {

                List<ClassDeclarationSyntax> classDeclarationSyntaxes = (basicsContext.nestContext?.partialClassMemberDeclarationSyntaxList ?? basicsContext.namespaceMemberDeclarationSyntaxList)
                    .OfType<ClassDeclarationSyntax>()
                    .Where(c => c.getHasGenericName().Equals(basicsContext.className))
                    .ToList();
                classDeclarationSyntaxes.Add(basicsContext.contextTargetNode);

                List<MemberDeclarationSyntax> memberDeclarationSyntaxes = basicsContext.partialClassMemberDeclarationSyntaxList
                    .Concat(classDeclarationSyntaxes.SelectMany(c => c.Members))
                    .ToList();

                IncrementContext incrementContext = new IncrementContext
                (
                    basicsContext,
                    classDeclarationSyntaxes,
                    memberDeclarationSyntaxes
                );

                foreach (IncrementComponent incrementComponent in incrementComponentList) {
                    incrementComponent.fill(incrementContext);
                }

            }

        }

    }

    /*
    [Generator]
    public sealed class ScriptGenerator : ISourceGenerator {

        public const string CsGeneratorScript = "CsGeneratorScript";

        public void Initialize(GeneratorInitializationContext context) {

            UnifiedGenerator.generatorComponentList.Clear();
            UnifiedGenerator.generatorComponentList.AddRange
            (
                typeof(UnifiedGenerator).Assembly.GetTypes()
                    .Where(t => t.GetCustomAttribute<GeneratorComponentAttribute>() != null)
                    .Select(Activator.CreateInstance)
                    .OfType<GeneratorComponent>()
                    .ToList()
            );

            Console.WriteLine($"Method: Initialize");
        }

        public void Execute(GeneratorExecutionContext context) {
            Console.WriteLine($"Method: Execute");
            string[] sourceTexts = context.AdditionalFiles
                .Where(file => context.AnalyzerConfigOptions.GetOptions(file).TryGetValue($"build_metadata.AdditionalFiles.{CsGeneratorScript}", out _))
                .Select(file => file.GetText()?.ToString())
                .Where(s => s != null)
                .ToArray()!;

            if (sourceTexts.Length == 0) {
                return;
            }

            /*Script<object>? script = CSharpScript.Create<object>
            (
                sourceTexts.FirstOrDefault(),
                ScriptOptions.Default
                    .WithReferences(AppDomain.CurrentDomain.GetAssemblies())
            );
            script.Compile();
            ScriptState<object> scriptState = script.RunAsync().Result;
            ScriptState<object> result = scriptState.ContinueWithAsync<object>("new BuffGenerationComponent()").Result;
            object resultReturnValue = result.ReturnValue;
            GeneratorComponent generatorComponent = (GeneratorComponent) resultReturnValue ;
            #1#

            Type assemblyType = typeof(Assembly);
            MethodInfo loadMethodInfo = assemblyType.GetMethod("Load", new[] { typeof(byte[]) })!;
            MethodInfo loadFileMethodInfo = assemblyType.GetMethod("loadFile", new[] { typeof(string) })!;

            Dictionary<string, Assembly> dictionary = AppDomain.CurrentDomain.GetAssemblies().ToDictionary(a => a.GetName().Name, a => a);
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) => {
                /*string name = args.Name.Substring(0, args.Name.IndexOf(','));
                if (dictionary.TryGetValue(name, out Assembly? expression)) {
                    return expression;
                }
                return null;#1#

                if (args.Name.StartsWith("Lombok,")) {
                    //return typeof(ScriptGenerator).Assembly;
                    //return (Assembly)loadFileMethodInfo.Invoke(null, new object[] {  typeof(ScriptGenerator).Assembly.Location }) ;
                    return AppDomain.CurrentDomain.Load(typeof(ScriptGenerator).Assembly.Location);
                }
                return null;
            };

            CSharpCompilationOptions options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
            CSharpCompilation compilation = CSharpCompilation.Create
            (
                "ScriptAssembly",
                sourceTexts.Select(s => CSharpSyntaxTree.ParseText(s)),
                AppDomain.CurrentDomain.GetAssemblies().Select(assembly => MetadataReference.CreateFromFile(assembly.Location)),
                options
            );

            using MemoryStream ms = new MemoryStream();

            EmitResult result = compilation.Emit(ms);
            if (!result.Success) {
                foreach (Diagnostic diagnostic in result.Diagnostics) {
                    Console.WriteLine(diagnostic.ToString());
                }
                return;
            }

            /*Assembly invoke = (Assembly)loadMethodInfo.Invoke(null, new object[] { ms.ToArray() });#1#
            Assembly invoke = AppDomain.CurrentDomain.Load(ms.ToArray());

            UnifiedGenerator.generatorComponentList.AddRange
            (
                invoke.GetTypes()
                    .Where(t => t.GetCustomAttribute<GeneratorComponentAttribute>() != null)
                    .Select(Activator.CreateInstance)
                    .OfType<GeneratorComponent>()
                    .ToList()
            );

        }

    }*/

}
