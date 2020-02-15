using System;
using System.Threading;

namespace Docdown.Net
{
    public class CancelToken
    {
        public event EventHandler Canceled;

        public bool IsCanceled { get; private set; }

        public CancellationToken ToCancellationToken()
        {
            var token = new CancellationTokenSource();
            Canceled += delegate { token.Cancel(); };
            return token.Token;
        }

        public void Cancel()
        {
            Canceled?.Invoke(this, EventArgs.Empty);
            IsCanceled = true;
        }
    }
}
