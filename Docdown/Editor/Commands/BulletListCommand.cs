using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Docdown.Editor.Commands
{
    public class BulletListCommand : ListCommand
    {
        public BulletListCommand() : base("*")
        {
        }
    }
}