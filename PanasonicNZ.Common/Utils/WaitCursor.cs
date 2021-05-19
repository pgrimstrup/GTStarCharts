using System;
using System.Windows;
using System.Windows.Input;

namespace PanasonicNZ.Common
{
    // Thread-safe Hourglass cursor
    public class WpfWaitCursor : IDisposable
    {
        Cursor _previous;

        public WpfWaitCursor(Cursor cursor = null)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _previous = Mouse.OverrideCursor;
                Mouse.OverrideCursor = cursor ?? Cursors.Wait;
            });
        }

        public void Dispose()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Mouse.OverrideCursor = _previous;
            });
        }
    }
}
