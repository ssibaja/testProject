using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Windows.Interop;

using System.Diagnostics;

namespace EGOSK {

    public partial class EGKeyboard : UserControl {
        private Window parentWindow;
        private List<Button> keyCollection = new List<Button>();
        private IInputElement focusedInputElement;
        private bool capsLock = false;

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern UInt32 GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SendInput", SetLastError = true)]               
        static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);
        [DllImport("user32.dll")]
        static extern byte VkKeyScan(char ch);
        [DllImport("user32.dll")]
        static extern IntPtr GetMessageExtraInfo();

        private enum KeyEvent {
            KeyUp = 0x0002,
            KeyDown = 0x0000,
            ExtendedKey = 0x0001

        }

        private struct KEYBDINPUT {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public long time;
            public uint dwExtraInfo;
        };

        [StructLayout(LayoutKind.Explicit, Size = 28)]        
        private struct INPUT {
            [FieldOffset(0)]
            public uint type;
            [FieldOffset(4)]
            public KEYBDINPUT ki;
        };


        public EGKeyboard(Window parent) {
            parentWindow = parent;
            setupKeyboardControl();
        }

        public EGKeyboard(IInputElement elementToFocusOn) {
            focusedInputElement = elementToFocusOn;
            setupKeyboardControl();
        }

        private void setupKeyboardControl() {
            InitializeComponent();
            addAllKeysToInternalCollection();
            installAllClickEventsForCollection(this.keyCollection);
        }

        private void addAllKeysToInternalCollection() {
            foreach (Panel panelElement in allKeysGrid.Children) {
                foreach (Button buttonElement in panelElement.Children) {
                    keyCollection.Add(buttonElement);
                }
            }
        }

        private void installAllClickEventsForCollection(List<Button> keysToInstall) {
            foreach (Button buttonElement in keysToInstall) {
                buttonElement.Click += new RoutedEventHandler(buttonElement_Click);
            }
        }

        private void switchCase(List<Button> keysToSwitch) {
            try {
                foreach (Button buttonElement in keysToSwitch) {
                    if (buttonElement.Content.ToString().Length == 1) {
                        if (buttonElement.CommandParameter.Equals("QUIT")) {
                            continue;
                        } else if (buttonElement.CommandParameter.Equals("CAPITAL")) {
                            continue;
                        } else if (buttonElement.Content.Equals(":")) {
                            buttonElement.Content = ";";
                            buttonElement.CommandParameter = ";";
                        } else if (buttonElement.Content.Equals(";")) {
                            buttonElement.Content = ":";
                            buttonElement.CommandParameter = ":";
                        } else if (buttonElement.Content.Equals("'")) {
                            buttonElement.Content = "\"";
                            buttonElement.CommandParameter = "\"";
                        } else if (buttonElement.Content.Equals("\"")) {
                            buttonElement.Content = "'";
                            buttonElement.CommandParameter = "'";
                        } else if (buttonElement.Content.Equals(",")) {
                            buttonElement.Content = "<";
                            buttonElement.CommandParameter = "<";
                        } else if (buttonElement.Content.Equals("<")) {
                            buttonElement.Content = ",";
                            buttonElement.CommandParameter = ",";
                        } else if (buttonElement.Content.Equals(".")) {
                            buttonElement.Content = ">";
                            buttonElement.CommandParameter = ">";
                        } else if (buttonElement.Content.Equals(">")) {
                            buttonElement.Content = ".";
                            buttonElement.CommandParameter = ".";
                        } else if (buttonElement.Content.Equals("/")) {
                            buttonElement.Content = "?";
                            buttonElement.CommandParameter = "?";
                        } else if (buttonElement.Content.Equals("?")) {
                            buttonElement.Content = "/";
                            buttonElement.CommandParameter = "/";
                        } else if (buttonElement.Content.Equals("-")) {
                            buttonElement.Content = "_";
                            buttonElement.CommandParameter = "_";
                        } else if (buttonElement.Content.Equals("_")) {
                            buttonElement.Content = "-";
                            buttonElement.CommandParameter = "-";
                        } else if (buttonElement.Content.Equals("=")) {
                            buttonElement.Content = "+";
                            buttonElement.CommandParameter = "+";
                        } else if (buttonElement.Content.Equals("+")) {
                            buttonElement.Content = "=";
                            buttonElement.CommandParameter = "=";
                        } else {
                            buttonElement.Content = this.switchCase(buttonElement.Content.ToString());
                            //buttonElement.CommandParameter = this.switchCase(buttonElement.CommandParameter.ToString());
                        }
                    }
                }
            }catch(Exception ex) {
                Console.WriteLine(ex);
            }

            if (capsLock) capsLock = false;
            else capsLock = true;

            //this.capsButton.Content = this.switchCase(this.capsButton.Content.ToString());
        }

        private String switchCase(String inputString) {
            if (!String.IsNullOrEmpty(inputString)) {
                String returnString = "";

                foreach (Char currentChar in inputString) {
                    //if ((currentChar >= 65) && (currentChar <= 90)) {
                    if(capsLock) {
                        returnString += currentChar.ToString().ToLower();
                    }
                    else {//if ((currentChar >= 97) && (currentChar <= 122)) {
                        returnString += currentChar.ToString().ToUpper();
                    } /*else {
                        returnString += currentChar.ToString();
                    }*/
                }

                return returnString;
            } else {
                return "";
            }
        }

        void buttonElement_Click(object sender, RoutedEventArgs e) {
            String sendString = "";

            try {
                e.Handled = true;

                sendString = ((Button)sender).CommandParameter.ToString();

                if (!String.IsNullOrEmpty(sendString)) {
                    /*if (sendString.Length > 1) {
                        sendString = "\"{" + sendString + "}\"";
                    }*/

                    if (sendString.Equals("QUIT")) Application.Current.Shutdown();

                    if (this.focusedInputElement != null) {
                        Keyboard.Focus(this.focusedInputElement);
                        this.focusedInputElement.Focus();
                    }

                    //System.Windows.Forms.SendKeys.SendWait(sendString);
                    pressAndRelease(sendString);


                }
            } catch (Exception ex) {
                // do nothing
                Console.WriteLine(ex);
            }
        }

        private void pressAndRelease(string sendString) {
            //Key down the key.
            INPUT keyInput = new INPUT();            
            keyInput.type = 1;

            keyInput.ki.wScan = 0;
            keyInput.ki.time = 0;
            keyInput.ki.dwFlags = (int)KeyEvent.KeyDown;
            keyInput.ki.dwExtraInfo = (uint)GetMessageExtraInfo();
            //keyInput.ki.wVk = (ushort)VkKeyScan(char.Parse(sendString));
            keyInput.ki.wVk = (ushort)Enum.Parse(typeof(VK), sendString);

            INPUT[] keyInputs = { keyInput };

            SendInput(1, keyInputs, Marshal.SizeOf(keyInput));            

            //Key up the key.
            keyInput.ki.dwFlags = (int)KeyEvent.KeyUp;
            INPUT[] keyInputs2 = { keyInput };
   
            SendInput(1, keyInputs2, Marshal.SizeOf(keyInput));           
        }

        private void capsButton_Click(object sender, RoutedEventArgs e) {
            this.switchCase(this.keyCollection);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            if (this.parentWindow != null) {
                IntPtr HWND = new WindowInteropHelper(this.parentWindow).Handle;

                int GWL_EXSTYLE = (-20);
                GetWindowLong(HWND, GWL_EXSTYLE);
                SetWindowLong(HWND, GWL_EXSTYLE, (IntPtr)(0x8000000));
                if(capsLock) pressAndRelease("CAPITAL");
            }
        }

        public void keyPanel(bool show) {
            if (!show) {
                keyboardGrid.Visibility = Visibility.Hidden;
                allKeysGrid.ColumnDefinitions[0].Width = new GridLength(0);
            } else {
                keyboardGrid.Visibility = Visibility.Visible;
                allKeysGrid.ColumnDefinitions[0].Width = new GridLength(100, GridUnitType.Star);
                this.MinWidth = 450;
            }
        }

        public void numPanel(bool show) {
            if (!show) {
                numPadGrid.Visibility = Visibility.Hidden;
                allKeysGrid.ColumnDefinitions[1].Width = new GridLength(0);
            }
            else {
                numPadGrid.Visibility = Visibility.Visible;
                allKeysGrid.ColumnDefinitions[1].Width = new GridLength(100, GridUnitType.Star);
                this.MinWidth = 192;
            }
        }

        public enum VK : ushort {
            _0 = 0x30,
            _1 = 0x31,
            _2 = 50,
            _3 = 0x33,
            _4 = 0x34,
            _5 = 0x35,
            _6 = 0x36,
            _7 = 0x37,
            _8 = 0x38,
            _9 = 0x39,
            A = 0x41,
            ACCEPT = 30,
            ADD = 0x6b,
            APPS = 0x5d,
            B = 0x42,
            BACK = 8,
            BROWSER_BACK = 0xa6,
            BROWSER_FAVORITES = 0xab,
            BROWSER_FORWARD = 0xa7,
            BROWSER_HOME = 0xac,
            BROWSER_REFRESH = 0xa8,
            BROWSER_SEARCH = 170,
            BROWSER_STOP = 0xa9,
            C = 0x43,
            CANCEL = 3,
            CAPITAL = 20,
            CLEAR = 12,
            CONTROL = 0x11,
            CONVERT = 0x1c,
            D = 0x44,
            DECIMAL = 110,
            DELETE = 0x2e,
            DIVIDE = 0x6f,
            DOWN = 40,
            E = 0x45,
            END = 0x23,
            ESCAPE = 0x1b,
            EXECUTE = 0x2b,
            F = 70,
            F1 = 0x70,
            F10 = 0x79,
            F11 = 0x7a,
            F12 = 0x7b,
            F13 = 0x7c,
            F14 = 0x7d,
            F15 = 0x7e,
            F16 = 0x7f,
            F17 = 0x80,
            F18 = 0x81,
            F19 = 130,
            F2 = 0x71,
            F20 = 0x83,
            F21 = 0x84,
            F22 = 0x85,
            F23 = 0x86,
            F24 = 0x87,
            F3 = 0x72,
            F4 = 0x73,
            F5 = 0x74,
            F6 = 0x75,
            F7 = 0x76,
            F8 = 0x77,
            F9 = 120,
            FINAL = 0x18,
            G = 0x47,
            H = 0x48,
            HANGEUL = 0x15,
            HANGUL = 0x15,
            HANJA = 0x19,
            HELP = 0x2f,
            HOME = 0x24,
            I = 0x49,
            INSERT = 0x2d,
            J = 0x4a,
            JUNJA = 0x17,
            K = 0x4b,
            KANA = 0x15,
            KANJI = 0x19,
            L = 0x4c,
            LAUNCH_APP1 = 0xb6,
            LAUNCH_APP2 = 0xb7,
            LAUNCH_MAIL = 180,
            LAUNCH_MEDIA_SELECT = 0xb5,
            LBUTTON = 1,
            LCONTROL = 0xa2,
            LEFT = 0x25,
            LMENU = 0xa4,
            LSHIFT = 160,
            LWIN = 0x5b,
            M = 0x4d,
            MBUTTON = 4,
            MEDIA_NEXT_TRACK = 0xb0,
            MEDIA_PLAY_PAUSE = 0xb3,
            MEDIA_PREV_TRACK = 0xb1,
            MEDIA_STOP = 0xb2,
            MENU = 0x12,
            MODECHANGE = 0x1f,
            MULTIPLY = 0x6a,
            N = 0x4e,
            NEXT = 0x22,
            NONCONVERT = 0x1d,
            NUMLOCK = 0x90,
            NUMPAD0 = 0x60,
            NUMPAD1 = 0x61,
            NUMPAD2 = 0x62,
            NUMPAD3 = 0x63,
            NUMPAD4 = 100,
            NUMPAD5 = 0x65,
            NUMPAD6 = 0x66,
            NUMPAD7 = 0x67,
            NUMPAD8 = 0x68,
            NUMPAD9 = 0x69,
            O = 0x4f,
            OEM_1 = 0xba,
            OEM_2 = 0xbf,
            OEM_3 = 0xc0,
            OEM_4 = 0xdb,
            OEM_5 = 220,
            OEM_6 = 0xdd,
            OEM_7 = 0xde,
            OEM_8 = 0xdf,
            OEM_COMMA = 0xbc,
            OEM_MINUS = 0xbd,
            OEM_PERIOD = 190,
            OEM_PLUS = 0xbb,
            P = 80,
            PAUSE = 0x13,
            PLUS = 0x6B,
            PRINT = 0x2a,
            PRIOR = 0x21,
            Q = 0x51,
            R = 0x52,
            RBUTTON = 2,
            RCONTROL = 0xa3,
            RETURN = 13,
            RIGHT = 0x27,
            RMENU = 0xa5,
            RSHIFT = 0xa1,
            RWIN = 0x5c,
            S = 0x53,
            SCROLL = 0x91,
            SELECT = 0x29,
            SEPARATOR = 0x6c,
            SHIFT = 0x10,
            SLEEP = 0x5f,
            SNAPSHOT = 0x2c,
            SPACE = 0x20,
            SUBTRACT = 0x6d,
            T = 0x54,
            TAB = 9,
            U = 0x55,
            UP = 0x26,
            V = 0x56,
            VOLUME_DOWN = 0xae,
            VOLUME_MUTE = 0xad,
            VOLUME_UP = 0xaf,
            W = 0x57,
            X = 0x58,
            XBUTTON1 = 5,
            XBUTTON2 = 6,
            Y = 0x59,
            Z = 90
        }
    }
}
