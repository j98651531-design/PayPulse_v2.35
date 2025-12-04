using System;
using System.Windows.Forms;
using PayPulse.Core.Services;
using PayPulse.Domain.Entities;

namespace PayPulse.WinForms
{
    public class ProfileLoginForm : Form
    {
        private readonly ProfileAuthService _authService;
        private readonly Profile _profile;

        private TextBox _txtEmail;
        private TextBox _txtPhone;
        private TextBox _txtPassword;
        private TextBox _txtOtp;
        private TextBox _txtTotpSecret;
        private Button _btnLogin;
        private Button _btnCancel;

        public string Token { get; private set; }

        public ProfileLoginForm(ProfileAuthService authService, Profile profile)
        {
            _authService = authService;
            _profile = profile;

            InitializeUi();
            LoadProfile();
        }

        private void InitializeUi()
        {
            Text = "Login – " + _profile.Name;
            Width = 500;
            Height = 320;

            var lblEmail = new Label { Left = 10, Top = 20, Text = "Email", Width = 120 };
            _txtEmail = new TextBox { Left = 140, Top = 18, Width = 300 };

            var lblPhone = new Label { Left = 10, Top = 50, Text = "Phone", Width = 120 };
            _txtPhone = new TextBox { Left = 140, Top = 48, Width = 300 };

            var lblPwd = new Label { Left = 10, Top = 80, Text = "Password", Width = 120 };
            _txtPassword = new TextBox { Left = 140, Top = 78, Width = 300, UseSystemPasswordChar = true };

            var lblTotp = new Label { Left = 10, Top = 110, Text = "TOTP Secret (optional)", Width = 120 };
            _txtTotpSecret = new TextBox { Left = 140, Top = 108, Width = 300 };

            var lblOtp = new Label { Left = 10, Top = 140, Text = "OTP code", Width = 120 };
            _txtOtp = new TextBox { Left = 140, Top = 138, Width = 150 };

            _btnLogin = new Button { Left = 140, Top = 190, Width = 90, Text = "Login" };
            _btnCancel = new Button { Left = 240, Top = 190, Width = 90, Text = "Cancel" };

            _btnLogin.Click += BtnLogin_Click;
            _btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

            Controls.AddRange(new Control[]
            {
                lblEmail, _txtEmail,
                lblPhone, _txtPhone,
                lblPwd, _txtPassword,
                lblTotp, _txtTotpSecret,
                lblOtp, _txtOtp,
                _btnLogin, _btnCancel
            });
        }

        private void LoadProfile()
        {
            _txtEmail.Text = _profile.LoginEmail;
            _txtPhone.Text = _profile.LoginPhoneNumber;
            _txtPassword.Text = _profile.LoginPassword;
            _txtTotpSecret.Text = _profile.TotpSecret;
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                _profile.LoginEmail = _txtEmail.Text.Trim();
                _profile.LoginPhoneNumber = _txtPhone.Text.Trim();
                _profile.LoginPassword = _txtPassword.Text;
                _profile.TotpSecret = _txtTotpSecret.Text.Trim();

                string otp = _txtOtp.Text.Trim();
                if (string.IsNullOrWhiteSpace(otp))
                {
                    MessageBox.Show("Please enter OTP code (from SMS or authenticator app).",
                        "PayPulse – Login", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Token = _authService.LoginWithUserProvidedOtp(_profile, otp);
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Login error:\n\n" + ex.Message,
                    "PayPulse – Login",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
