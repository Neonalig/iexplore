using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Win32;
// ReSharper disable CommentTypo

namespace iexplore {
    public partial class MainWindow/* : Window*/ {
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool FreeConsole();

        public bool extoff; //ie: -extoff | chrome: --disable-extensions | means: No extensions
        public bool prv;  //ie: -private | chrome: --incognito | means: Incognito mode
        public bool kiosk; //ie: -k | chrome: --enable-kiosk-mode & --start-fullscreen | means: Fullscreen mode
        public List<string> urls;
        public List<string> args;

        public List<string> log;
        public bool logging;

        public MainWindow() {
            Hide();
            FreeConsole();

            urls = new List<string>();
            args = new List<string>();
            log = new List<string>();

            args = Environment.GetCommandLineArgs().ToList();
            args.RemoveAt(0);

            if(File.Exists(GetSelf() + "log.txt")) {
                logging = true;
                log.Add(">>File: \"" + GetSelf() + "log.txt\" found, logging enabled<<");
            }
            Start();
            //InitializeComponent(); //Starts the window
        }

        public void Start() {
            if (args != null && args.Count >= 1) {
                foreach (string arg in args) {
                    Debug.WriteLine("Got Arg: " + arg);
                    ParseArg(arg);
                }
            }

            RunBrowser(); //Currently chrome only
            if (logging) { SaveLog(); }
            Environment.Exit(0);
        }

        public string GetSelf() => Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\";

        public void ParseArg(string arg) {
            switch (arg) {
                case @"-extoff":
                    extoff = true;
                    log.Add("Arg: \"" + arg + "\" | Response: extoff = true");
                    Debug.WriteLine("Arg: \"" + arg + "\" | Response: extoff = true");
                    break;
                case "-private":
                    prv = true;
                    log.Add("Arg: \"" + arg + "\" | Response: prv = true");
                    Debug.WriteLine("Arg: \"" + arg + "\" | Response: prv = true");
                    break;
                case "-k":
                    kiosk = true;
                    log.Add("Arg: \"" + arg + "\" | Response: kiosk = true");
                    Debug.WriteLine("Arg: \"" + arg + "\" | Response: kiosk = true");
                    break;
                default:
                    if (arg.Contains("http") || arg.Contains("//") || arg.Contains("www.")) {
                        urls.Add(arg);
                        log.Add("<>Found Url: \"" + arg + "\"<>");
                    } else {
                        log.Add("<<Unknown Arg: \"" + arg + "\">>");
                    }
                    break;
            }
        }

        public string GetChromePath() {
            try {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\App Paths\\chrome.exe")) {
                    return key?.GetValue(null).ToString();
                }
            } catch (Exception e) {
                log.Add("<<Caught exception when reading registry: [[" + e + "]]>>");
                Debug.WriteLine("<<Caught exception when reading registry: \n" + e + ">>");
            }
            return null;
        }

        public string GetExt() => (extoff ? "--disable-extensions " : "") + (prv ? "--incognito " : "") + (kiosk ? @"--start-maximized" : "");

        public void RunBrowser() {
            if (urls == null || urls.Count < 1) {
                log.Add("<<No URL was found! Opening blank session>>");
                Process p = new Process {
                    StartInfo = {
                        FileName = GetChromePath(),
                        Arguments = GetExt()
                    }
                };
                p.Start();
            } else {
                log.Add(">>Running: " + GetChromePath() + " " + GetExt() + " \"" + urls[0] + "\"<<");
                try {
                    Process p = new Process {
                        StartInfo = {
                            FileName = GetChromePath(),
                            Arguments = GetExt() + " \"" + urls[0] + "\""
                        }
                    };
                    p.Start();
                } catch (Exception e) {
                    log.Add("<<Caught exception when loading chrome: [[" + e + "]]>>");
                    Debug.WriteLine("<<Caught exception when loading chrome: \n" + e + ">>");
                }
            }
        }

        public void SaveLog() {
            log.Add("<>Saving Log<>");
            File.WriteAllLines(GetSelf() + "log.txt", log);
        }
    }
}
