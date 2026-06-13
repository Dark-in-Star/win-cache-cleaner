using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WinCacheCleaner.Models
{
    public class CleanupTask : INotifyPropertyChanged
    {
        private bool _isSelected = true;

        public string Name { get; set; }
        public string Description { get; set; }
        public bool HasSizeReport { get; set; } = true;

        public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; OnPropertyChanged(); }
        }

        public Func<TaskResult> Execute { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
