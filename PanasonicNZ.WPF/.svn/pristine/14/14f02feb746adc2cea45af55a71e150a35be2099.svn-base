using PanasonicNZ.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using FilePath = System.IO.Path;

namespace PanasonicNZ.WPF
{
    /// <summary>
    /// Interaction logic for FileReplaceDialog.xaml
    /// </summary>
    public partial class FileReplaceDialog : Window
    {
        public bool ApplyToAll { get; set; }

        public int ConflictCount { get; set; }

        public bool HasMultipleConflicts { get => ConflictCount > 0; }

        public ConflictAction ConflictAction { get; set; }

        public string ActionAndReplace { get; set; } // Copy and Replace
        public string DontAction { get; set; } // Don't Copy
        public string ActionChangedFiles { get; set; } // Copy only those files that have changed
        public bool IsActionChangedVisible { get; set; }

        public string SourceFilename { get; set; }
        public string SourceFileType { get; set; }
        public string SourceFileSize { get; set; }
        public string SourceModified { get; set; }

        public string TargetFilename { get; set; }
        public string TargetFileType { get; set; }
        public string TargetFileSize { get; set; }
        public string TargetModified { get; set; }

        public ImageSource SourceThumbnail { get; set; }
        public ImageSource TargetThumbnail { get; set; }

        public FileReplaceDialog()
        {
            InitializeComponent();
            DataContext = this;
            this.LoadPosition();
        }

        protected override void OnClosed(EventArgs e)
        {
            this.SavePosition();
            base.OnClosed(e);
        }

        internal void SetFileOperation(OneFileOperation fileop, int conflictCount)
        {
            Title = $"{fileop.Action} Files";
            ConflictCount = conflictCount - 1; // Additional conflicts
            IsActionChangedVisible = fileop.Action == FileOperation.Copy || fileop.Action == FileOperation.Move;
            ActionAndReplace = $"{fileop.Action} and Replace";
            DontAction = $"Don't {fileop.Action}";
            ActionChangedFiles = $"{fileop.Action} only those files that have changed";

            TargetFilename = FilePath.GetFileName(fileop.TargetPath);
            TargetFileType = ShellInfo.GetFileTypeName(fileop.TargetPath);
            TargetFileSize = "Size: " + ShellInfo.GetFileSizeText(fileop.TargetLength.Value);
            TargetModified = "Date modified: " + fileop.TargetModified.Value.ToString();
            TargetThumbnail = ShellInfo.GetLargeIcon(fileop.TargetPath);


            if (fileop.Action == FileOperation.Zip)
            {
                // Source is a list of files that will be added to the new Zip Archive
                SourceFilename = FilePath.GetFileName(fileop.TargetPath);
                SourceFileSize = "Size: " + ShellInfo.GetFileSizeText(fileop.SourceLength.Value) + " (uncompressed)";

                SourceFileType = ShellInfo.GetFileTypeName(fileop.TargetPath); // To display zip file type name
                SourceThumbnail = ShellInfo.GetLargeIcon(fileop.TargetPath); // To display the zip icon
            }
            else
            {
                // Source is a normal file
                SourceFilename = FilePath.GetFileName(fileop.SourcePath);
                SourceFileType = ShellInfo.GetFileTypeName(fileop.SourcePath);
                SourceFileSize = "Size: " + ShellInfo.GetFileSizeText(fileop.SourceLength.Value);
                SourceModified = "Date modified: " + fileop.SourceModified.Value.ToString();
                SourceThumbnail = ShellInfo.GetLargeIcon(fileop.SourcePath);

                if (fileop.SourceLength.Value < fileop.TargetLength.Value)
                {
                    SourceFileSize += " (smaller)";
                    TargetFileSize += " (larger)";
                }
                else if (fileop.SourceLength.Value > fileop.TargetLength.Value)
                {
                    SourceFileSize += " (larger)";
                    TargetFileSize += " (smaller)";
                }
                else
                {
                    SourceFileSize += " (same)";
                    TargetFileSize += " (same)";
                }

                if (fileop.SourceModified.Value < fileop.TargetModified.Value)
                {
                    SourceModified += " (older)";
                    TargetModified += " (newer)";
                }
                else if (fileop.SourceModified.Value > fileop.TargetModified.Value)
                {
                    SourceModified += " (newer)";
                    TargetModified += " (older)";
                }
                else
                {
                    SourceModified += " (same)";
                    TargetModified += " (same)";
                }
            }

        }

        private void CopyAndReplace_Click(object sender, RoutedEventArgs e)
        {
            ConflictAction = ConflictAction.Replace;
            DialogResult = true;
        }

        private void DontCopy_Click(object sender, RoutedEventArgs e)
        {
            ConflictAction = ConflictAction.DoNotReplace;
            DialogResult = true;
        }

        private void CopyNewer_Click(object sender, RoutedEventArgs e)
        {
            ConflictAction = ConflictAction.ReplaceNewer;
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
