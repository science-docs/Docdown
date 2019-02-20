using System;

namespace Docdown.Util
{
    public class CancelToken
    {
        public event EventHandler Canceled;

        public bool IsCanceled { get; private set; }

        public void Cancel()
        {
            Canceled?.Invoke(this, EventArgs.Empty);
            IsCanceled = true;
        }
    }
}
