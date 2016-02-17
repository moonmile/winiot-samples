using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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

namespace RPiServo
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
            slider1.Maximum = 90;
            slider1.Minimum = -90;
            slider1.Value = 0;
            slider1.StepFrequency = 1.0;
            slider2.Maximum = 90;
            slider2.Minimum = -90;
            slider2.Value = 0;
            slider2.StepFrequency = 1.0;

            slider1.ValueChanged += (_, __) => { servo1.Position = (int)slider1.Value; };
            slider2.ValueChanged += (_, __) => { servo2.Position = (int)slider2.Value; };

        }

        Servo servo1;
        Servo servo2;
        Servo servo3;
        Servo servo4;

        /// <summary>
        /// 制御開始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clickStart(object sender, RoutedEventArgs e)
        {
            GpioController gpio = GpioController.GetDefault();
            if (servo1 == null)
            {
                servo1 = new Servo(gpio.OpenPin(13).DriveMode(GpioPinDriveMode.Output));
                servo2 = new Servo(gpio.OpenPin(6).DriveMode(GpioPinDriveMode.Output));
            }
            servo1.Start();
            servo2.Start();
            servo1.Position = 0;
            servo2.Position = 0;
        }

        /// <summary>
        /// 制御停止
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void clickStop(object sender, RoutedEventArgs e)
        {
            if ( servo1 != null )
            {
                servo1.Position = 0;
                servo2.Position = 0;
                await Task.Delay(100);
                servo1.Stop();
                servo2.Stop();

                slider1.Value = 0;
                slider2.Value = 0;
            }
        }
        private void click0(object sender, RoutedEventArgs e)
        {
            servo1.Position = 0;
        }
        private void click90(object sender, RoutedEventArgs e)
        {
            servo1.Position = 90;
        }
        private void click180(object sender, RoutedEventArgs e)
        {
            servo1.Position = -90;
        }
        private void click0b(object sender, RoutedEventArgs e)
        {
            servo2.Position = 0;
        }
        private void click90b(object sender, RoutedEventArgs e)
        {
            servo2.Position = 90;
        }
        private void click180b(object sender, RoutedEventArgs e)
        {
            servo2.Position = -90;
        }

    }

    static class GpioPinEx
    {
        public static GpioPin DriveMode(this GpioPin t, GpioPinDriveMode mode)
        {
            t.SetDriveMode(mode);
            return t;
        }
    }
}
