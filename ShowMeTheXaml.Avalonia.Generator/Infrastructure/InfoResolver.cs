using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using ShowMeTheXaml.Avalonia.Infrastructure.Data;
using XamlX;
using XamlX.Parsers;

namespace ShowMeTheXaml.Avalonia.Infrastructure {
    internal class InfoResolver : IInfoResolver {
        private const string AvaloniaXmlnsAttribute = "Avalonia.Metadata.XmlnsDefinitionAttribute";
        private const string TargetClrNamespace = "clr-namespace:ShowMeTheXaml;assembly=ShowMeTheXaml.Avalonia";
        private readonly CSharpCompilation _compilation;

        public InfoResolver(CSharpCompilation compilation) => _compilation = compilation;

        public XamlDisplayContainer ResolveInfos(string xaml) {
            var parsed = XDocumentXamlParser.Parse(xaml, new Dictionary<string, string> {
                {XamlNamespaces.Blend2008, XamlNamespaces.Blend2008}
            });

            if (!parsed.NamespaceAliases.ContainsValue(TargetClrNamespace)) {
                return new XamlDisplayContainer("", new List<XamlDisplayInfo>());
            }

            MiniCompiler.CreateDefault(new RoslynTypeSystem(_compilation), AvaloniaXmlnsAttribute).Transform(parsed);

            var visitor = new InfoReceiver();
            parsed.Root.Visit(visitor);
            parsed.Root.VisitChildren(visitor);
            return new XamlDisplayContainer(parsed.NamespaceAliases.FirstOrDefault(pair => pair.Value == TargetClrNamespace).Key, visitor.DisplayInfos);
        }
    }
}