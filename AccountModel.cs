using System.IO;

namespace MDSync
{
    public class AccountModel
    {
        public string AccountId { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public string AssetPath => Path.Combine(FullPath, "0000");
        public string LocalSavePath => Path.Combine(FullPath, "localsave");

        public bool HasAssetFolder => Directory.Exists(AssetPath);
        public bool IsAssetLinked => JunctionHelper.IsReparsePoint(AssetPath);

        public bool HasLocalSaveFolder => Directory.Exists(LocalSavePath);
        public bool IsLocalSaveLinked => JunctionHelper.IsReparsePoint(LocalSavePath);

        public long AssetSize { get; set; }
        public bool IsMaster { get; set; }

        public string DisplayStatus
        {
            get
            {
                if (IsMaster)
                    return "⭐ Nguồn chính (Master)";
                if (IsAssetLinked)
                    return "🔗 Đã liên kết (Linked)";
                if (HasAssetFolder && AssetSize > 100 * 1024 * 1024)
                    return $"⚠️ Độc lập ({JunctionHelper.FormatBytes(AssetSize)})";
                return "⚠️ Chưa liên kết";
            }
        }
    }
}
