using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace SuperStart
{
    public partial class Main : Form
    {
        private bool IsPinOpen = false;
        private Pin Pin = null;
        private Timer PinTimer = new Timer();
        private Timer StartTimer = new Timer();
        private int StartTimerDelay = 0;
        private Control Mdi = null;
        private Process StartedProcess = new Process();
        public Main()
        {
            Config.LoadConfig();
            StartTimerDelay = int.Parse(Config.Settings["StartDelay"]);
            Pin.UnlockMessage = Config.Settings["UnlockMessage"];
            Pin.UnlockPin = Config.Settings["UnlockPin"];
            PinTimer.Interval = StartTimer.Interval = 1000;
            PinTimer.Tick += PinTimer_Tick;
            StartTimer.Tick += StartTimer_Tick;
            StartedProcess.EnableRaisingEvents = true;
            StartedProcess.Exited += Process_Exited;
            StartedProcess.StartInfo.FileName = Config.Settings["StartProcess"];
            InitializeComponent();
        }
        private bool CheckForRunningProcess()
        {
            foreach (Process process in Process.GetProcesses())
            {
                if (process.ProcessName == Path.GetFileNameWithoutExtension(Config.Settings["StartProcess"]))
                {
                    StartTimer.Stop();
                    process.EnableRaisingEvents = true;
                    process.Exited += Process_Exited;
                    Hide();
                    return true;
                }
            }
            return false;
        }
        private void StartTimer_Tick(object sender, EventArgs e)
        {
            CheckForRunningProcess();
            if (--StartTimerDelay <= 0)
            {
                StartTimer.Stop();
                StartedProcess.Start();
                Hide();
                return;
            }
            Mdi.Invalidate(new Rectangle(10, 10, 50, 20));
        }
        private void Process_Exited(object sender, EventArgs e)
        {
            Invoke(new Action(() => {
                if(!CheckForRunningProcess())
                {
                    Show();
                    StartTimerDelay = int.Parse(Config.Settings["RestartDelay"]);
                    StartTimer.Start();
                }
            }));
        }
        private void Main_Load(object sender, EventArgs e)
        {
            foreach(Control control in Controls)
            {
                if(control is MdiClient)
                {
                    Mdi = control;
                    break;
                }
            }
            Mdi.BackColor = Color.Black;
            Mdi.Dock = DockStyle.None;
            Mdi.Width = Width + 4;
            Mdi.Height = Height + 4;
            Mdi.Left = -2;
            Mdi.Top = -2;
            Mdi.Click += Control_Click;
            Mdi.Paint += Control_Paint;
            if (File.Exists(Config.Settings["BackgroundImage"]))
            {
                Mdi.BackgroundImage = Image.FromFile(Config.Settings["BackgroundImage"]);
            }
            if (!CheckForRunningProcess())
            {
                StartTimer.Start();
            }
        }
        private void Control_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.DrawString(StartTimerDelay.ToString(), Font, Brushes.White, 10, 10);
        }
        private void Control_Click(object sender, EventArgs e)
        {
            if(!IsPinOpen)
            {
                IsPinOpen = true;
                Pin = new Pin();
                Pin.FormClosing += Pin_FormClosing;
                Pin.Text = Config.Settings["UnlockTimeout"];
                Pin.MdiParent = this;
                Pin.Show();
                PinTimer.Start();
                StartTimer.Stop();
            }
        }
        private void PinTimer_Tick(object sender, EventArgs e)
        {
            Pin.Text = (int.Parse(Pin.Text) - 1).ToString();
            if(Pin.Text == "0")
            {
                Pin.Close();
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
            e.Cancel = true;
        }
    }
}
