using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;

namespace WordChainGame
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        char firstLetter; // Первая буква следующего города
        Random random;
        string access_token; // Для работы с VK API
        string user_id; // Для работы с VK API
        Task humanResponse; // Запрос с википедии информации о городе
        Task skynetResponse; // Запрос с википедии информации о городе
        uint points; // Баллы в игре
        bool isFinished;
        bool isWin;
        public MainWindow()
        {
            InitializeComponent();
            firstLetter = (char)0;
            random = new Random();
            points = 0;
            LocalDbQuery.DisableAll(); // Пометить все города неиспользованными
        }

        private async void stepButton_Click(object sender, RoutedEventArgs e)
        {
            progressBar.IsIndeterminate = true;
            stepButton.IsEnabled = false;
            if(humanResponse != null)
            {
                humanResponse.Dispose();
            }
            if(skynetResponse!= null)
            {
                skynetResponse.Dispose();
            }
            string cityName = cityTextBox.Text.Trim();
            if (cityName == String.Empty || firstLetter.ToString() != cityName[0].ToString().ToUpper() && firstLetter != 0)
            { // Если город не на ту букву или отсутствует
                progressBar.IsIndeterminate = false;
                stepButton.IsEnabled = true;
                if(firstLetter == (char)0)
                {
                    MessageBox.Show("Напишите город на любую букву");
                    return;
                }
                else
                {
                    MessageBox.Show(String.Format("Напишите город на букву '{0}'", firstLetter));
                    return;
                }
            }
            Dictionary<string, string> dict = new Dictionary<string, string>(); // Параметры для запроса к БД
            dict.Add("name", String.Format("N'{0}'", cityName));
            dict.Add("disable", "'0'");
            List<City> results;
            try
            {
                results = await Task.Run(() => { return LocalDbQuery.Select(dict);  }); // Проверка существования города в БД
            }
            catch
            {
                progressBar.IsIndeterminate = false;
                stepButton.IsEnabled = true;
                MessageBox.Show("Ошибка чтения базы данных!");
                this.Close();
                return;
            }
            if (results.Count == 0)
            {
                progressBar.IsIndeterminate = false;
                stepButton.IsEnabled = true;
                MessageBox.Show("Города нет в базе или он уже использован!");
                return;
            }
            foreach (City item in results)
            {
                historyListBox.Items.Insert(0, item); // Вывод города в историю игры
                LocalDbQuery.DisableCity(item.Id); // Пометка города как использованного
            }
            string humanCity = (historyListBox.Items[0] as City).Name;
            points++;
            humanResponse = Task.Run(() => // Запрос информации о городе с википедии
            {
                Action act = () =>
                {
                    humanTextBlock.Text = humanCity + "\n";
                    try
                    {
                        humanTextBlock.Text += JArray.Parse(new StreamReader(WebRequests.WikiRequest(humanCity).GetResponseStream()).ReadToEnd())[2][0].ToString();
                    }
                    catch { }
                };
                humanTextBlock.Dispatcher.Invoke(act);
            });
            firstLetter = (historyListBox.Items[0] as City).Name.ToUpper().Last();
            if (firstLetter == 'Ь' || firstLetter == 'Ы')
            {
                firstLetter = (historyListBox.Items[0] as City).Name.ToUpper()[(historyListBox.Items[0] as City).Name.Length - 2];
            }
            dict = new Dictionary<string, string>(); // Параметры для запроса к БД
            dict.Add("first", String.Format("N'{0}'", firstLetter));
            dict.Add("disable", "'0'");
            try
            {
                results = await Task.Run(() => { return LocalDbQuery.Select(dict); }); // Проверка существования города в БД
            }
            catch
            {
                progressBar.IsIndeterminate = false;
                stepButton.IsEnabled = true;
                MessageBox.Show("Ошибка чтения базы данных!");
                this.Close();
                return;
            }
            if (results.Count == 0)
            {
                isFinished = true;
                postButton.Visibility = Visibility.Visible;
                stepButton.Visibility = Visibility.Hidden;
                finishButton.Content = "Начать заново";
                resultLabel.Content = String.Format("Вы набрали {0} баллов в игре!", points);
                progressBar.IsIndeterminate = false;
                stepButton.IsEnabled = true;
                isWin = true;
                MessageBox.Show("Поздравляю! Вы выиграли компьютер!");
                return;
            }
            string name = results[random.Next(results.Count)].Name;
            foreach (City item in results)
            {
                if (item.Name == name)
                {
                    historyListBox.Items.Insert(0, item); // Вывод города в историю игры
                    LocalDbQuery.DisableCity(item.Id); // Пометка города как использованного
                }
            }
            string skynetCity = (historyListBox.Items[0] as City).Name;
            skynetResponse = Task.Run(() => // Запрос информации о городе с википедии
            {
                Action act = () => {
                    skynetTextBlock.Text = skynetCity + "\n";
                    try
                    {
                        skynetTextBlock.Text += JArray.Parse(new StreamReader(WebRequests.WikiRequest(skynetCity).GetResponseStream()).ReadToEnd())[2][0].ToString();
                    }
                    catch { }
                };
                humanTextBlock.Dispatcher.Invoke(act);
            });
            firstLetter = (historyListBox.Items[0] as City).Name.ToUpper().Last();
            if (firstLetter == 'Ь' || firstLetter == 'Ы')
            {
                firstLetter = (historyListBox.Items[0] as City).Name.ToUpper()[(historyListBox.Items[0] as City).Name.Length - 2];
            }
            label.Content = String.Format("Введите город на букву '{0}':", firstLetter);
            cityTextBox.Text = String.Empty;
            progressBar.IsIndeterminate = false;
            stepButton.IsEnabled = true;
        }

        private async void postButton_Click(object sender, RoutedEventArgs e)
        {
            postButton.IsEnabled = false;
            progressBar.IsIndeterminate = true;
            if (String.IsNullOrEmpty(access_token) || String.IsNullOrEmpty(user_id))
            { // Если нет токена, авторизация с использованием протокола OAuth
                OAuthVk oauthvk = new OAuthVk();
                oauthvk.ShowDialog();
                access_token = oauthvk.Access_token;
                user_id = oauthvk.User_id;
                if(String.IsNullOrEmpty(access_token) || String.IsNullOrEmpty(user_id))
                {
                    progressBar.IsIndeterminate = false;
                    return;
                }
            }
            dynamic result = await Task.Run(() => // Отправка поста на страницу ВК
            {
                string message = String.Format("Я набрал {0} баллов в игре \"Города\"!", points);
                if(isWin)
                {
                    message += " Я победил компьютер!";
                }
                HttpWebResponse response = WebRequests.PostVk(message, access_token, user_id);
                if (response == null)
                {
                    return Task.FromResult<dynamic>(null);                    
                }
                StreamReader sr = new StreamReader(response.GetResponseStream());
                dynamic json = JValue.Parse(sr.ReadToEnd());
                return Task.FromResult<dynamic>(json);
            });
            // Проверка ответа
            if(result.Result == null)
            {
                postButton.IsEnabled = true;
                progressBar.IsIndeterminate = false;
                MessageBox.Show("Проверьте подключеник к нтернету!");
                return;
            }
            try
            {
                int post_id = result.Result.response.post_id;
            }
            catch
            {
                if (result.Result.error.error_code == 17)
                {
                    access_token = null;
                    user_id = null;
                    postButton.IsEnabled = true;
                    progressBar.IsIndeterminate = false;
                    MessageBox.Show("Возникла ошибка авторизации. Попробуйте повторить попытку!");
                    return;
                }
                else
                {
                    postButton.IsEnabled = true;
                    progressBar.IsIndeterminate = false;
                    MessageBox.Show("Не удалось опубликовать запись!");
                    return;
                }
            }
            postButton.IsEnabled = true;
            progressBar.IsIndeterminate = false;
            MessageBox.Show("Запись успешно опубликована на вашей стене во Вконтакте!");
        }

        private async void finishButton_Click(object sender, RoutedEventArgs e)
        {
            if(isFinished)
            {
                postButton.IsEnabled = false;
                progressBar.IsIndeterminate = true;
                try
                {
                    await Task.Run(() => { LocalDbQuery.DisableAll(); }); // Пометка всех городов неиспользованными
                }
                catch
                {
                    postButton.IsEnabled = true;
                    progressBar.IsIndeterminate = false;
                    MessageBox.Show("Ошибка чтения базы данных!");
                    this.Close();
                    return;
                }
                isFinished = false;
                postButton.Visibility = Visibility.Hidden;
                stepButton.Visibility = Visibility.Visible;
                finishButton.Content = "Закончить игру";
                resultLabel.Content = String.Empty;
                historyListBox.Items.Clear();
                label.Content = "Введите город:";
                points = 0;
                firstLetter = (char)0;
                humanTextBlock.Text = String.Empty;
                skynetTextBlock.Text = String.Empty;
                isWin = false;
                postButton.IsEnabled = true;
                progressBar.IsIndeterminate = false;
            }
            else
            {
                isFinished = true;
                postButton.Visibility = Visibility.Visible;
                stepButton.Visibility = Visibility.Hidden;
                finishButton.Content = "Начать заново";
                resultLabel.Content = String.Format("Вы набрали {0} баллов в игре!", points);
            }
        }
    }
}