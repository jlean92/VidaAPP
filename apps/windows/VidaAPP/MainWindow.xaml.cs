using Microsoft.UI.Xaml;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace VidaAPP
{
    public sealed partial class MainWindow : Window
    {
        private static readonly HttpClient _http = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(5)
        };

        public MainWindow()
        {
            this.InitializeComponent();
        }

        private async void CheckHealthButton_Click(object sender, RoutedEventArgs e)
        {
            await CheckAsync("https://jlean92.es/health");
        }

        private async void CheckDbButton_Click(object sender, RoutedEventArgs e)
        {
            await CheckAsync("https://jlean92.es/health/db");
        }

        private async Task CheckAsync(string url)
        {
            try
            {
                CheckHealthButton.IsEnabled = false;
                CheckDbButton.IsEnabled = false;
                StatusText.Text = "Comprobando...";

                using var resp = await _http.GetAsync(url);

                StatusText.Text = resp.IsSuccessStatusCode
                    ? "OK"
                    : $"ERROR {(int)resp.StatusCode}";
            }
            catch (TaskCanceledException)
            {
                StatusText.Text = "TIMEOUT";
            }
            catch
            {
                StatusText.Text = "ERROR";
            }
            finally
            {
                CheckHealthButton.IsEnabled = true;
                CheckDbButton.IsEnabled = true;
            }
        }
    }
}