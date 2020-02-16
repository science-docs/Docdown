using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Docdown
{
    public abstract class TestBase
    {
        [TestInitialize]
        public void InitializeApp()
        {
            if (Application.Current == null)
            {
                _ = new Application();
            }
        }
    }
}
