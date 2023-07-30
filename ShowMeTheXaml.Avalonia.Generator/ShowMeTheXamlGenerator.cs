using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using ShowMeTheXaml.Avalonia.Infrastructure;

namespace ShowMeTheXaml.Avalonia {
    [Generator]
    public class ShowMeTheXamlGenerator : ISourceGenerator {
        public void Initialize(GeneratorInitializationContext context) { }

        public void Execute(GeneratorExecutionContext context) {
            try {
                ExecuteInternal(context);
            }
            catch (Exception e) {
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "XD0000",
                        $"ShowMeTheXaml.Generator exited with exception",
                        $"ShowMeTheXaml.Generator throw exception. Report it on GitHub and attach project. Exception type: {e}",
                        "General",
                        DiagnosticSeverity.Error,
                        true
                    ),
                    Location.None));
            }
        }
        private void ExecuteInternal(GeneratorExecutionContext context) {
            var codeDictionary = new Dictionary<string, (string XamlText, string FileName, Dictionary<string, string> Aliases)>();
            var files = context.AdditionalFiles.Where(text => text.Path.EndsWith(".xaml") || text.Path.EndsWith(".axaml")).ToList();
            if (files.Count == 0) {
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "XD0003",
                        "No xaml files detected. Consider read \"Getting started\" at our github.",
                        "Add all xaml and axaml files as AdditionalFiles in csproj",
                        "Usage",
                        DiagnosticSeverity.Error,
                        true
                    ),
                    Location.None));
            }

            foreach (var markupFile in files) {
                var infoResolver = new InfoResolver((CSharpCompilation)context.Compilation);
                var sources = markupFile.GetText() ?? throw new ArgumentNullException("markupFile.GetText()");
                var xamlDisplayContainer = infoResolver.ResolveInfos(sources);
                var xamlDisplayInfos = xamlDisplayContainer.XamlDisplayInfos
                    .OrderByDescending(info => info.LinePosition.Line)
                    .ThenByDescending(info => info.LinePosition.Line)
                    .ToList();

                foreach (var info in xamlDisplayInfos) {
                    if (info.UniqueId == null) {
                        context.ReportDiagnostic(Diagnostic.Create(
                            new DiagnosticDescriptor(
                                "XD0001",
                                "UniqueId not set",
                                "Each XamlDisplay must have a UniqueId property",
                                "Usage",
                                DiagnosticSeverity.Error,
                                true
                            ),
                            Location.Create(markupFile.Path, info.GetDisplayStartingTagTextSpan(xamlDisplayContainer, out var linePositionSpan), linePositionSpan)
                        ));
                    }
                    else if (codeDictionary.ContainsKey(info.UniqueId)) {
                        context.ReportDiagnostic(Diagnostic.Create(
                            new DiagnosticDescriptor(
                                "XD0002",
                                "UniqueId duplicate",
                                "Each XamlDisplay must have a unique value in UniqueId property",
                                "Usage",
                                DiagnosticSeverity.Error,
                                true
                            ),
                            Location.Create(markupFile.Path, info.GetDisplayStartingTagTextSpan(xamlDisplayContainer, out var linePositionSpan), linePositionSpan)
                        ));
                    }
                    else {
                        codeDictionary.Add(info.UniqueId, (info.AstText, Path.GetFileName(markupFile.Path), xamlDisplayContainer.NamespaceAliases));
                    }
                }
            }

            var generatedCode = ShowMeTheXamlCodeTemplatesGenerator.GenerateXamlDisplayInternalData(codeDictionary);
            var sourceText = SourceText.From(generatedCode, Encoding.UTF8);
            context.AddSource("XamlDisplayInternalData.g.cs", sourceText);
        }

        internal static void CallDebugger() {
            if (Debugger.IsAttached) {
                Debugger.Break();
            }
            else {
                Debugger.Launch();
            }
        }
    }
}