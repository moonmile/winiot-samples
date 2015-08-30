using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace RPiGripperController
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            new Button[] {
                gripOpen, gripClose,
                arm1Up, arm1Down,
                arm2Up, arm1Down,
                arm3Up, arm1Down,
                turnLeft, turnRight,
            }.Select(b => {
                b.AddHandler(PointerPressedEvent, new PointerEventHandler(OnButtonPressed), true );
                b.AddHandler(PointerReleasedEvent, new PointerEventHandler(OnButtonRelease), true);
                return b;
            });
            ledOn.Click += LedOn_Click;
            ledOff.Click += LedOff_Click;

            _cmdDic = new Dictionary<Button, Tuple<string, string>>();
            _cmdDic.Add(gripOpen, new Tuple<string, string>("/grip/open", "/grip/stop"));
            _cmdDic.Add(gripClose, new Tuple<string, string>("/grip/close", "/grip/stop"));
            _cmdDic.Add(arm1Up, new Tuple<string, string>("/arm1/up", "/arm1/stop"));
            _cmdDic.Add(arm2Up, new Tuple<string, string>("/arm2/up", "/arm2/stop"));
            _cmdDic.Add(arm3Up, new Tuple<string, string>("/arm3/up", "/arm3/stop"));
            _cmdDic.Add(arm1Down, new Tuple<string, string>("/arm1/Down", "/arm1/stop"));
            _cmdDic.Add(arm2Down, new Tuple<string, string>("/arm2/Down", "/arm2/stop"));
            _cmdDic.Add(arm3Down, new Tuple<string, string>("/arm3/Down", "/arm3/stop"));
            _cmdDic.Add(turnLeft, new Tuple<string, string>("/turn/left", "/turn/stop"));
            _cmdDic.Add(turnRight, new Tuple<string, string>("/turn/right", "/turn/stop"));
        }
        int PORT = 8080;

        async void GetCommand( string cmd )
        {
            string hostname = textTarget.Text;
            var url = string.Format("http://{0}:{1}{2}", hostname, PORT, cmd);
            var cl = new HttpClient();
            await cl.GetStringAsync(url);

        }


        private void LedOff_Click(object sender, RoutedEventArgs e)
        {
            GetCommand("/led/on");
        }

        private void LedOn_Click(object sender, RoutedEventArgs e)
        {
            GetCommand("/led/off");
        }

        Dictionary<Button, Tuple<string, string>> _cmdDic;

        void OnButtonPressed( object sender, PointerRoutedEventArgs e )
        {
            var cmd = _cmdDic[sender as Button].Item1;
            GetCommand(cmd);
        }
        void OnButtonRelease(object sender, PointerRoutedEventArgs e)
        {
            var cmd = _cmdDic[sender as Button].Item2;
            GetCommand(cmd);
        }

    }
}
