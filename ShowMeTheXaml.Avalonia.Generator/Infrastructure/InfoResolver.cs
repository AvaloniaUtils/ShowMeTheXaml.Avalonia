using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using ShowMeTheXaml.Avalonia.Infrastructure.Data;
using ShowMeTheXaml.Avalonia.Infrastructure.XamlParsers;
using XamlX;

namespace ShowMeTheXaml.Avalonia.Infrastructure {
    internal class InfoResolver : IInfoResolver {
        private const string AvaloniaXmlnsAttribute = "Avalonia.Metadata.XmlnsDefinitionAttribute";
        private const string TargetClrNamespace = "clr-namespace:ShowMeTheXaml;assembly=ShowMeTheXaml.Avalonia";
        private readonly CSharpCompilation _compilation;
        private readonly Dictionary<string, string> _compatibilityMappings = new() { {XamlNamespaces.Blend2008, XamlNamespaces.Blend2008} };

        public InfoResolver(CSharpCompilation compilation) => _compilation = compilation;

        public XamlDisplayContainer ResolveInfos(SourceText sourceText) {
            var parsed = XdXDocumentXamlParser.Parse(sourceText.ToString(), _compatibilityMappings);

            if (!parsed.NamespaceAliases.ContainsValue(TargetClrNamespace)) { 
                return new XamlDisplayContainer(sourceText, new Dictionary<string, string>(), new List<XamlDisplayInfo>());
            }
            
            MiniCompiler.CreateDefault(new RoslynTypeSystem(_compilation), AvaloniaXmlnsAttribute).Transform(parsed);

            var visitor = new InfoReceiver();
            parsed.Root.Visit(visitor);
            parsed.Root.VisitChildren(visitor);
            return new XamlDisplayContainer(sourceText, parsed.NamespaceAliases, visitor.DisplayInfos);
        }
    }
}