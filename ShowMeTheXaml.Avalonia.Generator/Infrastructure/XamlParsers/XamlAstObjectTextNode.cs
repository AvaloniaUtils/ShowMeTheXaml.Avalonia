using System.Linq;
using System.Xml.Linq;
using JetBrains.Annotations;
using XamlX.Ast;

namespace ShowMeTheXaml.Avalonia.Infrastructure.XamlParsers;

public class XamlAstObjectTextNode : XamlAstObjectNode {
    public XElement _element;
    [CanBeNull] private string _elementText;
    public XamlAstObjectTextNode(IXamlLineInfo lineInfo, IXamlAstTypeReference type, XElement element) : base(lineInfo, type) {
        _element = element;
    }

    public string ElementContentText => _elementText ??= ProvideElementText();
    private string ProvideElementText() {
        var dialogHostElement = RemoveAllNamespaces(_element);
        var elementText = dialogHostElement.Elements().FirstOrDefault()?.ToString().Replace("___", ":");
        if (elementText == null) {
            return dialogHostElement.FirstNode is XText
                ? dialogHostElement.FirstNode.ToString().Trim()
                : string.Empty;
        }

        var idx = elementText.LastIndexOf('\n');
        // Let's remove unnecessary indentation
        if (idx != -1) {
            var result = elementText.Substring(idx + 1);
            var whitespacesCount = result.TakeWhile(char.IsWhiteSpace).Count();
            if (whitespacesCount != 0) return elementText.Replace("\n" + new string(' ', whitespacesCount), "\n");
        }
        else {
            return elementText.Trim();
        }
        return elementText;
    }

    private static XElement RemoveAllNamespaces(XElement e) {
        var content = e.Nodes()
            .Where(node => node is not XComment)
            .Select(node => node is XElement xElement ? RemoveAllNamespaces(xElement) : node);
        var newElement = new XElement(GetNameWithNamespace(e), content);
        newElement.Add(e.Attributes());
        return newElement;
    }

    private static string GetNameWithNamespace(XElement e) {
        var prefixOfNamespace = e.GetPrefixOfNamespace(e.Name.Namespace);
        // This is done cuz xml doesn't allow to use : in name
        // WE TEMPORARY REPLACE ACTUAL NAMESPACES WITH ___ PREFIXES
        // To avoid namespaces definitions in XElement.ToString calls
        return prefixOfNamespace == null ? e.Name.LocalName : $"{prefixOfNamespace}___{e.Name.LocalName}";
    }
}