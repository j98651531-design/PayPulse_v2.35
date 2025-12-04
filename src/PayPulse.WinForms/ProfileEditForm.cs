using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using PayPulse.Domain.Entities;

namespace PayPulse.WinForms
{
    public class ProfileEditForm : Form
    {
        private readonly Profile _profile;
        private readonly IList<UserRef> _users;
        private readonly IList<CashboxRef> _cashboxes;

        private TextBox _txtName;
        private ComboBox _cmbProvider;
        private CheckBox _chkActive;
        private TextBox _txtManualToken;
        private TextBox _txtLoginEmailOrPhone;
        private TextBox _txtPassword;
        private TextBox _txtTotpSecret;
        private ComboBox _cmbPosUser;
        private ComboBox _cmbPosCashbox;
        private Button _btnOk;
        private Button _btnCancel;

        public ProfileEditForm(Profile profile, IList<UserRef> users, IList<CashboxRef> cashboxes)
        {
            _profile = profile;
            _users = users ?? new List<UserRef>();
            _cashboxes = cashboxes ?? new List<CashboxRef>();

            InitializeUi();
            LoadProfile();
        }

        private void InitializeUi()
        {
            Text = "Edit Profile";
            Width = 600;
            Height = 420;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;

            var panel = new Panel { Dock = DockStyle.Fill };
            Controls.Add(panel);

            int top = 10;
            int labelWidth = 150;
            int inputWidth = 360;

            Func<string, Control, int> addRow = (label, ctrl) =>
            {
                var lbl = new Label
                {
                    Left = 10,
                    Top = top + 4,
                    Width = labelWidth,
                    Text = label
                };
                ctrl.Left = 170;
                ctrl.Top = top;
                ctrl.Width = inputWidth;
                panel.Controls.Add(lbl);
                panel.Controls.Add(ctrl);
                top += 28;
                return top;
            };

            _txtName = new TextBox();
            addRow("Profile name", _txtName);

            _cmbProvider = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbProvider.Items.AddRange(new object[] { "STB", "GMT", "WIC" });
            addRow("Provider", _cmbProvider);

            _chkActive = new CheckBox
            {
                Left = 170,
                Top = top,
                Text = "Active"
            };
            panel.Controls.Add(_chkActive);
            top += 30;

            _txtManualToken = new TextBox
            {
                Multiline = true,
                Height = 60,
                ScrollBars = ScrollBars.Vertical
            };
            addRow("Manual JWT token", _txtManualToken);
            top += 40;

            _txtLoginEmailOrPhone = new TextBox();
            addRow("Login email / phone", _txtLoginEmailOrPhone);

            _txtPassword = new TextBox { UseSystemPasswordChar = true };
            addRow("Login password", _txtPassword);

            _txtTotpSecret = new TextBox();
            addRow("TOTP secret", _txtTotpSecret);


            _cmbPosUser = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            addRow("POS User", _cmbPosUser);

            _cmbPosCashbox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            addRow("POS Cashbox", _cmbPosCashbox);

            _btnOk = new Button { Text = "OK", Left = 170, Top = top + 10, Width = 90 };
            _btnCancel = new Button { Text = "Cancel", Left = 270, Top = top + 10, Width = 90 };
            _btnOk.Click += BtnOk_Click;
            _btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;
            panel.Controls.Add(_btnOk);
            panel.Controls.Add(_btnCancel);
        }

        private void LoadProfile()
        {
            _txtName.Text = _profile.Name;
            _cmbProvider.SelectedItem = string.IsNullOrWhiteSpace(_profile.ProviderType)
                ? "STB"
                : _profile.ProviderType;
            _chkActive.Checked = _profile.IsActive;
            _txtManualToken.Text = _profile.ManualToken ?? string.Empty;

            // Collapse email/phone into a single field for editing.
            _txtLoginEmailOrPhone.Text = !string.IsNullOrWhiteSpace(_profile.LoginEmail)
                ? _profile.LoginEmail
                : _profile.LoginPhoneNumber ?? string.Empty;

            _txtPassword.Text = _profile.LoginPassword ?? string.Empty;
            _txtTotpSecret.Text = _profile.TotpSecret ?? string.Empty;

            _cmbPosUser.DataSource = _users.ToList();
            _cmbPosUser.DisplayMember = "Display";
            _cmbPosUser.ValueMember = "Id";
            if (!string.IsNullOrWhiteSpace(_profile.PosUserId))
            {
                _cmbPosUser.SelectedValue = _profile.PosUserId;
            }

            _cmbPosCashbox.DataSource = _cashboxes.ToList();
            _cmbPosCashbox.DisplayMember = "Display";
            _cmbPosCashbox.ValueMember = "Id";
            if (!string.IsNullOrWhiteSpace(_profile.PosCashboxId))
            {
                _cmbPosCashbox.SelectedValue = _profile.PosCashboxId;
            }
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_txtName.Text))
            {
                MessageBox.Show(
                    "Profile name is required.",
                    "PayPulse – Profile",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(_txtLoginEmailOrPhone.Text))
            {
                MessageBox.Show(
                    "Login email or phone is required.",
                    "PayPulse – Profile",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            _profile.Name = _txtName.Text.Trim();
            _profile.ProviderType = _cmbProvider.SelectedItem == null
                ? "STB"
                : _cmbProvider.SelectedItem.ToString();
            _profile.IsActive = _chkActive.Checked;
            _profile.ManualToken = string.IsNullOrWhiteSpace(_txtManualToken.Text)
                ? null
                : _txtManualToken.Text.Trim();

            // Decide if it's email or phone based on presence of '@'
            var login = _txtLoginEmailOrPhone.Text.Trim();
            if (login.Contains("@"))
            {
                _profile.LoginEmail = login;
                _profile.LoginPhoneNumber = null;
            }
            else
            {
                _profile.LoginEmail = null;
                _profile.LoginPhoneNumber = login;
            }

            _profile.LoginPassword = _txtPassword.Text;
            _profile.TotpSecret = string.IsNullOrWhiteSpace(_txtTotpSecret.Text)
                ? null
                : _txtTotpSecret.Text.Trim();

            _profile.PosUserId = _cmbPosUser.SelectedValue == null
                ? null
                : _cmbPosUser.SelectedValue.ToString();
            _profile.PosCashboxId = _cmbPosCashbox.SelectedValue == null
                ? null
                : _cmbPosCashbox.SelectedValue.ToString();

            DialogResult = DialogResult.OK;
        }
    }
}
