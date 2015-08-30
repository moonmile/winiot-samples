using IoTCoreDefaultApp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Gpio;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using Windows.Devices.HumanInterfaceDevice;
using Windows.Devices.Enumeration;
using System.Diagnostics;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace RPiGripperArm
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;       // GPIOの初期化
            RPi.gpio = GpioController.GetDefault();
            this.grip = new Gripper(18, 27);
            this.arm1 = new Arm(22, 23);
            this.arm2 = new Arm(24, 25);
            this.arm3 = new Arm(5, 6);
            this.pedestal = new Pedestal(12, 13);
            this.led = new Led(26);

            // ボタンのポイントイベントを追加
            foreach (var btn in new Button[] {
                arm1Up, arm1Down,
                arm2Up, arm2Down,
                arm3Up, arm3Down,
                gripOpen, gripClose,
                LEDOn, LEDOff,
                PedestalLeft, PedestalRight,
            })
            {
                btn.Click += Btn_Click;
            }
        }

        Gripper grip;
        Arm arm1, arm2, arm3;
        Pedestal pedestal;
        Led led;

        SimpleWebServer _server;
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.textIP.Text = NetworkPresenter.GetCurrentIpv4Address();

            _server = new SimpleWebServer();
            _server.OnReceived += _server_OnReceived;
            _server.Start();
        }

        /// <summary>
        /// Win API 
        /// </summary>
        /// <param name="data"></param>
        private async void _server_OnReceived(string data)
        {
            // GET 受信
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                () => { textGet.Text = data; });

            switch (data)
            {
                case "/led/on":
                    this.led.On();
                    break;
                case "/led/off":
                    this.led.Off();
                    break;
                case "/grip/open":
                    this.grip.Open();
                    await Task.Delay(1000).ContinueWith(t => grip.Stop());   // 1秒後には止める
                    break;
                case "/grip/close":
                    this.grip.Close();
                    await Task.Delay(1000).ContinueWith(t => grip.Stop());   // 1秒後には止める
                    break;
                case "/turn/left":
                    this.pedestal.Left();
                    await Task.Delay(1000).ContinueWith(t => pedestal.Stop());   // 1秒後には止める
                    break;
                case "/turn/right":
                    this.pedestal.Right();
                    await Task.Delay(1000).ContinueWith(t => pedestal.Stop());   // 1秒後には止める
                    break;

                case "/arm1/up":
                case "/arm2/up":
                case "/arm3/up":
                    {
                        var arm = arm1;
                        if (data.StartsWith("/arm2")) arm = arm2;
                        if (data.StartsWith("/arm3")) arm = arm3;
                        arm.Up();
                        await Task.Delay(1000).ContinueWith(t => arm.Stop());   // 1秒後には止める
                    }
                    break;
                case "/arm1/down":
                case "/arm2/down":
                case "/arm3/down":
                    {
                        var arm = arm1;
                        if (data.StartsWith("/arm2")) arm = arm2;
                        if (data.StartsWith("/arm3")) arm = arm3;
                        arm.Down();
                        await Task.Delay(1000).ContinueWith(t => arm.Stop());   // 1秒後には止める
                    }
                    break;

                case "/grip/stop":
                    this.grip.Stop();
                    break;
                case "/arm1/stop": this.arm1.Stop(); break;
                case "/arm2/stop": this.arm2.Stop(); break;
                case "/arm3/stop": this.arm3.Stop(); break;
                case "/turn/stop": this.pedestal.Stop(); break;
            }
            // 応答を送信
            _server.SendResponse("response " + data);
        }

        /// <summary>
        /// ボタンクリック時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            btn.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Green);

            var dic = new Dictionary<Button, Action>();
            dic.Add(gripOpen, async delegate { this.grip.Open(); await Task.Delay(1000).ContinueWith(t => grip.Stop()); });
            dic.Add(gripClose, async delegate { this.grip.Close(); await Task.Delay(1000).ContinueWith(t => grip.Stop()); });
            dic.Add(arm1Up, async delegate { this.arm1.Up(); await Task.Delay(1000).ContinueWith(t => arm1.Stop()); });
            dic.Add(arm1Down, async delegate { this.arm1.Down(); await Task.Delay(1000).ContinueWith(t => arm1.Stop()); });
            dic.Add(arm2Up, async delegate { this.arm2.Up(); await Task.Delay(1000).ContinueWith(t => arm2.Stop()); });
            dic.Add(arm2Down, async delegate { this.arm2.Down(); await Task.Delay(1000).ContinueWith(t => arm2.Stop()); });
            dic.Add(arm3Up, async delegate { this.arm3.Up(); await Task.Delay(1000).ContinueWith(t => arm3.Stop()); });
            dic.Add(arm3Down, async delegate { this.arm3.Down(); await Task.Delay(1000).ContinueWith(t => arm3.Stop()); });
            dic.Add(PedestalLeft, async delegate { this.pedestal.Left(); await Task.Delay(1000).ContinueWith(t => pedestal.Stop()); });
            dic.Add(PedestalRight, async delegate { this.pedestal.Right(); await Task.Delay(1000).ContinueWith(t => pedestal.Stop()); });
            dic.Add(LEDOn, delegate { this.led.On();  this.sb1.Begin();  });
            dic.Add(LEDOff, delegate { this.led.Off();  this.sb1.Stop(); });

            if (dic.ContainsKey(btn))
                dic[sender as Button]();
        }

        private HidDevice deviceHandle { get; set; }
        private void DeviceHandle_InputReportReceived(HidDevice sender, HidInputReportReceivedEventArgs args)
        {
            int dPad = (int)args.Report.GetNumericControl(0x01, 0x39).Value;
        }
    }

    /// <summary>
    /// Raspberry Pi の GPIO を保持
    /// </summary>
    public static class RPi
    {
        public static GpioController gpio { get; set; }
    }

    /// <summary>
    /// 単純なモーター制御
    /// </summary>
    public class Motor
    {
        GpioPin out1 { get; set; }
        GpioPin out2 { get; set; }
        public Motor( int pin1, int pin2 )
        {
            this.out1 = RPi.gpio.OpenPin(pin1);
            this.out2 = RPi.gpio.OpenPin(pin2);
            this.out1.Write(GpioPinValue.Low);
            this.out2.Write(GpioPinValue.Low);
            this.out1.SetDriveMode(GpioPinDriveMode.Output);
            this.out2.SetDriveMode(GpioPinDriveMode.Output);
            _dir = 0;
        }
        int _dir = 0;
        /// <summary>
        /// 回転方向を変える
        /// </summary>
        public int Direction
        {
            get { return _dir; }
            set
            {
                if (_dir != value)
                {
                    _dir = value;
                    if (_dir == 0)
                    {
                        this.out1.Write(GpioPinValue.Low);
                        this.out2.Write(GpioPinValue.Low);
                    }
                    else if (_dir > 0)
                    {
                        this.out1.Write(GpioPinValue.High);
                        this.out2.Write(GpioPinValue.Low);
                    }
                    else
                    {
                        this.out1.Write(GpioPinValue.Low);
                        this.out2.Write(GpioPinValue.High);
                    }
                }
            }
        }
    }
    /// <summary>
    /// LEDのON/OFF
    /// </summary>
    public class Led
    {
        GpioPin out1 { get; set; }

        public Led(int pin1)
        {
            this.out1 = RPi.gpio.OpenPin(pin1);
            this.out1.SetDriveMode(GpioPinDriveMode.Output);
            this.out1.Write(GpioPinValue.Low);
        }

        bool _sw = false;
        /// <summary>
        /// ON/OFF
        /// </summary>
        bool Light
        {
            get { return _sw; }
            set
            {
                if (_sw != value)
                {
                    _sw = value;
                    this.out1.Write(_sw ? GpioPinValue.High : GpioPinValue.Low);
                }
            }
        }
        public void On() { this.Light = true; }
        public void Off() { this.Light = false; }

    }

    /// girpper arm 用のクラス
    public class Gripper : Motor
    {
        public Gripper(int pin1, int pin2) : base( pin1, pin2 ){ }
        public void Open() { base.Direction = 1; }
        public void Close() { base.Direction = -1; }
        public void Stop() { base.Direction = 0; }
    }
    public class Pedestal : Motor
    {
        public Pedestal(int pin1, int pin2) : base(pin1, pin2) { }
        public void Right() { base.Direction = 1; }
        public void Left() { base.Direction = -1; }
        public void Stop() { base.Direction = 0; }
    }
    public class Arm : Motor
    {
        public Arm(int pin1, int pin2) : base(pin1, pin2) { }
        public void Up() { base.Direction = 1; }
        public void Down() { base.Direction = -1; }
        public void Stop() { base.Direction = 0; }
    }
}
