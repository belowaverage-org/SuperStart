using System.Xml.Linq;
using System.IO;
using System;
using System.Collections.Generic;

namespace SuperStart
{
    class Config
    {
        public static Dictionary<string, string> Settings = new Dictionary<string, string>() {
            {"XComment0", "The process that will be launched."},
            {"StartProcessFileName", @"C:\Windows\System32\cmd.exe"},
            {"StartProcessWorkingDirectory", @"C:\"},
            {"StartProcessArguments", "/K echo Start Process"},
            {"XComment8", "What to do if the start process wont start. Choices: StartExitProcessAndClose, DoNothing, KeepTrying, Close"},
            {"StartProcessFailBehavior", "DoNothing"},
            {"XComment1", "The process that will be launched on exit."},
            {"ExitProcessFileName", @"C:\Windows\System32\cmd.exe"},
            {"ExitProcessWorkingDirectory", @"C:\"},
            {"ExitProcessArguments", "/K echo Exit Process"},
            {"XComment2", "The delay that the program will be launched with the first time."},
            {"StartDelay", "1"},
            {"XComment3", "The delay that the program will be launched with the consecutive times."},
            {"RestartDelay", "5"},
            {"XComment4", "The image that will be shown full screen while the program is closed."},
            {"BackgroundImage", @"C:\Windows\Web\Screen\img100.jpg"},
            {"XComment5", "The message displayed when the PIN screen appears."},
            {"UnlockMessage", "Enter Unlock PIN"},
            {"XComment6", "The PIN used to quit out after double clicking the background if the program is closed."},
            {"UnlockPin", "1234"},
            {"XComment7", "The amount of time in seconds to enter the PIN. (To disable timer, set to nothing or a string)"},
            {"UnlockTimeout", "10"}
        };
        public static void GenerateConfig()
        {
            XElement root = new XElement("SuperStart");
            foreach (KeyValuePair<string, string> setting in Settings)
            {
                if (setting.Key.StartsWith("XComment"))
                {
                    root.Add(new XComment(setting.Value));
                }
                else
                {
                    root.Add(new XElement(setting.Key, setting.Value));
                }
            }
            new XDocument(root).Save(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @".\SuperStart.xml"));
        }
        public static void LoadConfig(string[] parameters = null)
        {
            if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @".\SuperStart.xml")))
            {
                GenerateConfig();
            }
            try
            {
                XDocument config = XDocument.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @".\SuperStart.xml"));
                XElement root = config.Element("SuperStart");
                Dictionary<string, string> xmlSettings = new Dictionary<string, string>();
                foreach (KeyValuePair<string, string> setting in Settings)
                {
                    if (!setting.Key.StartsWith("XComment"))
                    {
                        XElement element = root.Element(setting.Key);
                        if (element != null)
                        {
                            xmlSettings[setting.Key] = element.Value;
                        }
                    }
                }
                Settings = xmlSettings;
            }
            catch (Exception) { }
            if (parameters != null && parameters.Length > 0)
            {
                foreach(string parameter in parameters)
                {
                    if(parameter.StartsWith("/") || parameter.StartsWith("-") && parameter.Contains(":"))
                    {
                        string[] param_parts = parameter.Substring(1).Split(':');
                        if (Settings.ContainsKey(param_parts[0]))
                        {
                            Settings[param_parts[0]] = param_parts[1];
                        }
                    }
                }
            }
        }
    }
}