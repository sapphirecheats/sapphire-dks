using KeyListener;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Sapphire_DKS.Variables;
using static Sapphire_DKS.KeyboardHook;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using System.IO;
using System.Diagnostics;

namespace Sapphire_DKS
{

    public partial class Form1 : Form
    {
        private readonly GlobalKeyListener _keyListener = new GlobalKeyListener();
        private readonly Thread _actuationThread;

        public Form1()
        {
            InitializeComponent();
            SetHook();
            _actuationThread = new Thread(ActuateKeyThread) { IsBackground = true };
            _actuationThread.Start();
            _keyListener.ListenForKey();
            LoadConfig();

            if (!vars.trueStreamerMode) Process.Start("https://discord.sapphire.ac/");
        }

        private void ActuateKeyThread()
        {
            var random = new Random();
            while (true)
            {
                if (vars.sendKeyPress)
                {
                    // Get the true min/max values based on the randomization percentage set by the user
                    int minInput = Math.Max(0, (int)inputDelaySlider.Value - vars.inputRandomRange);
                    int maxInput = (int)inputDelaySlider.Value + vars.inputRandomRange;
                    int minHold = Math.Max(0, (int)holdDelaySlider.Value - vars.holdRandomRange);
                    int maxHold = (int)holdDelaySlider.Value + vars.holdRandomRange;

                    // Determine the type of input to send based on if DKS mode is enabled or not
                    ushort key = vars.dksMode ? (ushort)vars.activationKey : (ushort)vars.alternateKey;
                    ActuateKey(key, random.Next(minInput, maxInput), random.Next(minHold, maxHold));
                    Console.WriteLine($"Sent '{(Keys)key}' input");

                    // Disable the thread from sending any more inputs until we detect a new key press
                    vars.sendKeyPress = false;
                }
                Thread.Sleep(1);
            }
        }

        private void LoadConfig()
        {
            try
            {
                if (!File.Exists("config.json")) return;

                string json = File.ReadAllText("config.json");
                dynamic config = JsonConvert.DeserializeObject(json);

                vars.dksMode = config.dksMode;
                vars.trueStreamerMode = config.trueStreamerMode;
                vars.inputDelay = config.inputDelay;
                vars.inputRandomization = config.inputRandomization;
                vars.holdDelay = config.holdDelay;
                vars.holdRandomization = config.holdRandomization;
                vars.activationKey = (Keys)Enum.Parse(typeof(Keys), config.activationKey.ToString());
                vars.alternateKey = (Keys)Enum.Parse(typeof(Keys), config.alternateKey.ToString());
                vars.holdKey = (Keys)Enum.Parse(typeof(Keys), config.holdKey.ToString());
                vars.hideKey = (Keys)Enum.Parse(typeof(Keys), config.hideWindowKey.ToString());
                _keyListener.BindKey(vars.hideKey, ToggleVisibility);

                dksModeToggle.IsChecked = vars.alternateKey != Keys.None ? config.dksMode : true;
                vars.dksMode = dksModeToggle.IsChecked;
                activationKeyText.Text = $"[{vars.activationKey}]".ToLower();
                alternateKeyText.Text = $"[{vars.alternateKey}]".ToLower();
                holdKeyText.Text = $"[{vars.holdKey}]".ToLower();
                hideKeyText.Text = $"[{vars.hideKey}]".ToLower();

                inputDelaySlider.Value = config.inputDelay;
                inputRandomizationSlider.Value = config.inputRandomization;
                holdDelaySlider.Value = config.holdDelay;
                holdRandomizationSlider.Value = config.holdRandomization;

                if (vars.trueStreamerMode)
                {
                    SetWindowDisplayAffinity(Handle, WDA_EXCLUDEFROMCAPTURE);
                    streamerModeWarning.Visible = true;
                    ToolTip.Active = false;
                }
            }
            catch
            {
                MessageBox.Show("Error loading config.");
            }
        }

        private void SaveConfig()
        {
            var config = new
            {
                vars.dksMode,
                vars.trueStreamerMode,
                vars.inputDelay,
                vars.inputRandomization,
                vars.holdDelay,
                vars.holdRandomization,
                activationKey = vars.activationKey.ToString(),
                alternateKey = vars.alternateKey.ToString(),
                holdKey = vars.holdKey.ToString(),
                hideWindowKey = vars.hideKey.ToString(),
            };

            string json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText("config.json", json);
        }

        #region Widget controls

        private void ToggleVisibility()
        {
            if (InvokeRequired)
                Invoke(new Action(ToggleVisibility));
            else
                Visible = !Visible;
        }

        private static int CalculatePercent(int inputDelay, int inputRandomizationPercentage)
        {
            double randomizationFactor = inputRandomizationPercentage / 100.0;
            return (int)(inputDelay * randomizationFactor);
        }

        private void UpdateDelayText(int slider, int randomRange, Label delayText)
        {
            int min = Math.Max(0, slider - randomRange);
            int max = slider + randomRange;
            delayText.Text = $"{min} - {max} ms";
        }

        private void inputDelaySlider_Paint(object sender, PaintEventArgs e)
        {
            vars.inputDelay = (int)inputDelaySlider.Value;
            vars.inputRandomRange = CalculatePercent((int)inputDelaySlider.Value, (int)inputRandomizationSlider.Value);
            UpdateDelayText((int)inputDelaySlider.Value, vars.inputRandomRange, inputDelayText);
        }

        private void holdDelaySlider_Paint(object sender, PaintEventArgs e)
        {
            vars.holdDelay = (int)holdDelaySlider.Value;
            vars.holdRandomRange = CalculatePercent((int)holdDelaySlider.Value, (int)holdRandomizationSlider.Value);
            UpdateDelayText((int)holdDelaySlider.Value, vars.holdRandomRange, holdDelayText);
        }

        private void inputRandomizationSlider_Paint(object sender, PaintEventArgs e)
        {
            inputRandomizationText.Text = $"{inputRandomizationSlider.Value}%";
            vars.inputRandomization = (int)inputRandomizationSlider.Value;
            vars.inputRandomRange = CalculatePercent((int)inputDelaySlider.Value, (int)inputRandomizationSlider.Value);
            UpdateDelayText((int)inputDelaySlider.Value, vars.inputRandomRange, inputDelayText);
        }

        private void holdRandomizationSlider_Paint(object sender, PaintEventArgs e)
        {
            holdRandomizationText.Text = $"{holdRandomizationSlider.Value}%";
            vars.holdRandomization = (int)holdRandomizationSlider.Value;
            vars.holdRandomRange = CalculatePercent((int)holdDelaySlider.Value, (int)holdRandomizationSlider.Value);
            UpdateDelayText((int)holdDelaySlider.Value, vars.holdRandomRange, holdDelayText);
        }

        private async void activationKeyText_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                activationKeyText.Text = "[none]";
                vars.activationKey = Keys.None;
                return;
            }

            activationKeyText.Text = "[...]";
            vars.activationKey = await Task.Run(() => _keyListener.GetKeyPressed());
            activationKeyText.Text = $"[{vars.activationKey.ToString().ToLower()}]";
        }

        private async void alternateKeyText_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                alternateKeyText.Text = "[none]";
                vars.alternateKey = Keys.None;
                return;
            }

            alternateKeyText.Text = "[...]";
            vars.alternateKey = await Task.Run(() => _keyListener.GetKeyPressed());
            alternateKeyText.Text = $"[{vars.alternateKey.ToString().ToLower()}]";
        }

        private async void holdKeyText_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                holdKeyText.Text = "[none]";
                vars.holdKey = Keys.None;
                return;
            }

            holdKeyText.Text = "[...]";
            vars.holdKey = await Task.Run(() => _keyListener.GetKeyPressed());
            holdKeyText.Text = $"[{vars.holdKey.ToString().ToLower()}]";
        }

        private async void hideKeyText_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                _keyListener.RemoveKeyBinding(vars.hideKey);
                hideKeyText.Text = "[none]";
                vars.hideKey = Keys.None;
                return;
            }

            _keyListener.RemoveKeyBinding(vars.hideKey);

            hideKeyText.Text = "[...]";
            vars.hideKey = await Task.Run(() => _keyListener.GetKeyPressed());
            hideKeyText.Text = $"[{vars.hideKey.ToString().ToLower()}]";

            _keyListener.BindKey(vars.hideKey, ToggleVisibility);
        }

        private void dksModeToggle_Click(object sender, EventArgs e)
        {
            // If the user tries to disable DKS mode without an alternate key we dont allow them to, this fixes some edge cases within the actual
            // keyboard hook and the key actuation code
            if (!dksModeToggle.IsChecked && vars.alternateKey == Keys.None) {
                MessageBox.Show("You must set an alternate key before you can disable DKS mode.", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                dksModeToggle.IsChecked = true;
            }
            vars.dksMode = dksModeToggle.IsChecked;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            File.Delete("config.json");
            SaveConfig();
            UnHook();
            Environment.Exit(0);
        }

        private void dksToolTip_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "When DKS mode is ENABLED only 1 key is effected (your DKS key), that key will actuate when you lift up on the key rather than when you press it down.\n\n" +
                "When DKS mode is DISABLED your DKS key now acts as an activation key for your \"alternate key\". When pressing your DKS key it will send an alternate key input.\n\n" +
                "For example, with 'Z' as your DKS key & 'X' as your alternate key, with DKS mode off you can simply press Z & it will send an X input after.", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void watermarkLink_Click(object sender, EventArgs e)
        {
            Process.Start("https://discord.sapphire.ac/");
        }
        #endregion
    }
}