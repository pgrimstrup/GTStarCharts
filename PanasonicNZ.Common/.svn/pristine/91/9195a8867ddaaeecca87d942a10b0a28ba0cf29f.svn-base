using System;
using System.Linq;
using System.Threading;

namespace PanasonicNZ.Common
{
    public class Lock : IDisposable
    {
        static int _lockid = 0;
        bool _locked;
        readonly object _locker;
#if DEBUG
        readonly string _lockedby;
#endif

        public Lock(object locker, int timeout, bool throwOnTimeout)
            : this(locker, TimeSpan.FromMilliseconds(timeout), throwOnTimeout)
        {
            
        }

        public Lock(object locker, TimeSpan timeout, bool throwOnTimeout)
        {
            _lockid = Interlocked.Increment(ref _lockid);
            _locker = locker;
            Monitor.TryEnter(_locker, timeout, ref _locked);

#if DEBUG
            if(!String.IsNullOrEmpty(Environment.StackTrace))
            {
                var lines = Environment.StackTrace.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                _lockedby = lines.Skip(3).FirstOrDefault(s => !s.Contains("..ctor"));

                if (!_locked)
                    System.Diagnostics.Debug.WriteLine($"Lock {_lockid} failed for " + _lockedby);
            }
#endif

            if (!_locked)
            {
                if (throwOnTimeout)
                    throw new TimeoutException("Unable to acquire lock on object");
                else
                    new TimeoutException("Unable to acquire lock on object").LogException(false);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if(_locked)
            {
                Monitor.Exit(_locker);
                _locked = false;

            }
        }

        public bool IsLocked { get => _locked; }

        ~Lock()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
