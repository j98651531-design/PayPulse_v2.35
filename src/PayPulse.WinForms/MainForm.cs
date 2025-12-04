using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using PayPulse.Core.DTOs;
using PayPulse.Core.Services;
using PayPulse.Core.Billing;
using PayPulse.Domain.Entities;
using PayPulse.Domain.Interfaces.Repositories;
using PayPulse.Domain.Interfaces.Services;
using PayPulse.Infrastructure.Time;
using PayPulse.WinForms.Forms;

namespace PayPulse.WinForms
{
    public class MainForm : Form
    {
        private readonly SettingsService _settingsService;
        private readonly TransferFetchService _fetchService;
        private readonly TransferNormalizationService _normalizeService;
        private readonly AddTransfersToPosService _addToPosService;
        private readonly ReportsQueryService _reportsService;
        private readonly PosMetadataService _posMetadataService;
        private readonly IProfileRepository _profileRepository;
        private readonly BackgroundSyncService _backgroundSyncService;
        private readonly LoggerService _logger;
        private readonly ILogRepository _logRepository;
        private readonly ProfileAuthService _profileAuthService;
        private readonly IAppUserRepository _appUserRepository;
        private readonly CurrentUserContext _currentUserContext;
        private readonly IBillingEngine _billingEngine;
        private readonly IBillingRepository _billingRepository;

        private readonly IDateTimeProvider _clock = new SystemDateTimeProvider();

        private DataGridView _grid;
        private BindingSource _binding;
        private DateTimePicker _dtFrom;
        private DateTimePicker _dtTo;
        private Button _btnFetch;
        private Button _btnNormalize;
        private Button _btnAddPos;
        private Button _btnToday;
        private Button _btnYesterday;
        private Button _btnLast7;
        private Button _btnThisMonth;
        private StatusStrip _status;
        private ToolStripStatusLabel _statusDb;
        private ToolStripStatusLabel _statusPosDb;
        private ToolStripStatusLabel _statusJwt;
        private ToolStripStatusLabel _statusBg;
        private ComboBox _profileCombo;

        public MainForm(
            SettingsService settingsService,
            TransferFetchService fetchService,
            TransferNormalizationService normalizeService,
            AddTransfersToPosService addToPosService,
            ReportsQueryService reportsService,
            PosMetadataService posMetadataService,
            IProfileRepository profileRepository,
            BackgroundSyncService backgroundSyncService,
            LoggerService logger,
            ILogRepository logRepository,
            ProfileAuthService profileAuthService,
            IAppUserRepository appUserRepository,
            CurrentUserContext currentUserContext,
            IBillingEngine billingEngine,
            IBillingRepository billingRepository)
        {
            _settingsService = settingsService;
            _fetchService = fetchService;
            _normalizeService = normalizeService;
            _addToPosService = addToPosService;
            _reportsService = reportsService;
            _posMetadataService = posMetadataService;
            _profileRepository = profileRepository;
            _backgroundSyncService = backgroundSyncService;
            _logger = logger;
            _logRepository = logRepository;
            _profileAuthService = profileAuthService;
            _appUserRepository = appUserRepository;
            _currentUserContext = currentUserContext;
            _billingEngine = billingEngine;
            _billingRepository = billingRepository;

            InitializeUi();
            LoadProfiles();
            SetRangeToday();
            UpdateStatus();
        }
        private void LoadProfiles()
        {
            var list = _profileRepository.GetAll();
            _profileCombo.DataSource = list;
            _profileCombo.DisplayMember = "Name";
            _profileCombo.ValueMember = "ProfileId";
        }
        private void OpenSettingsForm()
        {
            using (var frm = new SettingsForm(_settingsService, _posMetadataService))
            {
                frm.ShowDialog(this);
            }
            UpdateStatus();
        }
        private void InitializeUi()
        {

            // Icon loader for menu items (optional; icons are loaded from Assets\Icons if present).
            string iconsRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Icons");
            Image LoadIcon(string name)
            {
                try
                {
                    var path = Path.Combine(iconsRoot, name + ".png");
                    if (File.Exists(path))
                    {
                        return Image.FromFile(path);
                    }
                }
                catch
                {
                    // ignore icon load errors
                }
                return null;
            }

            Text = "PayPulse v2.14 – Multi-Providers (RBAC)";
            Width = 1100;
            Height = 650;

            
            var menu = new MenuStrip();

            var profilesItem = new ToolStripMenuItem("Profiles")
            {
                Image = LoadIcon("profiles")
            };
            profilesItem.Click += (s, e) => OpenProfilesForm();

            var settingsItem = new ToolStripMenuItem("Settings")
            {
                Image = LoadIcon("settings")
            };
            settingsItem.Click += (s, e) => OpenSettingsForm();

            var reportsItem = new ToolStripMenuItem("Reports")
            {
                Image = LoadIcon("reports")
            };
            reportsItem.Click += (s, e) => OpenReportsForm();

            var securityItem = new ToolStripMenuItem("Security")
            {
                Image = LoadIcon("security")
            };
            securityItem.Click += (s, e) => OpenSecurityReportForm();

            // Logs menu (History + Live)
            var logsMenu = new ToolStripMenuItem("Logs")
            {
                Image = LoadIcon("logs")
            };
            var logsHistoryItem = new ToolStripMenuItem("History")
            {
                Image = LoadIcon("logs_history")
            };
            logsHistoryItem.Click += (s, e) => OpenLogsForm();

            var logsLiveItem = new ToolStripMenuItem("Live Logs")
            {
                Image = LoadIcon("logs_live")
            };
            logsLiveItem.Click += (s, e) => OpenLiveLogsForm();
            logsMenu.DropDownItems.Add(logsHistoryItem);
            logsMenu.DropDownItems.Add(logsLiveItem);

            var usersItem = new ToolStripMenuItem("Users")
            {
                Image = LoadIcon("users")
            };
            usersItem.Click += (s, e) => OpenUsersForm();

            var providersItem = new ToolStripMenuItem("Providers")
            {
                Image = LoadIcon("providers")
            };
            providersItem.Click += (s, e) => OpenProvidersForm();

            var billingMenu = new ToolStripMenuItem("Billing")
            {
                Image = LoadIcon("billing")
            };
            var billingDashboardItem = new ToolStripMenuItem("Dashboard")
            {
                Image = LoadIcon("billing_dashboard")
            };
            billingDashboardItem.Click += (s, e) => OpenBillingDashboardForm();
            var billingConfigItem = new ToolStripMenuItem("Configuration")
            {
                Image = LoadIcon("billing_config")
            };
            billingConfigItem.Click += (s, e) => OpenBillingConfigForm();
            billingMenu.DropDownItems.Add(billingDashboardItem);
            billingMenu.DropDownItems.Add(billingConfigItem);

            var bgMenu = new ToolStripMenuItem("Background")
            {
                Image = LoadIcon("background")
            };
            var bgStart = new ToolStripMenuItem("Start worker")
            {
                Image = LoadIcon("bg_start")
            };
            bgStart.Click += (s, e) =>
            {
                if (_currentUserContext == null || !_currentUserContext.CanControlBackgroundWorker)
                {
                    MessageBox.Show("You do not have permission to control the background worker.",
                        "PayPulse – Permissions",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                _backgroundSyncService.Start();
                _statusBg.Text = "BG: Running";
            };
            var bgStop = new ToolStripMenuItem("Stop worker")
            {
                Image = LoadIcon("bg_stop")
            };
            bgStop.Click += (s, e) =>
            {
                _backgroundSyncService.Stop();
                _statusBg.Text = "BG: Stopped";
            };
            var bgDashboard = new ToolStripMenuItem("Sync dashboard")
            {
                Image = LoadIcon("dashboard")
            };
            bgDashboard.Click += (s, e) => OpenSyncDashboardForm();

            bgMenu.DropDownItems.Add(bgStart);
            bgMenu.DropDownItems.Add(bgStop);
            bgMenu.DropDownItems.Add(bgDashboard);

            var exitItem = new ToolStripMenuItem("Exit")
            {
                Image = LoadIcon("exit")
            };
            exitItem.Click += (s, e) => Close();

            menu.Items.AddRange(new ToolStripItem[]
            {
                profilesItem,
                settingsItem,
                providersItem,
                billingMenu,
                reportsItem,
                securityItem,
                logsMenu,
                usersItem,
                bgMenu,
                exitItem
            });

            // Apply role-based UI restrictions
            if (_currentUserContext != null && _currentUserContext.CurrentUser != null)
            {
                // Everyone that is authenticated can see transfers, fetch, normalize and reports.
                reportsItem.Visible = _currentUserContext.CanViewReports;

                // Profiles can be managed by Admin + Manager.
                profilesItem.Visible = _currentUserContext.CanManageProfiles;

                // Settings & providers are Admin-only.
                settingsItem.Visible = _currentUserContext.CanEditSettings;
                providersItem.Visible = _currentUserContext.CanManageProviders;
                usersItem.Visible = _currentUserContext.CanManageUsers;
                securityItem.Visible = _currentUserContext.CanViewSecurityReport;

                // Logs are visible for Admin + Manager.
                logsMenu.Visible = _currentUserContext.CanViewLogs;

                // Background worker menu (start/stop) is Admin + Manager.
                bgMenu.Visible = _currentUserContext.CanControlBackgroundWorker;
            }

            MainMenuStrip = menu;
            Controls.Add(menu);

var topPanel = new Panel { Dock = DockStyle.Top, Height = 70 };
            Controls.Add(topPanel);

            var lblProfile = new Label { Left = 10, Top = 10, Text = "Profile:", Width = 60 };
            _profileCombo = new ComboBox { Left = 70, Top = 8, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            topPanel.Controls.Add(lblProfile);
            topPanel.Controls.Add(_profileCombo);

            _dtFrom = new DateTimePicker { Left = 290, Top = 8, Width = 140, Format = DateTimePickerFormat.Short };
            _dtTo = new DateTimePicker { Left = 440, Top = 8, Width = 140, Format = DateTimePickerFormat.Short };
            topPanel.Controls.Add(_dtFrom);
            topPanel.Controls.Add(_dtTo);

            _btnToday = new Button { Left = 600, Top = 8, Width = 70, Text = "Today" };
            _btnYesterday = new Button { Left = 680, Top = 8, Width = 80, Text = "Yesterday" };
            _btnLast7 = new Button { Left = 770, Top = 8, Width = 80, Text = "Last 7" };
            _btnThisMonth = new Button { Left = 860, Top = 8, Width = 90, Text = "This month" };
            _btnToday.Click += (s, e) => SetRangeToday();
            _btnYesterday.Click += (s, e) => SetRangeYesterday();
            _btnLast7.Click += (s, e) => SetRangeLast7();
            _btnThisMonth.Click += (s, e) => SetRangeThisMonth();
            topPanel.Controls.Add(_btnToday);
            topPanel.Controls.Add(_btnYesterday);
            topPanel.Controls.Add(_btnLast7);
            topPanel.Controls.Add(_btnThisMonth);

            _btnFetch = new Button { Left = 10, Top = 38, Width = 80, Text = "Fetch" };
            _btnNormalize = new Button { Left = 100, Top = 38, Width = 80, Text = "Normalize" };
            _btnAddPos = new Button { Left = 190, Top = 38, Width = 100, Text = "Add to POS" };
            _btnFetch.Click += BtnFetch_Click;
            _btnNormalize.Click += BtnNormalize_Click;
            _btnAddPos.Click += BtnAddPos_Click;
            topPanel.Controls.Add(_btnFetch);
            topPanel.Controls.Add(_btnNormalize);
            topPanel.Controls.Add(_btnAddPos);

            _grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true };
            _binding = new BindingSource();
            _grid.DataSource = _binding;
            Controls.Add(_grid);

            _status = new StatusStrip();
            _statusDb = new ToolStripStatusLabel("Sync DB: ?");
            _statusPosDb = new ToolStripStatusLabel("POS DB: ?");
            _statusJwt = new ToolStripStatusLabel("JWT: ?");
            _statusBg = new ToolStripStatusLabel("BG: Running");
            _status.Items.AddRange(new ToolStripItem[] { _statusDb, _statusPosDb, _statusJwt, _statusBg });
            Controls.Add(_status);
        }
        private void OpenProvidersForm()
        {
            using (var frm = new ProvidersForm(_settingsService))
            {
                frm.ShowDialog(this);
            }
        }

        private void OpenBillingDashboardForm()
        {
            using (var form = new BillingDashboardForm(_billingEngine, _billingRepository))
            {
                form.StartPosition = FormStartPosition.CenterParent;
                form.ShowDialog(this);
            }
        }


        private void OpenBillingConfigForm()
        {
            using (var form = new BillingConfigForm(_billingRepository))
            {
                form.StartPosition = FormStartPosition.CenterParent;
                form.ShowDialog(this);
            }
        }


        private void OpenUsersForm()
        {
            using (var frm = new UsersForm(_appUserRepository, _currentUserContext))
            {
                frm.ShowDialog(this);
            }
        }

        private void SetRangeToday()
        {
            DateTime today = _clock.Today;
            _dtFrom.Value = today;
            _dtTo.Value = today;
        }

        private void SetRangeYesterday()
        {
            DateTime today = _clock.Today;
            _dtFrom.Value = today.AddDays(-1);
            _dtTo.Value = today.AddDays(-1);
        }

        private void SetRangeLast7()
        {
            DateTime today = _clock.Today;
            _dtFrom.Value = today.AddDays(-6);
            _dtTo.Value = today;
        }

        private void SetRangeThisMonth()
        {
            DateTime today = _clock.Today;
            DateTime first = new DateTime(today.Year, today.Month, 1);
            _dtFrom.Value = first;
            _dtTo.Value = today;
        }

        private DateRangeRequestDto GetRange()
        {
            return new DateRangeRequestDto
            {
                Start = _dtFrom.Value.Date,
                End = _dtTo.Value.Date.AddDays(1).AddSeconds(-1)
            };
        }

        private void UpdateStatus()
        {
            _statusDb.Text = "Sync DB: OK";

            try
            {
                var settings = _settingsService.Load();
                if (string.IsNullOrWhiteSpace(settings.Databases.PosDbPath))
                {
                    _statusPosDb.Text = "POS DB: path not set";
                }
                else
                {
                    string full = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, settings.Databases.PosDbPath);
                    _statusPosDb.Text = System.IO.File.Exists(full) ? "POS DB: OK" : "POS DB: missing";
                }
            }
            catch
            {
                _statusPosDb.Text = "POS DB: error";
            }

            var profile = _profileCombo.SelectedItem as Profile;
            if (profile == null)
            {
                _statusJwt.Text = "JWT: n/a";
            }
            else if (!string.IsNullOrWhiteSpace(profile.ManualToken))
            {
                _statusJwt.Text = "JWT: manual";
            }
            else
            {
                _statusJwt.Text = "JWT: per-profile";
            }
        }

        private string GetEffectiveToken(out Profile profileUsed)
        {
            profileUsed = _profileCombo.SelectedItem as Profile;

            if (profileUsed != null)
            {
                bool needsCredentials;
                string token = _profileAuthService.EnsureToken(profileUsed, out needsCredentials);
                if (needsCredentials)
                {
                    using (var frm = new ProfileLoginForm(_profileAuthService, profileUsed))
                    {
                        if (frm.ShowDialog(this) != DialogResult.OK)
                        {
                            return null;
                        }
                        token = frm.Token;
                    }
                }
                return token;
            }

            return null;
        }

        private void BtnFetch_Click(object sender, EventArgs e)
        {
            string correlationId = Guid.NewGuid().ToString("N");
            _logger.Info("Manual Fetch clicked", correlationId, null, "UI/FETCH");

            try
            {
                var range = GetRange();
                var token = GetEffectiveToken(out var profile);
                if (string.IsNullOrWhiteSpace(token))
                {
                    MessageBox.Show("No token available. Configure a profile with credentials/TOTP first.",
                        "PayPulse – Fetch", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var data = _fetchService.FetchForProfile(range, profile, token, correlationId);
                _binding.DataSource = data;
                ReloadGrid();
            }
            catch (Exception ex)
            {
                _logger.Error("Fetch click error", ex, correlationId, null, "UI/FETCH");
                MessageBox.Show(@"Fetch error:\r\n\r\n" + ex.Message,
                    "PayPulse – Fetch",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void BtnNormalize_Click(object sender, EventArgs e)
        {
            string correlationId = Guid.NewGuid().ToString("N");
            _logger.Info("Manual Normalize clicked", correlationId, null, "UI/NORMALIZE");

            try
            {
                var range = GetRange();
                var token = GetEffectiveToken(out var profile);
                if (string.IsNullOrWhiteSpace(token))
                {
                    MessageBox.Show("No token available.",
                        "PayPulse – Normalize", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _normalizeService.NormalizeForProfile(range, profile, token, correlationId);
                ReloadGrid();
            }
            catch (Exception ex)
            {
                _logger.Error("Normalize click error", ex, correlationId, null, "UI/NORMALIZE");
                MessageBox.Show(@"Normalize error:\r\n\r\n" + ex.Message,
                    "PayPulse – Normalize",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void BtnAddPos_Click(object sender, EventArgs e)
        {
            if (_currentUserContext == null || !_currentUserContext.CanAddToPos)
            {
                MessageBox.Show("You do not have permission to add transfers to POS.",
                    "PayPulse – Permissions",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            string correlationId = Guid.NewGuid().ToString("N");
            _logger.Info("Manual AddToPos clicked", correlationId, null, "UI/ADDPOS");

            try
            {
                var profile = _profileCombo.SelectedItem as Profile;
                string userOverride = profile == null ? null : profile.PosUserId;
                string cashboxOverride = profile == null ? null : profile.PosCashboxId;

                _addToPosService.AddPendingToPos(profile, userOverride, cashboxOverride, correlationId);
                ReloadGrid();
            }
            catch (Exception ex)
            {
                _logger.Error("AddToPos click error", ex, correlationId, null, "UI/ADDPOS");
                MessageBox.Show(@"Add-to-POS error:\r\n\r\n" + ex.Message,
                    "PayPulse – Add to POS",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }


        private void ReloadGrid()
        {
            try
            {
                var token = GetEffectiveToken(out var profile);
                if (string.IsNullOrWhiteSpace(token))
                {
                    _binding.DataSource = null;
                    return;
                }

                var range = GetRange();
                var data = _reportsService.GetReportForToken(range, token);
                _binding.DataSource = data;
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"Reload error:\r\n\r\n" + ex.Message,
                    "PayPulse – Reload",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void OpenReportsForm()
        {
            using (var frm = new ReportsForm(_reportsService, _profileAuthService, _profileRepository))
            {
                frm.ShowDialog(this);
            }
        }

        private void OpenSecurityReportForm()
        {
            using (var frm = new SecurityReportForm(_appUserRepository, _currentUserContext))
            {
                frm.ShowDialog(this);
            }
        }


        private void OpenLogsForm()
        {
            using (var frm = new LogsForm(_logRepository))
            {
                frm.ShowDialog(this);
            }
        }


        private void OpenLiveLogsForm()
        {
            var frm = new LiveLogsForm();
            frm.Show(this);
        }


        private void OpenSyncDashboardForm()
        {
            var dashboardService = new ProfileSyncDashboardService(_profileRepository, _logRepository, _clock);
            using (var frm = new SyncDashboardForm(dashboardService, _logRepository, _clock))
            {
                frm.ShowDialog(this);
            }
        }

        private void OpenProfilesForm()
        {
            using (var frm = new ProfilesForm(_profileRepository, _posMetadataService))
            {
                frm.ShowDialog(this);
            }
            LoadProfiles();
        }
    }
}
