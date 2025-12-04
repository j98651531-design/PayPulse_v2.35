using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using PayPulse.Core.Services;
using PayPulse.Domain.Entities;
using PayPulse.Domain.Interfaces.Repositories;

namespace PayPulse.WinForms
{
    public class ProfilesForm : Form
    {
        private readonly IProfileRepository _profileRepository;
        private readonly PosMetadataService _posMetadataService;

        private BindingSource _binding = new BindingSource();
        private DataGridView _grid;
        private Button _btnAdd;
        private Button _btnEdit;
        private Button _btnDelete;
        private Button _btnClose;

        private List<Profile> _profiles = new List<Profile>();
        private List<UserRef> _users = new List<UserRef>();
        private List<CashboxRef> _cashboxes = new List<CashboxRef>();

        public ProfilesForm(
            IProfileRepository profileRepository,
            PosMetadataService posMetadataService)
        {
            _profileRepository = profileRepository;
            _posMetadataService = posMetadataService;

            InitializeUi();
            LoadMetadata();
            LoadProfiles();
        }

        private void InitializeUi()
        {
            Text = "Profiles";
            Width = 950;
            Height = 500;
            StartPosition = FormStartPosition.CenterParent;

            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };
            Controls.Add(_grid);

            var bottom = new Panel { Dock = DockStyle.Bottom, Height = 40 };
            _btnAdd = new Button { Text = "Add", Left = 10, Top = 8, Width = 80 };
            _btnEdit = new Button { Text = "Edit", Left = 100, Top = 8, Width = 80 };
            _btnDelete = new Button { Text = "Delete", Left = 190, Top = 8, Width = 80 };
            _btnClose = new Button { Text = "Close", Left = 280, Top = 8, Width = 80 };

            _btnAdd.Click += BtnAdd_Click;
            _btnEdit.Click += BtnEdit_Click;
            _btnDelete.Click += BtnDelete_Click;
            _btnClose.Click += (s, e) => Close();

            bottom.Controls.AddRange(new Control[] { _btnAdd, _btnEdit, _btnDelete, _btnClose });
            Controls.Add(bottom);
        }

        private void LoadMetadata()
        {
            try
            {
                _users = _posMetadataService.GetUsers().ToList();
                _cashboxes = _posMetadataService.GetCashboxes().ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Cannot load POS metadata for profiles.\r\n\r\n" + ex.Message,
                    "PayPulse – Profiles",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                _users = new List<UserRef>();
                _cashboxes = new List<CashboxRef>();
            }
        }

        private void ConfigureColumns()
        {
            _grid.Columns.Clear();

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Name",
                HeaderText = "Profile name",
                Width = 180
            });

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ProviderType",
                HeaderText = "Provider",
                Width = 80
            });

            _grid.Columns.Add(new DataGridViewCheckBoxColumn
            {
                DataPropertyName = "IsActive",
                HeaderText = "Active",
                Width = 60
            });

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LoginEmail",
                HeaderText = "Email",
                Width = 200
            });

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LoginPhoneNumber",
                HeaderText = "Phone",
                Width = 140
            });

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "PosUserId",
                HeaderText = "POS User Id",
                Width = 120
            });

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "PosCashboxId",
                HeaderText = "POS Cashbox Id",
                Width = 120
            });

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ProfileId",
                HeaderText = "ProfileId",
                Visible = false
            });
        }

        private void LoadProfiles()
        {
            _profiles = _profileRepository.GetAll().ToList();
            _binding.DataSource = _profiles;
            _grid.DataSource = _binding;
            ConfigureColumns();
        }

        private Profile GetSelectedProfile()
        {
            if (_grid.CurrentRow == null)
                return null;

            return _grid.CurrentRow.DataBoundItem as Profile;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var p = new Profile
            {
                ProfileId = Guid.NewGuid().ToString(),
                Name = "New profile",
                ProviderType = "STB",
                IsActive = true
            };

            using (var dlg = new ProfileEditForm(p, _users, _cashboxes))
            {
                if (dlg.ShowDialog(this) != DialogResult.OK)
                    return;
            }

            try
            {
                _profileRepository.Insert(p);
                _profiles.Add(p);
                _binding.ResetBindings(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    @"Error saving new profile:\n\r\n" + ex.Message,
                    "PayPulse – Profiles",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            var p = GetSelectedProfile();
            if (p == null)
                return;

            using (var dlg = new ProfileEditForm(p, _users, _cashboxes))
            {
                if (dlg.ShowDialog(this) != DialogResult.OK)
                    return;
            }

            try
            {
                _profileRepository.Update(p);
                _binding.ResetBindings(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    @"Error updating profile:\n\r\n" + ex.Message,
                    "PayPulse – Profiles",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            var p = GetSelectedProfile();
            if (p == null)
                return;

            if (MessageBox.Show(
                    "Delete profile '" + p.Name + "'?",
                    "PayPulse – Profiles",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(p.ProfileId))
                    _profileRepository.Delete(p.ProfileId);

                _profiles.Remove(p);
                _binding.ResetBindings(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    @"Error deleting profile:\n\r\n" + ex.Message,
                    "PayPulse – Profiles",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
