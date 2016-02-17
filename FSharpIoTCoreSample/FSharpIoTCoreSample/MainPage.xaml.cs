using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

namespace FSharpIoTCoreSample
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

        FSharpPcl.Class1 fl;

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            fl = new FSharpPcl.Class1();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            text1.Text = fl.add(10, 20).ToString();

        }

        private void button_Click2(object sender, RoutedEventArgs e)
        {
            text1.Text = fl.mul(10, 20).ToString();
        }
    }
}
