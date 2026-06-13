using System;
using System.Diagnostics;
using System.IO;
using WinCacheCleaner.Models;

namespace WinCacheCleaner.Services
{
    public static class CleanerService
    {
        // ── Size measurement ─────────────────────────────────────────────────

        public static long MeasureFolder(string path)
        {
            if (!Directory.Exists(path)) return 0;
            long total = 0;
            try
            {
                foreach (string file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
                {
                    try { total += new FileInfo(file).Length; } catch { }
                }
            }
            catch { }
            return total;
        }

        public static long MeasureFolderPattern(string path, string pattern)
        {
            if (!Directory.Exists(path)) return 0;
            long total = 0;
            try
            {
                foreach (string file in Directory.EnumerateFiles(path, pattern, SearchOption.TopDirectoryOnly))
                {
                    try { total += new FileInfo(file).Length; } catch { }
                }
            }
            catch { }
            return total;
        }

        // ── Deletion ─────────────────────────────────────────────────────────

        /// <summary>Measures then clears all files and subdirs in path. Returns bytes freed.</summary>
        public static long CleanFolder(string path)
        {
            if (!Directory.Exists(path)) return 0;
            long freed = MeasureFolder(path);

            try
            {
                foreach (string f in Directory.GetFiles(path))
                    try { File.Delete(f); } catch { }
            }
            catch { }

            try
            {
                foreach (string d in Directory.GetDirectories(path))
                    try { Directory.Delete(d, true); } catch { }
            }
            catch { }

            return freed;
        }

        /// <summary>Deletes only files matching pattern in top-level of path. Returns bytes freed.</summary>
        public static long CleanFolderPattern(string path, string pattern)
        {
            if (!Directory.Exists(path)) return 0;
            long freed = MeasureFolderPattern(path, pattern);

            try
            {
                foreach (string f in Directory.GetFiles(path, pattern, SearchOption.TopDirectoryOnly))
                    try { File.Delete(f); } catch { }
            }
            catch { }

            return freed;
        }

        // ── External processes ───────────────────────────────────────────────

        public static void RunExternal(string exe, string args, int timeoutMs = 30000)
        {
            try
            {
                var psi = new ProcessStartInfo(exe, args)
                {
                    CreateNoWindow  = true,
                    UseShellExecute = false,
                    WindowStyle     = ProcessWindowStyle.Hidden
                };
                var p = Process.Start(psi);
                p?.WaitForExit(timeoutMs);
            }
            catch { }
        }

        // ── Task list ────────────────────────────────────────────────────────

        public static CleanupTask[] BuildTasks()
        {
            string userTemp = Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar);
            string winTemp  = @"C:\Windows\Temp";
            string prefetch = @"C:\Windows\Prefetch";
            string swDist   = @"C:\Windows\SoftwareDistribution\Download";

            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appData      = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            string thumbCache  = Path.Combine(localAppData, @"Microsoft\Windows\Explorer");
            string recentFiles = Path.Combine(appData,     @"Microsoft\Windows\Recent");
            string d3dCache       = Path.Combine(localAppData, "D3DSCache");
            string crashDumps     = Path.Combine(localAppData, "CrashDumps");
            string werArchive     = @"C:\ProgramData\Microsoft\Windows\WER\ReportArchive";
            string werQueue       = @"C:\ProgramData\Microsoft\Windows\WER\ReportQueue";
            string minidump       = @"C:\Windows\Minidump";
            string cbsLogs        = @"C:\Windows\Logs\CBS";
            string dismLogs       = @"C:\Windows\Logs\DISM";
            string deliveryOpt    = @"C:\Windows\SoftwareDistribution\DeliveryOptimization";

            return new CleanupTask[]
            {
                new CleanupTask
                {
                    Name        = "User Temp",
                    Description = "%TEMP% folder",
                    Execute     = () => TaskResult.Ok(CleanFolder(userTemp))
                },
                new CleanupTask
                {
                    Name        = "Windows Temp",
                    Description = @"C:\Windows\Temp",
                    Execute     = () => TaskResult.Ok(CleanFolder(winTemp))
                },
                new CleanupTask
                {
                    Name        = "Prefetch",
                    Description = @"C:\Windows\Prefetch",
                    Execute     = () => TaskResult.Ok(CleanFolder(prefetch))
                },
                new CleanupTask
                {
                    Name        = "SoftwareDist Cache",
                    Description = @"SoftwareDistribution\Download",
                    Execute     = () => TaskResult.Ok(CleanFolder(swDist))
                },
                new CleanupTask
                {
                    Name        = "Recycle Bin",
                    Description = "All drives",
                    Execute     = () => TaskResult.Ok(RecycleBinHelper.EmptyAndReturnFreed())
                },
                new CleanupTask
                {
                    Name          = "DNS Cache",
                    Description   = "Flush resolver cache",
                    HasSizeReport = false,
                    Execute       = () =>
                    {
                        RunExternal("ipconfig", "/flushdns");
                        return TaskResult.Ok(0, "DNS cache flushed");
                    }
                },
                new CleanupTask
                {
                    Name        = "Thumbnail Cache",
                    Description = "thumbcache_* files",
                    Execute     = () => TaskResult.Ok(CleanFolderPattern(thumbCache, "thumbcache_*"))
                },
                new CleanupTask
                {
                    Name        = "Recent Files",
                    Description = "Windows Recent list",
                    Execute     = () => TaskResult.Ok(CleanFolder(recentFiles))
                },
                new CleanupTask
                {
                    Name        = "DirectX Shader Cache",
                    Description = "D3DSCache folder",
                    Execute     = () => TaskResult.Ok(CleanFolder(d3dCache))
                },
                new CleanupTask
                {
                    Name        = "Windows Error Reports",
                    Description = "WER crash report archives",
                    Execute     = () =>
                    {
                        long freed = CleanFolder(werArchive) + CleanFolder(werQueue);
                        return TaskResult.Ok(freed);
                    }
                },
                new CleanupTask
                {
                    Name        = "Memory Dumps",
                    Description = "Minidump + CrashDumps",
                    Execute     = () =>
                    {
                        long freed = CleanFolder(minidump) + CleanFolder(crashDumps);
                        return TaskResult.Ok(freed);
                    }
                },
                new CleanupTask
                {
                    Name        = "CBS Logs",
                    Description = @"C:\Windows\Logs\CBS",
                    Execute     = () => TaskResult.Ok(CleanFolder(cbsLogs))
                },
                new CleanupTask
                {
                    Name        = "DISM Logs",
                    Description = @"C:\Windows\Logs\DISM",
                    Execute     = () => TaskResult.Ok(CleanFolder(dismLogs))
                },
                new CleanupTask
                {
                    Name        = "Delivery Optimization",
                    Description = "Windows Update peer cache",
                    Execute     = () => TaskResult.Ok(CleanFolder(deliveryOpt))
                },
                new CleanupTask
                {
                    Name        = "Icon Cache",
                    Description = "iconcache_* files",
                    Execute     = () => TaskResult.Ok(CleanFolderPattern(thumbCache, "iconcache_*"))
                },
                new CleanupTask
                {
                    Name          = "Disk Cleanup",
                    Description   = "Windows Disk Cleanup utility",
                    HasSizeReport = false,
                    Execute       = () =>
                    {
                        RunExternal("cleanmgr", "/sagerun:1", 60000);
                        return TaskResult.Ok(0, "Disk Cleanup launched");
                    }
                }
            };
        }
    }
}
