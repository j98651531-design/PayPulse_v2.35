using System;
using System.Windows.Forms;
using PayPulse.Domain.Entities;

namespace PayPulse.WinForms
{
    public class UserEditForm : Form
    {
        private readonly AppUser _user;
        private readonly bool _isNew;
        private readonly string _originalPasswordHash;

        private TextBox _txtUserName;
        private TextBox _txtDisplayName;
        private TextBox _txtPassword;
        private ComboBox _cmbRole;
        private CheckBox _chkActive;
        private Button _btnOk;
        private Button _btnCancel;

        public UserEditForm(AppUser user, bool isNew)
        {
            _user = user ?? throw new ArgumentNullException(nameof(user));
            _isNew = isNew;
            _originalPasswordHash = user.Password;

            InitializeUi();
            BindFromEntity();
        }

        private void InitializeUi()
        {
            Text = _isNew ? @"Add user" : @"Edit user";
            Width = 400;
            Height = 280;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            var lblUserName = new Label { Left = 15, Top = 20, Width = 100, Text = @"User name:" };
            _txtUserName = new TextBox { Left = 120, Top = 18, Width = 230 };

            var lblDisplayName = new Label { Left = 15, Top = 55, Width = 100, Text = @"Display name:" };
            _txtDisplayName = new TextBox { Left = 120, Top = 53, Width = 230 };

            var lblPassword = new Label { Left = 15, Top = 90, Width = 100, Text = @"Password:" };
            _txtPassword = new TextBox { Left = 120, Top = 88, Width = 230, UseSystemPasswordChar = true };

            var lblRole = new Label { Left = 15, Top = 125, Width = 100, Text = @"Role:" };
            _cmbRole = new ComboBox
            {
                Left = 120,
                Top = 123,
                Width = 160,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbRole.DataSource = Enum.GetValues(typeof(AppUserRole));

            _chkActive = new CheckBox { Left = 120, Top = 155, Width = 120, Text = @"Active" };

            _btnOk = new Button { Left = 190, Top = 190, Width = 75, Text = @"OK" };
            _btnOk.Click += BtnOkOnClick;

            _btnCancel = new Button { Left = 275, Top = 190, Width = 75, Text = @"Cancel" };
            _btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

            Controls.Add(lblUserName);
            Controls.Add(_txtUserName);
            Controls.Add(lblDisplayName);
            Controls.Add(_txtDisplayName);
            Controls.Add(lblPassword);
            Controls.Add(_txtPassword);
            Controls.Add(lblRole);
            Controls.Add(_cmbRole);
            Controls.Add(_chkActive);
            Controls.Add(_btnOk);
            Controls.Add(_btnCancel);
        }

        private void BindFromEntity()
        {
            _txtUserName.Text = _user.UserName;
            _txtDisplayName.Text = _user.DisplayName;
            // Never show the hashed password in UI. Leave empty unless setting a new one.
            _txtPassword.Text = string.Empty;
            _cmbRole.SelectedItem = _user.Role;
            _chkActive.Checked = _user.IsActive;
        }

        private void BtnOkOnClick(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_txtUserName.Text))
            {
                MessageBox.Show(
                    @"User name is required.",
                    @"PayPulse – User",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                _txtUserName.Focus();
                return;
            }

            var newPassword = _txtPassword.Text;

            // Password rules:
            //  - NEW user: password is mandatory and must be complex.
            //  - EXISTING user: empty textbox = keep existing hash.
            //    Non-empty textbox = new password (must be complex; will be hashed in repository).
            if (_isNew && string.IsNullOrWhiteSpace(newPassword))
            {
                MessageBox.Show(
                    @"Password is required.",
                    @"PayPulse – User",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                _txtPassword.Focus();
                return;
            }

            if (!string.IsNullOrWhiteSpace(newPassword) && !IsPasswordComplex(newPassword))
            {
                MessageBox.Show(
                    @"Password must be at least 8 characters and contain both letters and digits.",
                    @"PayPulse – User",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                _txtPassword.Focus();
                return;
            }

            _user.UserName = _txtUserName.Text.Trim();
            _user.DisplayName = string.IsNullOrWhiteSpace(_txtDisplayName.Text)
                ? null
                : _txtDisplayName.Text.Trim();

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                // Keep existing hash for existing user
                _user.Password = _originalPasswordHash;
                // Do not change MustChangePassword flag here; admin can toggle via other flows
            }
            else
            {
                // Store the plain text; repository will hash it.
                _user.Password = newPassword;
                // Force user to change password on first login with this temp password.
                _user.MustChangePassword = true;
            }

            _user.Role = (AppUserRole)_cmbRole.SelectedItem;
            _user.IsActive = _chkActive.Checked;

            DialogResult = DialogResult.OK;
        }

        private static bool IsPasswordComplex(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 8)
            {
                return false;
            }

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
