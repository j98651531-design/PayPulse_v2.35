using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using PayPulse.Core.Services;
using PayPulse.Domain.Entities;
using PayPulse.Domain.Interfaces.Repositories;

namespace PayPulse.WinForms
{
    public class UsersForm : Form
    {
        private readonly IAppUserRepository _userRepository;
        private readonly CurrentUserContext _currentUserContext;

        private readonly BindingSource _binding = new BindingSource();
        private DataGridView _grid;
        private Button _btnAdd;
        private Button _btnEdit;
        private Button _btnDelete;
        private Button _btnRefresh;
        private Button _btnClose;

        // Detail panel labels
        private Label _lblCreatedAtValue;
        private Label _lblCreatedByValue;
        private Label _lblUpdatedAtValue;
        private Label _lblUpdatedByValue;
        private Label _lblLastLoginAtValue;
        private Label _lblMustChangeValue;

        public UsersForm(IAppUserRepository userRepository, CurrentUserContext currentUserContext)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _currentUserContext = currentUserContext ?? throw new ArgumentNullException(nameof(currentUserContext));

            if (!_currentUserContext.CanManageUsers)
            {
                MessageBox.Show(
                    @"You do not have permission to manage users.",
                    @"PayPulse – Users",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                // Do not allow the form to be used if permission is missing.
                Load += (s, e) => Close();
                return;
            }

            InitializeUi();
            LoadUsers();
        }

        private void InitializeUi()
        {
            Text = @"Users Management";
            Width = 700;
            Height = 450;
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
            _grid.SelectionChanged += GridOnSelectionChanged;
            _grid.CellFormatting += GridOnCellFormatting;

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(AppUser.UserName),
                HeaderText = @"User name",
                Width = 150
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(AppUser.DisplayName),
                HeaderText = @"Display name",
                Width = 180
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(AppUser.Role),
                HeaderText = @"Role",
                Width = 80
            });
            _grid.Columns.Add(new DataGridViewCheckBoxColumn
            {
                DataPropertyName = nameof(AppUser.IsActive),
                HeaderText = @"Active",
                Width = 60
            });
            _grid.Columns.Add(new DataGridViewCheckBoxColumn
            {
                DataPropertyName = nameof(AppUser.MustChangePassword),
                HeaderText = @"Must change",
                Width = 90
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(AppUser.CreatedAt),
                HeaderText = @"Created",
                Width = 140,
                DefaultCellStyle = { Format = "g" }
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CreatedBy",
                HeaderText = @"Created by",
                Width = 120,
                ValueType = typeof(string)
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(AppUser.LastLoginAt),
                HeaderText = @"Last login",
                Width = 140,
                DefaultCellStyle = { Format = "g" }
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "UpdatedBy",
                HeaderText = @"Updated by",
                Width = 120,
                ValueType = typeof(string)
            });

            var buttonsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 45,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(5)
            };

            _btnClose = new Button { Text = @"Close", Width = 80 };
            _btnClose.Click += (s, e) => Close();

            _btnRefresh = new Button { Text = @"Refresh", Width = 80 };
            _btnRefresh.Click += (s, e) => LoadUsers();

            _btnDelete = new Button { Text = @"Delete", Width = 80 };
            _btnDelete.Click += (s, e) => DeleteSelected();

            _btnEdit = new Button { Text = @"Edit", Width = 80 };
            _btnEdit.Click += (s, e) => EditSelected();

            _btnAdd = new Button { Text = @"Add", Width = 80 };
            _btnAdd.Click += (s, e) => AddUser();

            buttonsPanel.Controls.Add(_btnClose);
            buttonsPanel.Controls.Add(_btnRefresh);
            buttonsPanel.Controls.Add(_btnDelete);
            buttonsPanel.Controls.Add(_btnEdit);
            buttonsPanel.Controls.Add(_btnAdd);

            var detailPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 70,
                ColumnCount = 4,
                RowCount = 3,
                Padding = new Padding(5)
            };

            detailPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15));
            detailPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
            detailPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15));
            detailPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));

            var lblCreatedAt = new Label { Text = @"Created:", AutoSize = true, Anchor = AnchorStyles.Left };
            _lblCreatedAtValue = new Label { AutoSize = true, Anchor = AnchorStyles.Left };

            var lblUpdatedAt = new Label { Text = @"Updated:", AutoSize = true, Anchor = AnchorStyles.Left };
            _lblUpdatedAtValue = new Label { AutoSize = true, Anchor = AnchorStyles.Left };

            detailPanel.Controls.Add(lblCreatedAt, 0, 0);
            detailPanel.Controls.Add(_lblCreatedAtValue, 1, 0);
            detailPanel.Controls.Add(lblUpdatedAt, 2, 0);
            detailPanel.Controls.Add(_lblUpdatedAtValue, 3, 0);

            var lblCreatedBy = new Label { Text = @"Created by:", AutoSize = true, Anchor = AnchorStyles.Left };
            _lblCreatedByValue = new Label { AutoSize = true, Anchor = AnchorStyles.Left };

            var lblUpdatedBy = new Label { Text = @"Updated by:", AutoSize = true, Anchor = AnchorStyles.Left };
            _lblUpdatedByValue = new Label { AutoSize = true, Anchor = AnchorStyles.Left };

            detailPanel.Controls.Add(lblCreatedBy, 0, 1);
            detailPanel.Controls.Add(_lblCreatedByValue, 1, 1);
            detailPanel.Controls.Add(lblUpdatedBy, 2, 1);
            detailPanel.Controls.Add(_lblUpdatedByValue, 3, 1);

            var lblLastLogin = new Label { Text = @"Last login:", AutoSize = true, Anchor = AnchorStyles.Left };
            _lblLastLoginAtValue = new Label { AutoSize = true, Anchor = AnchorStyles.Left };

            var lblMustChange = new Label { Text = @"Must change password:", AutoSize = true, Anchor = AnchorStyles.Left };
            _lblMustChangeValue = new Label { AutoSize = true, Anchor = AnchorStyles.Left };

            detailPanel.Controls.Add(lblLastLogin, 0, 2);
            detailPanel.Controls.Add(_lblLastLoginAtValue, 1, 2);
            detailPanel.Controls.Add(lblMustChange, 2, 2);
            detailPanel.Controls.Add(_lblMustChangeValue, 3, 2);

            Controls.Add(_grid);
            Controls.Add(detailPanel);
            Controls.Add(buttonsPanel);
        }

        private void LoadUsers()
        {
            try
            {
                var users = _userRepository.GetAll();
                _binding.DataSource = new BindingList<AppUser>(users.ToList());
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    @"Error loading users:

" + ex.Message,
                    @"PayPulse – Users",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private AppUser GetSelectedUser()
        {
            if (_binding.Current is AppUser user)
            {
                return user;
            }

            return null;
        }

        private void AddUser()
        {
            var user = new AppUser
            {
                IsActive = true,
                Role = AppUserRole.User,
                CreatedByUserId = _currentUserContext.CurrentUser?.UserId,
                UpdatedByUserId = _currentUserContext.CurrentUser?.UserId,
                MustChangePassword = true
            };

            using (var dlg = new UserEditForm(user, isNew: true))
            {
                if (dlg.ShowDialog(this) != DialogResult.OK)
                    return;

                try
                {
                    _userRepository.Insert(user);
                    LoadUsers();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        @"Error saving user:

" + ex.Message,
                        @"PayPulse – Users",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private void EditSelected()
        {
            var user = GetSelectedUser();
            if (user == null)
                return;

            // Create a working copy so cancel will not change the original instance.
            var editable = new AppUser
            {
                UserId = user.UserId,
                UserName = user.UserName,
                DisplayName = user.DisplayName,
                Password = user.Password,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                LastLoginAt = user.LastLoginAt,
                CreatedByUserId = user.CreatedByUserId,
                UpdatedByUserId = _currentUserContext.CurrentUser?.UserId,
                MustChangePassword = user.MustChangePassword
            };

            using (var dlg = new UserEditForm(editable, isNew: false))
            {
                if (dlg.ShowDialog(this) != DialogResult.OK)
                    return;

                try
                {
                    _userRepository.Update(editable);
                    LoadUsers();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        @"Error updating user:

" + ex.Message,
                        @"PayPulse – Users",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private void DeleteSelected()
        {
            var user = GetSelectedUser();
            if (user == null)
                return;

            if (MessageBox.Show(
                    $@"Delete user '{user.UserName}'?",
                    @"PayPulse – Users",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                _userRepository.Delete(user.UserId);
                LoadUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    @"Error deleting user:

" + ex.Message,
                    @"PayPulse – Users",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        private void GridOnCellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (_grid.Columns[e.ColumnIndex].Name != "CreatedBy" &&
                _grid.Columns[e.ColumnIndex].Name != "UpdatedBy")
            {
                return;
            }

            if (e.RowIndex < 0 || e.RowIndex >= _binding.Count)
            {
                return;
            }

            if (!(_binding[e.RowIndex] is AppUser user))
            {
                return;
            }

            if (_grid.Columns[e.ColumnIndex].Name == "CreatedBy")
            {
                e.Value = ResolveUserName(user.CreatedByUserId);
            }
            else if (_grid.Columns[e.ColumnIndex].Name == "UpdatedBy")
            {
                e.Value = ResolveUserName(user.UpdatedByUserId);
            }

            e.FormattingApplied = true;
        }

        private string ResolveUserName(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return string.Empty;
            }

            var users = _binding.List.Cast<AppUser>().ToList();
            var u = users.FirstOrDefault(x => x.UserId == userId);
            return u?.UserName;
        }


        private void GridOnSelectionChanged(object sender, EventArgs e)
        {
            if (_binding?.Current is AppUser user)
            {
                UpdateDetailPanel(user);
            }
            else
            {
                ClearDetailPanel();
            }
        }

        private void UpdateDetailPanel(AppUser user)
        {
            if (user == null)
            {
                ClearDetailPanel();
                return;
            }

            _lblCreatedAtValue.Text = user.CreatedAt.ToString("g");
            _lblUpdatedAtValue.Text = user.UpdatedAt.ToString("g");
            _lblCreatedByValue.Text = ResolveUserName(user.CreatedByUserId);
            _lblUpdatedByValue.Text = ResolveUserName(user.UpdatedByUserId);
            _lblLastLoginAtValue.Text = user.LastLoginAt.HasValue
                ? user.LastLoginAt.Value.ToString("g")
                : "(never)";
            _lblMustChangeValue.Text = user.MustChangePassword ? "Yes" : "No";
        }

        private void ClearDetailPanel()
        {
            if (_lblCreatedAtValue == null) return;

            _lblCreatedAtValue.Text = string.Empty;
            _lblUpdatedAtValue.Text = string.Empty;
            _lblCreatedByValue.Text = string.Empty;
            _lblUpdatedByValue.Text = string.Empty;
            _lblLastLoginAtValue.Text = string.Empty;
            _lblMustChangeValue.Text = string.Empty;
        }


    }
}