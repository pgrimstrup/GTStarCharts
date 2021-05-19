using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PanasonicNZ.WPF
{
    public enum OverwriteFileOption
    {
        PromptUser,
        Replace,
        DoNotReplace
    }

    public enum FileOperation
    {
        Copy,
        Move,
        Delete,
        Zip
    }

    public enum ConflictAction
    {
        Unknown,
        Checked,
        Replace,
        DoNotReplace,
        ReplaceNewer,
        Abort
    }

    public static class FileSystem
    {
        internal static List<OneFileOperation> FileOperations { get; } = new List<OneFileOperation>();
        static FileProgressDialog ProgressDialog;

        public static void CopyFiles(string source, string target, OverwriteFileOption overwrite = OverwriteFileOption.PromptUser, Action<OneFileOperation> callback = null)
        {
            CopyFiles(new[] { source }, new[] { target }, overwrite, callback);
        }

        public static void CopyFiles(IEnumerable<string> source, IEnumerable<string> target, OverwriteFileOption overwrite = OverwriteFileOption.PromptUser, Action<OneFileOperation> callback = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (source.Count() != target.Count())
                throw new ArgumentException("Lengths of the names lists do not match");

            // Determine default conflict resolution action
            ConflictAction conflict = ConflictAction.Unknown;
            switch (overwrite)
            {
                case OverwriteFileOption.Replace: conflict = ConflictAction.Replace; break;
                case OverwriteFileOption.DoNotReplace: conflict = ConflictAction.DoNotReplace; break;
            }

            lock (FileOperations)
            {
                var src = source.GetEnumerator();
                var tgt = target.GetEnumerator();

                while (src.MoveNext() && tgt.MoveNext())
                {
                    FileOperations.Add(new OneFileOperation
                    {
                        SourcePath = src.Current,
                        TargetPath = tgt.Current,
                        Action = FileOperation.Copy,
                        Conflict = conflict,
                        Callback = callback
                    });
                }

                ShowProgressDialog();
            }
        }

        public static void MoveFiles(string source, string target, OverwriteFileOption overwrite = OverwriteFileOption.PromptUser, Action<OneFileOperation> callback = null)
        {
            MoveFiles(new[] { source }, new[] { target }, overwrite, callback);
        }

        public static void MoveFiles(IEnumerable<string> source, IEnumerable<string> target, OverwriteFileOption overwrite = OverwriteFileOption.PromptUser, Action<OneFileOperation> callback = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (source.Count() != target.Count())
                throw new ArgumentException("Lengths of the names lists do not match");

            // Determine default conflict resolution action
            ConflictAction conflict = ConflictAction.Unknown;
            switch (overwrite)
            {
                case OverwriteFileOption.Replace: conflict = ConflictAction.Replace; break;
                case OverwriteFileOption.DoNotReplace: conflict = ConflictAction.DoNotReplace; break;
            }

            lock (FileOperations)
            {
                var src = source.GetEnumerator();
                var tgt = target.GetEnumerator();

                while (src.MoveNext() && tgt.MoveNext())
                {
                    FileOperations.Add(new OneFileOperation
                    {
                        SourcePath = src.Current,
                        TargetPath = tgt.Current,
                        Action = FileOperation.Move,
                        Conflict = conflict,
                        Callback = callback
                    });
                }

                ShowProgressDialog();
            }
        }

        public static void DeleteFiles(string source, Action<OneFileOperation> callback = null)
        {
            DeleteFiles(new[] { source }, callback);
        }

        public static void DeleteFiles(IEnumerable<string> source, Action<OneFileOperation> callback = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            lock (FileOperations)
            {
                var src = source.GetEnumerator();

                while (src.MoveNext())
                {
                    FileOperations.Add(new OneFileOperation
                    {
                        SourcePath = src.Current,
                        Action = FileOperation.Delete,
                        Conflict = ConflictAction.Checked, // Deletes never conflict
                        Callback = callback
                    });
                }

                ShowProgressDialog();
            }
        }

        public static void ZipFiles(IEnumerable<string> source, string archiveName, OverwriteFileOption overwrite = OverwriteFileOption.PromptUser, Action<OneFileOperation> callback = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (archiveName == null) throw new ArgumentNullException(nameof(archiveName));

            // Determine default conflict resolution action
            ConflictAction conflict = ConflictAction.Unknown;
            switch (overwrite)
            {
                case OverwriteFileOption.Replace: conflict = ConflictAction.Replace; break;
                case OverwriteFileOption.DoNotReplace: conflict = ConflictAction.DoNotReplace; break;
            }

            lock (FileOperations)
            {
                FileOperations.Add(new OneFileOperation
                {
                    SourcePaths = source.ToArray(),
                    TargetPath = archiveName,
                    Action = FileOperation.Zip,
                    Conflict = conflict,
                    Callback = callback
                });

                ShowProgressDialog();
            }
        }

        static void ShowProgressDialog()
        {
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                // Run the method on the current UI thread
                Application.Current.Dispatcher.Invoke(ShowProgressDialog);
            }
            else
            {
                if (ProgressDialog == null)
                {
                    ProgressDialog = new FileProgressDialog();
                    ProgressDialog.Owner = Application.Current.MainWindow;
                    ProgressDialog.Closing += ProgressDialog_Closing;
                    ProgressDialog.Show();
                }
                ProgressDialog.BringIntoView();
                ProgressDialog.CalculateRemaining();
            }
        }

        private static void ProgressDialog_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            lock (FileOperations)
            {
                // Make sure there are no more actions to be processed
                if (FileOperations.Count > 0)
                {
                    e.Cancel = true;
                    return;
                }

                ProgressDialog = null;
            }
        }
    }

    public class OneFileOperation
    {
        public string SourcePath { get; internal set; }
        public string[] SourcePaths { get; internal set; }
        public long? SourceLength { get; internal set; }
        public DateTime? SourceModified { get; internal set; }

        public string TargetPath { get; internal set; }
        public long? TargetLength { get; internal set; }
        public DateTime? TargetModified { get; internal set; }

        public bool MoveOnSameVolume { get; internal set; }
        public long BytesCopied { get; internal set; }

        public FileOperation Action { get; internal set; }
        public ConflictAction Conflict { get; internal set; }
        public bool Success { get; internal set; }
        internal Action<OneFileOperation> Callback { get; set; }
    }
}
