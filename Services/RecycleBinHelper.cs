using System;
using System.Runtime.InteropServices;

namespace WinCacheCleaner.Services
{
    public static class RecycleBinHelper
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct SHQUERYRBINFO
        {
            public int    cbSize;
            public long   i64Size;
            public long   i64NumItems;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern int SHQueryRecycleBin(string pszRootPath, ref SHQUERYRBINFO pSHQueryRBInfo);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern int SHEmptyRecycleBin(IntPtr hWnd, string pszRootPath, uint dwFlags);

        private const uint SHERB_NOCONFIRMATION = 0x00000001;
        private const uint SHERB_NOPROGRESSUI   = 0x00000002;
        private const uint SHERB_NOSOUND        = 0x00000004;

        public static long QuerySize()
        {
            try
            {
                var info = new SHQUERYRBINFO { cbSize = Marshal.SizeOf(typeof(SHQUERYRBINFO)) };
                SHQueryRecycleBin(null, ref info);
                return info.i64Size;
            }
            catch { return 0; }
        }

        /// <summary>Empties all recycle bins silently. Returns bytes freed.</summary>
        public static long EmptyAndReturnFreed()
        {
            long before = QuerySize();
            SHEmptyRecycleBin(IntPtr.Zero, null,
                SHERB_NOCONFIRMATION | SHERB_NOPROGRESSUI | SHERB_NOSOUND);
            return before;
        }
    }
}
