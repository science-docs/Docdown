using System;

namespace Docdown.Bibliography
{
    public enum SciHubExceptionType
    {
        Unknown,
        Address,
        Availability,
        Article,
        Captcha
    }

    public class SciHubException : Exception
    {
        public string MessageId { get; }

        public SciHubException(SciHubExceptionType type) : this(type, null)
        {
        }

        public SciHubException(SciHubExceptionType type, Exception inner) : base(ConstructMessage(type), inner)
        {
            MessageId = $"SciHub_{type}";
        }

        private static string ConstructMessage(SciHubExceptionType type)
        {
            switch (type)
            {
                case SciHubExceptionType.Address:
                    return "Could not find valid SciHub URL";
                default:
                    return "Unknown SciHub error";
            }
        }
    }
}
