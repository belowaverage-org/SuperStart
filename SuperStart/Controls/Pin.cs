using System;
using System.Windows.Forms;

namespace SuperStart
{
    public partial class Pin : Form
    {
        public static string UnlockPin = "";
        public static string UnlockMessage = "";
        private string pinBuffer = "";
        public Pin()
        {
            InitializeComponent();
            lblDisplay.Text = UnlockMessage;
        }
        private void Btn_Click(object sender, EventArgs e)
        {
            Button thisButton = (Button)sender;
            lblDisplay.Focus();
            if (thisButton.Text == "Enter")
            {
                CheckPin();
            }
            else
            {
                pinBuffer += thisButton.Text;
                UpdateDisplay();
            }
        }
        private void CheckPin()
        {
            if (pinBuffer != UnlockPin)
            {
                Close();
            }
            else
            {
                Main.PinTimer.Stop();
                Enabled = false;
                ControlBox = false;
                lblDisplay.Text = "Exiting Super Start...";
                Main.StartExitProcessAndClose();
            }
        }
        private void UpdateDisplay()
        {
            string starDisplay = "";
            for (int count = 0; count != pinBuffer.Length; count++)
            {
                starDisplay += '*';
            }
            lblDisplay.Text = starDisplay;
        }
        private void Pin_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            e.SuppressKeyPress = true;
            int key;
            if(int.TryParse(e.KeyCode.ToString().Replace("NumPad", "").Replace("D", ""), out key))
            {
                pinBuffer += key.ToString();
                UpdateDisplay();
            }
            else if(e.KeyCode == Keys.Enter)
            {
                CheckPin();
            }
        }
        private void Pin_Load(object sender, EventArgs e)
        {
            Icon = Properties.Resources.logo;
        }
    }
}