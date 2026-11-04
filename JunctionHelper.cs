using System;
using System.Diagnostics;
using System.IO;

namespace MDSync
{
    public static class JunctionHelper
    {
        public static bool IsReparsePoint(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                    return false;

                var di = new DirectoryInfo(path);
                return di.Attributes.HasFlag(FileAttributes.ReparsePoint);
            }
            catch
            {
                return false;
            }
        }

        public static (bool Success, string Message) CreateJunction(string linkPath, string targetPath)
        {
            try
            {
                // Đảm bảo thư mục cha của link tồn tại
                var parent = Path.GetDirectoryName(linkPath);
                if (!string.IsNullOrEmpty(parent) && !Directory.Exists(parent))
                {
                    Directory.CreateDirectory(parent);
                }

                // Nếu thư mục link đang tồn tại:
                if (Directory.Exists(linkPath))
                {
                    if (IsReparsePoint(linkPath))
                    {
                        // Đã là junction rồi, xóa junction cũ đi để tạo lại
                        Directory.Delete(linkPath, false);
                    }
                    else
                    {
                        // Là thư mục thường -> xóa nội dung bên trong
                        Directory.Delete(linkPath, true);
                    }
                }

                var psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c mklink /J \"{linkPath}\" \"{targetPath}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using var process = Process.Start(psi);
                if (process == null)
                    return (false, "Không thể khởi chạy cmd.exe.");

                string output = process.StandardOutput.ReadToEnd();
                string err = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode == 0 && Directory.Exists(linkPath))
                {
                    return (true, output.Trim());
                }

                return (false, string.IsNullOrWhiteSpace(err) ? output.Trim() : err.Trim());
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public static long GetDirectoryPhysicalSize(string path)
        {
            if (!Directory.Exists(path) || IsReparsePoint(path))
                return 0;

            try
            {
                long size = 0;
                var di = new DirectoryInfo(path);
                foreach (var file in di.EnumerateFiles("*", SearchOption.AllDirectories))
                {
                    try
                    {
                        size += file.Length;
                    }
                    catch { }
                }
                return size;
            }
            catch
            {
                return 0;
            }
        }

        public static string FormatBytes(long bytes)
        {
            if (bytes <= 0) return "0 MB";
            double mb = bytes / (1024.0 * 1024.0);
            if (mb >= 1024.0)
            {
                double gb = mb / 1024.0;
                return $"{gb:0.##} GB";
            }
            return $"{mb:0.##} MB";
        }
    }
}
