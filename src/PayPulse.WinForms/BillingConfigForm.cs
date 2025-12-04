using System;
using System.Windows.Forms;
using PayPulse.Core.Billing;

namespace PayPulse.WinForms.Forms
{
    public class BillingConfigForm : Form
    {
        private readonly IBillingRepository _billingRepository;

        private TextBox _txtPricePerTransfer;
        private TextBox _txtPricePerAddToPos;
        private TextBox _txtPricePerCustomer;
        private ComboBox _cmbCurrency;
        private Button _btnSave;
        private Button _btnCancel;

        public BillingConfigForm(IBillingRepository billingRepository)
        {
            _billingRepository = billingRepository ?? throw new ArgumentNullException("billingRepository");
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Billing configuration";
            Width = 420;
            Height = 250;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;

            var lblPriceTransfer = new Label { Left = 20, Top = 20, Width = 170, Text = "Price per transfer:" };
            var lblPriceAddToPos = new Label { Left = 20, Top = 55, Width = 170, Text = "Price per Add-to-POS:" };
            var lblPriceCustomer = new Label { Left = 20, Top = 90, Width = 170, Text = "Price per customer:" };
            var lblCurrency = new Label { Left = 20, Top = 125, Width = 170, Text = "Currency:" };

            _txtPricePerTransfer = new TextBox { Left = 210, Top = 16, Width = 150 };
            _txtPricePerAddToPos = new TextBox { Left = 210, Top = 51, Width = 150 };
            _txtPricePerCustomer = new TextBox { Left = 210, Top = 86, Width = 150 };

            _cmbCurrency = new ComboBox
            {
                Left = 210,
                Top = 121,
                Width = 120,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            _cmbCurrency.Items.AddRange(new object[]
            {
                "USD",
                "EUR",
                "GBP",
                "ILS",
                "CAD",
                "AUD",
                "TRY",
                "AED"
            });

            _btnSave = new Button { Left = 190, Top = 160, Width = 80, Text = "Save" };
            _btnCancel = new Button { Left = 280, Top = 160, Width = 80, Text = "Cancel" };

            _btnSave.Click += OnSaveClick;
            _btnCancel.Click += (s, e) => Close();

            AcceptButton = _btnSave;
            CancelButton = _btnCancel;

            Controls.Add(lblPriceTransfer);
            Controls.Add(lblPriceAddToPos);
            Controls.Add(lblPriceCustomer);
            Controls.Add(lblCurrency);
            Controls.Add(_txtPricePerTransfer);
            Controls.Add(_txtPricePerAddToPos);
            Controls.Add(_txtPricePerCustomer);
            Controls.Add(_cmbCurrency);
            Controls.Add(_btnSave);
            Controls.Add(_btnCancel);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                var tariffs = _billingRepository.GetTariffs();
                if (tariffs != null)
                {
                    _txtPricePerTransfer.Text = tariffs.PricePerTransfer.ToString("0.####");
                    _txtPricePerAddToPos.Text = tariffs.PricePerAddToPos.ToString("0.####");
                    _txtPricePerCustomer.Text = tariffs.PricePerCustomer.ToString("0.####");
                    if (!string.IsNullOrEmpty(tariffs.Currency) && _cmbCurrency.Items.Contains(tariffs.Currency))
                    {
                        _cmbCurrency.SelectedItem = tariffs.Currency;
                    }
                    else
                    {
                        _cmbCurrency.SelectedItem = "USD";
                    }
                }
                else
                {
                    _txtPricePerTransfer.Text = "0.02";
                    _txtPricePerAddToPos.Text = "0.03";
                    _txtPricePerCustomer.Text = "0.01";
                    _cmbCurrency.SelectedItem = "USD";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Failed to load billing configuration: " + ex.Message,
                    "PayPulse – Billing",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void OnSaveClick(object sender, EventArgs e)
        {
            if (!decimal.TryParse(_txtPricePerTransfer.Text, out var priceTransfer))
            {
                ShowValidationError("Invalid value for 'Price per transfer'.");
                return;
            }

            if (!decimal.TryParse(_txtPricePerAddToPos.Text, out var priceAddToPos))
            {
                ShowValidationError("Invalid value for 'Price per Add-to-POS'.");
                return;
            }

            if (!decimal.TryParse(_txtPricePerCustomer.Text, out var priceCustomer))
            {
                ShowValidationError("Invalid value for 'Price per customer'.");
                return;
            }

            var currency = _cmbCurrency.SelectedItem != null ? _cmbCurrency.SelectedItem.ToString() : string.Empty;
            if (string.IsNullOrEmpty(currency))
            {
                ShowValidationError("Please select a currency.");
                return;
            }

            var tariffs = new BillingTariffs
            {
                PricePerTransfer = priceTransfer,
                PricePerAddToPos = priceAddToPos,
                PricePerCustomer = priceCustomer,
                Currency = currency
            };

            try
            {
                _billingRepository.SaveTariffs(tariffs);

                MessageBox.Show(
                    "Billing configuration saved.\nChanges will be applied for new periods and new app sessions.",
                    "PayPulse – Billing",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Failed to save billing configuration: " + ex.Message,
                    "PayPulse – Billing",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void ShowValidationError(string message)
        {
            MessageBox.Show(
                message,
                "PayPulse – Billing configuration",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }
}