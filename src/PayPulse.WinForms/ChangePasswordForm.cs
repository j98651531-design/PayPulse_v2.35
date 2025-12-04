using System;
using System.Windows.Forms;

namespace PayPulse.WinForms
{
    /// <summary>
    /// Simple dialog used during first login / forced password change.
    /// Only sets a new password for the current user.
    /// </summary>
    public class ChangePasswordForm : Form
    {
        private readonly string _userName;

        private TextBox _txtNewPassword;
        private TextBox _txtConfirmPassword;
        private Button _btnOk;
        private Button _btnCancel;

        public string NewPassword { get; private set; }

        public ChangePasswordForm(string userName)
        {
            _userName = userName ?? throw new ArgumentNullException(nameof(userName));
            InitializeUi();
        }

        private void InitializeUi()
        {
            Text = $"Change password – {_userName}";
            Width = 420;
            Height = 230;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            var lblNew = new Label { Left = 20, Top = 25, Width = 140, Text = "New password:" };
            var lblConfirm = new Label { Left = 20, Top = 70, Width = 140, Text = "Confirm password:" };

            _txtNewPassword = new TextBox { Left = 170, Top = 22, Width = 200, UseSystemPasswordChar = true };
            _txtConfirmPassword = new TextBox { Left = 170, Top = 67, Width = 200, UseSystemPasswordChar = true };

            _btnOk = new Button { Left = 170, Top = 120, Width = 90, Text = "OK" };
            _btnCancel = new Button { Left = 280, Top = 120, Width = 90, Text = "Cancel" };

            _btnOk.Click += BtnOkOnClick;
            _btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            Controls.Add(lblNew);
            Controls.Add(lblConfirm);
            Controls.Add(_txtNewPassword);
            Controls.Add(_txtConfirmPassword);
            Controls.Add(_btnOk);
            Controls.Add(_btnCancel);
        }

        private void BtnOkOnClick(object sender, EventArgs e)
        {
            var pwd = _txtNewPassword.Text;
            var confirm = _txtConfirmPassword.Text;

            if (string.IsNullOrWhiteSpace(pwd))
            {
                MessageBox.Show("Password is required.", "PayPulse – Password", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtNewPassword.Focus();
                return;
            }

            if (!UserEditForm_IsPasswordComplex(pwd))
            {
                MessageBox.Show(
                    "Password must be at least 8 characters and contain both letters and digits.",
                    "PayPulse – Password",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                _txtNewPassword.Focus();
                return;
            }

            if (!string.Equals(pwd, confirm, StringComparison.Ordinal))
            {
                MessageBox.Show("Passwords do not match.", "PayPulse – Password", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtConfirmPassword.Focus();
                return;
            }

            NewPassword = pwd;
            DialogResult = DialogResult.OK;
            Close();
        }

        // Reuse same complexity rules as UserEditForm.
        private static bool UserEditForm_IsPasswordComplex(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 8)
                return false;

            bool hasLetter = false;
            bool hasDigit = false;

            foreach (var ch in password)
            {
                if (char.IsLetter(ch)) hasLetter = true;
                if (char.IsDigit(ch)) hasDigit = true;
            }

            return hasLetter && hasDigit;
        }
    }
}
