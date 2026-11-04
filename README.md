<p align="center">
  <h1 align="center">⚔️ MD-Sync</h1>
  <p align="center">
    <strong>Yu-Gi-Oh! Master Duel Multi-Account Data Linker</strong><br>
    Share game assets across multiple Steam accounts with 0 MB extra disk space.
  </p>
  <p align="center">
    <a href="README_VI.md">🇻🇳 Tiếng Việt</a> | 
    <a href="README.md">🇬🇧 English</a>
  </p>
</p>

---

## 📌 The Problem & The Solution

- **The Problem:** Whenever you log into a new Steam account or switch accounts to play *Yu-Gi-Oh! Master Duel*, the game creates a separate profile directory in `LocalData\<Account_ID>\` and demands re-downloading ~10 GB – 14 GB of game assets (`0000`).
- **The Solution:** **MD-Sync** solves this using Windows native **NTFS Directory Junctions** (`mklink /J`). It links the asset directory of secondary accounts directly to your main downloaded data.

### 🌟 Key Benefits:
- 🚀 **0 MB Extra Disk Space:** 10 accounts take up the exact same disk space as 1 account.
- 🔄 **Update Once, Play Everywhere:** When Konami pushes a game update, downloading it on any account automatically updates the shared assets for all accounts.
- ⚙️ **Shared Preferences (`localsave`):** Automatically links graphics, resolution, volume, and duel animation settings so you don't need to reconfigure them on every new account.
- 🔍 **Auto-Detection:** Automatically discovers your Master Duel path from Steam registry and library folders (supports drives `C:`, `D:`, `E:`, etc.).
- 🛡️ **Clean & Safe:** No game memory tampering, no DLL injection, no executable packing. 100% transparent and safe for Windows Defender.

---

## 🚀 How to Use

1. Download the latest **`MD-Sync.exe`** from the [Releases](../../releases) tab.
2. Log into your new Steam account and launch Master Duel once until the title screen loads (this allows the game to generate its account folder in `LocalData`), then exit the game.
3. Open **`MD-Sync.exe`**. It will automatically find your game directory and list all detected accounts.
4. Click **`Sync All`**.
5. Launch Master Duel on your new account — the download will instantly hit 100% and jump straight into the game!

---

## 🖥️ User Interface

The tool is intentionally built with a minimalist, clean Windows-native look:

- **Game Path:** Auto-detected with a manual "Browse..." button.
- **Account List:** Shows Account folder names, physical storage size, and link status (*Master / Linked / Unlinked*).
- **One-Click Sync:** Automatically cleans incomplete data in secondary accounts and applies directory junctions in under 2 seconds.

---

## 🛠️ Building from Source

### Prerequisites
- [.NET 10.0 SDK](https://dotnet.microsoft.com/) or newer

### Build Instructions
```bash
# 1. Clone repository
git clone https://github.com/nyanomise/MD-Sync.git
cd MD-Sync

# 2. Build standalone single-file executable
dotnet publish -c Release -o ./publish
```

The resulting standalone executable will be located at:
`./publish/MD-Sync.exe`

---

## 📄 License

This project is licensed under the [MIT License](LICENSE).
Feel free to use, modify, and distribute.
