namespace PayPulse.WinForms.Forms
{
    partial class BillingDashboardForm
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabSummary;
        private System.Windows.Forms.TabPage tabEvents;
        private System.Windows.Forms.TabPage tabPeriods;

        private System.Windows.Forms.Label lblCurrentPeriodLabel;
        private System.Windows.Forms.Label lblCurrentPeriod;
        private System.Windows.Forms.Label lblCurrencyLabel;
        private System.Windows.Forms.Label lblCurrency;

        private System.Windows.Forms.GroupBox grpTransfers;
        private System.Windows.Forms.Label lblTransfersCountLabel;
        private System.Windows.Forms.Label lblTransfersCount;
        private System.Windows.Forms.Label lblTransfersPriceLabel;
        private System.Windows.Forms.Label lblTransfersPrice;
        private System.Windows.Forms.Label lblTransfersTotalLabel;
        private System.Windows.Forms.Label lblTransfersTotal;

        private System.Windows.Forms.GroupBox grpAddToPos;
        private System.Windows.Forms.Label lblAddToPosCountLabel;
        private System.Windows.Forms.Label lblAddToPosCount;
        private System.Windows.Forms.Label lblAddToPosPriceLabel;
        private System.Windows.Forms.Label lblAddToPosPrice;
        private System.Windows.Forms.Label lblAddToPosTotalLabel;
        private System.Windows.Forms.Label lblAddToPosTotal;

        private System.Windows.Forms.GroupBox grpCustomers;
        private System.Windows.Forms.Label lblCustomersCountLabel;
        private System.Windows.Forms.Label lblCustomersCount;
        private System.Windows.Forms.Label lblCustomersPriceLabel;
        private System.Windows.Forms.Label lblCustomersPrice;
        private System.Windows.Forms.Label lblCustomersTotalLabel;
        private System.Windows.Forms.Label lblCustomersTotal;

        private System.Windows.Forms.Label lblGrandTotalLabel;
        private System.Windows.Forms.Label lblGrandTotal;
        private System.Windows.Forms.Button btnRefreshSummary;
        private System.Windows.Forms.Button btnClosePeriod;
        private System.Windows.Forms.Button btnGenerateInvoice;

        private System.Windows.Forms.DataGridView dgvEvents;
        private System.Windows.Forms.Label lblEventsFrom;
        private System.Windows.Forms.DateTimePicker dtpEventsFrom;
        private System.Windows.Forms.Label lblEventsTo;
        private System.Windows.Forms.DateTimePicker dtpEventsTo;
        private System.Windows.Forms.Label lblEventTypeFilter;
        private System.Windows.Forms.ComboBox cboEventType;
        private System.Windows.Forms.Label lblProfileFilter;
        private System.Windows.Forms.ComboBox cboProfileId;
        private System.Windows.Forms.Label lblProviderFilter;
        private System.Windows.Forms.ComboBox cboProvider;
        private System.Windows.Forms.Button btnRefreshEvents;

        private System.Windows.Forms.DataGridView dgvPeriods;
        private System.Windows.Forms.Button btnRefreshPeriods;
        private System.Windows.Forms.Button btnOpenSelectedInvoice;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabSummary = new System.Windows.Forms.TabPage();
            this.tabEvents = new System.Windows.Forms.TabPage();
            this.tabPeriods = new System.Windows.Forms.TabPage();

            this.lblCurrentPeriodLabel = new System.Windows.Forms.Label();
            this.lblCurrentPeriod = new System.Windows.Forms.Label();
            this.lblCurrencyLabel = new System.Windows.Forms.Label();
            this.lblCurrency = new System.Windows.Forms.Label();

            this.grpTransfers = new System.Windows.Forms.GroupBox();
            this.lblTransfersCountLabel = new System.Windows.Forms.Label();
            this.lblTransfersCount = new System.Windows.Forms.Label();
            this.lblTransfersPriceLabel = new System.Windows.Forms.Label();
            this.lblTransfersPrice = new System.Windows.Forms.Label();
            this.lblTransfersTotalLabel = new System.Windows.Forms.Label();
            this.lblTransfersTotal = new System.Windows.Forms.Label();

            this.grpAddToPos = new System.Windows.Forms.GroupBox();
            this.lblAddToPosCountLabel = new System.Windows.Forms.Label();
            this.lblAddToPosCount = new System.Windows.Forms.Label();
            this.lblAddToPosPriceLabel = new System.Windows.Forms.Label();
            this.lblAddToPosPrice = new System.Windows.Forms.Label();
            this.lblAddToPosTotalLabel = new System.Windows.Forms.Label();
            this.lblAddToPosTotal = new System.Windows.Forms.Label();

            this.grpCustomers = new System.Windows.Forms.GroupBox();
            this.lblCustomersCountLabel = new System.Windows.Forms.Label();
            this.lblCustomersCount = new System.Windows.Forms.Label();
            this.lblCustomersPriceLabel = new System.Windows.Forms.Label();
            this.lblCustomersPrice = new System.Windows.Forms.Label();
            this.lblCustomersTotalLabel = new System.Windows.Forms.Label();
            this.lblCustomersTotal = new System.Windows.Forms.Label();

            this.lblGrandTotalLabel = new System.Windows.Forms.Label();
            this.lblGrandTotal = new System.Windows.Forms.Label();
            this.btnRefreshSummary = new System.Windows.Forms.Button();
            this.btnClosePeriod = new System.Windows.Forms.Button();
            this.btnGenerateInvoice = new System.Windows.Forms.Button();

            this.dgvEvents = new System.Windows.Forms.DataGridView();
            this.lblEventsFrom = new System.Windows.Forms.Label();
            this.dtpEventsFrom = new System.Windows.Forms.DateTimePicker();
            this.lblEventsTo = new System.Windows.Forms.Label();
            this.dtpEventsTo = new System.Windows.Forms.DateTimePicker();
            this.lblEventTypeFilter = new System.Windows.Forms.Label();
            this.cboEventType = new System.Windows.Forms.ComboBox();
            this.lblProfileFilter = new System.Windows.Forms.Label();
            this.cboProfileId = new System.Windows.Forms.ComboBox();
            this.lblProviderFilter = new System.Windows.Forms.Label();
            this.cboProvider = new System.Windows.Forms.ComboBox();
            this.btnRefreshEvents = new System.Windows.Forms.Button();

            this.dgvPeriods = new System.Windows.Forms.DataGridView();
            this.btnRefreshPeriods = new System.Windows.Forms.Button();
            this.btnOpenSelectedInvoice = new System.Windows.Forms.Button();

            this.tabControl.SuspendLayout();
            this.tabSummary.SuspendLayout();
            this.tabEvents.SuspendLayout();
            this.tabPeriods.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvEvents)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPeriods)).BeginInit();

            this.tabControl.Controls.Add(this.tabSummary);
            this.tabControl.Controls.Add(this.tabEvents);
            this.tabControl.Controls.Add(this.tabPeriods);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(800, 500);
            this.tabControl.TabIndex = 0;

            this.tabSummary.Text = "Summary";
            this.tabSummary.UseVisualStyleBackColor = true;

            this.lblCurrentPeriodLabel.AutoSize = true;
            this.lblCurrentPeriodLabel.Location = new System.Drawing.Point(20, 20);
            this.lblCurrentPeriodLabel.Text = "Current Period:";

            this.lblCurrentPeriod.AutoSize = true;
            this.lblCurrentPeriod.Location = new System.Drawing.Point(120, 20);
            this.lblCurrentPeriod.Text = "-";

            this.lblCurrencyLabel.AutoSize = true;
            this.lblCurrencyLabel.Location = new System.Drawing.Point(250, 20);
            this.lblCurrencyLabel.Text = "Currency:";

            this.lblCurrency.AutoSize = true;
            this.lblCurrency.Location = new System.Drawing.Point(320, 20);
            this.lblCurrency.Text = "-";

            this.grpTransfers.Text = "Transfers";
            this.grpTransfers.Location = new System.Drawing.Point(20, 60);
            this.grpTransfers.Size = new System.Drawing.Size(230, 120);

            this.lblTransfersCountLabel.AutoSize = true;
            this.lblTransfersCountLabel.Location = new System.Drawing.Point(10, 25);
            this.lblTransfersCountLabel.Text = "Count:";
            this.lblTransfersCount.AutoSize = true;
            this.lblTransfersCount.Location = new System.Drawing.Point(90, 25);
            this.lblTransfersCount.Text = "0";

            this.lblTransfersPriceLabel.AutoSize = true;
            this.lblTransfersPriceLabel.Location = new System.Drawing.Point(10, 50);
            this.lblTransfersPriceLabel.Text = "Price:";
            this.lblTransfersPrice.AutoSize = true;
            this.lblTransfersPrice.Location = new System.Drawing.Point(90, 50);
            this.lblTransfersPrice.Text = "0.000";

            this.lblTransfersTotalLabel.AutoSize = true;
            this.lblTransfersTotalLabel.Location = new System.Drawing.Point(10, 75);
            this.lblTransfersTotalLabel.Text = "Total:";
            this.lblTransfersTotal.AutoSize = true;
            this.lblTransfersTotal.Location = new System.Drawing.Point(90, 75);
            this.lblTransfersTotal.Text = "0.00";

            this.grpTransfers.Controls.Add(this.lblTransfersCountLabel);
            this.grpTransfers.Controls.Add(this.lblTransfersCount);
            this.grpTransfers.Controls.Add(this.lblTransfersPriceLabel);
            this.grpTransfers.Controls.Add(this.lblTransfersPrice);
            this.grpTransfers.Controls.Add(this.lblTransfersTotalLabel);
            this.grpTransfers.Controls.Add(this.lblTransfersTotal);

            this.grpAddToPos.Text = "Add To POS";
            this.grpAddToPos.Location = new System.Drawing.Point(280, 60);
            this.grpAddToPos.Size = new System.Drawing.Size(230, 120);

            this.lblAddToPosCountLabel.AutoSize = true;
            this.lblAddToPosCountLabel.Location = new System.Drawing.Point(10, 25);
            this.lblAddToPosCountLabel.Text = "Count:";
            this.lblAddToPosCount.AutoSize = true;
            this.lblAddToPosCount.Location = new System.Drawing.Point(90, 25);
            this.lblAddToPosCount.Text = "0";

            this.lblAddToPosPriceLabel.AutoSize = true;
            this.lblAddToPosPriceLabel.Location = new System.Drawing.Point(10, 50);
            this.lblAddToPosPriceLabel.Text = "Price:";
            this.lblAddToPosPrice.AutoSize = true;
            this.lblAddToPosPrice.Location = new System.Drawing.Point(90, 50);
            this.lblAddToPosPrice.Text = "0.000";

            this.lblAddToPosTotalLabel.AutoSize = true;
            this.lblAddToPosTotalLabel.Location = new System.Drawing.Point(10, 75);
            this.lblAddToPosTotalLabel.Text = "Total:";
            this.lblAddToPosTotal.AutoSize = true;
            this.lblAddToPosTotal.Location = new System.Drawing.Point(90, 75);
            this.lblAddToPosTotal.Text = "0.00";

            this.grpAddToPos.Controls.Add(this.lblAddToPosCountLabel);
            this.grpAddToPos.Controls.Add(this.lblAddToPosCount);
            this.grpAddToPos.Controls.Add(this.lblAddToPosPriceLabel);
            this.grpAddToPos.Controls.Add(this.lblAddToPosPrice);
            this.grpAddToPos.Controls.Add(this.lblAddToPosTotalLabel);
            this.grpAddToPos.Controls.Add(this.lblAddToPosTotal);

            this.grpCustomers.Text = "New Customers";
            this.grpCustomers.Location = new System.Drawing.Point(540, 60);
            this.grpCustomers.Size = new System.Drawing.Size(230, 120);

            this.lblCustomersCountLabel.AutoSize = true;
            this.lblCustomersCountLabel.Location = new System.Drawing.Point(10, 25);
            this.lblCustomersCountLabel.Text = "Count:";
            this.lblCustomersCount.AutoSize = true;
            this.lblCustomersCount.Location = new System.Drawing.Point(90, 25);
            this.lblCustomersCount.Text = "0";

            this.lblCustomersPriceLabel.AutoSize = true;
            this.lblCustomersPriceLabel.Location = new System.Drawing.Point(10, 50);
            this.lblCustomersPriceLabel.Text = "Price:";
            this.lblCustomersPrice.AutoSize = true;
            this.lblCustomersPrice.Location = new System.Drawing.Point(90, 50);
            this.lblCustomersPrice.Text = "0.000";

            this.lblCustomersTotalLabel.AutoSize = true;
            this.lblCustomersTotalLabel.Location = new System.Drawing.Point(10, 75);
            this.lblCustomersTotalLabel.Text = "Total:";
            this.lblCustomersTotal.AutoSize = true;
            this.lblCustomersTotal.Location = new System.Drawing.Point(90, 75);
            this.lblCustomersTotal.Text = "0.00";

            this.grpCustomers.Controls.Add(this.lblCustomersCountLabel);
            this.grpCustomers.Controls.Add(this.lblCustomersCount);
            this.grpCustomers.Controls.Add(this.lblCustomersPriceLabel);
            this.grpCustomers.Controls.Add(this.lblCustomersPrice);
            this.grpCustomers.Controls.Add(this.lblCustomersTotalLabel);
            this.grpCustomers.Controls.Add(this.lblCustomersTotal);

            this.lblGrandTotalLabel.AutoSize = true;
            this.lblGrandTotalLabel.Location = new System.Drawing.Point(20, 210);
            this.lblGrandTotalLabel.Text = "Grand Total:";
            this.lblGrandTotal.AutoSize = true;
            this.lblGrandTotal.Location = new System.Drawing.Point(110, 210);
            this.lblGrandTotal.Text = "0.00";

            this.btnRefreshSummary.Location = new System.Drawing.Point(20, 250);
            this.btnRefreshSummary.Size = new System.Drawing.Size(120, 30);
            this.btnRefreshSummary.Text = "Refresh";
            this.btnRefreshSummary.Click += new System.EventHandler(this.btnRefreshSummary_Click);

            this.btnClosePeriod.Location = new System.Drawing.Point(160, 250);
            this.btnClosePeriod.Size = new System.Drawing.Size(160, 30);
            this.btnClosePeriod.Text = "Close Current Period";
            this.btnClosePeriod.Click += new System.EventHandler(this.btnClosePeriod_Click);

            this.btnGenerateInvoice.Location = new System.Drawing.Point(340, 250);
            this.btnGenerateInvoice.Size = new System.Drawing.Size(160, 30);
            this.btnGenerateInvoice.Text = "Generate Invoice (stub)";
            this.btnGenerateInvoice.Click += new System.EventHandler(this.btnGenerateInvoice_Click);

            this.tabSummary.Controls.Add(this.lblCurrentPeriodLabel);
            this.tabSummary.Controls.Add(this.lblCurrentPeriod);
            this.tabSummary.Controls.Add(this.lblCurrencyLabel);
            this.tabSummary.Controls.Add(this.lblCurrency);
            this.tabSummary.Controls.Add(this.grpTransfers);
            this.tabSummary.Controls.Add(this.grpAddToPos);
            this.tabSummary.Controls.Add(this.grpCustomers);
            this.tabSummary.Controls.Add(this.lblGrandTotalLabel);
            this.tabSummary.Controls.Add(this.lblGrandTotal);
            this.tabSummary.Controls.Add(this.btnRefreshSummary);
            this.tabSummary.Controls.Add(this.btnClosePeriod);
            this.tabSummary.Controls.Add(this.btnGenerateInvoice);

            this.tabEvents.Text = "Events";
            this.tabEvents.UseVisualStyleBackColor = true;

            this.lblEventsFrom.AutoSize = true;
            this.lblEventsFrom.Location = new System.Drawing.Point(10, 15);
            this.lblEventsFrom.Text = "From:";

            this.dtpEventsFrom.Location = new System.Drawing.Point(50, 10);
            this.dtpEventsFrom.Size = new System.Drawing.Size(140, 20);

            this.lblEventsTo.AutoSize = true;
            this.lblEventsTo.Location = new System.Drawing.Point(210, 15);
            this.lblEventsTo.Text = "To:";

            this.dtpEventsTo.Location = new System.Drawing.Point(240, 10);
            this.dtpEventsTo.Size = new System.Drawing.Size(140, 20);

            this.lblEventTypeFilter.AutoSize = true;
            this.lblEventTypeFilter.Location = new System.Drawing.Point(400, 15);
            this.lblEventTypeFilter.Text = "Type:";

            this.cboEventType.Location = new System.Drawing.Point(440, 10);
            this.cboEventType.Size = new System.Drawing.Size(90, 21);

            this.lblProfileFilter.AutoSize = true;
            this.lblProfileFilter.Location = new System.Drawing.Point(540, 15);
            this.lblProfileFilter.Text = "Profile:";

            this.cboProfileId.Location = new System.Drawing.Point(590, 10);
            this.cboProfileId.Size = new System.Drawing.Size(70, 21);

            this.lblProviderFilter.AutoSize = true;
            this.lblProviderFilter.Location = new System.Drawing.Point(670, 15);
            this.lblProviderFilter.Text = "Provider:";

            this.cboProvider.Location = new System.Drawing.Point(730, 10);
            this.cboProvider.Size = new System.Drawing.Size(60, 21);

            this.btnRefreshEvents.Location = new System.Drawing.Point(10, 40);
            this.btnRefreshEvents.Size = new System.Drawing.Size(100, 25);
            this.btnRefreshEvents.Text = "Refresh";
            this.btnRefreshEvents.Click += new System.EventHandler(this.btnRefreshEvents_Click);

            this.dgvEvents.Location = new System.Drawing.Point(10, 70);
            this.dgvEvents.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right);
            this.dgvEvents.Size = new System.Drawing.Size(770, 360);
            this.dgvEvents.ReadOnly = true;
            this.dgvEvents.AllowUserToAddRows = false;
            this.dgvEvents.AllowUserToDeleteRows = false;

            this.tabEvents.Controls.Add(this.lblEventsFrom);
            this.tabEvents.Controls.Add(this.dtpEventsFrom);
            this.tabEvents.Controls.Add(this.lblEventsTo);
            this.tabEvents.Controls.Add(this.dtpEventsTo);
            this.tabEvents.Controls.Add(this.lblEventTypeFilter);
            this.tabEvents.Controls.Add(this.cboEventType);
            this.tabEvents.Controls.Add(this.lblProfileFilter);
            this.tabEvents.Controls.Add(this.cboProfileId);
            this.tabEvents.Controls.Add(this.lblProviderFilter);
            this.tabEvents.Controls.Add(this.cboProvider);
            this.tabEvents.Controls.Add(this.btnRefreshEvents);
            this.tabEvents.Controls.Add(this.dgvEvents);

            this.tabPeriods.Text = "Periods / Invoices";
            this.tabPeriods.UseVisualStyleBackColor = true;

            this.btnRefreshPeriods.Location = new System.Drawing.Point(10, 10);
            this.btnRefreshPeriods.Size = new System.Drawing.Size(100, 25);
            this.btnRefreshPeriods.Text = "Refresh";
            this.btnRefreshPeriods.Click += new System.EventHandler(this.btnRefreshPeriods_Click);

            this.btnOpenSelectedInvoice.Location = new System.Drawing.Point(130, 10);
            this.btnOpenSelectedInvoice.Size = new System.Drawing.Size(180, 25);
            this.btnOpenSelectedInvoice.Text = "Open Selected Invoice (stub)";
            this.btnOpenSelectedInvoice.Click += new System.EventHandler(this.btnOpenSelectedInvoice_Click);

            this.dgvPeriods.Location = new System.Drawing.Point(10, 45);
            this.dgvPeriods.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right);
            this.dgvPeriods.Size = new System.Drawing.Size(770, 385);
            this.dgvPeriods.ReadOnly = true;
            this.dgvPeriods.AllowUserToAddRows = false;
            this.dgvPeriods.AllowUserToDeleteRows = false;

            this.tabPeriods.Controls.Add(this.btnRefreshPeriods);
            this.tabPeriods.Controls.Add(this.btnOpenSelectedInvoice);
            this.tabPeriods.Controls.Add(this.dgvPeriods);

            this.ClientSize = new System.Drawing.Size(800, 500);
            this.Controls.Add(this.tabControl);
            this.Name = "BillingDashboardForm";
            this.Text = "Billing Dashboard";

            this.tabControl.ResumeLayout(false);
            this.tabSummary.ResumeLayout(false);
            this.tabSummary.PerformLayout();
            this.tabEvents.ResumeLayout(false);
            this.tabEvents.PerformLayout();
            this.tabPeriods.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvEvents)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPeriods)).EndInit();
        }
    }
}
