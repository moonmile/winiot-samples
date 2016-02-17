using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;
using Windows.Foundation;
using Windows.System.Threading;

namespace RPiServoI2C
{
    public class PwmServo
    {
        const int PCA9685_SUBADR1 = 0x2;
        const int PCA9685_SUBADR2 = 0x3;
        const int PCA9685_SUBADR3 = 0x4;

        const int PCA9685_MODE1 = 0x0;
        const int PCA9685_PRESCALE = 0xFE;

        const int LED0_ON_L = 0x6;
        const int LED0_ON_H = 0x7;
        const int LED0_OFF_L = 0x8;
        const int LED0_OFF_H = 0x9;

        const int ALLLED_ON_L = 0xFA;
        const int ALLLED_ON_H = 0xFB;
        const int ALLLED_OFF_L = 0xFC;
        const int ALLLED_OFF_H = 0xFD;

        int _i2caddr;
        private I2cDevice i2c;
        int _num = 0;

        const int SERVOMIN = 150; // this is the 'minimum' pulse length count (out of 4096)
        const int SERVOMAX = 600; // this is the 'maximum' pulse length count (out of 4096)

        public PwmServo(I2cDevice i2cDev, int num = 0, int addr = 0x40)
        {
            i2c = i2cDev;
            _i2caddr = addr;
            _num = num;
        }
        public void Begin()
        {
            this.Reset();
        }
        public void Reset()
        {
            write8(PCA9685_MODE1, 0);
        }
        public async void SetPWMFreq(double freq)
        {
            //Serial.print("Attempting to set freq ");
            //Serial.println(freq);
            freq *= 0.9;  // Correct for overshoot in the frequency setting (see issue #11).
            double prescaleval = 25000000;
            prescaleval /= 4096;
            prescaleval /= freq;
            prescaleval -= 1;

            int prescale = (int)(prescaleval + 0.5);

            int oldmode = read8(PCA9685_MODE1);
            int newmode = (oldmode & 0x7F) | 0x10; // sleep
            write8(PCA9685_MODE1, newmode); // go to sleep
            write8(PCA9685_PRESCALE, prescale); // set the prescaler
            write8(PCA9685_MODE1, oldmode);
            await Task.Delay(5);
            write8(PCA9685_MODE1, oldmode | 0xa1);  //  This sets the MODE1 register to turn on auto increment.
                                                    //  This is why the beginTransmission below was not working.
                                                    //  Serial.print("Mode now 0x"); Serial.println(read8(PCA9685_MODE1), HEX);
        }
        public void SetPWM(int num, int on, int off)
        {
            /*
            WIRE.beginTransmission(_i2caddr);
            WIRE.write(LED0_ON_L + 4 * num);
            WIRE.write(on);
            WIRE.write(on >> 8);
            WIRE.write(off);
            WIRE.write(off >> 8);
            WIRE.endTransmission();
            */
            var data = new byte[]
            {
                (byte)(LED0_ON_L + 4 * num),
                (byte)on,
                (byte)(on >> 8),
                (byte)off ,
                (byte)(off >> 8),
            };
            i2c.Write(data);
        }
        public void SetPin(int num, int val, bool invert = false)
        {
            // Clamp value between 0 and 4095 inclusive.
            val = Math.Min(val, 4095);
            if (invert)
            {
                if (val == 0)
                {
                    // Special value for signal fully on.
                    SetPWM(num, 4096, 0);
                }
                else if (val == 4095)
                {
                    // Special value for signal fully off.
                    SetPWM(num, 0, 4096);
                }
                else {
                    SetPWM(num, 0, 4095 - val);
                }
            }
            else {
                if (val == 4095)
                {
                    // Special value for signal fully on.
                    SetPWM(num, 4096, 0);
                }
                else if (val == 0)
                {
                    // Special value for signal fully off.
                    SetPWM(num, 0, 4096);
                }
                else {
                    SetPWM(num, 0, val);
                }
            }
        }

        private int read8(int addr)
        {
            var wdata = new byte[] { (byte)addr };
            var rdata = new byte[1];

            i2c.WriteRead(wdata, rdata);
            return (int)rdata[0];
        }
        private void write8(int addr, int d)
        {
            var data = new byte[] { (byte)addr, (byte)d };
            i2c.Write(data);
        }

        private int _Position = 0;
        public int Position
        {
            get { return _Position; }
            set
            {
                value = Math.Max(-90, value);
                value = Math.Min(value, 90);
                _Position = value;

                int v = (int)((SERVOMAX - SERVOMIN) * ((double)(_Position + 90)) / 180.0 + SERVOMIN);
                this.SetPWM(_num, 0, v);
            }
        }
    }
}
