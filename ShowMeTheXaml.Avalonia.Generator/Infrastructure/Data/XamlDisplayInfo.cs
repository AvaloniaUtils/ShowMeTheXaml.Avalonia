using JetBrains.Annotations;
using Microsoft.CodeAnalysis.Text;
using XamlX.Ast;

namespace ShowMeTheXaml.Avalonia.Infrastructure.Data {
    public struct XamlDisplayInfo {
        [CanBeNull]
        public string UniqueId { get; set; }
        public LinePosition LinePosition { get; set; }
        public XamlAstObjectNode XamlDisplayNode { get; set; }
        public string AstText { get; set; }

        public TextSpan GetDisplayStartingTagTextSpan(XamlDisplayContainer container, out LinePositionSpan linePositionSpan) {
            // prefix + ':DialogHost' length
            var tagLength = 1 + 11 + container.TargetClrNamespacePrefix.Length;
            var end = new LinePosition(LinePosition.Line, LinePosition.Character + tagLength);
            linePositionSpan = new LinePositionSpan(LinePosition, end);
            return container.SourceText.Lines.GetTextSpan(linePositionSpan);
        }
    }
}