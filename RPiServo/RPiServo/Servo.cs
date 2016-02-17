using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Devices.Gpio;
using Windows.Foundation;
using Windows.System.Threading;

namespace RPiServo
{
    public class Servo
    {
        private GpioPin _pin;
        public Servo(GpioPin pin)
        {
            _pin = pin;
        }

        Task _task;
        bool _loop;
        Stopwatch stopwatch;
        const double PulseFrequency = 20; // 20 msec(50Hz)
        double _PulseWidth = PulseWidthCenter;
        int _Position;

        /*
         * SG90 http://akizukidenshi.com/download/ds/towerpro/SG90_a.pdf
         *  position 0  : 1.45 ms
         *           90 : 2.4  ms
         *          -90 : 0.5  ms
         *  PWM Period  : 20 ms(50Hz)
         *    
         */

        const double PulseWidthMax = 2.4;
        const double PulseWidthMin = 0.5;
        const double PulseWidthCenter = 1.45;

        /// <summary>
        /// 制御開始
        /// </summary>
        public void Start()
        {
            stopwatch = Stopwatch.StartNew();
            _task = new Task(() => {
                _loop = true;
                long initTick = stopwatch.ElapsedTicks;
                while ( _loop )
                {
                    // long initTick = stopwatch.ElapsedTicks;
                    long dutyTick = initTick + (long)( _PulseWidth / 1000.0 * Stopwatch.Frequency);
                    long pwmTick  = initTick + (long)(PulseFrequency / 1000.0 * Stopwatch.Frequency );
                    if (_PulseWidth != 0)
                    {
                        _pin.Write(GpioPinValue.High);
                        while (stopwatch.ElapsedTicks < dutyTick) { }
                    }
                    _pin.Write(GpioPinValue.Low);
                    while (stopwatch.ElapsedTicks < pwmTick) { }
                    initTick = pwmTick;
                }
            });
            _task.Start();

        }
        /// <summary>
        /// 制御終了
        /// </summary>
        public async void Stop()
        {
            if ( _task != null )
            {
                _loop = false;
                await Task.Delay(100);
                stopwatch.Stop();
            }
            _task = null;
            stopwatch = null;
        }




        /// <summary>
        /// パルスを変更
        /// </summary>
        public double PulseWidth
        {
            get { return _PulseWidth; }
            set {
                if (value < PulseWidthMin) value = PulseWidthMin;
                if (value > PulseWidthMax) value = PulseWidthMax;
                _PulseWidth = value;
                _Position = PulseToPos(_PulseWidth);

            }
        }
        /// <summary>
        /// 角度を変更
        /// </summary>
        public int Position
        {
            get { return _Position; }
            set { _Position = value;
                PulseWidth = PosToPulse(_Position);
            }
        }
        private double PosToPulse( int degree )
        {
            if (degree < -90) degree = -90;
            if (degree > 90) degree = 90;

            return (PulseWidthMax - PulseWidthMin) / 180.0 * ((double)(degree+90)) + PulseWidthMin;
        }
        private int PulseToPos( double pulse )
        {
            return (int)((pulse - PulseWidthMin) / (PulseWidthMax - PulseWidthMin) * 180 - 90);
        }
    }
}
