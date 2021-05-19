using PanasonicNZ.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using FilePath = System.IO.Path;

namespace PanasonicNZ.WPF
{
    /// <summary>
    /// Interaction logic for FileCopyDialog.xaml
    /// </summary>
    public partial class FileProgressDialog : Window
    {
        TimeSpan DelayBeforeClose = TimeSpan.FromSeconds(1.5);
        TimeSpan DelayBetweenActions = TimeSpan.FromSeconds(0);

        bool recalcSize = false;
        bool isPaused = false;
        Task sizeCalculationTask;
        Task fileActionTask;
        DispatcherTimer updateTimerFast;
        DispatcherTimer updateTimerSlow;
        OneFileOperation current;
        CancellationTokenSource cancel;

        long totalItems;
        long totalItemsRemaining;
        long totalBytes;
        long totalBytesRemaining;
        TimeSpan totalTimeTaken;

        public FileProgressDialog()
        {
            InitializeComponent();

            DataContext = new FileProgressViewModel();
            ((FileProgressViewModel)DataContext).ScreenScale = WpfScreenScaling.GetScreenScale();

            cancel = new CancellationTokenSource();
            sizeCalculationTask = Task.Factory.StartNew(CalculateTotalSizeTaskAction, cancel.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            fileActionTask = Task.Factory.StartNew(FileActionTaskAction, cancel.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            UpdateStatus(true);
            updateTimerFast = new DispatcherTimer(TimeSpan.FromMilliseconds(20), DispatcherPriority.DataBind, UpdateTimerFastCallback, Dispatcher);
            updateTimerFast.Start();

            updateTimerSlow = new DispatcherTimer(TimeSpan.FromMilliseconds(350), DispatcherPriority.DataBind, UpdateTimerSlowCallback, Dispatcher);
            updateTimerSlow.Start();

            this.LoadPosition();
        }

        protected override void OnClosed(EventArgs e)
        {
            this.SavePosition();

            // At this point, it has been confirmed that the dialog is to be closed, either
            // because we have run out of things to do or the user clicked cancel
            cancel.Cancel();
            updateTimerFast.Stop();
            updateTimerSlow.Stop();
            base.OnClosed(e);
        }

        public void CalculateRemaining()
        {
            recalcSize = true;
        }

        public void Pause()
        {
            isPaused = true;
            UpdateStatus(true);
        }

        public void Resume()
        {
            isPaused = false;
            UpdateStatus(true);
        }

        public void Cancel()
        {
            lock (FileSystem.FileOperations)
            {
                FileSystem.FileOperations.Clear();
                Close();
            }
        }

        void UpdateTimerFastCallback(object sender, EventArgs e)
        {
            UpdateStatus(false);
        }

        void UpdateTimerSlowCallback(object sender, EventArgs e)
        {
            UpdateStatus(true);

            // If there is nothing left to do, close the dialog after a small delay
            var next = GetNextOperation();
            if (next == null)
            {
                Task.Delay(DelayBeforeClose).ContinueWith(task =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        // If still nothing, then close the dialog
                        next = GetNextOperation();
                        if (next == null)
                            Close();
                    });
                });
            }
        }

        void UpdateStatus(bool updateEverything)
        {
            var model = (FileProgressViewModel)DataContext;

            // Overall statistics that aren't updated in the UI
            model.TotalItemCount = Interlocked.Read(ref totalItems);
            model.RemainingItemCount = Interlocked.Read(ref totalItemsRemaining);
            model.TotalBytes = Interlocked.Read(ref totalBytes);
            model.RemainingBytes = Interlocked.Read(ref totalBytesRemaining);

            // Update the file names
            if (current == null)
            {
                // "Preparing n items"
                model.Actioning = "Preparing";
                model.ItemsFrom = "";
                model.ItemsTo = "";
                model.SourceFolderName = "";
                model.TargetFolderName = "";
                model.SourceFileName = "Name:";

                if (model.TotalItemCount == 1)
                    model.ItemsFrom = "item";
                else
                    model.ItemsFrom = "items";
            }
            else
            {
                // "Copying n items from source to target"
                // "Moving n items from source to target"
                // "Deleting n items from source"
                // "Zipping n items from source to target.zip"

                switch (current.Action)
                {
                    case FileOperation.Copy:
                        model.Actioning = "Copying";
                        model.ItemsTo = "to";
                        model.TargetFolderName = FilePath.GetFileName(FilePath.GetDirectoryName(current.TargetPath));
                        break;
                    case FileOperation.Delete:
                        model.Actioning = "Deleting";
                        model.ItemsTo = "";
                        model.TargetFolderName = "";
                        break;
                    case FileOperation.Move:
                        model.Actioning = "Moving";
                        model.ItemsTo = "to";
                        model.TargetFolderName = FilePath.GetFileName(FilePath.GetDirectoryName(current.TargetPath));
                        break;
                    case FileOperation.Zip:
                        model.Actioning = "Zipping";
                        model.ItemsTo = "to";
                        model.TargetFolderName = FilePath.GetFileName(current.TargetPath);
                        if (String.IsNullOrEmpty(current.SourcePath))
                            current.SourcePath = current.SourcePaths.First();
                        break;
                }

                if (model.TotalItemCount == 1)
                    model.ItemsFrom = "item from";
                else
                    model.ItemsFrom = "items from";

                model.SourceFolderName = FilePath.GetFileName(FilePath.GetDirectoryName(current.SourcePath));
                model.SourceFileName = "Name: " + FilePath.GetFileName(current.SourcePath);
            }

            long bytesDone = model.TotalBytes - model.RemainingBytes;
            if (bytesDone < 0 || model.TotalBytes == 0)
                model.Percent = 0;
            else
                model.Percent = (100.0 * bytesDone / model.TotalBytes);

            if (updateEverything)
            {
                // Update displayed statistics only during the slow timer - this stops the numbers from changing too fast
                model.PercentComplete = $"{model.Percent:n0}% complete" + (isPaused ? " (Paused)" : "");
                model.TimeRemaining = "Time remaining: " + CalculateTimeRemaining();
                model.ItemsRemaining = $"Items remaining: {model.RemainingItemCount} ({ShellInfo.GetFileSizeText(model.RemainingBytes)})";
            }

            if (isPaused)
                model.PauseOrResume = "Resume";
            else
                model.PauseOrResume = "Pause";
        }


        string CalculateTimeRemaining()
        {
            long totalBytes = Interlocked.Read(ref this.totalBytes);
            long bytesRemaining = Interlocked.Read(ref this.totalBytesRemaining);

            if (totalTimeTaken.TotalSeconds < 1.5 || bytesRemaining >= totalBytes)
                return "calculating...";

            if (bytesRemaining <= 0)
                return "done...";

            long bytesDone = totalBytes - bytesRemaining;

            double speed = bytesDone / totalTimeTaken.TotalSeconds;
            if (speed < 1)
                return "stalled...";

            double time = bytesRemaining / speed;
            if (time < 1)
                return "any second now...";

            TimeSpan timeRemaining = TimeSpan.FromSeconds(time);
            return timeRemaining.ToLongString();
        }

        async void CalculateTotalSizeTaskAction()
        {
            while (!cancel.IsCancellationRequested)
            {
                if (recalcSize)
                {
                    recalcSize = false;

                    OneFileOperation[] files = null;
                    lock (FileSystem.FileOperations)
                    {
                        files = FileSystem.FileOperations.ToArray();
                    }

                    foreach (var file in files)
                    {
                        if (file.SourceLength == null)
                        {
                            if (file.Action == FileOperation.Zip)
                            {
                                Interlocked.Add(ref totalItems, file.SourcePaths.Length);
                                Interlocked.Add(ref totalItemsRemaining, file.SourcePaths.Length);

                                file.SourceLength = 0;
                                foreach (var source in file.SourcePaths)
                                {
                                    var info = new FileInfo(source);
                                    if (info.Exists)
                                    {
                                        file.SourceLength += info.Length;
                                        if (file.SourceModified == null || file.SourceModified.Value < info.LastWriteTime)
                                            file.SourceModified = info.LastWriteTime;

                                        // Total number of bytes for this session, and also the number of bytes left to copy
                                        Interlocked.Add(ref totalBytes, info.Length);
                                        Interlocked.Add(ref totalBytesRemaining, info.Length);
                                    }
                                }
                            }
                            else
                            {
                                // Total number of items for this session
                                Interlocked.Increment(ref totalItems);
                                Interlocked.Increment(ref totalItemsRemaining);

                                var info = new FileInfo(file.SourcePath);
                                if (info.Exists)
                                {
                                    file.SourceLength = info.Length;
                                    file.SourceModified = info.LastWriteTime;

                                    // Total number of bytes for this session, and also the number of bytes left to copy
                                    Interlocked.Add(ref totalBytes, info.Length);
                                    Interlocked.Add(ref totalBytesRemaining, info.Length);
                                }
                                else
                                {
                                    file.SourceLength = -1;
                                    file.SourceModified = DateTime.MinValue;
                                }
                            }
                        }
                    }
                }

                await Task.Delay(50);
            }
        }

        private void RemoveFileOperation(OneFileOperation fileop, bool decrementStats)
        {
            lock(FileSystem.FileOperations)
            {
                FileSystem.FileOperations.Remove(fileop);
            }

            if(decrementStats)
            {
                Interlocked.Decrement(ref totalItemsRemaining);
                Interlocked.Add(ref totalBytesRemaining, 0 - fileop.SourceLength.GetValueOrDefault());
            }
        }

        private OneFileOperation GetNextOperation()
        {
            lock (FileSystem.FileOperations)
                return FileSystem.FileOperations.FirstOrDefault();
        }

        async void FileActionTaskAction()
        {
            while (!cancel.IsCancellationRequested)
            {
                var next = GetNextOperation();
                if (next != null)
                {
                    current = next;
                    if (next.SourceLength == null)
                        CalculateRemaining();
                }

                if (next != null && next.SourceLength.HasValue)
                {
                    // Size has been obtained, so we can process this file. Otherwise we wait for
                    // the Size Calculation task to do its thing
                    if (next.SourceLength.HasValue)
                    {
                        if (next.SourceLength.Value >= 0)
                            await PerformFileOperation(next);
                        else
                        {
                            Dispatcher.Invoke(() =>
                            {
                                // File not found - notify the user and remove from the queue
                                MessageBoxWpf.Show(this, $"The file '{FilePath.GetFileName(next.SourcePath)}' could not be found.", Application.Current.Title(), MessageBoxButton.OK, MessageBoxImage.Warning);
                                RemoveFileOperation(next, true);
                            });
                        }
                    }
                }

                await Task.Delay(50);
            }
        }

        async Task PerformFileOperation(OneFileOperation fileop)
        {
            try
            {
                CheckForConflict(fileop);
                switch (fileop.Conflict)
                {
                    case ConflictAction.Abort:
                        // Fire and forget
                        var t = Dispatcher.InvokeAsync(Cancel);
                        return;

                    case ConflictAction.Replace:
                        break;

                    case ConflictAction.DoNotReplace:
                        if (fileop.TargetLength.HasValue)
                        {
                            // Target exists, so do not replace it. Reduce the number of bytes we need to copy
                            SkipFileOperation(fileop);
                            return;
                        }
                        break;

                    case ConflictAction.ReplaceNewer:
                        if (fileop.TargetModified.HasValue)
                        {
                            // Target exists, do not replace it if the target is newer
                            if (fileop.TargetModified.Value >= fileop.SourceModified.Value)
                            {
                                SkipFileOperation(fileop);
                                return;
                            }
                        }
                        break;
                }

                // At this point it is ok to overwrite the target file if it exists
                switch (fileop.Action)
                {
                    case FileOperation.Copy:
                        await PerformCopyOperation(fileop);
                        break;

                    case FileOperation.Move:
                        if (fileop.MoveOnSameVolume)
                            PerformMoveOperation(fileop);
                        else
                            await PerformCopyOperation(fileop); // This will delete source when complete
                        break;

                    case FileOperation.Delete:
                        PerformDeleteOperation(fileop);
                        break;

                    case FileOperation.Zip:
                        PerformZipOperation(fileop);
                        break;
                }
            }
            finally
            {
                if (DelayBetweenActions.TotalMilliseconds > 0)
                    Thread.Sleep(DelayBetweenActions);

                // Pop this file operation. Main loop will get the next one if there is one
                RemoveFileOperation(current, false);
                fileop.Callback?.Invoke(fileop);
            }
        }

        private void SkipFileOperation(OneFileOperation fileop)
        {
            Interlocked.Add(ref totalBytes, 0 - fileop.SourceLength.Value);
            Interlocked.Add(ref totalBytesRemaining, 0 - fileop.SourceLength.Value);

            if (fileop.Action == FileOperation.Zip)
            {
                Interlocked.Add(ref totalItems, 0 - fileop.SourcePaths.Length);
                Interlocked.Add(ref totalItemsRemaining, 0 - fileop.SourcePaths.Length);
            }
            else
            {
                Interlocked.Decrement(ref totalItems);
                Interlocked.Decrement(ref totalItemsRemaining);
            }
        }

        private void PerformZipOperation(OneFileOperation fileop)
        {
            TimeSpan startTimeTaken = totalTimeTaken;
            DateTime start = DateTime.Now;
            DateTime? caught = null;
            long endBytesRemaining = totalBytesRemaining - fileop.SourceLength.GetValueOrDefault();
            long endItemsRemaining = totalItemsRemaining = fileop.SourcePaths.Length;
            try
            {
                // Delete any current file with the same name
                if (File.Exists(fileop.TargetPath))
                    File.Delete(fileop.TargetPath);

                List<Stream> streams = new List<Stream>();
                var zip = new Ionic.Zip.ZipFile(fileop.TargetPath);
                long currentpos = 0;

                zip.SaveProgress += (s, e) =>
                {

                    if (e.CurrentEntry != null)
                    {
                        switch (e.EventType)
                        {
                            case Ionic.Zip.ZipProgressEventType.Saving_BeforeWriteEntry:
                                currentpos = 0;
                                fileop.SourcePath = fileop.SourcePaths.FirstOrDefault(fn => e.CurrentEntry.FileName == FilePath.GetFileName(fn));
                                break;

                            case Ionic.Zip.ZipProgressEventType.Saving_EntryBytesRead:
                                Interlocked.Add(ref totalBytesRemaining, currentpos - e.BytesTransferred);
                                currentpos = e.BytesTransferred;
                                break;

                            case Ionic.Zip.ZipProgressEventType.Saving_AfterWriteEntry:
                                Interlocked.Decrement(ref totalItemsRemaining);
                                break;
                        }
                    }

                    totalTimeTaken = startTimeTaken.Add(DateTime.Now.Subtract(start));
                };

                foreach (var name in fileop.SourcePaths)
                {
                    var info = new FileInfo(name);
                    fileop.SourcePath = info.FullName;
                    fileop.SourceLength = info.Length;
                    fileop.SourceModified = info.LastWriteTime;

                    var infile = info.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
                    streams.Add(infile);
                    zip.AddEntry(info.Name, infile);
                }
                zip.Save();

                foreach (var stream in streams)
                    stream.Dispose();

                fileop.Success = true;
            }
            catch (Exception ex)
            {
                caught = DateTime.Now;
                ex.LogException(true, this);
            }
            finally
            {
                totalTimeTaken = startTimeTaken.Add(caught.GetValueOrDefault(DateTime.Now).Subtract(start));
                Interlocked.Exchange(ref totalBytesRemaining, endBytesRemaining);
                Interlocked.Exchange(ref totalItemsRemaining, endItemsRemaining);
            }
        }

        private void PerformMoveOperation(OneFileOperation fileop)
        {
            DateTime start = DateTime.Now;
            DateTime? caught = null;
            long endBytesRemaining = totalBytesRemaining - fileop.SourceLength.GetValueOrDefault();
            try
            {
                FileInfo info = new FileInfo(fileop.TargetPath);
                if (info.Exists)
                    info.Delete();

                if (!info.Directory.Exists)
                    info.Directory.Create();

                File.Move(fileop.SourcePath, fileop.TargetPath);

                fileop.Success = true;

                totalTimeTaken = totalTimeTaken.Add(DateTime.Now.Subtract(start));
            }
            catch (Exception ex)
            {
                caught = DateTime.Now;
                ex.LogException(true, this);
            }
            finally
            {
                totalTimeTaken = totalTimeTaken.Add(caught.GetValueOrDefault(DateTime.Now).Subtract(start));
                Interlocked.Exchange(ref totalBytesRemaining, endBytesRemaining);
                Interlocked.Decrement(ref totalItemsRemaining);
            }
        }

        private void PerformDeleteOperation(OneFileOperation fileop)
        {
            DateTime start = DateTime.Now;
            DateTime? caught = null;
            long endBytesRemaining = totalBytesRemaining - fileop.SourceLength.GetValueOrDefault();
            try
            {
                FileInfo info = new FileInfo(fileop.SourcePath);
                if (info.Exists)
                {
                    info.Delete();
                    fileop.Success = true;
                }

            }
            catch (Exception ex)
            {
                caught = DateTime.Now;
                ex.LogException(true, this);
            }
            finally
            {
                totalTimeTaken = totalTimeTaken.Add(caught.GetValueOrDefault(DateTime.Now).Subtract(start));
                Interlocked.Exchange(ref totalBytesRemaining, endBytesRemaining);
                Interlocked.Decrement(ref totalItemsRemaining);
            }
        }

        private async Task PerformCopyOperation(OneFileOperation fileop)
        {
            int bufferSize = 81920;
            var fileOptions = FileOptions.Asynchronous | FileOptions.SequentialScan;

            DateTime startTime = DateTime.Now;
            DateTime? caught = null;
            TimeSpan startTimer = totalTimeTaken;
            TimeSpan timeTaken = TimeSpan.Zero;
            bool resumeFromPaused = false;
            bool targetFileWasCreated = false;
            long endBytesRemaining = totalBytesRemaining - fileop.SourceLength.GetValueOrDefault();
            try
            {
                using (var src = new FileStream(fileop.SourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, fileOptions))
                using (var dst = new FileStream(fileop.TargetPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, fileOptions))
                {
                    targetFileWasCreated = true;

                    byte[] buffer = new byte[bufferSize];
                    int bytesin = await src.ReadAsync(buffer, 0, buffer.Length);
                    while (bytesin > 0 && !cancel.IsCancellationRequested)
                    {
                        if (isPaused)
                        {
                            if (!resumeFromPaused)
                            {
                                // Just been paused - add the time taken to read the last block to the total
                                timeTaken = DateTime.Now.Subtract(startTime);
                                totalTimeTaken = startTimer.Add(timeTaken);
                                resumeFromPaused = true;
                            }

                            // Normal wait-while-paused loop
                            await Task.Delay(50);
                        }
                        else
                        {
                            if (resumeFromPaused)
                            {
                                // Just been unpaused - resume timer calculations
                                startTime = DateTime.Now;
                                startTimer = totalTimeTaken;
                                resumeFromPaused = false;
                            }

                            // Normal read/write loop
                            timeTaken = DateTime.Now.Subtract(startTime);
                            totalTimeTaken = startTimer.Add(timeTaken);

                            // Write the block, and read the next block
                            await dst.WriteAsync(buffer, 0, bytesin);
                            Interlocked.Add(ref totalBytesRemaining, 0 - bytesin);

                            bytesin = await src.ReadAsync(buffer, 0, buffer.Length);
                        }
                    }

                    // Time taken to write the last buffer contents
                    timeTaken = DateTime.Now.Subtract(startTime);
                    totalTimeTaken = startTimer.Add(timeTaken);
                }

                fileop.Success = !cancel.IsCancellationRequested;
                if (fileop.Success)
                {
                    // Copy attributes over
                    FileInfo srcinfo = new FileInfo(fileop.SourcePath);
                    FileInfo dstinfo = new FileInfo(fileop.TargetPath);
                    if (srcinfo.Exists && dstinfo.Exists)
                    {
                        dstinfo.LastWriteTime = srcinfo.LastWriteTime;
                        dstinfo.LastAccessTime = srcinfo.LastAccessTime;
                        dstinfo.Attributes = srcinfo.Attributes;

                        if (fileop.Action == FileOperation.Move)
                        {
                            // If the fileoperation is a Move, then delete the source file
                            srcinfo.Delete();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                caught = DateTime.Now;
                ex.LogException(true, this);
            }
            finally
            {
                totalTimeTaken = startTimer.Add(caught.GetValueOrDefault(DateTime.Now).Subtract(startTime));
                Interlocked.Exchange(ref totalBytesRemaining, endBytesRemaining);
                Interlocked.Decrement(ref totalItemsRemaining);
                if (!fileop.Success && targetFileWasCreated && File.Exists(fileop.TargetPath))
                    File.Delete(fileop.TargetPath);
            }
        }

        void CheckForConflict(OneFileOperation action)
        {
            if (action.Conflict == ConflictAction.Unknown)
            {
                var info = new FileInfo(action.TargetPath);
                if (info.Exists)
                {
                    action.Conflict = ConflictAction.Checked;
                    action.TargetLength = info.Length;
                    action.TargetModified = info.LastWriteTime;
                }
                else
                {
                    // For this file operation, flag as Replace allowed if the target does not exist
                    action.Conflict = ConflictAction.Replace;
                }

                if (action.Action == FileOperation.Move && !action.SourcePath.StartsWith(@"\\") && !action.TargetPath.StartsWith(@"\\"))
                {
                    // If the move is on the same physical disk, then we can perform a quick Move, rather than Copy/Delete
                    var srcinfo = new FileInfo(action.SourcePath);
                    if (srcinfo.Directory.Root == info.Directory.Root)
                        action.MoveOnSameVolume = true;
                }
            }

            if (action.Conflict == ConflictAction.Checked && action.TargetLength.HasValue)
            {
                // This file is in conflict 
                // Count the number of conflicts left in the queue, unless the current action is to zip 
                int conflictCount = 0;
                if (action.Action != FileOperation.Zip)
                {
                    OneFileOperation[] operations;
                    lock (FileSystem.FileOperations)
                        operations = FileSystem.FileOperations.ToArray();

                    // Count the number of in-conflict files we have. The conflict check is restricted
                    // to only those subsequent files that have the same action.
                    foreach (var fileop in operations)
                    {
                        // Different Action - stop checking
                        if (fileop.Action != action.Action)
                            break;

                        if (fileop.Conflict == ConflictAction.Unknown)
                        {
                            // Need to check whether it's in conflict
                            fileop.Conflict = ConflictAction.Checked;
                            var info = new FileInfo(fileop.TargetPath);
                            if (info.Exists)
                            {
                                fileop.TargetLength = info.Length;
                                fileop.TargetModified = info.LastWriteTime;
                            }
                        }

                        if (fileop.Conflict == ConflictAction.Checked && fileop.TargetLength.HasValue)
                        {
                            // Is in conflict, so count it
                            conflictCount++;
                        }
                    }
                }

                // There is a possible conflict, determine how we are going to resolve it
                Dispatcher.Invoke(() =>
                {
                    var dlg = new FileReplaceDialog();
                    dlg.Owner = this;
                    dlg.SetFileOperation(action, conflictCount);
                    if (dlg.ShowDialog() == true)
                    {
                        action.Conflict = dlg.ConflictAction;
                        if (dlg.ApplyToAll)
                        {
                            // Apply this decision to all subsequent files that have not yet been resolved
                            lock (FileSystem.FileOperations)
                            {
                                foreach (var file in FileSystem.FileOperations)
                                {
                                    switch (file.Conflict)
                                    {
                                        case ConflictAction.Unknown:
                                        case ConflictAction.Checked:
                                            file.Conflict = dlg.ConflictAction;
                                            break;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        action.Conflict = ConflictAction.Abort;
                    }
                });
            }
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Cancel();
            Close();
        }
    }
}
