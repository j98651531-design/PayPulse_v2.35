using System;
using System.Windows.Forms;
using PayPulse.Core.Services;
using PayPulse.Domain.Entities;
using PayPulse.Domain.Interfaces.Repositories;

namespace PayPulse.WinForms
{
    public class LoginForm : Form
    {
        private readonly IAppUserRepository _userRepository;
        private readonly CurrentUserContext _currentUserContext;

        private TextBox _txtUserName;
        private TextBox _txtPassword;
        private Button _btnLogin;
        private Button _btnCancel;

        public LoginForm(IAppUserRepository userRepository, CurrentUserContext currentUserContext)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _currentUserContext = currentUserContext ?? throw new ArgumentNullException(nameof(currentUserContext));

            InitializeUi();
        }

        private void InitializeUi()
        {
            Text = "PayPulse – Login";
            Width = 360;
            Height = 190;
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            var lblUser = new Label { Left = 20, Top = 22, Width = 80, Text = "User name:" };
            var lblPwd = new Label { Left = 20, Top = 57, Width = 80, Text = "Password:" };

            _txtUserName = new TextBox { Left = 110, Top = 20, Width = 200 };
            _txtPassword = new TextBox { Left = 110, Top = 53, Width = 200, UseSystemPasswordChar = true };

            _btnLogin = new Button { Left = 110, Top = 100, Width = 90, Text = "Login" };
            _btnCancel = new Button { Left = 220, Top = 100, Width = 90, Text = "Cancel" };

            _btnLogin.Click += BtnLogin_Click;
            _btnCancel.Click += (s, e) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };

            Controls.Add(lblUser);
            Controls.Add(_txtUserName);
            Controls.Add(lblPwd);
            Controls.Add(_txtPassword);
            Controls.Add(_btnLogin);
            Controls.Add(_btnCancel);
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            var userName = _txtUserName.Text.Trim();
            var password = _txtPassword.Text;

            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show(
                    "Please enter user name and password.",
                    "PayPulse – Login",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var user = _userRepository.GetByUserName(userName);
                if (user == null || !user.IsActive)
                {
                    MessageBox.Show(
                        "Invalid user name or password.",
                        "PayPulse – Login",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                // BCrypt verification
                var ok = BCrypt.Net.BCrypt.Verify(password, user.Password);
                if (!ok)
                {
                    MessageBox.Show(
                        "Invalid user name or password.",
                        "PayPulse – Login",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                // Forced password change on first login (or when flag is set)
                if (user.MustChangePassword)
                {
                    using (var dlg = new ChangePasswordForm(user.UserName))
                    {
                        var result = dlg.ShowDialog(this);
                        if (result != DialogResult.OK)
                        {
                            // User cancelled password change – do not log in.
                            return;
                        }

                        _userRepository.UpdatePassword(user.UserId, dlg.NewPassword, clearMustChangePassword: true);
                        // Reload to get fresh flags / timestamps
                        user = _userRepository.GetByUserName(userName) ?? user;
                    }
                }

                // Audit last login
                _userRepository.UpdateLastLogin(user.UserId, DateTime.UtcNow);

                _currentUserContext.Set(user);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    @"Error during login:\r\n\r\n" + ex.Message,
                    "PayPulse – Login",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
