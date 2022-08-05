using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.Text;
using ShowMeTheXaml.Avalonia.Infrastructure.Data;
using ShowMeTheXaml.Avalonia.Infrastructure.XamlParsers;
using XamlX.Ast;

namespace ShowMeTheXaml.Avalonia.Infrastructure {
    /// <remarks>
    /// This class was written to simplify getting text into XamlDisplay and to contain hacks relevant here
    /// DO NOT USE IT IN YOUR PROJECT
    /// </remarks>
    internal sealed class InfoReceiver : IXamlAstVisitor {
        private readonly List<XamlDisplayInfo> _items = new List<XamlDisplayInfo>();

        public List<XamlDisplayInfo> DisplayInfos => _items;

        public IXamlAstNode Visit(IXamlAstNode node) {
            if (node is XamlAstObjectTextNode objectNode) {
                var clrType = objectNode.Type.GetClrType();
                if (clrType.FullName != "ShowMeTheXaml.XamlDisplay")
                    return node;

                if (_items.Any(displayInfo => displayInfo.XamlDisplayNode == node))
                    return node;

                var info = new XamlDisplayInfo {LinePosition = new LinePosition(node.Line - 1, node.Position - 1), XamlDisplayNode = objectNode};
                foreach (var child in objectNode.Children) {
                    if (child is not XamlAstXamlPropertyValueNode {Property: XamlAstNamePropertyReference {Name: "UniqueId"}} propertyValueNode ||
                        propertyValueNode.Values.Count <= 0 || propertyValueNode.Values[0] is not XamlAstTextNode text) continue;
                    info.UniqueId = text.Text;
                    // Save original text (custom implementation)
                    info.AstText = objectNode.ElementContentText;
                    break;
                }

                _items.Add(info);

                return node;
            }

            return node;
        }

        public void Push(IXamlAstNode node) { }

        public void Pop() { }
    }
}