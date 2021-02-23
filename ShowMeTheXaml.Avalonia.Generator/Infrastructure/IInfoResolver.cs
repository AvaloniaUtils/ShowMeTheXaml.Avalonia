using System.Collections.Generic;

namespace ShowMeTheXaml.Avalonia.Infrastructure
{
    internal interface IInfoResolver
    {
        IReadOnlyList<XamlDisplayInfo> ResolveInfos(string xaml);
    }
}