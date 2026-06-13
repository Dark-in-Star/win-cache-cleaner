namespace WinCacheCleaner.Services
{
    public static class SizeFormatter
    {
        public static string Format(long bytes)
        {
            if (bytes <= 0)             return "0 B";
            if (bytes < 1024L)          return bytes + " B";
            if (bytes < 1024L * 1024)   return (bytes / 1024.0).ToString("F1") + " KB";
            if (bytes < 1024L * 1024 * 1024) return (bytes / (1024.0 * 1024)).ToString("F1") + " MB";
            return (bytes / (1024.0 * 1024 * 1024)).ToString("F2") + " GB";
        }
    }
}
