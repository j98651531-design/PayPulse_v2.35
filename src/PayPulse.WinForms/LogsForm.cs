using System;
using System.Linq;
using System.Windows.Forms;
using PayPulse.Domain.Interfaces.Repositories;

namespace PayPulse.WinForms
{
    public class LogsForm : Form
    {
        private readonly ILogRepository _logRepository;

        private DateTimePicker _dtFrom;
        private DateTimePicker _dtTo;
        private ComboBox _cmbLevel;
        private TextBox _txtProfileId;
        private TextBox _txtCorrelationId;
        private TextBox _txtUserName;
        private Button _btnRefresh;
        private Button _btnClose;
        private DataGridView _grid;
        private BindingSource _binding;

        public LogsForm(ILogRepository logRepository)
        {
            _logRepository = logRepository;
            InitializeUi();
        }

        public LogsForm(ILogRepository logRepository, string profileId, DateTime? fromUtc, DateTime? toUtc)
            : this(logRepository)
        {
            if (fromUtc.HasValue)
            {
                _dtFrom.Value = fromUtc.Value.ToLocalTime();
            }

            if (toUtc.HasValue)
            {
                _dtTo.Value = toUtc.Value.ToLocalTime();
            }

            if (!string.IsNullOrWhiteSpace(profileId))
            {
                _txtProfileId.Text = profileId;
            }

            BtnRefresh_Click(this, EventArgs.Empty);
        }


        private void InitializeUi()
        {
            Text = "Logs";
            Width = 1000;
            Height = 600;

            var top = new Panel { Dock = DockStyle.Top, Height = 80 };
            Controls.Add(top);

            var lblFrom = new Label { Left = 10, Top = 10, Width = 40, Text = "From" };
            var lblTo = new Label { Left = 170, Top = 10, Width = 25, Text = "To" };
            top.Controls.Add(lblFrom);
            top.Controls.Add(lblTo);

            _dtFrom = new DateTimePicker { Left = 10, Top = 30, Width = 150, Format = DateTimePickerFormat.Short };
            _dtTo = new DateTimePicker { Left = 170, Top = 30, Width = 150, Format = DateTimePickerFormat.Short };
            top.Controls.Add(_dtFrom);
            top.Controls.Add(_dtTo);

            _cmbLevel = new ComboBox { Left = 330, Top = 30, Width = 100, DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbLevel.Items.AddRange(new object[] { "", "INFO", "WARN", "ERROR" });
            top.Controls.Add(_cmbLevel);

            var lblProfile = new Label { Left = 450, Top = 10, Width = 60, Text = "ProfileId" };
            _txtProfileId = new TextBox { Left = 450, Top = 30, Width = 120 };
            top.Controls.Add(lblProfile);
            top.Controls.Add(_txtProfileId);

            var lblCorr = new Label { Left = 580, Top = 10, Width = 90, Text = "CorrelationId" };
            _txtCorrelationId = new TextBox { Left = 580, Top = 30, Width = 150 };
            top.Controls.Add(lblCorr);
            top.Controls.Add(_txtCorrelationId);

            var lblUser = new Label { Left = 740, Top = 10, Width = 80, Text = "User" };
            _txtUserName = new TextBox { Left = 740, Top = 30, Width = 120 };
            top.Controls.Add(lblUser);
            top.Controls.Add(_txtUserName);

            _btnRefresh = new Button { Left = 740, Top = 30, Width = 80, Text = "Refresh" };
            _btnClose = new Button { Left = 830, Top = 30, Width = 80, Text = "Close" };
            _btnRefresh.Click += BtnRefresh_Click;
            _btnClose.Click += (s, e) => Close();
            top.Controls.Add(_btnRefresh);
            top.Controls.Add(_btnClose);

            _grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true };
            _binding = new BindingSource();
            _grid.DataSource = _binding;
            Controls.Add(_grid);

            _dtFrom.Value = DateTime.Today;
            _dtTo.Value = DateTime.Today;
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                var fromUtc = _dtFrom.Value.Date.ToUniversalTime();
                var toUtc = _dtTo.Value.Date.AddDays(1).AddSeconds(-1).ToUniversalTime();
                var logs = _logRepository.Get(fromUtc, toUtc);

                var level = _cmbLevel.SelectedItem == null ? string.Empty : _cmbLevel.SelectedItem.ToString();
                if (!string.IsNullOrWhiteSpace(level))
                {
                    logs = logs.Where(l => string.Equals(l.Level, level, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                var profileId = _txtProfileId.Text.Trim();
                if (!string.IsNullOrWhiteSpace(profileId))
                {
                    logs = logs.Where(l => string.Equals(l.ProfileId, profileId, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                var corr = _txtCorrelationId.Text.Trim();
                if (!string.IsNullOrWhiteSpace(corr))
                {
                    logs = logs.Where(l => string.Equals(l.CorrelationId, corr, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                _binding.DataSource = logs;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    @"Error loading logs:\r\n\r\n" + ex.Message,
                    "PayPulse – Logs",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
