using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Docdown.Bibliography
{
    public class CaptchaResult
    {
        public bool Aborted { get; set; }
        public string Captcha { get; set; }
        public bool Reload { get; set; }

        public static CaptchaResult AbortedResult { get; } = new CaptchaResult { Aborted = true };
        public static CaptchaResult ReloadResult { get; } = new CaptchaResult { Reload = true };

        public CaptchaResult()
        {

        }

        public CaptchaResult(string captcha)
        {
            Captcha = captcha;
        }
    }
}
