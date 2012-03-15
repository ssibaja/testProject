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
using System.Deployment.Application;
using System.Collections.Specialized;
using System.Web;

namespace EGOSK {

    public partial class Window1 : Window {
        private const double OFFSET = -65;
        private const int V_POS = 2;

        private int vPos = V_POS;
        private bool justNumPad = false;

        public Window1() {
            string[] args = Environment.GetCommandLineArgs();
            /*if (args.Length > 1 && args[1].Equals("/numpad")) {
                justNumPad = true;
            }*/

            try {
                NameValueCollection parameters;
                if (ApplicationDeployment.IsNetworkDeployed) {
                    parameters = GetQueryStringParameters();

                    for (int i = 0; i < parameters.Count; i++) {
                        if (!parameters[i].ToString().Equals("-1") && !parameters[i].ToString().Equals("")) {
                            //Get keybors selection
                            if (parameters.GetKey(i).ToLower().Equals("keyboard")) {
                                if (parameters[i].ToString().ToLower().Equals("true")) {
                                    justNumPad = false;
                                } else {
                                    justNumPad = true;
                                }
                            }

                            //Get Print Debug Info
                            if (parameters.GetKey(i).ToLower().Equals("vpos")) {
                                if (parameters[i].ToString().ToLower().Equals("top")) {
                                    vPos = 0;
                                } else if (parameters[i].ToString().ToLower().Equals("middle")) {
                                    vPos = 1;
                                } else if (parameters[i].ToString().ToLower().Equals("bottom")) {
                                    vPos = 2;
                                }
                            }
                        }
                    }
                }
            } catch (Exception ex) { }

            InitializeComponent();
            // add keyboard
            EGKeyboard uc = new EGKeyboard(this);
            uc.numPanel(justNumPad);
            uc.keyPanel(!justNumPad);

            this.keyGrid.Children.Add(uc);
            Grid.SetRow(uc, 2);
            Grid.SetColumn(uc, 0);
            Grid.SetColumnSpan(uc, 3);
        }

        public Window1(IInputElement focusableElement) {
            InitializeComponent();
            this.keyGrid.Children.Add(new EGKeyboard(focusableElement));
        }

        private void setupKeyboardWindow() {
            /* //sizing for both keyboard and numpad
            this.Top = SystemParameters.PrimaryScreenHeight - this.Height-30;
            this.Left = 10;
            this.Width = SystemParameters.PrimaryScreenWidth-20;
             */

            //this.Top = SystemParameters.PrimaryScreenHeight - this.Height - 40;
            if (vPos == 0) {
                this.Top = 80;
            } else if (vPos == 1) {
                this.Top = (SystemParameters.PrimaryScreenHeight - this.Height - 40) / 2.0;
            } else if (vPos == 2) {
                this.Top = SystemParameters.PrimaryScreenHeight - this.Height - 40;
            }

            if (justNumPad) {
                this.Width = SystemParameters.PrimaryScreenWidth * 0.15;
                this.Left = (SystemParameters.PrimaryScreenWidth * 0.5) - (this.Width * 0.5) + OFFSET;
            } else {
                this.Width = SystemParameters.PrimaryScreenWidth * 0.98;
                this.Left = (SystemParameters.PrimaryScreenWidth * 0.5) - (this.Width * 0.5);
            }
        }

        private void Window_Initialized(object sender, EventArgs e) {
            this.setupKeyboardWindow();
        }
        
        
        private NameValueCollection GetQueryStringParameters() {
            NameValueCollection nameValueTable = new NameValueCollection();

            if (ApplicationDeployment.IsNetworkDeployed) {
                string queryString = ApplicationDeployment.CurrentDeployment.ActivationUri.Query;
                nameValueTable = HttpUtility.ParseQueryString(queryString);
            }

            return (nameValueTable);
        }

        private void btnPos_Click(object sender, RoutedEventArgs e) {
            if (vPos == 2) vPos = 0;
            else vPos++;

            setupKeyboardWindow();
        }
    }
}
