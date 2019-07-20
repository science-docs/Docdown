using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Docdown.Model
{
    public class User
    {
        public string Name { get; set; }
        public string Token { get; set; }
        public List<string> Workspaces { get; set; } = new List<string>();
        public string Password { get; set; }
    }
}
