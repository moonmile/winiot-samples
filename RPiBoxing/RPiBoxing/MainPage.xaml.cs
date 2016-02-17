using IoTCoreDefaultApp;
using RPiGripperArm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// 空白ページのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 を参照してください

namespace RPiBoxing
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

        Motor motorLeft;
        Motor motorRight;
        Led led;
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            // GPIOの初期化
            RPi.gpio = GpioController.GetDefault();
            if (RPi.gpio != null)
            {
                motorLeft = new Motor(23, 24);
                motorRight = new Motor(27, 22);
                /// 停止状態
                motorLeft.Direction = 0;
                motorRight.Direction = 0;
                // LED
                led = new Led(4);
                led.Off();
            }
            this.KeyLFront.PointerPressed += Key_PointerPressed;
            this.KeyLBack.PointerPressed += Key_PointerPressed;
            this.KeyRFront.PointerPressed += Key_PointerPressed;
            this.KeyRBack.PointerPressed += Key_PointerPressed;
            this.sw1.Toggled += Sw1_Toggled;
            // バインド
            _model = new DataModel();
            this.DataContext = _model;
        }
        DataModel _model;

        private void Sw1_Toggled(object sender, RoutedEventArgs e)
        {
            if (sw1.IsOn == true)
                led.On();
            else
                led.Off();
        }

        private async void Key_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (sender == KeyLFront)
            {
                _model.BLETap = "Left F";
                motorLeft.Direction = 1;
                await Task.Delay(1000);     // 1秒だけ動かす
                motorLeft.Direction = 0;
            }
            else if (sender == KeyLBack)
            {
                _model.BLETap = "Left B";
                motorLeft.Direction = -1;
                await Task.Delay(1000);     // 1秒だけ動かす
                motorLeft.Direction = 0;
            }
            else if (sender == KeyRFront)
            {
                _model.BLETap = "Right F";
                motorRight.Direction = 1;
                await Task.Delay(1000);     // 1秒だけ動かす
                motorRight.Direction = 0;
            }
            else if (sender == KeyRBack)
            {
                _model.BLETap = "Right B";
                motorRight.Direction = -1;
                await Task.Delay(1000);     // 1秒だけ動かす
                motorRight.Direction = 0;
            }
        }

        private GattDeviceService[] serviceList = new GattDeviceService[7];
        private GattCharacteristic[] activeCharacteristics = new GattCharacteristic[7];
        // IDs for Sensors
        const int NUM_SENSORS = 7;
        const int IR_SENSOR = 0;
        const int ACCELEROMETER = 1;
        const int HUMIDITY = 2;
        const int MAGNETOMETER = 3;
        const int BAROMETRIC_PRESSURE = 4;
        const int GYROSCOPE = 5;
        const int KEYS = 6;

        /// <summary>
        /// SenserTag を探す
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void clickSenserTag(object sender, RoutedEventArgs e)
        {
            UserOut.Text = "Setting up SensorTag";
            bool okay = await init();
            if (okay)
            {
                for (int i = 0; i < NUM_SENSORS; i++)
                {
                    enableSensor(i);
                }
                UserOut.Text = "Sensors on!";
            }
            else
            {
                UserOut.Text = "Something went wrong!";
            }
        }
        // Setup
        // Saves GATT service object in array
        private async Task<bool> init()
        {
            // Retrieve instances of the GATT services that we will use
            for (int i = 0; i < NUM_SENSORS; i++)
            {
                // Setting Service GUIDs
                // Built in enumerations are found in the GattServiceUuids class like this: GattServiceUuids.GenericAccess
                Guid BLE_GUID;
                if (i < 6)
                    BLE_GUID = new Guid("F000AA" + i + "0-0451-4000-B000-000000000000");
                else
                    BLE_GUID = new Guid("0000FFE0-0000-1000-8000-00805F9B34FB");

                // Retrieving and saving GATT services
                var services = await DeviceInformation.FindAllAsync(GattDeviceService.GetDeviceSelectorFromUuid(BLE_GUID), null);
                if (services != null && services.Count > 0)
                {
                    if (services[0].IsEnabled)
                    {
                        GattDeviceService service = await GattDeviceService.FromIdAsync(services[0].Id);
                        if (service.Device.ConnectionStatus == BluetoothConnectionStatus.Connected)
                        {
                            serviceList[i] = service;
                        }
                        else
                        {
                            return false;
                        }

                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            return true;
        }
        // Enable and subscribe to specified GATT characteristic
        private async void enableSensor(int sensor)
        {
            GattDeviceService gattService = serviceList[sensor];
            if (gattService != null)
            {
                // Turn on notifications
                IReadOnlyList<GattCharacteristic> characteristicList;
                if (sensor >= 0 && sensor <= 5)
                    characteristicList = gattService.GetCharacteristics(new Guid("F000AA" + sensor + "1-0451-4000-B000-000000000000"));
                else
                    characteristicList = gattService.GetCharacteristics(new Guid("0000FFE1-0000-1000-8000-00805F9B34FB"));

                if (characteristicList != null)
                {
                    GattCharacteristic characteristic = characteristicList[0];
                    if (characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Notify))
                    {
                        switch (sensor)
                        {
                            /*
                            case (IR_SENSOR):
                                characteristic.ValueChanged += tempChanged;
                                IRTitle.Foreground = new SolidColorBrush(Colors.Green);
                                break;
                            case (ACCELEROMETER):
                                characteristic.ValueChanged += accelChanged;
                                AccelTitle.Foreground = new SolidColorBrush(Colors.Green);
                                setSensorPeriod(ACCELEROMETER, 250);
                                break;
                            case (HUMIDITY):
                                characteristic.ValueChanged += humidChanged;
                                HumidTitle.Foreground = new SolidColorBrush(Colors.Green);
                                break;
                            case (MAGNETOMETER):
                                characteristic.ValueChanged += magnoChanged;
                                MagnoTitle.Foreground = new SolidColorBrush(Colors.Green);
                                break;
                            case (BAROMETRIC_PRESSURE):
                                characteristic.ValueChanged += pressureChanged;
                                BaroTitle.Foreground = new SolidColorBrush(Colors.Green);
                                calibrateBarometer();
                                break;
                            case (GYROSCOPE):
                                characteristic.ValueChanged += gyroChanged;
                                GyroTitle.Foreground = new SolidColorBrush(Colors.Green);
                                break;
                            */
                            case (KEYS):
                                characteristic.ValueChanged += keyChanged;
                                // KeyTitle.Foreground = new SolidColorBrush(Colors.Green);
                                break;
                            default:
                                break;
                        }

                        // Save a reference to each active characteristic, so that handlers do not get prematurely killed
                        activeCharacteristics[sensor] = characteristic;

                        // Set the notify enable flag
                        await characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                    }
                }

                // Turn on sensor
                if (sensor >= 0 && sensor <= 5)
                {
                    characteristicList = gattService.GetCharacteristics(new Guid("F000AA" + sensor + "2-0451-4000-B000-000000000000"));
                    if (characteristicList != null)
                    {
                        GattCharacteristic characteristic = characteristicList[0];
                        if (characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Write))
                        {
                            var writer = new Windows.Storage.Streams.DataWriter();
                            // Special value for Gyroscope to enable all 3 axes
                            if (sensor == GYROSCOPE)
                                writer.WriteByte((Byte)0x07);
                            else
                                writer.WriteByte((Byte)0x01);

                            await characteristic.WriteValueAsync(writer.DetachBuffer());
                        }
                    }
                }
            }
        }
        // Key press change handler
        // Algorithm taken from http://processors.wiki.ti.com/index.php/SensorTag_User_Guide#Simple_Key_Service
        async void keyChanged(GattCharacteristic sender, GattValueChangedEventArgs eventArgs)
        {
            byte[] bArray = new byte[eventArgs.CharacteristicValue.Length];
            DataReader.FromBuffer(eventArgs.CharacteristicValue).ReadBytes(bArray);

            byte data = bArray[0];

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if ((data & 0x01) == 0x01)
                {
                    _model.BLETap = "Right F";
                    KeyRFront.Background = new SolidColorBrush(Colors.Green);
                    motorRight.Direction = 1;
                }
                else
                {
                    _model.BLETap = "";
                    KeyRFront.Background = new SolidColorBrush(Colors.Red);
                    motorRight.Direction = 0;
                }
                if ((data & 0x02) == 0x02)
                {
                    _model.BLETap = "Left F";
                    KeyLFront.Background = new SolidColorBrush(Colors.Green);
                    motorLeft.Direction = 1;
                }
                else
                {
                    _model.BLETap = "";
                    KeyLFront.Background = new SolidColorBrush(Colors.Red);
                    motorLeft.Direction = 0;
                }
            });
        }

        SimpleWebServer _server;

        /// <summary>
        /// 簡易サーバーを開始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clickStartServer(object sender, RoutedEventArgs e)
        {
            this.textIP.Text = NetworkPresenter.GetCurrentIpv4Address();
            _server = new SimpleWebServer();
            _server.OnReceived += _server_OnReceived;
            _server.Start();

        }

        private async void _server_OnReceived(string data)
        {
            // GET 受信
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                () => { textGet.Text = data; });

            switch (data)
            {
                case "/left/f":
                    motorLeft.Direction = 1;
                    await Task.Delay(1000);     // 1秒だけ動かす
                    motorLeft.Direction = 0;
                    break;
                case "/left/b":
                    motorLeft.Direction = -1;
                    await Task.Delay(1000);     // 1秒だけ動かす
                    motorLeft.Direction = 0;
                    break;
                case "/right/f":
                    motorRight.Direction = 1;
                    await Task.Delay(1000);     // 1秒だけ動かす
                    motorRight.Direction = 0;
                    break;
                case "/right/b":
                    motorRight.Direction = -1;
                    await Task.Delay(1000);     // 1秒だけ動かす
                    motorRight.Direction = 0;
                    break;
            }
            // 応答を送信
            _server.SendResponse("response " + data);
        }
    }

    public class DataModel : BindableBase
    {
        private string _bleTap = "";
        public string BLETap
        {
            get { return _bleTap; }
            set { this.SetProperty(ref _bleTap, value);  }
        }
    }
}
