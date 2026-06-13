using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace WinCacheCleaner.Models
{
    public enum LogStatus { Pending, Running, Done, Failed, Skipped }

    public class LogEntry : INotifyPropertyChanged
    {
        private LogStatus _status = LogStatus.Pending;
        private string _message = "";

        public string TaskName { get; set; }

        public LogStatus Status
        {
            get { return _status; }
            set
            {
                _status = value;
                OnPropertyChanged();
                OnPropertyChanged("Icon");
                OnPropertyChanged("IconColor");
            }
        }

        public string Message
        {
            get { return _message; }
            set { _message = value; OnPropertyChanged(); }
        }

        public string Icon
        {
            get
            {
                switch (_status)
                {
                    case LogStatus.Pending:  return "○";
                    case LogStatus.Running:  return "▶";
                    case LogStatus.Done:     return "✓";
                    case LogStatus.Failed:   return "✗";
                    case LogStatus.Skipped:  return "—";
                    default:                 return "·";
                }
            }
        }

        public Brush IconColor
        {
            get
            {
                switch (_status)
                {
                    case LogStatus.Done:
                        return new SolidColorBrush(Color.FromRgb(0x10, 0xB9, 0x81)); // neon green
                    case LogStatus.Running:
                        return new SolidColorBrush(Color.FromRgb(0x63, 0x66, 0xF1)); // neon indigo
                    case LogStatus.Failed:
                        return new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44)); // neon red
                    default:
                        return new SolidColorBrush(Color.FromRgb(0x6B, 0x7D, 0xB3)); // blue-gray
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
