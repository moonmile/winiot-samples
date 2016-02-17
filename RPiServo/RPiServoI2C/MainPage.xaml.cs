using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;
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

namespace RPiServoI2C
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
            slider3.Maximum = 90;
            slider3.Minimum = -90;
            slider3.Value = 0;
            slider3.StepFrequency = 1.0;
            slider4.Maximum = 90;
            slider4.Minimum = -90;
            slider4.Value = 0;
            slider4.StepFrequency = 1.0;

            slider1.ValueChanged += (_, __) => { pwm1.Position = (int)slider1.Value; };
            slider2.ValueChanged += (_, __) => { pwm2.Position = (int)slider2.Value; };
            slider3.ValueChanged += (_, __) => { pwm3.Position = (int)slider3.Value; };
            slider4.ValueChanged += (_, __) => { pwm4.Position = (int)slider4.Value; };
        }

        private const byte PWM_I2C_ADDR = 0x40;
        PwmServo pwm1;
        PwmServo pwm2;
        PwmServo pwm3;
        PwmServo pwm4;

        private I2cDevice I2CPwm;

        /// <summary>
        /// 制御開始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void clickStart(object sender, RoutedEventArgs e)
        {
            try {
                string aqs = I2cDevice.GetDeviceSelector();                     /* Get a selector string that will return all I2C controllers on the system */
                var dis = await DeviceInformation.FindAllAsync(aqs);            /* Find the I2C bus controller device with our selector string           */
                var settings = new I2cConnectionSettings(PWM_I2C_ADDR);
                settings.BusSpeed = I2cBusSpeed.StandardMode;
                I2CPwm = await I2cDevice.FromIdAsync(dis[0].Id, settings);    /* Create an I2cDevice with our selected bus controller and I2C settings */
                pwm1 = new PwmServo(I2CPwm, 0);
                pwm2 = new PwmServo(I2CPwm, 1);
                pwm3 = new PwmServo(I2CPwm, 2);
                pwm4 = new PwmServo(I2CPwm, 3);
            }
            catch ( Exception ex )
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return;
            }

            // 一つだけ設定する
            pwm1.Begin();
            pwm1.SetPWMFreq(60); // 60Hz 
        }


        /// <summary>
        /// テストループ
        /// </summary>
        async void loop()
        {
            const int SERVOMIN = 150; // this is the 'minimum' pulse length count (out of 4096)
            const int SERVOMAX = 600; // this is the 'maximum' pulse length count (out of 4096)

            int servonum = 0;
            // Drive each servo one at a time
            for (int pulselen = SERVOMIN; pulselen < SERVOMAX; pulselen++)
            {
                pwm1.SetPWM(servonum, 0, pulselen);
            }
            await Task.Delay(500);
            for (int pulselen = SERVOMAX; pulselen > SERVOMIN; pulselen--)
            {
                pwm1.SetPWM(servonum, 0, pulselen);
            }
            await Task.Delay(500);
        }

        /// <summary>
        /// 制御停止
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clickStop(object sender, RoutedEventArgs e)
        {
            pwm1.Reset();
        }

        private void click0(object sender, RoutedEventArgs e)
        {
            loop();
        }
        private void click90(object sender, RoutedEventArgs e)
        {
        }
        private void click180(object sender, RoutedEventArgs e)
        {
        }
        private void click0b(object sender, RoutedEventArgs e)
        {
        }
        private void click90b(object sender, RoutedEventArgs e)
        {
        }
        private void click180b(object sender, RoutedEventArgs e)
        {
        }
    }

}

