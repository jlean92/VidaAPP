using Microsoft.UI.Xaml;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VidaApp.WinUI
{
    public sealed partial class MainWindow : Window
    {
        // HttpClient singleton: correcto para desktop apps, evita agotamiento de sockets.
        private static readonly HttpClient _http = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(15)
        };

        private CancellationTokenSource? _cts;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void OnPingHealthClick(object sender, RoutedEventArgs e)
            => await PingAsync("/health");

        private async void OnPingDbHealthClick(object sender, RoutedEventArgs e)
            => await PingAsync("/health/db");

        private void OnClearClick(object sender, RoutedEventArgs e)
        {
            OutputTextBox.Text = "";
            StatusTextBlock.Text = "";
        }

        private async Task PingAsync(string path)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            var baseUrl = (BaseUrlTextBox.Text ?? "").Trim();
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                StatusTextBlock.Text = "Base URL vacía.";
                return;
            }

            if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri))
            {
                StatusTextBlock.Text = "Base URL inválida (ej: https://jlean92.es)";
                return;
            }

            var target = new Uri(baseUri, path);

            SetBusy(true, $"GET {target}");

            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, target);
                using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseContentRead, _cts.Token);

                var body = await resp.Content.ReadAsStringAsync(_cts.Token);
                AppendOutput(FormatResponse(target.ToString(), resp.StatusCode, body));
                StatusTextBlock.Text = $"OK ({(int)resp.StatusCode})";
            }
            catch (TaskCanceledException)
            {
                AppendOutput($"[{DateTimeOffset.Now:u}] CANCELLED/timeout\n");
                StatusTextBlock.Text = "Cancelado o timeout.";
            }
            catch (Exception ex)
            {
                AppendOutput($"[{DateTimeOffset.Now:u}] ERROR\n{ex}\n\n");
                StatusTextBlock.Text = "Error (ver salida).";
            }
            finally
            {
                SetBusy(false, "");
            }
        }

        private void SetBusy(bool busy, string status)
        {
            StatusTextBlock.Text = status;
        }

        private void AppendOutput(string text)
        {
            OutputTextBox.Text += text;
            OutputTextBox.SelectionStart = OutputTextBox.Text.Length;
        }

        private static string FormatResponse(string url, System.Net.HttpStatusCode code, string body)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"[{DateTimeOffset.Now:u}] {url}");
            sb.AppendLine($"Status: {(int)code} ({code})");
            sb.AppendLine("Body:");
            sb.AppendLine(body);
            sb.AppendLine(new string('-', 72));
            sb.AppendLine();
            return sb.ToString();
        }
    }
}
