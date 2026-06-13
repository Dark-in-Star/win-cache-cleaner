using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WinCacheCleaner.Models;
using WinCacheCleaner.Services;
using WinCacheCleaner.ViewModels;

namespace WinCacheCleaner
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _vm;
        private bool _isDark = true;

        public MainWindow()
        {
            InitializeComponent();

            _vm = new MainViewModel();
            DataContext = _vm;

            foreach (CleanupTask task in CleanerService.BuildTasks())
                _vm.Tasks.Add(task);
        }

        // ── Window chrome ─────────────────────────────────────────────────

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void MinimizeBtn_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // ── Theme toggle ──────────────────────────────────────────────────

        private void ThemeBtn_Click(object sender, RoutedEventArgs e)
        {
            _isDark = !_isDark;

            var dicts = Application.Current.Resources.MergedDictionaries;

            // Remove current theme dict
            ResourceDictionary toRemove = null;
            foreach (ResourceDictionary d in dicts)
            {
                if (d.Source != null &&
                    (d.Source.OriginalString.Contains("Dark") ||
                     d.Source.OriginalString.Contains("Light")))
                {
                    toRemove = d;
                    break;
                }
            }
            if (toRemove != null) dicts.Remove(toRemove);

            // Add new theme
            string themePath = _isDark ? "Themes/Dark.xaml" : "Themes/Light.xaml";
            dicts.Add(new ResourceDictionary
            {
                Source = new Uri(themePath, UriKind.Relative)
            });

            ThemeBtn.Content = _isDark ? "🌙" : "☀";
        }

        // ── Task selection ────────────────────────────────────────────────

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (CleanupTask task in _vm.Tasks)
                task.IsSelected = true;
        }

        private void DeselectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (CleanupTask task in _vm.Tasks)
                task.IsSelected = false;
        }

        // ── Run cleanup ───────────────────────────────────────────────────

        private async void RunBtn_Click(object sender, RoutedEventArgs e)
        {
            List<CleanupTask> selected = _vm.Tasks.Where(t => t.IsSelected).ToList();
            if (!selected.Any()) return;

            // Reset state
            _vm.IsRunning      = true;
            _vm.TotalFreedText = "";
            _vm.ProgressPercent = 0;
            _vm.LogEntries.Clear();

            int total     = selected.Count;
            int completed = 0;
            long totalFreed = 0;

            // Build log entries upfront (pending state)
            var logMap = new Dictionary<CleanupTask, LogEntry>();
            foreach (CleanupTask task in selected)
            {
                var entry = new LogEntry
                {
                    TaskName = task.Name,
                    Status   = LogStatus.Pending,
                    Message  = "Waiting..."
                };
                _vm.LogEntries.Add(entry);
                logMap[task] = entry;
            }

            await Task.Run(() =>
            {
                foreach (CleanupTask task in selected)
                {
                    LogEntry entry = logMap[task];

                    // Mark running
                    Dispatcher.Invoke(() =>
                    {
                        entry.Status  = LogStatus.Running;
                        entry.Message = "Cleaning...";
                        _vm.ProgressText = string.Format(
                            "Running {0} of {1}  —  {2}", completed + 1, total, task.Name);
                        LogScroll.ScrollToBottom();
                    });

                    // Execute
                    TaskResult result;
                    try
                    {
                        result = task.Execute();
                    }
                    catch (Exception ex)
                    {
                        result = TaskResult.Fail(ex.Message);
                    }

                    completed++;
                    totalFreed += result.BytesFreed;

                    // Update UI
                    Dispatcher.Invoke(() =>
                    {
                        entry.Status = result.Success ? LogStatus.Done : LogStatus.Failed;

                        if (result.Success)
                        {
                            if (task.HasSizeReport && result.BytesFreed > 0)
                                entry.Message = "Cleared " + SizeFormatter.Format(result.BytesFreed);
                            else if (!string.IsNullOrEmpty(result.Message))
                                entry.Message = result.Message;
                            else
                                entry.Message = "Done";
                        }
                        else
                        {
                            entry.Message = string.IsNullOrEmpty(result.Message) ? "Failed" : result.Message;
                        }

                        _vm.ProgressPercent = (double)completed / total * 100.0;
                        _vm.ProgressText    = string.Format("{0} / {1} completed", completed, total);
                        LogScroll.ScrollToBottom();
                    });
                }
            });

            // Final state
            _vm.IsRunning = false;
            _vm.ProgressText = "Cleanup complete!";
            _vm.TotalFreedText = totalFreed > 0
                ? "Total freed: " + SizeFormatter.Format(totalFreed)
                : "Cleanup complete";
        }
    }
}
