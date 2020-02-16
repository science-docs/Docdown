using Docdown.Model.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Docdown.ViewModel.Test
{
    public class WorkspaceViewModelTestBase : WorkspaceTestBase
    {
        public WorkspaceViewModel WorkspaceVM { get; set; }

        [TestInitialize]
        public new void Initialize()
        {
            WorkspaceVM = new WorkspaceViewModel
            {
                Data = Workspace
            };
        }
    }
}
