using System.Threading.Tasks;

namespace Docdown.Bibliography
{
    public interface ICaptchaSolvingStrategy
    {
        Task Initialize();
        Task<CaptchaResult> Solve(byte[] imageData);
        Task Invalidate(string lastCaptcha);
        Task Finish();
    }
}
