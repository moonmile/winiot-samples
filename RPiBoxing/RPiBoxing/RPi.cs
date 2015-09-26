using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace RPiBoxing
{
    /// <summary>
    /// Raspberry Pi の GPIO を保持
    /// </summary>
    public static class RPi
    {
        public static GpioController gpio { get; set; }
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
