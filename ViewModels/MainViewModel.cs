using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WinCacheCleaner.Models;

namespace WinCacheCleaner.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private double _progressPercent = 0;
        private string _progressText    = "Ready — select tasks and click Run";
        private bool   _isRunning       = false;
        private string _totalFreedText  = "";

        public ObservableCollection<CleanupTask> Tasks      { get; } = new ObservableCollection<CleanupTask>();
        public ObservableCollection<LogEntry>    LogEntries { get; } = new ObservableCollection<LogEntry>();

        public double ProgressPercent
        {
            get { return _progressPercent; }
            set { _progressPercent = value; OnPropertyChanged(); }
        }

        public string ProgressText
        {
            get { return _progressText; }
            set { _progressText = value; OnPropertyChanged(); }
        }

        public bool IsRunning
        {
            get { return _isRunning; }
            set
            {
                _isRunning = value;
                OnPropertyChanged();
                OnPropertyChanged("CanRun");
            }
        }

        public bool CanRun => !_isRunning;

        public string TotalFreedText
        {
            get { return _totalFreedText; }
            set { _totalFreedText = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
