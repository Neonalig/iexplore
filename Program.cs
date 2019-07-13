using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Win32;

namespace iexplore {
    public static class Program {
        public static bool extoff; //ie: -extoff | chrome: --disable-extensions | means: No extensions
        public static bool prv;  //ie: -private | chrome: --incognito | means: Incognito mode
        public static bool kiosk; //ie: -k | chrome: --enable-kiosk-mode & --start-fullscreen | means: Fullscreen mode
        public static List<string> urls;

        public static void Main(string[] args) {
            Debug.WriteLine("Chrome Installation Path: " + GetChromePath());
            if (args != null && args.Length >= 1) {
                foreach (string arg in args) { ParseArg(arg); }
            }

            RunBrowser(); //currently chrome only
        }

        public static void ParseArg(string arg) {
            switch(arg) {
                case @"-extoff":
                    extoff = true;
                    break;
                case "-private":
                    prv = true;
                    break;
                case "-k":
                    kiosk = true;
                    break;
                default:
                    Debug.WriteLine("Unknown Argument, presumed url");
                    urls.Add(arg);
                    break;
            }
        }

        public static string GetChromePath() {
            try {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\App Paths\\chrome.exe")) {
                    return key?.GetValue(null).ToString();
                }
            } catch (Exception e) { Debug.WriteLine("Caught exception when reading registry: \n" + e); }
            return null;
        }

        public static string GetExt() => (extoff ? "--disable-extensions " : "") + (prv ? "--incognito " : "") + (kiosk ? "--enable-kiosk-mode --start-fullscreen " : "");

        public static void RunBrowser() {
            Process p = new Process {
                StartInfo = {
                    FileName = GetChromePath(),
                    Arguments = GetExt()
                }
            };
            p.Start();
        }
    }
}
