using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using PayPulse.Core.DTOs;
using PayPulse.Core.Services;
using PayPulse.Domain.Interfaces.Repositories;
using PayPulse.Domain.Interfaces.Services;

namespace PayPulse.WinForms
{
    public class SyncDashboardForm : Form
    {
        private readonly ProfileSyncDashboardService _dashboardService;
        private readonly ILogRepository _logRepository;
        private readonly IDateTimeProvider _clock;

        private readonly BindingSource _binding = new BindingSource();
        private readonly BindingSource _perfBinding = new BindingSource();
        private DataGridView _grid;
        private DataGridView _perfGrid;
        private NumericUpDown _numDays;
        private Button _btnRefresh;
        private Label _lblInfo;

        public SyncDashboardForm(ProfileSyncDashboardService dashboardService, ILogRepository logRepository, IDateTimeProvider clock)
        {
            _dashboardService = dashboardService ?? throw new ArgumentNullException(nameof(dashboardService));
            _logRepository = logRepository ?? throw new ArgumentNullException(nameof(logRepository));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));

            InitializeUi();
            LoadData();
        }

        private void InitializeUi()
        {
            Text = "Per-profile Sync Dashboard";
            Width = 1000;
            Height = 600;
            StartPosition = FormStartPosition.CenterParent;

            var top = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40
            };

            var lblDays = new Label
            {
                Left = 10,
                Top = 12,
                Width = 120,
                Text = "Lookback (days):",
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };

            _numDays = new NumericUpDown
            {
                Left = 130,
                Top = 8,
                Width = 60,
                Minimum = 1,
                Maximum = 90,
                Value = 7
            };

            _btnRefresh = new Button
            {
                Left = 210,
                Top = 7,
                Width = 80,
                Text = "Refresh"
            };
            _btnRefresh.Click += (s, e) => LoadData();

            _lblInfo = new Label
            {
                Left = 310,
                Top = 12,
                Width = 650,
                AutoSize = false,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };

            top.Controls.Add(lblDays);
            top.Controls.Add(_numDays);
            top.Controls.Add(_btnRefresh);
            top.Controls.Add(_lblInfo);

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
                DataPropertyName = nameof(ProfileSyncStatsDto.Name),
                HeaderText = "Profile",
                Width = 150
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(ProfileSyncStatsDto.ProviderType),
                HeaderText = "Provider",
                Width = 80
            });
            _grid.Columns.Add(new DataGridViewCheckBoxColumn
            {
                DataPropertyName = nameof(ProfileSyncStatsDto.IsActive),
                HeaderText = "Active",
                Width = 60
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(ProfileSyncStatsDto.HealthStatus),
                HeaderText = "Status",
                Width = 70
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(ProfileSyncStatsDto.ConsecutiveBgFailures),
                HeaderText = "BG failures",
                Width = 80
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(ProfileSyncStatsDto.MinutesSinceLastBgActivity),
                HeaderText = "Min since BG",
                Width = 90,
                DefaultCellStyle = { Format = "N1" }
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(ProfileSyncStatsDto.FailureRatePercent),
                HeaderText = "Fail %",
                Width = 70,
                DefaultCellStyle = { Format = "N1" }
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(ProfileSyncStatsDto.LastSyncUtc),
                HeaderText = "Last sync (UTC)",
                Width = 140,
                DefaultCellStyle = { Format = "g" }
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(ProfileSyncStatsDto.LastBackgroundActivityUtc),
                HeaderText = "Last BG activity (UTC)",
                Width = 160,
                DefaultCellStyle = { Format = "g" }
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(ProfileSyncStatsDto.InfoCount),
                HeaderText = "Info",
                Width = 60
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(ProfileSyncStatsDto.ErrorCount),
                HeaderText = "Errors",
                Width = 60
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(ProfileSyncStatsDto.BgFetchCount),
                HeaderText = "BG fetch",
                Width = 70
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(ProfileSyncStatsDto.BgNormalizeCount),
                HeaderText = "BG normalize",
                Width = 90
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(ProfileSyncStatsDto.BgAddToPosCount),
                HeaderText = "BG add-to-POS",
                Width = 100
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(ProfileSyncStatsDto.BgErrorCount),
                HeaderText = "BG errors",
                Width = 80
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(ProfileSyncStatsDto.NormalizeErrorCount),
                HeaderText = "Normalize errors",
                Width = 110
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(ProfileSyncStatsDto.AddToPosErrorCount),
                HeaderText = "POS errors",
                Width = 80
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(ProfileSyncStatsDto.LastErrorAtUtc),
                HeaderText = "Last error at (UTC)",
                Width = 150,
                DefaultCellStyle = { Format = "g" }
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(ProfileSyncStatsDto.LastErrorMessage),
                HeaderText = "Last error message",
                Width = 260
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(ProfileSyncStatsDto.LastFetchAtUtc),
                HeaderText = "Last fetch (UTC)",
                Width = 140,
                DefaultCellStyle = { Format = "g" }
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(ProfileSyncStatsDto.LastNormalizeAtUtc),
                HeaderText = "Last normalize (UTC)",
                Width = 140,
                DefaultCellStyle = { Format = "g" }
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(ProfileSyncStatsDto.LastAddToPosAtUtc),
                HeaderText = "Last add-to-POS (UTC)",
                Width = 160,
                DefaultCellStyle = { Format = "g" }
            });

            _grid.CellFormatting += Grid_CellFormatting;
            _grid.CellDoubleClick += Grid_CellDoubleClick;

            _perfGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };
            _perfGrid.DataSource = _perfBinding;

            _perfGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(ProfileSyncStatsDto.Name),
                HeaderText = "Profile",
                Width = 150
            });
            _perfGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(ProfileSyncStatsDto.ProviderType),
                HeaderText = "Provider",
                Width = 80
            });
            _perfGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(ProfileSyncStatsDto.BgFetchCount),
                HeaderText = "BG fetch",
                Width = 70
            });
            _perfGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(ProfileSyncStatsDto.AvgFetchMs),
                HeaderText = "Avg fetch ms",
                Width = 90,
                DefaultCellStyle = { Format = "N1" }
            });
            _perfGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(ProfileSyncStatsDto.AvgNormalizeMs),
                HeaderText = "Avg norm ms",
                Width = 90,
                DefaultCellStyle = { Format = "N1" }
            });
            _perfGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(ProfileSyncStatsDto.AvgAddToPosMs),
                HeaderText = "Avg POS ms",
                Width = 90,
                DefaultCellStyle = { Format = "N1" }
            });
            _perfGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(ProfileSyncStatsDto.AvgTotalMs),
                HeaderText = "Avg total ms",
                Width = 100,
                DefaultCellStyle = { Format = "N1" }
            });

            var tab = new TabControl
            {
                Dock = DockStyle.Fill
            };

            var overviewPage = new TabPage("Overview");
            overviewPage.Controls.Add(_grid);

            var perfPage = new TabPage("Performance");
            perfPage.Controls.Add(_perfGrid);

            tab.TabPages.Add(overviewPage);
            tab.TabPages.Add(perfPage);

            Controls.Add(tab);
            Controls.Add(top);
        }


        private void LoadData()
        {
            try
            {
                var days = (int)_numDays.Value;
                var stats = _dashboardService.GetSnapshot(days);

                _binding.DataSource = new BindingList<ProfileSyncStatsDto>(stats.ToList());

                _perfBinding.DataSource = new BindingList<ProfileSyncStatsDto>(stats.ToList());

                var totalProfiles = stats.Count;
                var activeProfiles = stats.Count(s => s.IsActive);
                var withErrors = stats.Count(s => s.ErrorCount > 0);

                var totalBgFetch = stats.Sum(s => s.BgFetchCount);
                var totalBgErrors = stats.Sum(s => s.BgErrorCount);
                var totalNormalizeErrors = stats.Sum(s => s.NormalizeErrorCount);
                var totalPosErrors = stats.Sum(s => s.AddToPosErrorCount);

                var providerParts = stats
                    .GroupBy(s => s.ProviderType)
                    .Select(g => string.Format("{0}: fetch {1}, BG errors {2}",
                        string.IsNullOrWhiteSpace(g.Key) ? "(none)" : g.Key,
                        g.Sum(x => x.BgFetchCount),
                        g.Sum(x => x.BgErrorCount)))
                    .ToList();

                var providersSummary = providerParts.Count == 0
                    ? "Providers: none"
                    : "Providers: " + string.Join(" | ", providerParts);

                _lblInfo.Text = string.Format(
                    "Profiles: {0} (active {1}), with errors in last {2} days: {3} | Global BG: fetch {4}, BG errors {5}, norm errors {6}, POS errors {7} | {8}",
                    totalProfiles,
                    activeProfiles,
                    days,
                    withErrors,
                    totalBgFetch,
                    totalBgErrors,
                    totalNormalizeErrors,
                    totalPosErrors,
                    providersSummary);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error loading sync dashboard:\r\n\r\n" + ex.Message,
                    "PayPulse – Sync dashboard",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void Grid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            var column = _grid.Columns[e.ColumnIndex];
            if (!string.Equals(column.DataPropertyName, nameof(ProfileSyncStatsDto.HealthStatus), StringComparison.Ordinal))
            {
                return;
            }

            var value = _grid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value as string;
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            if (value.Equals("ERROR", StringComparison.OrdinalIgnoreCase))
            {
                e.CellStyle.BackColor = Color.FromArgb(255, 200, 200);
                e.CellStyle.ForeColor = Color.DarkRed;
            }
            else if (value.Equals("WARNING", StringComparison.OrdinalIgnoreCase))
            {
                e.CellStyle.BackColor = Color.FromArgb(255, 245, 200);
                e.CellStyle.ForeColor = Color.DarkOrange;
            }
            else if (value.Equals("OK", StringComparison.OrdinalIgnoreCase))
            {
                e.CellStyle.BackColor = Color.FromArgb(220, 255, 220);
                e.CellStyle.ForeColor = Color.DarkGreen;
            }
        }

        private void Grid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            ProfileSyncStatsDto dto = null;
            if (_binding.Current is ProfileSyncStatsDto current && _grid.CurrentRow != null && _grid.CurrentRow.Index == e.RowIndex)
            {
                dto = current;
            }
            else
            {
                var row = _grid.Rows[e.RowIndex];
                dto = row.DataBoundItem as ProfileSyncStatsDto;
            }

            if (dto == null || string.IsNullOrWhiteSpace(dto.ProfileId))
            {
                return;
            }

            var days = (int)_numDays.Value;
            var toUtc = _clock.UtcNow;
            var fromUtc = toUtc.AddDays(-days);

            using (var frm = new LogsForm(_logRepository, dto.ProfileId, fromUtc, toUtc))
            {
                frm.ShowDialog(this);
            }
        }

    }
}