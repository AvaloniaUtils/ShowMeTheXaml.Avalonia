using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ShowMeTheXaml.Avalonia.Infrastructure.Data {
    public class XamlDisplayContainer {
        public XamlDisplayContainer(string namespaceAlias, IReadOnlyCollection<XamlDisplayInfo> xamlDisplayInfos) {
            XamlDisplayInfos = xamlDisplayInfos;
            NamespaceAlias = namespaceAlias;
        }

        public string NamespaceAlias { get; }
        public IReadOnlyCollection<XamlDisplayInfo> XamlDisplayInfos { get; }
    }
}