using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShowMeTheXaml.Avalonia;

public static class ShowMeTheXamlCodeTemplatesGenerator {
    private const string XamlDisplayInternalDataTemplate = """
    using System.Collections.Generic;
    using Avalonia.Controls;
    using ShowMeTheXaml;

    namespace ShowMeTheXaml {{
        public static class XamlDisplayInternalData {{
            // <id, xamltext>
            public static Dictionary<string, XamlDisplayInstanceData> Data {{ get; }} = new Dictionary<string, XamlDisplayInstanceData>() {{
    {0}
            }};

            // <filename, <alias, namespace>>
            public static Dictionary<string, Dictionary<string, string>> NamespaceAliases = new Dictionary<string, Dictionary<string, string>>() {{
    {1}
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
                XamlDisplay.XamlFilesNamespaceAliases = NamespaceAliases;
            }}
        }}
    }}
    """;

    private const string DataEntryTemplate = """ 
                {{ {0}, new XamlDisplayInstanceData({1}, {2}) }},
    """;

    private const string NamespaceAliasesEntryTemplate = """ 
                {{ {0}, new Dictionary<string, string> {{
    {1}
                    }}
                }},
    """;

    private const string AliasEntryTemplate = """
                        {{ {0}, {1} }},
    """;

    public static string GenerateXamlDisplayInternalData(Dictionary<string, (string XamlText, string FileName, Dictionary<string, string> Aliases)> data) {
        string FormatDataEntry(KeyValuePair<string, (string XamlText, string FileName, Dictionary<string, string> Aliases)> pair)
            => string.Format(DataEntryTemplate, ToLiteral(pair.Key), ToLiteral(pair.Value.XamlText), ToLiteral(pair.Value.FileName));

        string FormatNamespaceAliasEntry(IGrouping<string, KeyValuePair<string, (string XamlText, string FileName, Dictionary<string, string> Aliases)>> pair)
            => string.Format(NamespaceAliasesEntryTemplate, ToLiteral(pair.Key), string.Join(Environment.NewLine, pair.First().Value.Aliases.Select(FormatAliasEntry)));

        string FormatAliasEntry(KeyValuePair<string, string> valuePair)
            => string.Format(AliasEntryTemplate, ToLiteral(valuePair.Key), ToLiteral(valuePair.Value));

        var dataText = string.Join(Environment.NewLine, data.Select(FormatDataEntry));
        var namespaceAliasesText = string.Join(Environment.NewLine, data.GroupBy(pair => pair.Value.FileName).Select(FormatNamespaceAliasEntry));
        return string.Format(XamlDisplayInternalDataTemplate, dataText, namespaceAliasesText);
    }

    private static string ToLiteral(string input) {
        var literal = new StringBuilder(input.Length + 2);
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
                        literal.Append(((int)c).ToString("x4"));
                    }

                    break;
            }
        }

        literal.Append("\"");
        return literal.ToString();
    }
}