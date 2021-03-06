using JetBrains.Annotations;
using Microsoft.CodeAnalysis.Text;
using XamlX.Ast;

namespace ShowMeTheXaml.Avalonia.Infrastructure.Data {
    public struct XamlDisplayInfo {
        [CanBeNull]
        public string UniqueId { get; set; }
        public LinePosition LinePosition { get; set; }
        public XamlAstObjectNode XamlDisplayNode { get; set; }
    }
}