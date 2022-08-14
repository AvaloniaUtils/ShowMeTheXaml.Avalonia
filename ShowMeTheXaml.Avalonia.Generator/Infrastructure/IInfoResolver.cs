using Microsoft.CodeAnalysis.Text;
using ShowMeTheXaml.Avalonia.Infrastructure.Data;

namespace ShowMeTheXaml.Avalonia.Infrastructure
{
    internal interface IInfoResolver
    {
        XamlDisplayContainer ResolveInfos(SourceText sourceText);
    }
}