using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Docdown.Bibliography
{
    public interface ICaptchaSolvingStrategy
    {
        Task<string> Solve(byte[] imageData); 
    }
}
