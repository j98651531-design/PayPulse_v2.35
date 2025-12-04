using System;
using System.Linq;
using System.Windows.Forms;
using PayPulse.Core.Billing;

namespace PayPulse.WinForms.Forms
{
    public partial class BillingDashboardForm : Form
    {
        private readonly IBillingEngine _billingEngine;
        private readonly IBillingRepository _billingRepository;

        public BillingDashboardForm(IBillingEngine billingEngine, IBillingRepository billingRepository)
        {
            _billingEngine = billingEngine;
            _billingRepository = billingRepository;

            InitializeComponent();
            InitializeCustomUi();
        }

        private void InitializeCustomUi()
        {
            var now = DateTime.UtcNow;
            var firstDay = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var lastDay = firstDay.AddMonths(1).AddDays(-1);

            dtpEventsFrom.Value = firstDay.ToLocalTime();
            dtpEventsTo.Value = lastDay.ToLocalTime();

            cboEventType.Items.Clear();
            cboEventType.Items.Add("(All)");
            cboEventType.Items.Add("Transfer");
            cboEventType.Items.Add("AddToPos");
            cboEventType.Items.Add("Customer");
            cboEventType.SelectedIndex = 0;

            cboProfileId.Items.Clear();
            cboProfileId.Items.Add("(All)");
            cboProfileId.Items.Add("1");
            cboProfileId.SelectedIndex = 0;

            cboProvider.Items.Clear();
            cboProvider.Items.Add("(All)");
            cboProvider.Items.Add("STB");
            cboProvider.Items.Add("GMT");
            cboProvider.Items.Add("WIC");
            cboProvider.SelectedIndex = 0;

            ConfigureEventsGrid();
            ConfigurePeriodsGrid();

            LoadSummary();
            LoadEventsGrid();
            LoadPeriodsGrid();
        }

        private void LoadSummary()
        {
            try
            {
                var summary = _billingEngine.GetCurrentPeriodSummary();

                lblCurrentPeriod.Text = summary.PeriodKey;
                lblCurrency.Text = summary.Currency;

                lblTransfersCount.Text = summary.TransferCount.ToString();
                lblTransfersPrice.Text = summary.PricePerTransfer.ToString("0.000");
                lblTransfersTotal.Text = (summary.TransferCount * summary.PricePerTransfer).ToString("0.00");

                lblAddToPosCount.Text = summary.AddToPosCount.ToString();
                lblAddToPosPrice.Text = summary.PricePerAddToPos.ToString("0.000");
                lblAddToPosTotal.Text = (summary.AddToPosCount * summary.PricePerAddToPos).ToString("0.00");

                lblCustomersCount.Text = summary.CustomerCount.ToString();
                lblCustomersPrice.Text = summary.PricePerCustomer.ToString("0.000");
                lblCustomersTotal.Text = (summary.CustomerCount * summary.PricePerCustomer).ToString("0.00");

                lblGrandTotal.Text = summary.TotalAmount.ToString("0.00");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Failed to load billing summary.\n\n" + ex.Message,
                    "Billing", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRefreshSummary_Click(object sender, EventArgs e)
        {
            LoadSummary();
        }

        private void btnClosePeriod_Click(object sender, EventArgs e)
        {
            try
            {
                var result = MessageBox.Show(this,
                    "This will close the current period and finalize its totals.\nAre you sure?",
                    "Close Billing Period",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result != DialogResult.Yes) return;

                var closed = _billingEngine.CloseCurrentPeriod();

                MessageBox.Show(this,
                    "Billing period " + closed.PeriodKey + " closed.\nAmount: " +
                    closed.Amount.ToString("0.00") + " " + closed.Currency,
                    "Billing",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                LoadSummary();
                LoadPeriodsGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Failed to close billing period.\n\n" + ex.Message,
                    "Billing", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnGenerateInvoice_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this,
                "Invoice generation is not implemented in this demo.",
                "Billing",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void ConfigureEventsGrid()
        {
            dgvEvents.AutoGenerateColumns = false;
            dgvEvents.Columns.Clear();

            var colDate = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "CreatedAtUtc",
                HeaderText = "Timestamp (UTC)",
                Width = 150
            };
            dgvEvents.Columns.Add(colDate);

            var colType = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "EventType",
                HeaderText = "Type",
                Width = 80
            };
            dgvEvents.Columns.Add(colType);

            var colProfile = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ProfileId",
                HeaderText = "Profile",
                Width = 60
            };
            dgvEvents.Columns.Add(colProfile);

            var colProvider = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Provider",
                HeaderText = "Provider",
                Width = 80
            };
            dgvEvents.Columns.Add(colProvider);

            var colTransferId = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "TransferId",
                HeaderText = "Transfer Id",
                Width = 150
            };
            dgvEvents.Columns.Add(colTransferId);
        }

        private void btnRefreshEvents_Click(object sender, EventArgs e)
        {
            LoadEventsGrid();
        }

        private void LoadEventsGrid()
        {
            try
            {
                var fromLocal = dtpEventsFrom.Value.Date;
                var toLocal = dtpEventsTo.Value.Date.AddDays(1).AddSeconds(-1);

                var fromUtc = fromLocal.ToUniversalTime();
                var toUtc = toLocal.ToUniversalTime();

                string eventType = null;
                if (cboEventType.SelectedItem != null &&
                    !cboEventType.SelectedItem.ToString().Equals("(All)", StringComparison.OrdinalIgnoreCase))
                {
                    eventType = cboEventType.SelectedItem.ToString();
                }

                string profileId = null;
                if (cboProfileId.SelectedItem != null &&
                    !cboProfileId.SelectedItem.ToString().Equals("(All)", StringComparison.OrdinalIgnoreCase))
                {
                    profileId = cboProfileId.SelectedItem.ToString();
                }

                string provider = null;
                if (cboProvider.SelectedItem != null &&
                    !cboProvider.SelectedItem.ToString().Equals("(All)", StringComparison.OrdinalIgnoreCase))
                {
                    provider = cboProvider.SelectedItem.ToString();
                }

                var events = _billingRepository.GetEvents(fromUtc, toUtc, eventType, profileId, provider);
                dgvEvents.DataSource = events.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    "Failed to load billing events.\n\n" + ex.Message,
                    "Billing",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void ConfigurePeriodsGrid()
        {
            dgvPeriods.AutoGenerateColumns = false;
            dgvPeriods.Columns.Clear();

            var colPeriod = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "PeriodKey",
                HeaderText = "Period",
                Width = 80
            };
            dgvPeriods.Columns.Add(colPeriod);

            var colFrom = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "FromDateUtc",
                HeaderText = "From (UTC)",
                Width = 130
            };
            dgvPeriods.Columns.Add(colFrom);

            var colTo = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ToDateUtc",
                HeaderText = "To (UTC)",
                Width = 130
            };
            dgvPeriods.Columns.Add(colTo);

            var colAmount = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Amount",
                HeaderText = "Amount",
                Width = 80
            };
            dgvPeriods.Columns.Add(colAmount);

            var colCurrency = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Currency",
                HeaderText = "Currency",
                Width = 70
            };
            dgvPeriods.Columns.Add(colCurrency);

            var colClosed = new DataGridViewCheckBoxColumn
            {
                DataPropertyName = "IsClosed",
                HeaderText = "Closed",
                Width = 60
            };
            dgvPeriods.Columns.Add(colClosed);
        }

        private void LoadPeriodsGrid()
        {
            try
            {
                var periods = _billingRepository.GetClosedPeriods();
                dgvPeriods.DataSource = periods.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    "Failed to load billing periods.\n\n" + ex.Message,
                    "Billing",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btnRefreshPeriods_Click(object sender, EventArgs e)
        {
            LoadPeriodsGrid();
        }

        private void btnOpenSelectedInvoice_Click(object sender, EventArgs e)
        {
            if (dgvPeriods.CurrentRow == null)
            {
                MessageBox.Show(this,
                    "Please select a billing period.",
                    "Billing",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            var period = dgvPeriods.CurrentRow.DataBoundItem as BillingPeriod;
            if (period == null) return;

            MessageBox.Show(this,
                "Invoice PDF for period " + period.PeriodKey + " is not implemented in this demo.",
                "Billing",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }
}
