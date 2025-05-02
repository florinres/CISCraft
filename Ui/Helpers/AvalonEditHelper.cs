using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

namespace Ui.Helpers;

public static class AvalonEditHelper
{
    private static IHighlightingDefinition? _assemblyHighlightDefinition;

    public static IHighlightingDefinition AssemblyHighlightDefinition
    {
        get
        {
            if (_assemblyHighlightDefinition == null) LoadAssembly16BitHighlighting();
            return _assemblyHighlightDefinition!;
        }
    }

    private static void LoadAssembly16BitHighlighting()
    {
        using var reader = XmlReader.Create(@"Assets\AvalonEdit\Assembly16BitHighlighting.xshd");
        _assemblyHighlightDefinition = HighlightingLoader.Load(reader, HighlightingManager.Instance);
    }
}