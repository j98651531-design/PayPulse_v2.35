using System;
using System.Linq;
using System.Windows.Forms;
using PayPulse.Core.Services;
using PayPulse.Domain.Settings;

namespace PayPulse.WinForms
{
    public class SettingsForm : Form
    {
        private readonly SettingsService _settingsService;
        private readonly PosMetadataService _posMetadataService;

        private TextBox _txtSyncDb;
        private TextBox _txtPosDb;
        private TextBox _txtAppSettings;
        private ComboBox _cmbPosType;
        private TextBox _txtPosFox;
        private Button _btnBrowsePosFox;

        private ComboBox _cmbUsd;
        private ComboBox _cmbEur;
        private ComboBox _cmbIls;
        private ComboBox _cmbUser;
        private ComboBox _cmbCashbox;
        private ComboBox _cmbIdTypeId;
        private ComboBox _cmbIdTypePassport;
        private ComboBox _cmbUserTypeCitizen;
        private ComboBox _cmbUserTypeTourist;

        private Button _btnSave;
        private Button _btnClose;
        private Button _btnBrowseSync;
        private Button _btnBrowsePos;
        private Button _btnBrowseApp;
        private CheckBox _chkAutoStart;
        private CheckBox _chkCheckUpdates;
        private NumericUpDown _numUpdateInterval;
        private WindowsAutoStartService _autoStartService;


        public SettingsForm(SettingsService settingsService, PosMetadataService posMetadataService)
        {
            _settingsService = settingsService;
            _posMetadataService = posMetadataService;
            InitializeUi();
            LoadSettings();
        }

        private void InitializeUi()
        {
            Text = "Settings";
            Width = 700;
            Height = 550;
            AutoScroll = true;

            var panel = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            Controls.Add(panel);

            int top = 10;
            int labelWidth = 150;
            int inputWidth = 350;

            Action<string, Control> AddLabeled = (label, ctrl) =>
            {
                var lbl = new Label { Left = 10, Top = top + 3, Width = labelWidth, Text = label };
                ctrl.Left = 170;
                ctrl.Top = top;
                ctrl.Width = inputWidth;
                panel.Controls.Add(lbl);
                panel.Controls.Add(ctrl);
                top += 30;
            };

            _txtSyncDb = new TextBox();
            _btnBrowseSync = new Button { Text = "...", Width = 30, Left = 170 + inputWidth + 5, Top = top - 30 };
            _btnBrowseSync.Click += (s, e) => BrowseFile(_txtSyncDb, "SQLite DB (*.db)|*.db|All files|*.*");
            panel.Controls.Add(_btnBrowseSync);
            AddLabeled("Sync DB Path", _txtSyncDb);

            _txtPosDb = new TextBox();
            _btnBrowsePos = new Button { Text = "...", Width = 30, Left = 170 + inputWidth + 5, Top = top - 30 };
            _btnBrowsePos.Click += (s, e) => BrowseFile(_txtPosDb, "Access DB (*.mdb;*.accdb)|*.mdb;*.accdb|All files|*.*");
            panel.Controls.Add(_btnBrowsePos);
            AddLabeled("POS DB Path", _txtPosDb);


_cmbPosType = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
_cmbPosType.Items.AddRange(new object[] { "Changer", "ChangeMat" });
AddLabeled("POS Type", _cmbPosType);

_txtPosFox = new TextBox();
_btnBrowsePosFox = new Button { Text = "...", Width = 30, Left = 170 + inputWidth + 5, Top = top - 30 };
_btnBrowsePosFox.Click += (s, e) => BrowseFolder(_txtPosFox);
panel.Controls.Add(_btnBrowsePosFox);
AddLabeled("POS FoxPro Folder", _txtPosFox);

            _txtAppSettings = new TextBox();
            _btnBrowseApp = new Button { Text = "...", Width = 30, Left = 170 + inputWidth + 5, Top = top - 30 };
            _btnBrowseApp.Click += (s, e) => BrowseFile(_txtAppSettings, "JSON (*.json)|*.json|All files|*.*");
            panel.Controls.Add(_btnBrowseApp);
            AddLabeled("AppSettings Path", _txtAppSettings);

            // Behavior: auto-start + auto-update
            _chkAutoStart = new CheckBox
            {
                Text = "Start PayPulse automatically with Windows",
                AutoSize = true
            };
            AddLabeled("Auto start", _chkAutoStart);

            _chkCheckUpdates = new CheckBox
            {
                Text = "Check for updates on startup",
                AutoSize = true
            };
            AddLabeled("Auto-update", _chkCheckUpdates);

            _numUpdateInterval = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 168,
                Value = 4
            };
            AddLabeled("Update interval (hours)", _numUpdateInterval);

            _autoStartService = new WindowsAutoStartService("PayPulse");


            _cmbUsd = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbEur = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbIls = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbUser = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbCashbox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbIdTypeId = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbIdTypePassport = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbUserTypeCitizen = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbUserTypeTourist = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };

            AddLabeled("USD", _cmbUsd);
            AddLabeled("EUR", _cmbEur);
            AddLabeled("ILS", _cmbIls);
            AddLabeled("IdTypeID", _cmbIdTypeId);
            AddLabeled("IdTypePassport", _cmbIdTypePassport);
            AddLabeled("UserTypeCitizen", _cmbUserTypeCitizen);
            AddLabeled("UserTypeTourist", _cmbUserTypeTourist);

            _btnSave = new Button { Text = "Save", Left = 170, Top = top + 10, Width = 100 };
            _btnClose = new Button { Text = "Close", Left = 280, Top = top + 10, Width = 100 };
            _btnSave.Click += (s, e) => SaveSettings();
            _btnClose.Click += (s, e) => Close();
            panel.Controls.Add(_btnSave);
            panel.Controls.Add(_btnClose);
        }

        private void LoadSettings()
        {
            var s = _settingsService.Load() ?? new AppSettings();

            _txtSyncDb.Text = s.Databases.SyncDbPath;
            _txtPosDb.Text = s.Databases.PosDbPath;
            _txtAppSettings.Text = s.Databases.AppSettingsPath;

            try
            {
                var currencies = _posMetadataService.GetCurrencies().ToList();
                _cmbUsd.DataSource = currencies.ToList();
                _cmbUsd.DisplayMember = "Display";
                _cmbUsd.ValueMember = "Id";

                _cmbEur.DataSource = currencies.ToList();
                _cmbEur.DisplayMember = "Display";
                _cmbEur.ValueMember = "Id";

                _cmbIls.DataSource = currencies.ToList();
                _cmbIls.DisplayMember = "Display";
                _cmbIls.ValueMember = "Id";

                var users = _posMetadataService.GetUsers().ToList();
                _cmbUser.DataSource = users;
                _cmbUser.DisplayMember = "Display";
                _cmbUser.ValueMember = "Id";

                var cashboxes = _posMetadataService.GetCashboxes().ToList();
                _cmbCashbox.DataSource = cashboxes;
                _cmbCashbox.DisplayMember = "Display";
                _cmbCashbox.ValueMember = "Id";

                var idTypes = _posMetadataService.GetIdTypes().ToList();
                _cmbIdTypeId.DataSource = idTypes.ToList();
                _cmbIdTypeId.DisplayMember = "Display";
                _cmbIdTypeId.ValueMember = "Id";

                _cmbIdTypePassport.DataSource = idTypes.ToList();
                _cmbIdTypePassport.DisplayMember = "Display";
                _cmbIdTypePassport.ValueMember = "Id";

                var userTypes = _posMetadataService.GetUserTypes().ToList();
                _cmbUserTypeCitizen.DataSource = userTypes.ToList();
                _cmbUserTypeCitizen.DisplayMember = "Display";
                _cmbUserTypeCitizen.ValueMember = "Id";

                _cmbUserTypeTourist.DataSource = userTypes.ToList();
                _cmbUserTypeTourist.DisplayMember = "Display";
                _cmbUserTypeTourist.ValueMember = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Cannot load POS metadata from Access database.\n\n" +
                    "Check that the POS DB path is correct and the file exists.\n\n" +
                    "Details: " + ex.Message,
                    "PayPulse – Settings",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }

            _cmbUsd.SelectedValue = s.PosMappings.CurrencyUsdId;
            _cmbEur.SelectedValue = s.PosMappings.CurrencyEurId;
            _cmbIls.SelectedValue = s.PosMappings.CurrencyIlsId;
            _cmbIdTypeId.SelectedValue = s.PosMappings.IdTypeId;
            _cmbIdTypePassport.SelectedValue = s.PosMappings.IdTypePassport;
            _cmbUserTypeCitizen.SelectedValue = s.PosMappings.UserTypeCitizen;
            _cmbUserTypeTourist.SelectedValue = s.PosMappings.UserTypeTourist;


            // Behavior
            _chkAutoStart.Checked = s.AutoStartOnBoot;

            if (s.AutoUpdate == null)
            {
                s.AutoUpdate = new AutoUpdateSettings();
            }

            _chkCheckUpdates.Checked = s.AutoUpdate.CheckOnStartup;
            var hours = s.AutoUpdate.CheckIntervalHours <= 0 ? 4 : s.AutoUpdate.CheckIntervalHours;
            if (hours < 1) hours = 1;
            if (hours > 168) hours = 168;
            _numUpdateInterval.Value = hours;
        }

        private void SaveSettings()
        {
            try
            {
                var s = _settingsService.Load() ?? new AppSettings();
                s.Databases.SyncDbPath = _txtSyncDb.Text.Trim();
                s.Databases.PosDbPath = _txtPosDb.Text.Trim();
                s.Databases.AppSettingsPath = _txtAppSettings.Text.Trim();
                s.Databases.PosType = _cmbPosType.SelectedItem == null ? null : _cmbPosType.SelectedItem.ToString();
                s.Databases.PosFoxProFolder = _txtPosFox.Text.Trim();

                s.PosMappings.CurrencyUsdId = _cmbUsd.SelectedValue == null ? null : _cmbUsd.SelectedValue.ToString();
                s.PosMappings.CurrencyEurId = _cmbEur.SelectedValue == null ? null : _cmbEur.SelectedValue.ToString();
                s.PosMappings.CurrencyIlsId = _cmbIls.SelectedValue == null ? null : _cmbIls.SelectedValue.ToString();
                s.PosMappings.IdTypeId = _cmbIdTypeId.SelectedValue == null ? null : _cmbIdTypeId.SelectedValue.ToString();
                s.PosMappings.IdTypePassport = _cmbIdTypePassport.SelectedValue == null ? null : _cmbIdTypePassport.SelectedValue.ToString();
                s.PosMappings.UserTypeCitizen = _cmbUserTypeCitizen.SelectedValue == null ? null : _cmbUserTypeCitizen.SelectedValue.ToString();
                s.PosMappings.UserTypeTourist = _cmbUserTypeTourist.SelectedValue == null ? null : _cmbUserTypeTourist.SelectedValue.ToString();

                // Behavior
                s.AutoStartOnBoot = _chkAutoStart.Checked;

                if (s.AutoUpdate == null)
                {
                    s.AutoUpdate = new AutoUpdateSettings();
                }

                s.AutoUpdate.CheckOnStartup = _chkCheckUpdates.Checked;
                s.AutoUpdate.CheckIntervalHours = (int)_numUpdateInterval.Value;

                // Apply auto-start to Windows registry
                if (_autoStartService != null)
                {
                    if (s.AutoStartOnBoot)
                        _autoStartService.Enable();
                    else
                        _autoStartService.Disable();
                }

                _settingsService.Save(s);
                MessageBox.Show("Settings saved.", "PayPulse – Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"Error saving settings:\r\n\r\n" + ex.Message,
                    "PayPulse – Settings",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }


        private void BrowseFolder(TextBox txt)
        {
            using (var dlg = new FolderBrowserDialog())
            {
                if (!string.IsNullOrWhiteSpace(txt.Text))
                {
                    try
                    {
                        dlg.SelectedPath = txt.Text;
                    }
                    catch
                    {
                        // ignore invalid paths
                    }
                }

                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    txt.Text = dlg.SelectedPath;
                }
            }
        }

        private void BrowseFile(TextBox txt, string filter)
        {
            using (var dlg = new OpenFileDialog { Filter = filter })
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    txt.Text = dlg.FileName;
                }
            }
        }
    }
}
