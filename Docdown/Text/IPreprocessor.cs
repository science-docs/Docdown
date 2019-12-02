using Docdown.Model;

namespace Docdown.Text
{
    public interface IPreprocessor
    {
        byte[] Preprocess(IWorkspaceItem workspace, byte[] content);
    }
}
