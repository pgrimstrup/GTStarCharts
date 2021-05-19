using PanasonicNZ.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanasonicNZ.WPF
{
    public class FileProgressViewModel : NotifyPropertyChanged
    {
        public double ScreenScale { get => Get<double>(); set => Set(value); }
        public string Actioning { get => Get<string>(); set => Set(value); }
        public long TotalItemCount { get => Get<long>(); set => Set(value); }
        public long TotalBytes { get => Get<long>(); set => Set(value); }
        public long RemainingItemCount { get => Get<long>(); set => Set(value); }
        public long RemainingBytes { get => Get<long>(); set => Set(value); }

        public double Percent { get => Get<double>(); set => Set(value); }
        public string ItemsFrom { get => Get<string>(); set => Set(value); }
        public string ItemsTo { get => Get<string>(); set => Set(value); }
        public string PercentComplete { get => Get<string>(); set => Set(value); }
        public string SourceFolderName { get => Get<string>(); set => Set(value); }
        public string SourceFileName { get => Get<string>(); set => Set(value); }
        public string TargetFolderName { get => Get<string>(); set => Set(value); }
        public string TimeRemaining { get => Get<string>(); set => Set(value); }
        public string ItemsRemaining { get => Get<string>(); set => Set(value); }
        public string PauseOrResume { get => Get<string>(); set => Set(value); }
    }
}
