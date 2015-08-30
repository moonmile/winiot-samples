using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;
using Windows.Devices.HumanInterfaceDevice;
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

namespace RPiTank
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            RPi.gpio = GpioController.GetDefault();


            this.motorLeft = new Motor(23,24);
            this.motorRight = new Motor(27, 22);

            // ボタンのポイントイベントを追加
            foreach (var btn in new Button[] {
                btnForward, btnBack, 
                btnLeft, btnRight, 
                btnStop,
        })
            {
                btn.Click += Btn_Click;
            }
            // xbox コントローラの初期化
            XboxJoystickInit();

        }
        Motor motorLeft, motorRight;

        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            btn.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Green);

            Action motorStop = () =>  {
                motorLeft.Direction = 1;
                motorRight.Direction = 1;
            };

            // 暴走しないように1秒間で止める
            var dic = new Dictionary<Button, Action>();
            dic.Add(btnForward, async delegate {
                this.motorLeft.Direction = 1;
                this.motorRight.Direction = 1;
                await Task.Delay(1000).ContinueWith(t => motorStop());
            });
            dic.Add(btnBack, async delegate {
                this.motorLeft.Direction = -1;
                this.motorRight.Direction = -1;
                await Task.Delay(1000).ContinueWith(t => motorStop());
            });
            dic.Add(btnLeft, async delegate {
                this.motorLeft.Direction = -1;
                this.motorRight.Direction = 1;
                await Task.Delay(1000).ContinueWith(t => motorStop());
            });
            dic.Add(btnRight, async delegate {
                this.motorLeft.Direction = 1;
                this.motorRight.Direction = -1;
                await Task.Delay(1000).ContinueWith(t => motorStop());
            });
            dic.Add(btnStop, delegate
            {
                this.motorLeft.Direction = 0;
                this.motorRight.Direction = 0;
            });

            if (dic.ContainsKey(btn))
                dic[sender as Button]();
        }

        private int lastControllerCount = 0;
        private bool FoundLocalControlsWorking = false;
        private async void XboxJoystickInit()
        {
            string deviceSelector = HidDevice.GetDeviceSelector(0x01, 0x05);
            DeviceInformationCollection deviceInformationCollection = await DeviceInformation.FindAllAsync(deviceSelector);

            if (deviceInformationCollection.Count == 0)
            {
                Debug.WriteLine("No Xbox360 controller found!");
            }
            lastControllerCount = deviceInformationCollection.Count;

            foreach (DeviceInformation d in deviceInformationCollection)
            {
                Debug.WriteLine("Device ID: " + d.Id);

                HidDevice hidDevice = await HidDevice.FromIdAsync(d.Id, Windows.Storage.FileAccessMode.Read);

                if (hidDevice == null)
                {
                    try
                    {
                        var deviceAccessStatus = DeviceAccessInformation.CreateFromId(d.Id).CurrentStatus;

                        if (!deviceAccessStatus.Equals(DeviceAccessStatus.Allowed))
                        {
                            Debug.WriteLine("DeviceAccess: " + deviceAccessStatus.ToString());
                            FoundLocalControlsWorking = true;
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("Xbox init - " + e.Message);
                    }

                    Debug.WriteLine("Failed to connect to the controller!");
                }

                // controller = new XboxHidController(hidDevice);
                // controller.DirectionChanged += Controller_DirectionChanged;
                this.deviceHandle = hidDevice;
                this.deviceHandle.InputReportReceived += DeviceHandle_InputReportReceived1;
            }
        }

        private HidDevice deviceHandle { get; set; }
        private void DeviceHandle_InputReportReceived1(HidDevice sender, HidInputReportReceivedEventArgs args)
        {
            int dPad = (int)args.Report.GetNumericControl(0x01, 0x39).Value;
            Debug.WriteLine("dpad: {0}", dPad);
        }
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
        public Motor(int pin1, int pin2)
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
}

