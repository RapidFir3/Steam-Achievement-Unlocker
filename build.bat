@echo off
echo [*] Compiling Steam Achievement Unlocker...
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /target:exe /platform:x64 /optimize+ /out:bin\SteamAchievementUnlocker.exe src\Program.cs
if %ERRORLEVEL% equ 0 (
    echo [+] Compilation successful! Output: bin\SteamAchievementUnlocker.exe
) else (
    echo [-] Compilation failed.
)
pause
