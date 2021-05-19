using PanasonicNZ.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PanasonicNZ.WPF
{
    public static class WpfScreenScaling
    {
        public static double GetScreenScale()
        {
            double factor = WpfScreen.FontScaling(Application.Current.MainWindow);
            var bounds = WpfScreen.GetScreenFrom(Application.Current.MainWindow).DeviceBounds;
            double width = bounds.Width / factor;

            if (width < 1300) // 150% font scaling at 1920x1080, or a small screen
                return 0.75;

            if (width < 1540) // 125% font scaling at 1920x1080, or a small screen
                return 0.85;

            if (width < 2050) // 1920x1080 is the expected size
                return 1;

            if (factor > 1) // User has applied Font Scaling, so keep at normal size
                return 1;

            if (width < 3000) // 2560x1440 probably, so increase font size a bit
                return 1.2;

            if (width < 4000) // 3840x2060 - 4K (UHD), so make everything bigger
                return 1.4;

            return 1.6; // Wow, an 8K screen perhaps? 
        }
    }
}
