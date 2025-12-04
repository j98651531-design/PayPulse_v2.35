using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using PayPulse.Core.Services;
using PayPulse.Domain.Entities;
using PayPulse.Domain.Interfaces.Repositories;

namespace PayPulse.WinForms
{
    /// <summary>
    /// Simple security report listing users, last login and inactive accounts.
    /// Read-only. Intended for Admins (and optionally Managers).
    /// </summary>
    public class SecurityReportForm : Form
    {
        private readonly IAppUserRepository _userRepository;
        private readonly CurrentUserContext _currentUserContext;
        private readonly BindingSource _binding = new BindingSource();
        private DataGridView _grid;
        private Button _btnRefresh;
        private Button _btnClose;
        private CheckBox _chkOnlyInactive;
        private CheckBox _chkNoLoginDays;
        private NumericUpDown _numDays;
        private Label _lblSummary;

        public SecurityReportForm(IAppUserRepository userRepository, CurrentUserContext currentUserContext)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _currentUserContext = currentUserContext ?? throw new ArgumentNullException(nameof(currentUserContext));

            if (!_currentUserContext.CanViewSecurityReport)
            {
                MessageBox.Show(
                    @"You do not have permission to view the security report.",
                    @"PayPulse – Security",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                Load += (s, e) => Close();
                return;
            }

            InitializeUi();
            LoadData();
        }

        private void InitializeUi()
        {
            Text = @"Security report – Users";
            Width = 900;
            Height = 520;
            StartPosition = FormStartPosition.CenterParent;

            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };
            _grid.DataSource = _binding;

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(SecurityUserRow.UserName),
                HeaderText = @"User name",
                Width = 120
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(SecurityUserRow.DisplayName),
                HeaderText = @"Display name",
                Width = 150
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(SecurityUserRow.Role),
                HeaderText = @"Role",
                Width = 80
            });
            _grid.Columns.Add(new DataGridViewCheckBoxColumn
            {
                DataPropertyName = nameof(SecurityUserRow.IsActive),
                HeaderText = @"Active",
                Width = 60
            });
            _grid.Columns.Add(new DataGridViewCheckBoxColumn
            {
                DataPropertyName = nameof(SecurityUserRow.MustChangePassword),
                HeaderText = @"Must change",
                Width = 90
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(SecurityUserRow.CreatedAt),
                HeaderText = @"Created",
                Width = 140,
                DefaultCellStyle = { Format = "g" }
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(SecurityUserRow.CreatedByUserName),
                HeaderText = @"Created by",
                Width = 120
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(SecurityUserRow.LastLoginAt),
                HeaderText = @"Last login",
                Width = 140,
                DefaultCellStyle = { Format = "g" }
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(SecurityUserRow.UpdatedAt),
                HeaderText = @"Updated",
                Width = 140,
                DefaultCellStyle = { Format = "g" }
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(SecurityUserRow.UpdatedByUserName),
                HeaderText = @"Updated by",
                Width = 120
            });

            // Top filter panel
            var filterPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 40,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(5)
            };

            _chkOnlyInactive = new CheckBox { Text = @"Only inactive", AutoSize = true, Checked = false };
            _chkNoLoginDays = new CheckBox { Text = @"No login X days", AutoSize = true, Checked = false };
            _numDays = new NumericUpDown { Minimum = 1, Maximum = 365, Value = 30, Width = 60 };

            filterPanel.Controls.Add(_chkOnlyInactive);
            filterPanel.Controls.Add(_chkNoLoginDays);
            filterPanel.Controls.Add(_numDays);

            // Summary panel
            var summaryPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 22,
                Padding = new Padding(5, 2, 5, 2)
            };

            _lblSummary = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };

            summaryPanel.Controls.Add(_lblSummary);

            var panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 45,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(5)
            };

            _btnClose = new Button { Text = @"Close", Width = 80 };
            _btnClose.Click += (s, e) => Close();

            _btnRefresh = new Button { Text = @"Refresh", Width = 80 };
            _btnRefresh.Click += (s, e) => LoadData();

            panel.Controls.Add(_btnClose);
            panel.Controls.Add(_btnRefresh);

            Controls.Add(_grid);
            Controls.Add(panel);
            Controls.Add(summaryPanel);
            Controls.Add(filterPanel);
        }


        private void LoadData()
        {
            try
            {
                var users = _userRepository.GetAll();
                var nameById = users.ToDictionary(u => u.UserId, u => u.UserName);

                var rows = new List<SecurityUserRow>();
                foreach (var u in users)
                {
                    nameById.TryGetValue(u.CreatedByUserId ?? string.Empty, out var createdBy);
                    nameById.TryGetValue(u.UpdatedByUserId ?? string.Empty, out var updatedBy);

                    var row = new SecurityUserRow
                    {
                        UserName = u.UserName,
                        DisplayName = u.DisplayName,
                        Role = u.Role.ToString(),
                        IsActive = u.IsActive,
                        MustChangePassword = u.MustChangePassword,
                        CreatedAt = u.CreatedAt,
                        CreatedByUserName = createdBy,
                        LastLoginAt = u.LastLoginAt,
                        UpdatedAt = u.UpdatedAt,
                        UpdatedByUserName = updatedBy
                    };

                    rows.Add(row);
                }

                // Base counts (before filters)
                var total = rows.Count;
                var inactiveTotal = rows.Count(r => !r.IsActive);

                var daysForNoLogin = (int)(_numDays?.Value ?? 30);
                var cutoff = DateTime.UtcNow.AddDays(-daysForNoLogin);
                var noLoginTotal = rows.Count(r =>
                    !r.LastLoginAt.HasValue || r.LastLoginAt.Value < cutoff);

                // Apply filters
                var filtered = rows.AsEnumerable();

                if (_chkOnlyInactive != null && _chkOnlyInactive.Checked)
                {
                    filtered = filtered.Where(r => !r.IsActive);
                }

                if (_chkNoLoginDays != null && _chkNoLoginDays.Checked)
                {
                    filtered = filtered.Where(r =>
                        !r.LastLoginAt.HasValue || r.LastLoginAt.Value < cutoff);
                }

                var filteredList = filtered.ToList();
                _binding.DataSource = new BindingList<SecurityUserRow>(filteredList);

                if (_lblSummary != null)
                {
                    _lblSummary.Text =
                        string.Format(
                            "Total: {0}, Inactive: {1}, No login ≥ {2} days: {3}, Shown: {4}",
                            total,
                            inactiveTotal,
                            daysForNoLogin,
                            noLoginTotal,
                            filteredList.Count);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    @"Error loading security report:\r\n\r\n" + ex.Message,
                    @"PayPulse – Security",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }


        private class SecurityUserRow
        {
            public string UserName { get; set; }
            public string DisplayName { get; set; }
            public string Role { get; set; }
            public bool IsActive { get; set; }
            public bool MustChangePassword { get; set; }
            public DateTime CreatedAt { get; set; }
            public string CreatedByUserName { get; set; }
            public DateTime? LastLoginAt { get; set; }
            public DateTime UpdatedAt { get; set; }
            public string UpdatedByUserName { get; set; }
        }
    }
}
