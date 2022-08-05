using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.CodeAnalysis.Text;

namespace ShowMeTheXaml.Avalonia.Infrastructure.Data {
    public class XamlDisplayContainer {
        private const string TargetClrNamespace = "clr-namespace:ShowMeTheXaml;assembly=ShowMeTheXaml.Avalonia";
        public XamlDisplayContainer(SourceText sourceText, Dictionary<string, string> namespaceAliases, IReadOnlyCollection<XamlDisplayInfo> xamlDisplayInfos) {
            SourceText = sourceText;
            XamlDisplayInfos = xamlDisplayInfos;
            NamespaceAliases = namespaceAliases;
        }

        public SourceText SourceText { get; }
        public Dictionary<string, string> NamespaceAliases { get; }
        public IReadOnlyCollection<XamlDisplayInfo> XamlDisplayInfos { get; }
        public string TargetClrNamespacePrefix => NamespaceAliases.First(pair => pair.Value == TargetClrNamespace).Key;
    }
}