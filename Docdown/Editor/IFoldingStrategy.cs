using ICSharpCode.AvalonEdit.Folding;
using System.Collections.Generic;

namespace Docdown.Editor
{
    public interface IFoldingStrategy
    {
        IEnumerable<NewFolding> GenerateFoldings();
    }
}
