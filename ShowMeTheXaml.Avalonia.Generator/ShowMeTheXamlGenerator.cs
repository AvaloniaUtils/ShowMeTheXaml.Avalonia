using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using ShowMeTheXaml.Avalonia.Infrastructure;
using XamlX;
using XamlX.Parsers;

namespace ShowMeTheXaml.Avalonia {
    [Generator]
    public class ShowMeTheXamlGenerator : ISourceGenerator {
        private CSharpCompilation _compilation;
        private const string AvaloniaXmlnsAttribute = "Avalonia.Metadata.XmlnsDefinitionAttribute";
        public void Initialize(GeneratorInitializationContext context) { }

        public void Execute(GeneratorExecutionContext context) {
            _compilation = (CSharpCompilation) context.Compilation;
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
                var xamlDisplayInfos = ExtractFromXaml(markupFile)
                                      .OrderByDescending(info => info.LinePosition.Line).ThenByDescending(info => info.LinePosition.Line)
                                      .ToList();
                var sources = markupFile.GetText() ?? throw new ArgumentNullException("markupFile.GetText()");
                var processableSourceText = sources;

                //Remove all comments
                var sourceText = processableSourceText!.ToString();
                int commentStart;
                while ((commentStart = sourceText.IndexOf("<!--", StringComparison.Ordinal)) != -1) {
                    var commentEnd = sourceText.IndexOf("-->", StringComparison.Ordinal);
                    if (commentEnd == -1) commentEnd = sourceText.Length;
                    else commentEnd += 3;
                    processableSourceText = HideSourceText(commentStart, commentEnd, ref processableSourceText);
                    sourceText = processableSourceText!.ToString();
                }

                foreach (var info in xamlDisplayInfos) {
                    var xamlDisplayPosition = ParsePositions(processableSourceText, info.LinePosition);

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
                            Location.Create(markupFile.Path,
                                sources.Lines.GetTextSpan(xamlDisplayPosition.XamlDisplaySpan),
                                xamlDisplayPosition.XamlDisplaySpan)));
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
                            Location.Create(markupFile.Path,
                                sources.Lines.GetTextSpan(xamlDisplayPosition.XamlDisplaySpan),
                                xamlDisplayPosition.XamlDisplaySpan)));
                    }
                    else {
                        codeDictionary.Add(info.UniqueId, GetContent(sources, xamlDisplayPosition));
                    }

                    var displayStartIndex = processableSourceText.Lines.GetPosition(xamlDisplayPosition.XamlDisplaySpan.Start);
                    var displayEndIndex = processableSourceText.Lines.GetPosition(xamlDisplayPosition.XamlDisplaySpan.End);
                    HideSourceText(displayStartIndex, displayEndIndex, ref processableSourceText);
                }
            }

            context.AddSource("XamlDisplayInternalData.g.cs", SourceText.From(GenerateSources(codeDictionary), Encoding.UTF8));

            SourceText HideSourceText(int commentStart, int commentEnd, ref SourceText processableSourceText) {
                string sourceText = processableSourceText.ToString();
                var commentText = sourceText.Substring(commentStart, commentEnd - commentStart);
                var newLinesCount = commentText.Length - commentText.Replace(Environment.NewLine, string.Empty).Length;
                processableSourceText = processableSourceText.WithChanges(new TextChange(
                        TextSpan.FromBounds(commentStart, commentEnd),
                        new string('\n', newLinesCount) + new string('%', commentText.Length - newLinesCount)
                    )
                );
                return processableSourceText;
            }
        }

        private static string GetContent(SourceText sources, XamlDisplayPosition xamlDisplayPosition) {
            var content = sources.GetSubText(sources.Lines.GetTextSpan(xamlDisplayPosition.ContentSpan)).ToString();

            // Remove empty lines
            content = Regex.Replace(content, @"^\s*$[\r\n]*", string.Empty, RegexOptions.Multiline);
            content = content.TrimEnd('\r', '\n', ' ');

            // Remove unnecessary indentation
            var lines = content.Split('\n', '\r').Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
            var minimum = int.MaxValue;
            foreach (var line in lines) {
                if (line.Length == 0) continue;
                minimum = Math.Min(minimum, line.Length - line.TrimStart().Length);
                if (minimum == 0) break;
            }

            if (minimum != 0 && minimum != int.MaxValue) {
                content = string.Join(Environment.NewLine, lines.Select(s => s.Substring(minimum)));
            }


            return content;
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
using Avalonia.Controls;
using ShowMeTheXaml.Avalonia;

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

        private List<XamlDisplayInfo> ExtractFromXaml(AdditionalText xamlFile) {
            var parsed = XDocumentXamlParser.Parse(xamlFile.GetText()!.ToString(), new Dictionary<string, string> {
                {XamlNamespaces.Blend2008, XamlNamespaces.Blend2008}
            });

            MiniCompiler
               .CreateDefault(new RoslynTypeSystem(_compilation), AvaloniaXmlnsAttribute)
               .Transform(parsed);

            var visitor = new InfoReceiver();
            parsed.Root.Visit(visitor);
            parsed.Root.VisitChildren(visitor);
            return visitor.Controls;
        }

        private XamlDisplayPosition ParsePositions(SourceText sourceText, LinePosition startLinePosition) {
            var text = sourceText!.ToString();

            // Extracting element fullname, eg avalonia:XamlDisplay
            var startPosition = sourceText.Lines.GetPosition(startLinePosition);
            var regexMatch = Regex.Match(text.Substring(startPosition), @"([^ \/>]*)", RegexOptions.Singleline);
            var elementFullnameEscaped = Regex.Escape(regexMatch.Groups[0].Value);

            var pattern =
                $@"((?<start><{elementFullnameEscaped}[^\/]*?>)(?<content>.*)(?<end><\/{elementFullnameEscaped}>))|(?<startend><{elementFullnameEscaped}.*?\/>)";
            var regexText = text.Substring(startPosition - 1);
            var match = Regex.Match(regexText, pattern, RegexOptions.Singleline);
            if (match.Groups["startend"].Success) {
                var endLinePosition = sourceText.Lines.GetLinePosition(startPosition + match.Groups["startend"].Value.Length - 1);
                return new XamlDisplayPosition {
                    OpeningTag = new LinePositionSpan(
                        sourceText.Lines.GetLinePosition(startPosition - 1),
                        endLinePosition
                    ),
                    ClosingTag = new LinePositionSpan(endLinePosition, endLinePosition),
                    ContentSpan = new LinePositionSpan(endLinePosition, endLinePosition)
                };
            }


            var openingTagStart = startPosition - 1;
            var openingTagEnd = openingTagStart + match.Groups["start"].Value.Length;
            var contentEnd = openingTagEnd + match.Groups["content"].Value.Length;
            var endingTagEnd = contentEnd + match.Groups["end"].Value.Length;
            return new XamlDisplayPosition {
                OpeningTag = new LinePositionSpan(
                    sourceText.Lines.GetLinePosition(openingTagStart),
                    sourceText.Lines.GetLinePosition(openingTagEnd)
                ),
                ContentSpan = new LinePositionSpan(
                    sourceText.Lines.GetLinePosition(openingTagEnd),
                    sourceText.Lines.GetLinePosition(contentEnd)
                ),
                ClosingTag = new LinePositionSpan(
                    sourceText.Lines.GetLinePosition(contentEnd),
                    sourceText.Lines.GetLinePosition(endingTagEnd)
                )
            };
        }
    }
}