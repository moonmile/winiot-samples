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
using CoreTweet;


// 空白ページのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 を参照してください

namespace RPiFavo
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        // ここは消しておくこと!!! 
        // MACアドレスで暗号化するとか。
        /*
        const string ApiKey = "API_KEY";
        const string ApiSecret = "API_SECRET";
        const string AccessToken = "ACCESS_TOKEN";
        const string AccessTokenSecret = "ACCESS_TOKEN_SECRET";
        */

        const string ApiKey = "9pdZqU2keFbH1CKm1Cbu9Xhtz";
        const string ApiSecret = "AsSbNtcT2YgCKkVAbVqDjLqwVu4vGTAVOT109Eztug5pjymVZR";
        const string AccessToken = "794799829218009088-2FYHyZRgLs8tchkZXGxRgH2DQ1Mm3an";
        const string AccessTokenSecret = "Zn2BX7A266tMYtTWHo9hZau04pcXY2DUnrDkWLsyEQ0LJ";

        /// <summary>
        /// ファボを取得
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var tokens = CoreTweet.Tokens.Create(ApiKey, ApiSecret, AccessToken, AccessTokenSecret);
            var favs = await tokens.Favorites.ListAsync();


            var items = new List<Status>();
            foreach (var it in favs)
            {
                items.Add(it);
            }

            lv.ItemsSource = items;
            text1.Text = $"Count: {items.Count}";
            return;

        }
    }
}
