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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace RPiMeArm
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;

            this.PedestalLeft.Click += (s, e) => { this.servo0.AddPWM(-4); };
            this.PedestalRight.Click += (s,e) => { this.servo0.AddPWM(4); };
            this.arm1Up.Click += (s, e) => { this.servo1.AddPWM(-4); };
            this.arm1Down.Click += (s, e) => { this.servo1.AddPWM(4); };
            this.arm2Up.Click += (s, e) => { this.servo2.AddPWM(-4); };
            this.arm2Down.Click += (s, e) => { this.servo2.AddPWM(4); };
            this.gripOpen.Click += (s, e) => { this.servo3.AddPWM(-4); };
            this.gripClose.Click += (s, e) => { this.servo3.AddPWM(4); };

        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            _i2c = new I2C();
            _i2c.Initialize();

            this.servo0 = _i2c.CreatePWM(0, 200, 500);
            this.servo1 = _i2c.CreatePWM(1, 200, 500);
            this.servo2 = _i2c.CreatePWM(2, 200, 500);
            this.servo3 = _i2c.CreatePWM(3, 200, 500);


        }

        private I2C _i2c;
        private PWM servo0;
        private PWM servo1;
        private PWM servo2;
        private PWM servo3;

    }

    public class PWM
    {
        public int ServoMin { get; set; }
        public int ServoMax { get; set; }
        public int ServoNum { get; set; }
        public int ServoPluse { get; set; }
        private I2C _i2c;
        public PWM( I2C i2c, int num )
        {
            _i2c = i2c;
            this.ServoMin = 200;    // default
            this.ServoMax = 500;
            this.ServoPluse = (ServoMin + ServoMax) / 2;
            this.ServoNum = num;    // Servo No.
        }

        private void SetPWM(int on, int off)
        {
            // 5バイトを一気に送らないと駄目
            _i2c.Write(new byte[] {
                (byte)(I2C.LED0_ON_L + 4 * this.ServoNum),
                (byte)(on & 0xFF),
                (byte)(on >> 8),
                (byte)(off & 0xFF),
                (byte)(off >> 8) });
        }
        public void SetPulse(double pulse)
        {
            if (pulse < this.ServoMin) pulse = this.ServoMin;
            if (pulse > this.ServoMax) pulse = this.ServoMax;
            this.ServoPluse = (int)pulse;


            double pulselength = 1000000;   // 1,000,000 us per second
            pulselength /= 60;   // 60 Hz
            pulselength /= 4096;  // 12 bits of resolution
            pulse *= 1000;
            pulse /= pulselength;
            this.SetPWM(0, (int)pulse);
        }
        public void AddPWM( int x )
        {
            this.ServoPluse += x;
            if (this.ServoPluse < this.ServoMin) this.ServoPluse = this.ServoMin;
            if (this.ServoPluse > this.ServoMax) this.ServoPluse = this.ServoMax;

            SetPWM(0, this.ServoPluse);
        }
    }

    /// <summary>
    /// I2C(PCA9685)を保持するクラス
    /// </summary>
    public class I2C
    {
        private const byte PCA9685_I2C_ADDR = 0x40;           /* 7-bit I2C address of the PCA9685  */
        private const byte PCA9685_MODE1 = 0x00;
        private const byte PCA9685_PRESCALE = 0xFE;
        public const byte LED0_ON_L = 0x6;
        public const byte LED0_ON_H = 0x7;
        public const byte LED0_OFF_L = 0x8;
        public const byte LED0_OFF_H = 0x9;

        private I2cDevice _device;


        /// <summary>
        /// 初期化
        /// </summary>
        public async void Initialize()
        {
            string aqs = I2cDevice.GetDeviceSelector();                     /* Get a selector string that will return all I2C controllers on the system */
            var dis = await DeviceInformation.FindAllAsync(aqs);            /* Find the I2C bus controller device with our selector string           */
            if (dis.Count == 0)
            {
                throw new Exception("No I2C controllers were found on the system");
            }
            var settings = new I2cConnectionSettings(PCA9685_I2C_ADDR);
            settings.BusSpeed = I2cBusSpeed.StandardMode;
            settings.SharingMode = I2cSharingMode.Shared;
            _device = await I2cDevice.FromIdAsync(dis[0].Id, settings);    /* Create an I2cDevice with our selected bus controller and I2C settings */
            if (_device == null)
            {
                throw new Exception(string.Format(
                    "Slave address {0} on I2C Controller {1} is currently in use by " +
                    "another application. Please ensure that no other applications are using I2C.",
                    settings.SlaveAddress,
                    dis[0].Id));
            }
            SetPWMFreq(60.0);
            return;
        }



        public void SetPWMFreq(double freq)
        {
            freq *= 0.9;  // Correct for overshoot in the frequency setting (see issue #11).
            double prescaleval = 25000000;
            prescaleval /= 4096;
            prescaleval /= freq;
            prescaleval -= 1;
            int prescale = (int)(prescaleval+0.5);
            int oldmode = read8(PCA9685_MODE1);
            int newmode = (oldmode & 0x7F) | 0x10; // sleep
            write8(PCA9685_MODE1, newmode); // go to sleep
            write8(PCA9685_PRESCALE, prescale); // set the prescaler
            write8(PCA9685_MODE1, oldmode);
            // delay(5);
            write8(PCA9685_MODE1, oldmode | 0xa1);  //  This sets the MODE1 register to turn on auto increment.
                                                    //  This is why the beginTransmission below was not working.

        }
        public void Write( byte[] data )
        {
            this._device.Write(data);
        }
        /// <summary>
        /// 8ビットデータ読み込み
        /// </summary>
        /// <param name="addr"></param>
        /// <returns></returns>
        private int read8( int addr )
        {
            _device.Write(new byte[] { (byte)addr });
            var buf = new byte[1];
            _device.Read(buf);
            return buf[0];
        }
        /// <summary>
        /// 8ビットデータ書き込み
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="data"></param>
        private void write8( int addr, int data )
        {
            var buf = new byte[] { (byte)addr, (byte)data };
            _device.Write(buf);
        }

        public PWM CreatePWM( int num, int min, int max )
        {
            PWM pwm = new PWM(this, num);
            pwm.ServoMin = min;
            pwm.ServoMax = max;
            return pwm;
        }
    }

}
