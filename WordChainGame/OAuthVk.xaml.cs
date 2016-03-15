using System.Windows;
using System.Text.RegularExpressions;

namespace WordChainGame
{
    /// <summary>
    /// Логика взаимодействия для OAuthVk.xaml
    /// </summary>
    public partial class OAuthVk : Window
    {
        public string Access_token { get; private set; }
        public string User_id { get; private set; }
        public OAuthVk()
        {
            InitializeComponent();
            webBrowser.Navigate("https://oauth.vk.com/authorize?" +
                                                               "client_id=5126725" +
                                                               "&redirect_uri=https://oauth.vk.com/blank.html" +
                                                               "&scope=8192" +
                                                               "&display=page" +
                                                               "&response_type=token");
        }

        private void webBrowser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        { // Закрыть окно, как получен токен доступа
            Match access_token = Regex.Match(webBrowser.Source.AbsoluteUri, "(?<=access_token=)[\\da-z]+");
            Match user_id = Regex.Match(webBrowser.Source.AbsoluteUri, "(?<=user_id=)\\d+");
            if(access_token.Success && user_id.Success)
            {
                Access_token = access_token.Value;
                User_id = user_id.Value;
                Close();
            }
        }
    }
}
