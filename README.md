# Steam Achievement Unlocker (Auto & Universal) 🏆

A lightweight, high-performance tool written in C# to automatically discover and unlock achievements for any Steam game or your entire Steam library in one click using native Steamworks API bindings.

![Language](https://img.shields.io/badge/Language-C%23-blue.svg)
![Platform](https://img.shields.io/badge/Platform-Windows%20(x64)-brightgreen.svg)
![License](https://img.shields.io/badge/License-MIT-yellow.svg)

---

## ✨ Features

- **🚀 Auto Library Discovery:** Automatically parses local Steam configuration files (`localconfig.vdf`, `libraryfolders.vdf`, and `appmanifest_*.acf`) to find all owned games.
- **⚡ Bulk Mode:** Scans and unlocks missing achievements across your entire account automatically.
- **🎯 Single Game / AppID Mode:** Supports unlocking specific games by entering custom Steam AppIDs (e.g., `414700` for Outlast 2, `1304930` for The Outlast Trials, `381210` for Dead by Daylight).
- **🛡️ 100% Anti-Cheat Safe:** Does NOT inject into or run game processes (EAC, BattlEye, VAC are completely bypassed as games remain unopened).
- **📦 Zero External Dependencies:** Compiles directly with native Windows .NET without external NuGet packages.

---

## 📥 Quick Start (Ready-to-Use)

1. Ensure the **Steam client is running** and logged in.
2. Download the latest release from the [Releases](../../releases) section or clone the repo.
3. Keep `SteamAchievementUnlocker.exe` in the same directory as `steam_api64.dll`.
4. Run `SteamAchievementUnlocker.exe` and select:
   - `[1]` to scan and unlock achievements for **all** games in your library.
   - `[2]` to scan only currently **installed** games.
   - `[3]` to enter an **AppID** manually.

---

## 🛠️ How to Build from Source

You do not need Visual Studio installed. You can compile it instantly using the built-in Windows C# compiler:

### Option A: Using `build.bat`
Simply double click `build.bat` in the repository root.

### Option B: Command Line (Windows x64)
```cmd
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /target:exe /platform:x64 /optimize+ /out:bin\SteamAchievementUnlocker.exe src\Program.cs
```

---

## ⚙️ How It Works (Technical Overview)

1. The master process locates the Steam client path via the Windows Registry (`HKCU\Software\Valve\Steam`).
2. It discovers games and validates which AppIDs have active achievements on the Steamworks server.
3. For each game, it runs an isolated worker process with `steam_appid.txt` set to that game's AppID.
4. The worker establishes a native Steamworks session via `SteamAPI_Init()`, queries missing achievements using `ISteamUserStats`, and syncs them to Steam via `StoreStats()`.

---

## ⚠️ Disclaimer

This tool is created for educational and personal use only. Use responsibly.
Not affiliated with Valve Corporation or Steam.

---

## 📄 License

This project is licensed under the [MIT License](LICENSE).
