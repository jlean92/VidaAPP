using Microsoft.UI.Xaml;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace VidaAPP
{
    public sealed partial class MainWindow : Window
    {
        private static readonly HttpClient httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(5)
        };
        private static readonly HttpClient _http = httpClient;

        public MainWindow()
        {
            this.InitializeComponent();
        }

        private async void CheckHealthButton_Click(object sender, RoutedEventArgs e)
        {
            await CheckAsync("https://jlean92.es:4431/health");
        }

        private async void CheckDbButton_Click(object sender, RoutedEventArgs e)
        {
            await CheckAsync("https://jlean92.es:4431/health/db");
        }
        private async void CheckUpdatesButton_Click(object sender, RoutedEventArgs e)
        {
            await CheckUpdatesAsync("https://jlean92.es:4431/update/check");
        }
        private async Task CheckUpdatesAsync(string url)
        {
            try
            {
                SetButtonsEnabled(false);
                StatusText.Text = "Comprobando actualización...";

                using var resp = await _http.GetAsync(url);
                var body = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                {
                    StatusText.Text = $"ERROR {(int)resp.StatusCode}";
                    return;
                }

                // Parseo simple sin librerías extra:
                // Busca "hasUpdate":true/false y "latestVersion":"x"
                bool hasUpdate = body.Contains("\"hasUpdate\":true", StringComparison.OrdinalIgnoreCase);

                string latestVersion = "desconocida";
                var tag = "\"latestVersion\":\"";
                var i = body.IndexOf(tag, StringComparison.OrdinalIgnoreCase);
                if (i >= 0)
                {
                    i += tag.Length;
                    var j = body.IndexOf("\"", i);
                    if (j > i) latestVersion = body.Substring(i, j - i);
                }

                StatusText.Text = hasUpdate
                    ? $"⬆️ Hay actualización\n{latestVersion}"
                    : "✅ No hay actualizaciones";
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
                SetButtonsEnabled(true);
            }
        }

        private void SetButtonsEnabled(bool enabled)
        {
            CheckHealthButton.IsEnabled = enabled;
            CheckDbButton.IsEnabled = enabled;
            CheckUpdatesButton.IsEnabled = enabled;
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