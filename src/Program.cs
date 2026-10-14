using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Win32;

namespace SteamAchievementUnlocker
{
    internal class Program
    {
        #region Steamworks Native API Bindings
        [DllImport("steam_api64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool SteamAPI_Init();

        [DllImport("steam_api64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SteamAPI_Shutdown();

        [DllImport("steam_api64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr SteamUserStats();

        [DllImport("steam_api64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool SteamAPI_ISteamUserStats_RequestCurrentStats(IntPtr instance);

        [DllImport("steam_api64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint SteamAPI_ISteamUserStats_GetNumAchievements(IntPtr instance);

        [DllImport("steam_api64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr SteamAPI_ISteamUserStats_GetAchievementName(IntPtr instance, uint iAchievement);

        [DllImport("steam_api64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool SteamAPI_ISteamUserStats_GetAchievement(IntPtr instance, string pchName, out bool pbAchieved);

        [DllImport("steam_api64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool SteamAPI_ISteamUserStats_SetAchievement(IntPtr instance, string pchName);

        [DllImport("steam_api64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool SteamAPI_ISteamUserStats_StoreStats(IntPtr instance);
        #endregion

        static int Main(string[] args)
        {
            // Worker process mode for isolated AppID execution
            if (args.Length >= 2 && args[0] == "--worker")
            {
                return RunWorker(args[1]);
            }

            RunMaster();
            return 0;
        }

        private static int RunWorker(string appId)
        {
            try
            {
                File.WriteAllText("steam_appid.txt", appId);
            }
            catch { }

            if (!SteamAPI_Init())
            {
                Console.WriteLine("RESULT:FAIL_INIT");
                return 1;
            }

            IntPtr userStats = SteamUserStats();
            if (userStats == IntPtr.Zero)
            {
                SteamAPI_Shutdown();
                Console.WriteLine("RESULT:FAIL_STATS");
                return 2;
            }

            SteamAPI_ISteamUserStats_RequestCurrentStats(userStats);
            Thread.Sleep(2000);

            uint total = SteamAPI_ISteamUserStats_GetNumAchievements(userStats);
            if (total == 0)
            {
                SteamAPI_Shutdown();
                Console.WriteLine("RESULT:NO_ACHIEVEMENTS");
                return 0;
            }

            uint unlocked = 0;
            uint already = 0;

            for (uint i = 0; i < total; i++)
            {
                IntPtr ptrName = SteamAPI_ISteamUserStats_GetAchievementName(userStats, i);
                if (ptrName == IntPtr.Zero) continue;
                string achName = Marshal.PtrToStringAnsi(ptrName);

                bool isAchieved = false;
                SteamAPI_ISteamUserStats_GetAchievement(userStats, achName, out isAchieved);
                if (!isAchieved)
                {
                    if (SteamAPI_ISteamUserStats_SetAchievement(userStats, achName))
                    {
                        unlocked++;
                    }
                }
                else
                {
                    already++;
                }
            }

            bool stored = false;
            if (unlocked > 0)
            {
                stored = SteamAPI_ISteamUserStats_StoreStats(userStats);
            }
            else
            {
                stored = true;
            }

            SteamAPI_Shutdown();
            Console.WriteLine(string.Format("RESULT:SUCCESS:{0}:{1}:{2}:{3}", unlocked, already, total, stored ? "1" : "0"));
            return 0;
        }

        private static void RunMaster()
        {
            Console.Title = "Steam Achievement Unlocker";
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("==================================================================");
            Console.WriteLine("                 STEAM ACHIEVEMENT UNLOCKER                       ");
            Console.WriteLine("==================================================================");
            Console.ResetColor();

            string steamPath = GetSteamPath();
            if (string.IsNullOrEmpty(steamPath) || !Directory.Exists(steamPath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[-] Steam installation path not found.");
                Console.ResetColor();
                Console.ReadKey();
                return;
            }

            Console.WriteLine(string.Format("[*] Steam Path: {0}", steamPath));
            Console.WriteLine("[*] Scanning owned and installed games...");

            Dictionary<string, string> games = DiscoverAllGames(steamPath);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(string.Format("[+] Discovered {0} games/apps in your Steam library.\n", games.Count));
            Console.ResetColor();

            Console.WriteLine("Choose an option:");
            Console.WriteLine(" [1] Auto: Scan and unlock achievements for ALL library games");
            Console.WriteLine(" [2] Scan and unlock only currently installed games");
            Console.WriteLine(" [3] Enter custom Steam AppID manually");
            Console.WriteLine(" [4] Exit");
            Console.Write("\nSelection: ");

            string choice = Console.ReadLine();
            if (choice != null) choice = choice.Trim();

            if (choice == "4") return;

            List<string> targets = new List<string>();

            if (choice == "3")
            {
                Console.Write("Enter Steam AppID: ");
                string customId = Console.ReadLine();
                if (!string.IsNullOrEmpty(customId)) targets.Add(customId.Trim());
            }
            else if (choice == "2")
            {
                Dictionary<string, string> installed = DiscoverInstalledGames(steamPath);
                foreach (var kvp in installed) targets.Add(kvp.Key);
            }
            else
            {
                foreach (var kvp in games) targets.Add(kvp.Key);
            }

            if (targets.Count == 0)
            {
                Console.WriteLine("No target games found.");
                return;
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(string.Format("\n[*] Processing {0} game(s)...\n", targets.Count));
            Console.ResetColor();

            int totalUnlocked = 0;
            int totalGamesWithAch = 0;
            int index = 0;

            string currentExe = Process.GetCurrentProcess().MainModule.FileName;

            foreach (string appId in targets)
            {
                index++;
                string gameName = games.ContainsKey(appId) ? games[appId] : ("AppID " + appId);
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(string.Format("[{0}/{1}] {2} (AppID: {3})... ", index, targets.Count, gameName, appId));
                Console.ResetColor();

                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = currentExe;
                psi.Arguments = string.Format("--worker {0}", appId);
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                psi.CreateNoWindow = true;

                string output = "";
                try
                {
                    using (Process p = Process.Start(psi))
                    {
                        output = p.StandardOutput.ReadToEnd();
                        p.WaitForExit(10000);
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error: " + ex.Message);
                    Console.ResetColor();
                    continue;
                }

                if (output.Contains("RESULT:SUCCESS"))
                {
                    string[] parts = output.Substring(output.IndexOf("RESULT:SUCCESS")).Split(':');
                    if (parts.Length >= 5)
                    {
                        int unl = int.Parse(parts[2]);
                        int alr = int.Parse(parts[3]);
                        int tot = int.Parse(parts[4]);

                        totalGamesWithAch++;
                        totalUnlocked += unl;

                        if (unl > 0)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine(string.Format("[+] UNLOCKED {0} achievements! (Total: {1})", unl, tot));
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                            Console.WriteLine(string.Format("[=] Already unlocked ({0}/{1})", alr, tot));
                            Console.ResetColor();
                        }
                    }
                }
                else if (output.Contains("RESULT:NO_ACHIEVEMENTS"))
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine("No achievements.");
                    Console.ResetColor();
                }
                else if (output.Contains("RESULT:FAIL_INIT"))
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine("Init skipped (tool or no license).");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine("Skipped.");
                    Console.ResetColor();
                }
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n==================================================================");
            Console.WriteLine("                         SUMMARY REPORT                           ");
            Console.WriteLine("==================================================================");
            Console.WriteLine(string.Format(" Games with achievements : {0}", totalGamesWithAch));
            Console.WriteLine(string.Format(" Total achievements unlocked : {0}", totalUnlocked));
            Console.WriteLine("==================================================================");
            Console.ResetColor();

            Console.WriteLine("\nDone. Press any key to exit...");
            Console.ReadKey();
        }

        private static string GetSteamPath()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam"))
                {
                    if (key != null)
                    {
                        object path = key.GetValue("SteamPath");
                        if (path != null) return path.ToString().Replace('/', '\\');
                    }
                }
            }
            catch { }
            return @"C:\Program Files (x86)\Steam";
        }

        private static Dictionary<string, string> DiscoverInstalledGames(string steamPath)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            try
            {
                string steamApps = Path.Combine(steamPath, "steamapps");
                if (Directory.Exists(steamApps))
                {
                    foreach (string file in Directory.GetFiles(steamApps, "appmanifest_*.acf"))
                    {
                        string id = Regex.Match(Path.GetFileName(file), @"\d+").Value;
                        string name = GetGameNameFromAcf(file);
                        if (!string.IsNullOrEmpty(id) && id != "228980" && !dict.ContainsKey(id))
                        {
                            dict[id] = string.IsNullOrEmpty(name) ? ("AppID " + id) : name;
                        }
                    }
                }

                string libFolders = Path.Combine(steamApps, "libraryfolders.vdf");
                if (File.Exists(libFolders))
                {
                    string text = File.ReadAllText(libFolders);
                    MatchCollection matches = Regex.Matches(text, @"""path""\s+""([^""]+)""");
                    foreach (Match m in matches)
                    {
                        string libPath = m.Groups[1].Value.Replace(@"\\", @"\");
                        string subApps = Path.Combine(libPath, "steamapps");
                        if (Directory.Exists(subApps))
                        {
                            foreach (string file in Directory.GetFiles(subApps, "appmanifest_*.acf"))
                            {
                                string id = Regex.Match(Path.GetFileName(file), @"\d+").Value;
                                string name = GetGameNameFromAcf(file);
                                if (!string.IsNullOrEmpty(id) && id != "228980" && !dict.ContainsKey(id))
                                {
                                    dict[id] = string.IsNullOrEmpty(name) ? ("AppID " + id) : name;
                                }
                            }
                        }
                    }
                }
            }
            catch { }
            return dict;
        }

        private static Dictionary<string, string> DiscoverAllGames(string steamPath)
        {
            Dictionary<string, string> dict = DiscoverInstalledGames(steamPath);
            try
            {
                string userDir = Path.Combine(steamPath, "userdata");
                if (Directory.Exists(userDir))
                {
                    foreach (string uDir in Directory.GetDirectories(userDir))
                    {
                        string localCfg = Path.Combine(uDir, @"config\localconfig.vdf");
                        if (File.Exists(localCfg))
                        {
                            string content = File.ReadAllText(localCfg);
                            MatchCollection matches = Regex.Matches(content, @"""([0-9]{3,8})""\s*\{");
                            foreach (Match m in matches)
                            {
                                string id = m.Groups[1].Value;
                                int num;
                                if (int.TryParse(id, out num))
                                {
                                    if (num > 100 && num != 760 && num != 228980 && !dict.ContainsKey(id))
                                    {
                                        dict[id] = "AppID " + id;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch { }
            return dict;
        }

        private static string GetGameNameFromAcf(string acfPath)
        {
            try
            {
                string text = File.ReadAllText(acfPath);
                Match m = Regex.Match(text, @"""name""\s+""([^""]+)""");
                if (m.Success) return m.Groups[1].Value;
            }
            catch { }
            return null;
        }
    }
}
