using System;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace PaL.X.Client.Video
{
    public class VideoCallForm : Form
    {
        private readonly Label _lblName = new();
        private readonly Label _lblStatus = new();

        private readonly Panel _videoHost = new();
        private readonly WebView2 _webView = new();
        private readonly TaskCompletionSource<bool> _pageReady = new(TaskCreationOptions.RunContinuationsAsynchronously);

        private readonly Button _btnCam = new();
        private readonly Button _btnMic = new();
        private readonly Button _btnHangup = new();
        private readonly Button _btnAccept = new();
        private readonly Button _btnReject = new();

        private readonly ToolTip _tt = new();

        private FlowLayoutPanel? _buttonsPanel;

        private bool _micOn = true;
        private bool _camOn = true;
        private readonly bool _incoming;
        private bool _inCall;

        private Image? _imgCamOn;
        private Image? _imgCamOff;
        private Image? _imgMicOn;
        private Image? _imgMicOff;
        private Image? _imgHangup;
        private Image? _imgCalling;

        public sealed class RtcSignalToSendEventArgs : EventArgs
        {
            public required string SignalType { get; init; }
            public required string Payload { get; init; }
        }

        public event EventHandler? HangupRequested;
        public event EventHandler<bool>? MicToggled;
        public event EventHandler<bool>? CamToggled;
        public event EventHandler? AcceptRequested;
        public event EventHandler? RejectRequested;
        public event EventHandler<RtcSignalToSendEventArgs>? RtcSignalToSend;

        public bool IsIncoming => _incoming;
        public bool IsInCall => _inCall;
        public bool IsMicOn => _micOn;
        public bool IsCamOn => _camOn;

        public VideoCallForm(string peerName, bool incoming)
        {
            Text = "Appel vidéo";
            BackColor = Color.White;
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = true;
            Size = new Size(820, 520);

            _tt.ShowAlways = true;

            _incoming = incoming;
            _inCall = !incoming;

            LoadAssets();
            BuildLayout(peerName);
            UpdateButtonsUi();

            Shown += async (_, __) =>
            {
                try
                {
                    await InitializeWebViewAsync();
                }
                catch
                {
                    // If WebView2 init fails, keep UI usable for accept/reject/hangup.
                }
            };

            if (_incoming)
            {
                _lblStatus.Text = "Appel vidéo entrant";
            }
        }

        private void LoadAssets()
        {
            _imgCalling = TryLoadAsset("Appel en cour.png", new Size(64, 64));
            _imgHangup = TryLoadAsset("raccrocher.png", new Size(28, 28));
            _imgCamOff = TryLoadAsset("Cam_OFF.png", new Size(28, 28));
            _imgCamOn = TryLoadAsset("Cam_ON.png", new Size(28, 28));
            _imgMicOff = TryLoadAsset("Mic_OFF.png", new Size(28, 28));
            _imgMicOn = TryLoadAsset("Mic_ON.png", new Size(28, 28));
        }

        private static Image? TryLoadAsset(string fileName, Size size)
        {
            try
            {
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Video CaLL", fileName);
                if (!File.Exists(path))
                {
                    return null;
                }

                using var img = Image.FromFile(path);
                return new Bitmap(img, size);
            }
            catch
            {
                return null;
            }
        }

        private void BuildLayout(string peerName)
        {
            _lblName.Text = peerName;
            _lblName.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            _lblName.ForeColor = Color.FromArgb(32, 32, 32);
            _lblName.AutoSize = true;
            _lblName.Location = new Point(16, 14);

            _lblStatus.Text = "Connexion…";
            _lblStatus.Font = new Font("Segoe UI", 9, FontStyle.Regular);
            _lblStatus.ForeColor = Color.FromArgb(90, 90, 90);
            _lblStatus.AutoSize = true;
            _lblStatus.Location = new Point(16, 40);

            _videoHost.Size = new Size(780, 360);
            _videoHost.Location = new Point(16, 70);
            _videoHost.BackColor = Color.Black;

            _webView.Dock = DockStyle.Fill;
            _videoHost.Controls.Add(_webView);

            var buttonsPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Dock = DockStyle.Bottom,
                Height = 76,
                Padding = new Padding(12, 12, 12, 12),
                BackColor = Color.FromArgb(245, 246, 248)
            };

            ConfigureIconButton(_btnCam, _imgCamOn, "Caméra ON/OFF", (_, __) => ToggleCam());
            ConfigureIconButton(_btnMic, _imgMicOn, "Micro ON/OFF", (_, __) => ToggleMic());

            ConfigureIconButton(_btnHangup, _imgHangup, "Raccrocher", (_, __) => HangupRequested?.Invoke(this, EventArgs.Empty));
            _btnHangup.BackColor = Color.FromArgb(232, 67, 67);
            _btnHangup.ForeColor = Color.White;

            ConfigureIconButton(_btnAccept, _imgCalling, "Accepter", (_, __) => AcceptRequested?.Invoke(this, EventArgs.Empty));
            _btnAccept.BackColor = Color.FromArgb(54, 179, 126);
            _btnAccept.ForeColor = Color.White;

            ConfigureIconButton(_btnReject, _imgHangup, "Refuser", (_, __) => RejectRequested?.Invoke(this, EventArgs.Empty));
            _btnReject.BackColor = Color.FromArgb(232, 67, 67);
            _btnReject.ForeColor = Color.White;

            if (_incoming)
            {
                buttonsPanel.Controls.Add(_btnAccept);
                buttonsPanel.Controls.Add(_btnReject);
            }
            else
            {
                buttonsPanel.Controls.Add(_btnCam);
                buttonsPanel.Controls.Add(_btnMic);
                buttonsPanel.Controls.Add(_btnHangup);
            }

            _buttonsPanel = buttonsPanel;

            Controls.Add(_lblName);
            Controls.Add(_lblStatus);
            Controls.Add(_videoHost);
            Controls.Add(buttonsPanel);
        }

        private void ConfigureIconButton(Button btn, Image? icon, string tooltip, EventHandler onClick)
        {
            btn.Text = string.Empty;
            btn.Image = icon;
            btn.ImageAlign = ContentAlignment.MiddleCenter;
            btn.AutoSize = false;
            btn.Size = new Size(52, 52);
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = Color.FromArgb(220, 220, 220);
            btn.BackColor = Color.White;
            btn.ForeColor = Color.FromArgb(40, 40, 40);
            btn.Padding = new Padding(6);
            btn.Margin = new Padding(10, 0, 10, 0);
            btn.Click += onClick;
            _tt.SetToolTip(btn, tooltip);
        }

        private void ToggleMic()
        {
            _micOn = !_micOn;
            UpdateButtonsUi();
            MicToggled?.Invoke(this, _micOn);
            _ = SetMicAsync(_micOn);
        }

        private void ToggleCam()
        {
            _camOn = !_camOn;
            UpdateButtonsUi();
            CamToggled?.Invoke(this, _camOn);
            _ = SetCamAsync(_camOn);
        }

        private void UpdateButtonsUi()
        {
            _btnMic.Image = _micOn ? _imgMicOn : _imgMicOff;
            _btnCam.Image = _camOn ? _imgCamOn : _imgCamOff;
        }

        public void SwitchToInCallMode()
        {
            if (_inCall) return;

            _inCall = true;
            _lblStatus.Text = "En appel";

            if (_incoming && _buttonsPanel != null)
            {
                _buttonsPanel.Controls.Clear();
                _buttonsPanel.Controls.Add(_btnCam);
                _buttonsPanel.Controls.Add(_btnMic);
                _buttonsPanel.Controls.Add(_btnHangup);
            }
        }

        private async Task InitializeWebViewAsync()
        {
            await _webView.EnsureCoreWebView2Async();
            if (_webView.CoreWebView2 == null)
            {
                return;
            }

            _webView.CoreWebView2.PermissionRequested += (_, e) =>
            {
                try
                {
                    if (e.PermissionKind == CoreWebView2PermissionKind.Microphone || e.PermissionKind == CoreWebView2PermissionKind.Camera)
                    {
                        e.State = CoreWebView2PermissionState.Allow;
                    }
                }
                catch { }
            };

            _webView.CoreWebView2.WebMessageReceived += (_, e) =>
            {
                try
                {
                    var json = e.WebMessageAsJson;
                    if (string.IsNullOrWhiteSpace(json))
                    {
                        return;
                    }

                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;
                    if (!root.TryGetProperty("kind", out var kindProp))
                    {
                        return;
                    }

                    var kind = kindProp.GetString() ?? string.Empty;
                    if (string.Equals(kind, "webrtc-page-loaded", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(kind, "webrtc-ready", StringComparison.OrdinalIgnoreCase))
                    {
                        _pageReady.TrySetResult(true);
                        return;
                    }

                    if (string.Equals(kind, "webrtc-signal", StringComparison.OrdinalIgnoreCase))
                    {
                        var signalType = root.TryGetProperty("signalType", out var st) ? (st.GetString() ?? string.Empty) : string.Empty;
                        var payload = root.TryGetProperty("payload", out var pl) ? (pl.GetString() ?? string.Empty) : string.Empty;
                        if (!string.IsNullOrWhiteSpace(signalType))
                        {
                            RtcSignalToSend?.Invoke(this, new RtcSignalToSendEventArgs { SignalType = signalType, Payload = payload });
                        }
                    }
                }
                catch
                {
                    // ignore malformed messages
                }
            };

            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            _webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                "palx.local",
                baseDir,
                CoreWebView2HostResourceAccessKind.Allow);

            var target = "https://palx.local/Video/WebRtc/webrtc_call.html";
            _webView.CoreWebView2.Navigate(target);
        }

        private async Task ExecuteWhenReadyAsync(string script)
        {
            try
            {
                await _pageReady.Task;
                if (_webView.CoreWebView2 == null)
                {
                    return;
                }

                await _webView.ExecuteScriptAsync(script);
            }
            catch
            {
                // ignore
            }
        }

        public Task StartWebRtcAsync(bool isInitiator)
        {
            return ExecuteWhenReadyAsync($"window.__palxStart({(isInitiator ? "true" : "false")});");
        }

        public Task ApplyRemoteSignalAsync(string signalType, string payload)
        {
            var safeType = JsonSerializer.Serialize(signalType ?? string.Empty);
            var safePayload = JsonSerializer.Serialize(payload ?? string.Empty);
            return ExecuteWhenReadyAsync($"window.__palxReceiveSignal({safeType}, {safePayload});");
        }

        public Task SetMicAsync(bool on)
        {
            return ExecuteWhenReadyAsync($"window.__palxSetMic({(on ? "true" : "false")});");
        }

        public Task SetCamAsync(bool on)
        {
            return ExecuteWhenReadyAsync($"window.__palxSetCam({(on ? "true" : "false")});");
        }

        public void SetStatus(string status)
        {
            _lblStatus.Text = status;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);

            try
            {
                _webView.Dispose();
            }
            catch
            {
                // ignore
            }

            _imgCamOn?.Dispose();
            _imgCamOff?.Dispose();
            _imgMicOn?.Dispose();
            _imgMicOff?.Dispose();
            _imgHangup?.Dispose();
            _imgCalling?.Dispose();
        }
    }
}
