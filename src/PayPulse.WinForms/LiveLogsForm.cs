using System;
using System.ComponentModel;
using System.Windows.Forms;
using PayPulse.Domain.Entities;
using PayPulse.Domain.Logging;

namespace PayPulse.WinForms
{
    public class LiveLogsForm : Form
    {
        private readonly BindingList<LogEntry> _items = new BindingList<LogEntry>();
        private DataGridView _grid;
        private CheckBox _chkAutoScroll;

        public LiveLogsForm()
        {
            InitializeUi();
        }

        private void InitializeUi()
        {
            Text = "Live Logs";
            Width = 1000;
            Height = 500;

            _chkAutoScroll = new CheckBox
            {
                Text = "Auto scroll",
                Dock = DockStyle.Top,
                Checked = true
            };

            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false
            };

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Time (UTC)",
                DataPropertyName = nameof(LogEntry.TimestampUtc),
                Width = 150
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Level",
                DataPropertyName = nameof(LogEntry.Level),
                Width = 60
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Profile",
                DataPropertyName = nameof(LogEntry.ProfileId),
                Width = 120
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Operation",
                DataPropertyName = nameof(LogEntry.Operation),
                Width = 160
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "User",
                DataPropertyName = nameof(LogEntry.UserName),
                Width = 140
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Message",
                DataPropertyName = nameof(LogEntry.Message),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            _grid.DataSource = _items;

            Controls.Add(_grid);
            Controls.Add(_chkAutoScroll);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LogEventHub.LogAdded += OnLogAdded;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            LogEventHub.LogAdded -= OnLogAdded;
            base.OnFormClosed(e);
        }

        private void OnLogAdded(object sender, LogEntry e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<object, LogEntry>(OnLogAdded), sender, e);
                return;
            }

            _items.Add(e);
            if (_chkAutoScroll.Checked && _grid.Rows.Count > 0)
            {
                var lastIndex = _grid.Rows.Count - 1;
                _grid.FirstDisplayedScrollingRowIndex = lastIndex;
            }
        }
    }
}
