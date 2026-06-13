namespace WinCacheCleaner.Models
{
    public class TaskResult
    {
        public bool Success { get; set; }
        public long BytesFreed { get; set; }
        public string Message { get; set; }

        public static TaskResult Ok(long bytes, string message = null)
            => new TaskResult { Success = true, BytesFreed = bytes, Message = message };

        public static TaskResult Fail(string message)
            => new TaskResult { Success = false, Message = message };
    }
}
