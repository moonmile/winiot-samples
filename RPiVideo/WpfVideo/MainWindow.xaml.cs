using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfVideo
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // ウォッチリスト
            text1.Text = "https://www.amazon.co.jp/gp/video/watchlist?ie=UTF8&ref_=pet_yr_prime_recs_left_cta";
        }

        private void clickGo(object sender, RoutedEventArgs e)
        {
            var path = text1.Text;
            var ua = "User-Agent: Mozilla/5.0 (Windows NT 6.3; WOW64; Trident/7.0; rv:11.0) like Gecko"; // IE
            web1.Navigate(new Uri(path), null, null, ua);
        }
    }
}
