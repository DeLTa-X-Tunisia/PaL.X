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
            BackColor = Color.FromArgb(20, 20, 20); // Dark background
            ForeColor = Color.White;
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.Sizable; // Allow resizing
            MinimumSize = new Size(600, 400);
            Size = new Size(900, 600);
            Icon = null; // Or set a specific icon if available

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
                _lblStatus.Text = "Appel vidéo entrant...";
                _lblStatus.ForeColor = Color.FromArgb(54, 179, 126); // Green accent
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
            // Main Layout Container (TableLayoutPanel) to prevent overlap
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Color.Black
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F)); // Header
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Video
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100F)); // Bottom Bar

            // Header Panel
            var headerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(20, 10, 20, 0)
            };

            _lblName.Text = peerName;
            _lblName.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            _lblName.ForeColor = Color.White;
            _lblName.AutoSize = true;
            _lblName.Location = new Point(20, 10);

            _lblStatus.Text = "Connexion…";
            _lblStatus.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            _lblStatus.ForeColor = Color.FromArgb(180, 180, 180);
            _lblStatus.AutoSize = true;
            _lblStatus.Location = new Point(22, 38);

            headerPanel.Controls.Add(_lblName);
            headerPanel.Controls.Add(_lblStatus);

            // Video Host
            _videoHost.Dock = DockStyle.Fill;
            _videoHost.BackColor = Color.Black;
            _videoHost.Padding = new Padding(0);
            
            _webView.Dock = DockStyle.Fill;
            _videoHost.Controls.Add(_webView);

            // Bottom Controls Bar
            var bottomBar = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(20, 20, 20) // Dark background matching theme
            };

            // Center container for buttons
            var buttonsContainer = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.None
            };
            
            // Using a TableLayoutPanel to center the FlowLayoutPanel inside the bottom bar
            var centeringTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            centeringTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            centeringTable.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            centeringTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            centeringTable.Controls.Add(buttonsContainer, 1, 0);

            bottomBar.Controls.Add(centeringTable);

            // Configure Buttons
            ConfigureIconButton(_btnCam, _imgCamOn, "Caméra", (_, __) => ToggleCam(), isSecondary: true);
            ConfigureIconButton(_btnMic, _imgMicOn, "Micro", (_, __) => ToggleMic(), isSecondary: true);

            ConfigureIconButton(_btnHangup, _imgHangup, "Raccrocher", (_, __) => HangupRequested?.Invoke(this, EventArgs.Empty), isDanger: true);
            
            ConfigureIconButton(_btnAccept, _imgCalling, "Accepter", (_, __) => AcceptRequested?.Invoke(this, EventArgs.Empty), isSuccess: true);
            ConfigureIconButton(_btnReject, _imgHangup, "Refuser", (_, __) => RejectRequested?.Invoke(this, EventArgs.Empty), isDanger: true);

            if (_incoming)
            {
                buttonsContainer.Controls.Add(_btnAccept);
                buttonsContainer.Controls.Add(_btnReject);
            }
            else
            {
                buttonsContainer.Controls.Add(_btnMic);
                buttonsContainer.Controls.Add(_btnHangup);
                buttonsContainer.Controls.Add(_btnCam);
            }

            _buttonsPanel = buttonsContainer;

            // Add to Main Layout
            mainLayout.Controls.Add(headerPanel, 0, 0);
            mainLayout.Controls.Add(_videoHost, 0, 1);
            mainLayout.Controls.Add(bottomBar, 0, 2);

            Controls.Add(mainLayout);
        }

        private void ConfigureIconButton(Button btn, Image? icon, string tooltip, EventHandler onClick, bool isDanger = false, bool isSuccess = false, bool isSecondary = false)
        {
            btn.Text = string.Empty;
            btn.Image = icon;
            btn.ImageAlign = ContentAlignment.MiddleCenter;
            btn.AutoSize = false;
            btn.Size = new Size(60, 60);
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Cursor = Cursors.Hand;
            
            // Colors
            if (isDanger)
            {
                btn.BackColor = Color.FromArgb(235, 87, 87); // Red
                btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(200, 50, 50);
            }
            else if (isSuccess)
            {
                btn.BackColor = Color.FromArgb(39, 174, 96); // Green
                btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 150, 80);
            }
            else
            {
                btn.BackColor = Color.FromArgb(60, 60, 60); // Dark Gray
                btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(80, 80, 80);
            }

            btn.Margin = new Padding(15, 15, 15, 15); // Spacing between buttons
            btn.Click += onClick;
            _tt.SetToolTip(btn, tooltip);

            // Make it circular
            btn.Paint += (s, e) =>
            {
                using var path = new System.Drawing.Drawing2D.GraphicsPath();
                path.AddEllipse(0, 0, btn.Width, btn.Height);
                btn.Region = new Region(path);
            };
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
            _lblStatus.ForeColor = Color.FromArgb(39, 174, 96); // Green status

            if (_incoming && _buttonsPanel != null)
            {
                _buttonsPanel.Controls.Clear();
                _buttonsPanel.Controls.Add(_btnMic);
                _buttonsPanel.Controls.Add(_btnHangup);
                _buttonsPanel.Controls.Add(_btnCam);
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
