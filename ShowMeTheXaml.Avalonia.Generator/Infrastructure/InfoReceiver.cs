using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.Text;
using ShowMeTheXaml.Avalonia.Infrastructure.Data;
using XamlX.Ast;

namespace ShowMeTheXaml.Avalonia.Infrastructure {
    internal sealed class InfoReceiver : IXamlAstVisitor {
        private readonly List<XamlDisplayInfo> _items = new List<XamlDisplayInfo>();

        public List<XamlDisplayInfo> DisplayInfos => _items;

        public IXamlAstNode Visit(IXamlAstNode node) {
            if (node is XamlAstObjectNode objectNode) {
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