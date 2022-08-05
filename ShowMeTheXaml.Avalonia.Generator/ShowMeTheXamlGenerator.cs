using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using ShowMeTheXaml.Avalonia.Infrastructure;

namespace ShowMeTheXaml.Avalonia {
    [Generator]
    public class ShowMeTheXamlGenerator : ISourceGenerator {
        private CSharpCompilation _compilation;
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
            _compilation = (CSharpCompilation)context.Compilation;
            Dictionary<string, string> codeDictionary = new Dictionary<string, string>();
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
                var infoResolver = new InfoResolver(_compilation);
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
                        codeDictionary.Add(info.UniqueId, info.AstText);
                    }
                }
            }

            context.AddSource("XamlDisplayInternalData.g.cs", SourceText.From(GenerateSources(codeDictionary), Encoding.UTF8));
        }

        internal static void CallDebugger() {
            if (Debugger.IsAttached) {
                Debugger.Break();
            }
            else {
                Debugger.Launch();
            }
        }

        private string GenerateSources(Dictionary<string, string> codeDictionary) {
            var values = string.Join("\n", codeDictionary.Select(pair => $"            {{\"{pair.Key}\", {ToLiteral(pair.Value)}}},"));
            return
                $@"using System.Collections.Generic;
using System.Collections.Generic;
using Avalonia.Controls;
using ShowMeTheXaml;

namespace ShowMeTheXaml {{
    public static class XamlDisplayInternalData {{
        public static Dictionary<string, string> Data {{ get; }} = new Dictionary<string, string>() {{
{values}
        }};

        /// <summary>
        /// Loads data for xaml displays
        /// </summary>
        public static TAppBuilder UseXamlDisplay<TAppBuilder>(this TAppBuilder builder)
            where TAppBuilder : AppBuilderBase<TAppBuilder>, new()
        {{
            RegisterXamlDisplayData();
            return builder;
        }}

        /// <summary>
        /// Loads data for xaml displays
        /// </summary>
        public static void RegisterXamlDisplayData()
        {{
            XamlDisplay.DisplayContent = Data;
        }}
    }}
}}";
        }

        static string ToLiteral(string input) {
            StringBuilder literal = new StringBuilder(input.Length + 2);
            literal.Append("\"");
            foreach (var c in input) {
                switch (c) {
                    case '\'':
                        literal.Append(@"\'");
                        break;
                    case '\"':
                        literal.Append("\\\"");
                        break;
                    case '\\':
                        literal.Append(@"\\");
                        break;
                    case '\0':
                        literal.Append(@"\0");
                        break;
                    case '\a':
                        literal.Append(@"\a");
                        break;
                    case '\b':
                        literal.Append(@"\b");
                        break;
                    case '\f':
                        literal.Append(@"\f");
                        break;
                    case '\n':
                        literal.Append(@"\n");
                        break;
                    case '\r':
                        literal.Append(@"\r");
                        break;
                    case '\t':
                        literal.Append(@"\t");
                        break;
                    case '\v':
                        literal.Append(@"\v");
                        break;
                    default:
                        // ASCII printable character
                        if (c >= 0x20 && c <= 0x7e) {
                            literal.Append(c);
                            // As UTF16 escaped character
                        }
                        else {
                            literal.Append(@"\u");
                            literal.Append(((int) c).ToString("x4"));
                        }

                        break;
                }
            }

            literal.Append("\"");
            return literal.ToString();
        }
    }
}