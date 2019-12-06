using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SuperStart
{
    public partial class Main : Form
    {
        public static Form Form = null;
        public static Timer PinTimer = new Timer();
        private bool IsPinOpen = false;
        private Pin Pin = null;
        private Timer StartTimer = new Timer();
        private int TimerRemaining = int.Parse(Config.Settings["StartDelay"]);
        private int RestartTimerDelay = int.Parse(Config.Settings["RestartDelay"]);
        private Process ProcessToStart = new Process();
        private static bool AllowClose = false;
        private bool ProcessIsStarting = false;
        public Main(string[] parameters)
        {
            Form = this;
            Config.LoadConfig(parameters);
            int Delay;
            if (int.TryParse(Config.Settings["StartDelay"], out Delay))
            {
                TimerRemaining = Delay;
            }
            if (int.TryParse(Config.Settings["RestartDelay"], out Delay))
            {
                RestartTimerDelay = Delay;
            }
            Pin.UnlockMessage = Config.Settings["UnlockMessage"];
            Pin.UnlockPin = Config.Settings["UnlockPin"];
            PinTimer.Interval = StartTimer.Interval = 1000;
            PinTimer.Tick += PinTimer_Tick;
            StartTimer.Tick += StartTimer_Tick;
            ProcessToStart.EnableRaisingEvents = true;
            ProcessToStart.Exited += Invoke_Relaunch_Timeout;
            ProcessToStart.StartInfo.FileName = Config.Settings["StartProcessFileName"];
            ProcessToStart.StartInfo.WorkingDirectory = Config.Settings["StartProcessWorkingDirectory"];
            ProcessToStart.StartInfo.Arguments = Config.Settings["StartProcessArguments"];
            InitializeComponent();
        }
        public async static void StartExitProcessAndClose()
        {
            if (File.Exists(Config.Settings["ExitProcessFileName"]) && Path.GetExtension(Config.Settings["ExitProcessFileName"]).ToLower() == ".exe")
            {
                try
                {
                    await Task.Run(() => {
                        Process ExitProcess = new Process();
                        ExitProcess.StartInfo.FileName = Config.Settings["ExitProcessFileName"];
                        ExitProcess.StartInfo.WorkingDirectory = Config.Settings["ExitProcessWorkingDirectory"];
                        ExitProcess.StartInfo.Arguments = Config.Settings["ExitProcessArguments"];
                        ExitProcess.Start();
                    });
                }
                catch (Exception e)
                {
                    Console.WriteLine("Could not start exit process: ");
                    Console.WriteLine(e.Message);
                }
            }
            AllowClose = true;
            Form.Close();
        }
        private bool StartProcessIsRunning()
        {
            foreach (Process process in Process.GetProcesses())
            {
                if (process.ProcessName == Path.GetFileNameWithoutExtension(Config.Settings["StartProcessFileName"]))
                {
                    try
                    {
                        process.EnableRaisingEvents = true;
                        process.Exited += Invoke_Relaunch_Timeout;
                        return true;
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine("Failed to bind to an existing process: ");
                        Console.WriteLine(e.Message);
                    }
                }
            }
            return false;
        }
        private async void StartTimer_Tick(object sender, EventArgs e)
        {
            if(StartProcessIsRunning())
            {
                StartTimer.Stop();
                Hide();
                Enabled = false;
                return;
            }
            if (TimerRemaining <= 0)
            {
                StartTimer.Stop();
                try
                {
                    ProcessIsStarting = true;
                    Background.Invalidate(new Rectangle(10, 10, 100, 20));
                    await Task.Run(() => {
                        ProcessToStart.Start();
                    });
                    Hide();
                    ProcessIsStarting = false;
                    Enabled = false;
                    return;
                }
                catch(Exception exc)
                {
                    Console.WriteLine("Failed to start process: ");
                    Console.WriteLine(exc.Message);
                    switch (Config.Settings["StartProcessFailBehavior"])
                    {
                        case "StartExitProcessAndClose":
                            {
                                StartExitProcessAndClose();
                                break;
                            }
                        case "KeepTrying":
                            {
                                Invoke_Relaunch_Timeout(null, null);
                                break;
                            }
                        case "Close":
                            {
                                AllowClose = true;
                                Form.Close();
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }
                }
            }
            else if(!Visible || WindowState == FormWindowState.Minimized)
            {
                Enabled = true;
                Show();
                WindowState = FormWindowState.Maximized;
            }
            if(TimerRemaining >= 0)
            {
                TimerRemaining--;
            }
            Background.Invalidate(new Rectangle(10, 10, 50, 20));
        }
        private void Invoke_Relaunch_Timeout(object sender, EventArgs e)
        {
            Invoke(new Action(() => {
                TimerRemaining = RestartTimerDelay;
                StartTimer.Start();
                StartTimer_Tick(null, null);
            }));
        }
        private void Main_Load(object sender, EventArgs e)
        {
            Icon = Properties.Resources.keys_5;
            if (File.Exists(Config.Settings["BackgroundImage"]))
            {
                Background.Image = Image.FromFile(Config.Settings["BackgroundImage"]);
            }
            StartTimer.Start();
            StartTimer_Tick(null, null);
        }
        private void Control_Click_Or_Key(object sender, EventArgs e)
        {
            if(!IsPinOpen)
            {
                IsPinOpen = true;
                Pin = new Pin();
                Pin.FormClosing += Pin_FormClosing;
                Pin.Text = Config.Settings["UnlockTimeout"];
                Pin.Owner = this;
                Pin.Show();
                PinTimer.Start();
                StartTimer.Stop();
            }
        }
        private void PinTimer_Tick(object sender, EventArgs e)
        {
            int PinNumber;
            if (int.TryParse(Pin.Text, out PinNumber))
            {
                Pin.Text = (PinNumber - 1).ToString();
                if (Pin.Text == "0")
                {
                    Pin.Close();
                }
            }
        }
        private void Pin_FormClosing(object sender, FormClosingEventArgs e)
        {
            StartTimer.Start();
            PinTimer.Stop();
            IsPinOpen = false;
        }
        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(!AllowClose)
            {
                e.Cancel = true;
            }
        }
        private void Background_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            if(ProcessIsStarting)
            {
                g.DrawString("Starting...", Font, Brushes.White, 10, 10);
            }
            else
            {
                g.DrawString((TimerRemaining + 1).ToString(), Font, Brushes.White, 10, 10);
            }
        }
    }
}
