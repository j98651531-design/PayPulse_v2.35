using System;
using System.Windows.Forms;
using PayPulse.Core.DTOs;
using PayPulse.Core.Services;
using PayPulse.Domain.Entities;
using PayPulse.Domain.Interfaces.Repositories;

namespace PayPulse.WinForms
{
    public class ReportsForm : Form
    {
        private readonly ReportsQueryService _reportsService;
        private readonly ProfileAuthService _profileAuthService;
        private readonly IProfileRepository _profileRepository;

        private ComboBox _profileCombo;

        private DateTimePicker _dtFrom;
        private DateTimePicker _dtTo;
        private Button _btnToday;
        private Button _btnYesterday;
        private Button _btnLast7;
        private Button _btnThisMonth;
        private Button _btnShow;
        private Button _btnClose;
        private DataGridView _grid;
        private BindingSource _binding;

        public ReportsForm(
            ReportsQueryService reportsService,
            ProfileAuthService profileAuthService,
            IProfileRepository profileRepository)
        {
            _reportsService = reportsService;
            _profileAuthService = profileAuthService;
            _profileRepository = profileRepository;

            InitializeUi();
            LoadProfiles();
            SetRangeToday();
        }

        private void InitializeUi()
        {
            Text = "PayPulse – Reports";
            Width = 900;
            Height = 600;
            StartPosition = FormStartPosition.CenterParent;

            var top = new Panel { Dock = DockStyle.Top, Height = 70 };
            Controls.Add(top);

            var lblProfile = new Label { Left = 10, Top = 10, Text = "Profile:", Width = 60 };
            _profileCombo = new ComboBox { Left = 70, Top = 8, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            top.Controls.Add(lblProfile);
            top.Controls.Add(_profileCombo);

            _dtFrom = new DateTimePicker { Left = 290, Top = 8, Width = 140, Format = DateTimePickerFormat.Short };
            _dtTo = new DateTimePicker { Left = 440, Top = 8, Width = 140, Format = DateTimePickerFormat.Short };
            top.Controls.Add(_dtFrom);
            top.Controls.Add(_dtTo);

            _btnToday = new Button { Left = 600, Top = 8, Width = 70, Text = "Today" };
            _btnYesterday = new Button { Left = 680, Top = 8, Width = 80, Text = "Yesterday" };
            _btnLast7 = new Button { Left = 770, Top = 8, Width = 80, Text = "Last 7" };
            _btnThisMonth = new Button { Left = 860, Top = 8, Width = 90, Text = "This month" };
            _btnToday.Click += (s, e) => SetRangeToday();
            _btnYesterday.Click += (s, e) => SetRangeYesterday();
            _btnLast7.Click += (s, e) => SetRangeLast7();
            _btnThisMonth.Click += (s, e) => SetRangeThisMonth();
            top.Controls.Add(_btnToday);
            top.Controls.Add(_btnYesterday);
            top.Controls.Add(_btnLast7);
            top.Controls.Add(_btnThisMonth);

            _btnShow = new Button { Left = 10, Top = 38, Width = 80, Text = "Show" };
            _btnClose = new Button { Left = 100, Top = 38, Width = 80, Text = "Close" };
            _btnShow.Click += BtnShow_Click;
            _btnClose.Click += (s, e) => Close();
            top.Controls.Add(_btnShow);
            top.Controls.Add(_btnClose);

            _grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true };
            _binding = new BindingSource();
            _grid.DataSource = _binding;
            Controls.Add(_grid);
        }

        private void LoadProfiles()
        {
            try
            {
                var list = _profileRepository.GetAll();
                _profileCombo.DataSource = list;
                _profileCombo.DisplayMember = "Name";
                _profileCombo.ValueMember = "ProfileId";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Cannot load profiles for reports." +
                    "Details: " + ex.Message,
                    "PayPulse – Reports",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void SetRangeToday()
        {
            DateTime today = DateTime.Today;
            _dtFrom.Value = today;
            _dtTo.Value = today;
        }

        private void SetRangeYesterday()
        {
            DateTime today = DateTime.Today;
            _dtFrom.Value = today.AddDays(-1);
            _dtTo.Value = today.AddDays(-1);
        }

        private void SetRangeLast7()
        {
            DateTime today = DateTime.Today;
            _dtFrom.Value = today.AddDays(-6);
            _dtTo.Value = today;
        }

        private void SetRangeThisMonth()
        {
            DateTime today = DateTime.Today;
            var first = new DateTime(today.Year, today.Month, 1);
            var last = first.AddMonths(1).AddDays(-1);
            _dtFrom.Value = first;
            _dtTo.Value = last;
        }

        private void BtnShow_Click(object sender, EventArgs e)
        {
            try
            {
                var profile = _profileCombo.SelectedItem as Profile;
                if (profile == null)
                {
                    MessageBox.Show("Please select a profile first.",
                        "PayPulse – Reports", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                bool needsCredentials;
                string token = _profileAuthService.EnsureToken(profile, out needsCredentials);
                if (needsCredentials || string.IsNullOrWhiteSpace(token))
                {
                    using (var dlg = new ProfileLoginForm(_profileAuthService, profile))
                    {
                        if (dlg.ShowDialog(this) != DialogResult.OK)
                        {
                            return;
                        }
                        token = dlg.Token;
                    }
                }

                var range = new DateRangeRequestDto
                {
                    Start = _dtFrom.Value.Date,
                    End = _dtTo.Value.Date.AddDays(1).AddSeconds(-1)
                };

                var data = _reportsService.GetReportForToken(range, token);
                _binding.DataSource = data;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Report error:\n\n" + ex.Message,
                    "PayPulse – Reports",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
