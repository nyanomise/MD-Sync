using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace MDSync
{
    public static class SteamHelper
    {
        private const string GameFolderName = "Yu-Gi-Oh!  Master Duel";

        public static string? AutoDetectGamePath()
        {
            var candidates = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // 1. Thử đọc từ Registry Steam (HKCU)
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");
                if (key?.GetValue("SteamPath") is string steamPath)
                {
                    steamPath = steamPath.Replace('/', Path.DirectorySeparatorChar);
                    ScanSteamRoot(steamPath, candidates);
                }
            }
            catch { }

            // 2. Thử đọc từ Registry Steam (HKLM 64-bit / 32-bit)
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Valve\Steam") 
                             ?? Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Valve\Steam");
                if (key?.GetValue("InstallPath") is string installPath)
                {
                    ScanSteamRoot(installPath, candidates);
                }
            }
            catch { }

            // 3. Các đường dẫn mặc định phổ biến
            var defaultRoots = new[]
            {
                @"C:\Program Files (x86)\Steam",
                @"C:\Program Files\Steam",
                @"D:\Steam",
                @"D:\SteamLibrary",
                @"E:\Steam",
                @"E:\SteamLibrary"
            };

            foreach (var root in defaultRoots)
            {
                ScanSteamRoot(root, candidates);
            }

            // Kiểm tra thư mục nào thực sự tồn tại
            foreach (var candidate in candidates)
            {
                if (Directory.Exists(candidate) && Directory.Exists(Path.Combine(candidate, "LocalData")))
                {
                    return candidate;
                }
            }

            // Nếu tìm thấy thư mục game kể cả khi chưa có LocalData
            foreach (var candidate in candidates)
            {
                if (Directory.Exists(candidate))
                {
                    return candidate;
                }
            }

            return null;
        }

        private static void ScanSteamRoot(string steamRoot, HashSet<string> candidates)
        {
            if (string.IsNullOrWhiteSpace(steamRoot) || !Directory.Exists(steamRoot))
                return;

            // Thư mục game trong Steam root chính
            candidates.Add(Path.Combine(steamRoot, "steamapps", "common", GameFolderName));

            // Đọc file libraryfolders.vdf để tìm các ổ đĩa khác
            var vdfPath = Path.Combine(steamRoot, "steamapps", "libraryfolders.vdf");
            if (File.Exists(vdfPath))
            {
                try
                {
                    var content = File.ReadAllText(vdfPath);
                    // Match các dòng dạng: "path"		"D:\\SteamLibrary"
                    var matches = Regex.Matches(content, @"""path""\s+""([^""]+)""");
                    foreach (Match m in matches)
                    {
                        if (m.Groups.Count > 1)
                        {
                            var libPath = m.Groups[1].Value.Replace(@"\\", @"\");
                            candidates.Add(Path.Combine(libPath, "steamapps", "common", GameFolderName));
                        }
                    }
                }
                catch { }
            }
        }
    }
}
