using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MDSync
{
    public class MainForm : Form
    {
        private TextBox txtGamePath = null!;
        private Button btnBrowse = null!;
        private Button btnRefresh = null!;
        private Button btnSync = null!;
        private ListView lvAccounts = null!;
        private StatusStrip statusStrip = null!;
        private ToolStripStatusLabel lblStatus = null!;

        private readonly List<AccountModel> accounts = new();
        private string? currentMasterId;

        public MainForm()
        {
            InitializeComponentCustom();
            Load += MainForm_Load;
            Shown += (s, e) => AutoFitLastColumn();
        }

        private void InitializeComponentCustom()
        {
            Text = "MD-Sync";
            Size = new Size(580, 420);
            MinimumSize = new Size(500, 360);
            StartPosition = FormStartPosition.CenterScreen;
            Font = SystemFonts.MessageBoxFont ?? new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            // 1. Thư mục cài đặt game
            var lblPath = new Label
            {
                Text = "Đường dẫn Master Duel:",
                Location = new Point(14, 14),
                AutoSize = true
            };
            Controls.Add(lblPath);

            txtGamePath = new TextBox
            {
                Location = new Point(14, 34),
                Size = new Size(450, 23),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            txtGamePath.TextChanged += (s, e) => ScanAccountsAsync();
            Controls.Add(txtGamePath);

            btnBrowse = new Button
            {
                Text = "Chọn...",
                Location = new Point(472, 33),
                Size = new Size(80, 25),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnBrowse.Click += BtnBrowse_Click;
            Controls.Add(btnBrowse);

            // 2. Danh sách tài khoản (ListView đơn giản, sạch sẽ)
            lvAccounts = new ListView
            {
                Location = new Point(14, 68),
                Size = new Size(538, 250),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                MultiSelect = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            lvAccounts.Columns.Add("Tài khoản (Folder)", 160);
            lvAccounts.Columns.Add("Dung lượng", 120);
            lvAccounts.Columns.Add("Trạng thái", 250);

            lvAccounts.Resize += (s, e) => AutoFitLastColumn();
            Controls.Add(lvAccounts);

            // 3. Nút bấm hành động
            btnSync = new Button
            {
                Text = "Đồng bộ (Sync)",
                Location = new Point(14, 328),
                Size = new Size(130, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            btnSync.Click += BtnSync_Click;
            Controls.Add(btnSync);

            btnRefresh = new Button
            {
                Text = "Làm mới",
                Location = new Point(152, 328),
                Size = new Size(90, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            btnRefresh.Click += (s, e) => ScanAccountsAsync();
            Controls.Add(btnRefresh);

            // 4. Thanh trạng thái ở đáy
            statusStrip = new StatusStrip();
            lblStatus = new ToolStripStatusLabel
            {
                Text = "Sẵn sàng"
            };
            statusStrip.Items.Add(lblStatus);
            Controls.Add(statusStrip);
        }

        private void AutoFitLastColumn()
        {
            if (lvAccounts.Columns.Count >= 3)
            {
                int remaining = lvAccounts.ClientSize.Width - lvAccounts.Columns[0].Width - lvAccounts.Columns[1].Width;
                if (remaining > 50)
                {
                    lvAccounts.Columns[2].Width = remaining;
                }
            }
        }

        private void MainForm_Load(object? sender, EventArgs e)
        {
            var detected = SteamHelper.AutoDetectGamePath();
            if (!string.IsNullOrEmpty(detected))
            {
                txtGamePath.Text = detected;
            }
            else
            {
                lblStatus.Text = "Không tự tìm thấy game. Vui lòng bấm 'Chọn...'";
            }
        }

        private void BtnBrowse_Click(object? sender, EventArgs e)
        {
            using var fbd = new FolderBrowserDialog
            {
                Description = "Chọn thư mục game Yu-Gi-Oh! Master Duel"
            };
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                txtGamePath.Text = fbd.SelectedPath;
            }
        }

        private string GetLocalDataPath()
        {
            return Path.Combine(txtGamePath.Text.Trim(), "LocalData");
        }

        private async void ScanAccountsAsync()
        {
            var localData = GetLocalDataPath();
            if (string.IsNullOrWhiteSpace(txtGamePath.Text) || !Directory.Exists(localData))
            {
                accounts.Clear();
                lvAccounts.Items.Clear();
                lblStatus.Text = "Không tìm thấy thư mục LocalData.";
                return;
            }

            lblStatus.Text = "Đang quét danh sách tài khoản...";
            btnRefresh.Enabled = false;
            btnSync.Enabled = false;

            try
            {
                await Task.Run(() =>
                {
                    accounts.Clear();
                    var dirs = Directory.GetDirectories(localData);

                    foreach (var dir in dirs)
                    {
                        var id = Path.GetFileName(dir);
                        var acc = new AccountModel
                        {
                            AccountId = id,
                            FullPath = dir
                        };

                        if (acc.HasAssetFolder)
                        {
                            acc.AssetSize = JunctionHelper.GetDirectoryPhysicalSize(acc.AssetPath);
                        }

                        accounts.Add(acc);
                    }

                    var master = accounts.OrderByDescending(a => a.AssetSize).FirstOrDefault();
                    if (master != null && master.AssetSize > 0)
                    {
                        master.IsMaster = true;
                        currentMasterId = master.AccountId;
                    }
                    else
                    {
                        currentMasterId = null;
                    }
                });

                lvAccounts.Items.Clear();
                foreach (var acc in accounts)
                {
                    var item = new ListViewItem(acc.AccountId);
                    item.SubItems.Add(acc.IsAssetLinked ? "Liên kết" : JunctionHelper.FormatBytes(acc.AssetSize));

                    string statusText = acc.IsMaster
                        ? "Chính (Master)"
                        : (acc.IsAssetLinked ? "Đã liên kết" : "Chưa liên kết");

                    item.SubItems.Add(statusText);
                    lvAccounts.Items.Add(item);
                }

                if (currentMasterId != null)
                {
                    lblStatus.Text = $"Đã tìm thấy {accounts.Count} tài khoản. Tài khoản chính: [{currentMasterId}].";
                }
                else
                {
                    lblStatus.Text = $"Tìm thấy {accounts.Count} tài khoản (chưa có dữ liệu tải về).";
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"Lỗi: {ex.Message}";
            }
            finally
            {
                btnRefresh.Enabled = true;
                btnSync.Enabled = true;
            }
        }

        private async void BtnSync_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(currentMasterId))
            {
                MessageBox.Show("Không tìm thấy tài khoản chính có dữ liệu!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var masterAcc = accounts.FirstOrDefault(a => a.AccountId == currentMasterId);
            if (masterAcc == null || !Directory.Exists(masterAcc.AssetPath))
            {
                MessageBox.Show("Thư mục dữ liệu chính (0000) không tồn tại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var targets = accounts.Where(a => a.AccountId != currentMasterId).ToList();
            if (targets.Count == 0)
            {
                MessageBox.Show("Chưa có tài khoản phụ nào trong LocalData để đồng bộ.\n(Hãy đăng nhập acc Steam mới vào game trước để game tạo thư mục).", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            btnSync.Enabled = false;
            btnRefresh.Enabled = false;
            lblStatus.Text = "Đang đồng bộ dữ liệu...";

            int success = 0;
            int failed = 0;

            await Task.Run(() =>
            {
                foreach (var target in targets)
                {
                    try
                    {
                        // 1. Link 0000
                        var (assetOk, _) = JunctionHelper.CreateJunction(target.AssetPath, masterAcc.AssetPath);
                        if (!assetOk)
                        {
                            failed++;
                            continue;
                        }

                        // 2. Link localsave (ngầm)
                        if (Directory.Exists(masterAcc.LocalSavePath))
                        {
                            JunctionHelper.CreateJunction(target.LocalSavePath, masterAcc.LocalSavePath);
                        }

                        success++;
                    }
                    catch
                    {
                        failed++;
                    }
                }
            });

            ScanAccountsAsync();

            if (failed == 0)
            {
                lblStatus.Text = $"Đã đồng bộ thành công {success} tài khoản.";
                MessageBox.Show($"Đồng bộ thành công {success} tài khoản!\nBạn có thể vào game chơi ngay.", "Hoàn tất", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                lblStatus.Text = $"Đồng bộ xong ({success} thành công, {failed} lỗi).";
                MessageBox.Show($"Có {failed} tài khoản gặp lỗi trong quá trình liên kết.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
