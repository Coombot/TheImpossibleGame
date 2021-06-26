using EventHook;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoombotTIG
{
    public partial class Form1 : Form
    {
        // Game
        public static IntPtr BaseAddress;
        public static VAMemory GameMemory;
        public static Process GameProcess;
        public static bool GameHooked = false;

        // Hooks
        public static EventHookFactory eventHookFactory = new EventHookFactory();
        public static MouseWatcher mouseWatcher = eventHookFactory.GetMouseWatcher();

        public static Thread recordPlayThread;

        // Useful Bot Variables
        public static bool IsPaused
        {
            get
            {
                return false; 
            }
        }
        public static int CurFrame
        {
            get
            {
                IntPtr timerAddress = BaseAddress + 0x59518;
                return GameMemory.ReadInt32(timerAddress);
            }
            set
            {
                IntPtr timerAddress = BaseAddress + 0x59518;
                GameMemory.WriteInt32(timerAddress, value);
            }
        }

        // Bot Bools
        public static bool isRecording = false;
        public static bool isPlayingBack = false;
        public Form1()
        {
            InitializeComponent();
        }

        public static bool IsArrayNullOrEmpty<T>(T[] array) where T : class
        {
            if (array == null || array.Length == 0)
                return true;
            else
                return array.All(item => item == null);
        }

        public static void Playback()
        {

        }

        public static void StartRecording(string macroName)
        {
            using (var eventHookFactory = new EventHookFactory())
            {
                // need to implement save reading
                if (!Directory.Exists($@"C:\Users\{Environment.UserName}\AppData\Local\Coombot TIG Macros"))
                {
                    Directory.CreateDirectory($@"C:\Users\{Environment.UserName}\AppData\Local\Coombot TIG Macros");
                }
                string MacroDirectory = $@"C:\Users\{Environment.UserName}\AppData\Local\Coombot TIG Macros";
                File.Create($@"{MacroDirectory}\{macroName}.cbt").Close();
                CurFrame = 0;
                File.WriteAllText($@"{MacroDirectory}\{macroName}.cbt", $"-- COOMBOT FOR THE IMPOSSIBLE GAME MACRO --");

                mouseWatcher.Start();
                mouseWatcher.OnMouseInput += (s, e) =>
                {
                    // DEFAULT TO LBUTTON, NEEDS TO IMPLEMENT SAVE READING
                    if (e.Message.ToString().Trim() == "WM_LBUTTONDOWN")
                    {
                        string MacroText = File.ReadAllText($@"{MacroDirectory}\{macroName}.cbt");
                        File.WriteAllText($@"{MacroDirectory}\{macroName}.cbt", MacroText + $"\ninputType:1 timerFrame:{CurFrame} x:{e.Point.x}, y:{e.Point.y}");
                    }
                    else if (e.Message.ToString().Trim() == "WM_LBUTTONUP")
                    {
                        string MacroText = File.ReadAllText($@"{MacroDirectory}\{macroName}.cbt");
                        File.WriteAllText($@"{MacroDirectory}\{macroName}.cbt", MacroText + $"\ninputType:0 timerFrame:{CurFrame} x:{e.Point.x}, y:{e.Point.y}");
                    }
                };
            }
        }

        public static void StopRecording(string macroName)
        {
            mouseWatcher.Stop();
        }

        public void LookForGame()
        {
            lookForGame1.Text = "Looking for game instance...";
            if (!IsArrayNullOrEmpty(Process.GetProcessesByName("ImpossibleGame")))
            {
                GameProcess = Process.GetProcessesByName("ImpossibleGame").FirstOrDefault();
                GameMemory = new VAMemory("ImpossibleGame");

                BaseAddress = GameProcess.MainModule.BaseAddress;

                GameHooked = true;
                lookForGame1.Text = "Game instanced attached!";
            }
            else
            {
                lookForGame1.Text = "No game instance found.";
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LookForGame();

            // inputting
            var keyboardWatcher = eventHookFactory.GetKeyboardWatcher();
            keyboardWatcher.Start();
            keyboardWatcher.OnKeyInput += (s, ev) =>
            {
                if (ev.KeyData.Keyname == "F2")
                {
                    StartRecording(textBox1.Text);
                }
                if (ev.KeyData.Keyname == "F10")
                {
                    StopRecording(textBox1.Text);
                }
                Console.WriteLine(string.Format("Key {0} event of key {1}", ev.KeyData.EventType, ev.KeyData.Keyname));
            };
        }
    }
}
