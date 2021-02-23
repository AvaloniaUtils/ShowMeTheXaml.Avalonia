using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using XamlX;
using XamlX.Parsers;

namespace ShowMeTheXaml.Avalonia.Infrastructure
{
    internal class InfoResolver : IInfoResolver
    {
        private const string AvaloniaXmlnsAttribute = "Avalonia.Metadata.XmlnsDefinitionAttribute";
        private readonly CSharpCompilation _compilation;

        public InfoResolver(CSharpCompilation compilation) => _compilation = compilation;

        public IReadOnlyList<XamlDisplayInfo> ResolveInfos(string xaml)
        {
            var parsed = XDocumentXamlParser.Parse(xaml, new Dictionary<string, string>
            {
                {XamlNamespaces.Blend2008, XamlNamespaces.Blend2008}
            });

            MiniCompiler
                .CreateDefault(new RoslynTypeSystem(_compilation), AvaloniaXmlnsAttribute)
                .Transform(parsed);
            
            var visitor = new InfoReceiver();
            parsed.Root.Visit(visitor);
            parsed.Root.VisitChildren(visitor);
            return visitor.Controls;
        }
    }
}