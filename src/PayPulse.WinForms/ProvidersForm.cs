using System;
using System.Windows.Forms;
using PayPulse.Core.Services;
using PayPulse.Domain.Settings;

namespace PayPulse.WinForms
{
    public class ProvidersForm : Form
    {
        private readonly SettingsService _settingsService;

        private TabControl _tabs;

        private TextBox _txtStbBase;
        private TextBox _txtStbAuth;
        private TextBox _txtStbTransfers;
        private TextBox _txtStbDetails;
        private TextBox _txtStbFind;

        private TextBox _txtGmtBase;
        private TextBox _txtGmtAuth;
        private TextBox _txtGmtTransfers;
        private TextBox _txtGmtDetails;
        private TextBox _txtGmtFind;

        private TextBox _txtWicBase;
        private TextBox _txtWicAuth;
        private TextBox _txtWicTransfers;
        private TextBox _txtWicDetails;
        private TextBox _txtWicFind;

        private Button _btnSave;
        private Button _btnClose;

        public ProvidersForm(SettingsService settingsService)
        {
            _settingsService = settingsService;
            InitializeUi();
            LoadSettings();
        }

        private void InitializeUi()
        {
            Text = "Providers";
            Width = 700;
            Height = 450;
            StartPosition = FormStartPosition.CenterParent;

            _tabs = new TabControl { Dock = DockStyle.Fill };
            Controls.Add(_tabs);

            _tabs.TabPages.Add(CreateProviderPage(
                "STB",
                out _txtStbBase,
                out _txtStbAuth,
                out _txtStbTransfers,
                out _txtStbDetails,
                out _txtStbFind));

            _tabs.TabPages.Add(CreateProviderPage(
                "GMT",
                out _txtGmtBase,
                out _txtGmtAuth,
                out _txtGmtTransfers,
                out _txtGmtDetails,
                out _txtGmtFind));

            _tabs.TabPages.Add(CreateProviderPage(
                "WIC",
                out _txtWicBase,
                out _txtWicAuth,
                out _txtWicTransfers,
                out _txtWicDetails,
                out _txtWicFind));

            var bottom = new Panel { Dock = DockStyle.Bottom, Height = 40 };
            _btnSave = new Button { Text = "Save", Left = 10, Top = 8, Width = 80 };
            _btnClose = new Button { Text = "Close", Left = 100, Top = 8, Width = 80 };
            _btnSave.Click += BtnSave_Click;
            _btnClose.Click += (s, e) => Close();
            bottom.Controls.Add(_btnSave);
            bottom.Controls.Add(_btnClose);
            Controls.Add(bottom);
        }

        private TabPage CreateProviderPage(
            string title,
            out TextBox txtBase,
            out TextBox txtAuth,
            out TextBox txtTransfers,
            out TextBox txtDetails,
            out TextBox txtFind)
        {
            var page = new TabPage(title);
            var panel = new Panel { Dock = DockStyle.Fill };
            page.Controls.Add(panel);

            int top = 10;
            int labelWidth = 140;
            int inputWidth = 450;

            Func<string, TextBox> addRow = label =>
            {
                var lbl = new Label
                {
                    Left = 10,
                    Top = top + 4,
                    Width = labelWidth,
                    Text = label
                };
                var tb = new TextBox
                {
                    Left = 160,
                    Top = top,
                    Width = inputWidth
                };
                panel.Controls.Add(lbl);
                panel.Controls.Add(tb);
                top += 28;
                return tb;
            };

            txtBase = addRow("Base URL");
            txtAuth = addRow("Auth URL");
            txtTransfers = addRow("Transfers URL");
            txtDetails = addRow("Details URL");
            txtFind = addRow("Find customer URL");

            return page;
        }

        private void LoadSettings()
        {
            var s = _settingsService.Load() ?? new AppSettings();
            var p = s.Providers ?? new ProvidersSettings();

            _txtStbBase.Text = p.STB.BaseUrl;
            _txtStbAuth.Text = p.STB.AuthUrl;
            _txtStbTransfers.Text = p.STB.TransfersUrl;
            _txtStbDetails.Text = p.STB.TransferDetailsUrl;
            _txtStbFind.Text = p.STB.FindCustomerUrl;

            _txtGmtBase.Text = p.GMT.BaseUrl;
            _txtGmtAuth.Text = p.GMT.AuthUrl;
            _txtGmtTransfers.Text = p.GMT.TransfersUrl;
            _txtGmtDetails.Text = p.GMT.TransferDetailsUrl;
            _txtGmtFind.Text = p.GMT.FindCustomerUrl;

            _txtWicBase.Text = p.WIC.BaseUrl;
            _txtWicAuth.Text = p.WIC.AuthUrl;
            _txtWicTransfers.Text = p.WIC.TransfersUrl;
            _txtWicDetails.Text = p.WIC.TransferDetailsUrl;
            _txtWicFind.Text = p.WIC.FindCustomerUrl;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                var s = _settingsService.Load() ?? new AppSettings();
                var p = s.Providers ?? new ProvidersSettings();

                p.STB.BaseUrl = _txtStbBase.Text.Trim();
                p.STB.AuthUrl = _txtStbAuth.Text.Trim();
                p.STB.TransfersUrl = _txtStbTransfers.Text.Trim();
                p.STB.TransferDetailsUrl = _txtStbDetails.Text.Trim();
                p.STB.FindCustomerUrl = _txtStbFind.Text.Trim();

                p.GMT.BaseUrl = _txtGmtBase.Text.Trim();
                p.GMT.AuthUrl = _txtGmtAuth.Text.Trim();
                p.GMT.TransfersUrl = _txtGmtTransfers.Text.Trim();
                p.GMT.TransferDetailsUrl = _txtGmtDetails.Text.Trim();
                p.GMT.FindCustomerUrl = _txtGmtFind.Text.Trim();

                p.WIC.BaseUrl = _txtWicBase.Text.Trim();
                p.WIC.AuthUrl = _txtWicAuth.Text.Trim();
                p.WIC.TransfersUrl = _txtWicTransfers.Text.Trim();
                p.WIC.TransferDetailsUrl = _txtWicDetails.Text.Trim();
                p.WIC.FindCustomerUrl = _txtWicFind.Text.Trim();

                s.Providers = p;
                _settingsService.Save(s);

                MessageBox.Show(
                    "Provider settings saved.",
                    "PayPulse – Providers",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    @"Error saving provider settings:

" + ex.Message,
                    "PayPulse – Providers",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
