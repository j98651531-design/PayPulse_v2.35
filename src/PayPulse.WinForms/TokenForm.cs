using System;
using System.Windows.Forms;
using PayPulse.Core.Services;

namespace PayPulse.WinForms
{
    public class TokenForm : Form
    {
        private readonly TokenService _tokenService;

        private RichTextBox _txtToken;
        private Button _btnSave;
        private Button _btnClear;
        private Button _btnClose;

        public TokenForm(TokenService tokenService)
        {
            _tokenService = tokenService;
            InitializeUi();
        }

        private void InitializeUi()
        {
            Text = "Global JWT Token";
            Width = 600;
            Height = 400;

            _txtToken = new RichTextBox { Dock = DockStyle.Fill };
            Controls.Add(_txtToken);

            var bottom = new Panel { Dock = DockStyle.Bottom, Height = 40 };
            _btnSave = new Button { Text = "Save", Left = 10, Top = 8, Width = 80 };
            _btnClear = new Button { Text = "Clear", Left = 100, Top = 8, Width = 80 };
            _btnClose = new Button { Text = "Close", Left = 190, Top = 8, Width = 80 };

            _btnSave.Click += (s, e) => SaveToken();
            _btnClear.Click += (s, e) => _txtToken.Text = string.Empty;
            _btnClose.Click += (s, e) => Close();
            bottom.Controls.AddRange(new Control[] { _btnSave, _btnClear, _btnClose });

            Controls.Add(bottom);

            _txtToken.Text = _tokenService.GetGlobalToken();
        }

        private void SaveToken()
        {
            try
            {
                _tokenService.SaveGlobalToken(_txtToken.Text.Trim());
                MessageBox.Show("Token saved.", "PayPulse – Token", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"Error saving token:\r\n\r\n" + ex.Message,
                    "PayPulse – Token",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
