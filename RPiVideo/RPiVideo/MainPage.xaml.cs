using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
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

namespace RPiVideo
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            // ウォッチリスト
            text1.Text = "https://www.amazon.co.jp/gp/video/watchlist?ie=UTF8&ref_=pet_yr_prime_recs_left_cta";

            _vm = new ViewModel();
            this.DataContext = _vm;
        }
        ViewModel _vm;

        private async void clickGo(object sender, RoutedEventArgs e)
        {
            var path = text1.Text;
            web1.Navigate(new Uri(path));
            /*
            var ua = "Mozilla/5.0 (Windows NT 6.3; WOW64; Trident/7.0; rv:11.0) like Gecko"; // IE
            // web1.Navigate(new Uri(path), null, null, ua);

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add(
                "user-agent", ua);
            // "Mozilla/5.0 (compatible; MSIE 10.0; Windows Phone 8.0; Trident/6.0; IEMobile/10.0; ARM; Touch)");
            client.DefaultRequestHeaders.Add("Accept-Charset", "UTF-8");
            HttpResponseMessage response = await client.GetAsync(path);
            response.EnsureSuccessStatusCode();
            string html = await response.Content.ReadAsStringAsync();
            _vm.HTML = html;
            */
        }
    }

    public class ViewModel : BindableBase
    {
        private string _html;
        public string HTML
        {
            get { return _html; }
            set { this.SetProperty(ref _html, value); }
        }
    }

    class HtmlBindingExtension
    {
        public static string GetHTML(DependencyObject obj)
        {
            return (string)obj.GetValue(HTMLProperty);
        }

        public static void SetHTML(DependencyObject obj, string value)
        {
            obj.SetValue(HTMLProperty, value);
        }

        // Using a DependencyProperty as the backing store for HTML.  This enables animation, styling, binding, etc...  
        public static readonly DependencyProperty HTMLProperty =
            DependencyProperty.RegisterAttached("HTML", typeof(string), typeof(HtmlBindingExtension), new PropertyMetadata(0, new PropertyChangedCallback(OnHTMLChanged)));

        private static void OnHTMLChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

            WebView wv = d as WebView;
            if (wv != null)
            {
                wv.NavigateToString((string)e.NewValue);
            }
        }
    }
}
