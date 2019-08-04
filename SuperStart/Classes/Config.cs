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
            {"StartProcess", @"C:\Windows\System32\cmd.exe"},
            {"XComment1", "The process that will be launched on exit."},
            {"ExitProcess", @"C:\Windows\System32\cmd.exe"},
            {"XComment2", "The delay that the program will be launched with the first time."},
            {"StartDelay", "1"},
            {"XComment3", "The delay that the program will be launched with the consecutive times."},
            {"RestartDelay", "5"},
            {"XComment4", "The image that will be shown full screen while the program is closed."},
            {"BackgroundImage", @"C:\Windows\Web\Screen\img100.jpg"},
            {"XComment5", "The message displayed when the PIN screen appears."},
            {"UnlockMessage", "Enter Unlock PIN."},
            {"XComment6", "The PIN used to quit out after double clicking the background if the program is closed."},
            {"UnlockPin", "1234"},
            {"XComment7", "The amount of time in seconds to enter the PIN."},
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
            new XDocument(root).Save(@".\SuperStart.xml");
        }
        public static void LoadConfig()
        {
            if (!File.Exists(@".\SuperStart.xml"))
            {
                GenerateConfig();
            }
            try
            {
                XDocument config = XDocument.Load(@".\SuperStart.xml");
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
        }
    }
}