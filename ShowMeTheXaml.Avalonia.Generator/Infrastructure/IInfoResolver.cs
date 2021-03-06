using System.Collections.Generic;
using ShowMeTheXaml.Avalonia.Infrastructure.Data;

namespace ShowMeTheXaml.Avalonia.Infrastructure
{
    internal interface IInfoResolver
    {
        XamlDisplayContainer ResolveInfos(string xaml);
    }
}