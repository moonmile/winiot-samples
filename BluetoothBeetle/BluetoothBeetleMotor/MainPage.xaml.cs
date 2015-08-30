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

// 空白ページのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 を参照してください

namespace BluetoothBeetleMotor
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;

        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            // GPIOの初期化
            RPi.gpio = GpioController.GetDefault();
            if (RPi.gpio != null)
            {
                motorRight = new Motor(23, 24);
                motorLeft = new Motor(22, 27);

                /// 停止状態
                motorLeft.Direction = 0;
                motorRight.Direction = 0;
            }
        }
        Motor motorLeft, motorRight;

        private void clickLeft(object sender, RoutedEventArgs e)
        {
            this.motorLeft.Direction = 0;
            this.motorRight.Direction = 1;
        }

        private void clickRight(object sender, RoutedEventArgs e)
        {
            this.motorLeft.Direction = 1;
            this.motorRight.Direction = 0;
        }

        private void clickBack(object sender, RoutedEventArgs e)
        {
            this.motorLeft.Direction = -1;
            this.motorRight.Direction = -1;
        }

        private void clickStop(object sender, RoutedEventArgs e)
        {
            this.motorLeft.Direction = 0;
            this.motorRight.Direction = 0;
        }

        private void clickForward(object sender, RoutedEventArgs e)
        {
            this.motorLeft.Direction = 1;
            this.motorRight.Direction = 1;
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
}
