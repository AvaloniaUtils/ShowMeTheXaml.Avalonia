using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ShowMeTheXaml;

/// <summary>
/// Experimental AvaloniaRuntimeXamlLoader helper, allows to compile xaml with separated namespace declarations
/// </summary>
public static class AvaloniaRuntimeXamlLoaderHelper {
    private const string XamlTemplate = """
    <ContentControl {0}> 
    {1}
    </ContentControl>
    """;

    public static object Parse(string content, Dictionary<string, string> namespaces) {
        string FormatNamespace(KeyValuePair<string, string> pair)
            => string.IsNullOrEmpty(pair.Key) ? $"xmlns=\"{pair.Value}\"" : $"xmlns:{pair.Key}=\"{pair.Value}\"";

        var namespacesString = string.Join(" ", namespaces.Select(FormatNamespace));
        return Parse(content, namespacesString);
    }

    public static object Parse(string content, string namespaces) {
        try {
            // Try to insert namespaces before closing first tag
            var endTagIndex = content.IndexOf('>');
            // No closing symbol (>), this is just a string probably
            if (endTagIndex == -1) return content;

            var slashAndMoreThenIndex = content.IndexOf("/>", StringComparison.InvariantCulture);
            // This is a probably one tag without content
            if (slashAndMoreThenIndex != -1) endTagIndex = Math.Min(endTagIndex, slashAndMoreThenIndex);
            var finalXaml = content.Insert(endTagIndex, " " + namespaces);
            return AvaloniaRuntimeXamlLoader.Parse<object>(finalXaml);
        }
        catch (Exception) {
            // Falling back to old method
            return ContentControlWrapperParse(content, namespaces);
        }
    }

    private static object ContentControlWrapperParse(string content, string namespaces) {
        var finalXaml = string.Format(XamlTemplate, namespaces, content);
        try {
            var contentControl = AvaloniaRuntimeXamlLoader.Parse<ContentControl>(finalXaml);
            return contentControl.Content;
        }
        catch (XmlException e) {
            // cut line info
            var lastIndexOfDot = e.Message.LastIndexOf('.', e.Message.Length - 2);
            var meaningfulMessage = lastIndexOfDot == -1
                ? e.Message
                : e.Message.Substring(0, lastIndexOfDot + 1);

            throw new XmlException(meaningfulMessage, e.InnerException, e.LineNumber - 1, e.LinePosition);
        }
    }
}