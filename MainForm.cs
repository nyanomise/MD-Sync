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
        private Button btnSetMaster = null!;
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

            // 1. Game Directory
            var lblPath = new Label
            {
                Text = "Game Directory:",
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
                Text = "Browse...",
                Location = new Point(472, 33),
                Size = new Size(80, 25),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnBrowse.Click += BtnBrowse_Click;
            Controls.Add(btnBrowse);

            // 2. Accounts List
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
            lvAccounts.Columns.Add("Account ID", 160);
            lvAccounts.Columns.Add("Size", 120);
            lvAccounts.Columns.Add("Status", 250);

            lvAccounts.Resize += (s, e) => AutoFitLastColumn();
            lvAccounts.SelectedIndexChanged += (s, e) => UpdateSetMasterButtonState();
            Controls.Add(lvAccounts);

            // 3. Action Buttons
            btnSync = new Button
            {
                Text = "Sync All",
                Location = new Point(14, 328),
                Size = new Size(100, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            btnSync.Click += BtnSync_Click;
            Controls.Add(btnSync);

            btnSetMaster = new Button
            {
                Text = "Set as Master",
                Location = new Point(122, 328),
                Size = new Size(110, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                Enabled = false
            };
            btnSetMaster.Click += BtnSetMaster_Click;
            Controls.Add(btnSetMaster);

            btnRefresh = new Button
            {
                Text = "Refresh",
                Location = new Point(240, 328),
                Size = new Size(90, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            btnRefresh.Click += (s, e) => ScanAccountsAsync();
            Controls.Add(btnRefresh);

            // 4. Status Bar
            statusStrip = new StatusStrip();
            lblStatus = new ToolStripStatusLabel
            {
                Text = "Ready"
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
                lblStatus.Text = "Game path not found automatically. Please click 'Browse...'";
            }
        }

        private void BtnBrowse_Click(object? sender, EventArgs e)
        {
            using var fbd = new FolderBrowserDialog
            {
                Description = "Select Yu-Gi-Oh! Master Duel installation directory"
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
                lblStatus.Text = "LocalData directory not found.";
                return;
            }

            lblStatus.Text = "Scanning accounts...";
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
                    item.SubItems.Add(acc.IsAssetLinked ? "Linked" : JunctionHelper.FormatBytes(acc.AssetSize));
                    item.SubItems.Add(acc.DisplayStatus);
                    lvAccounts.Items.Add(item);
                }

                if (currentMasterId != null)
                {
                    lblStatus.Text = $"Found {accounts.Count} account(s). Master: [{currentMasterId}].";
                }
                else
                {
                    lblStatus.Text = $"Found {accounts.Count} account(s) (no downloaded game assets found).";
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"Error: {ex.Message}";
            }
            finally
            {
                btnRefresh.Enabled = true;
                btnSync.Enabled = true;
                UpdateSetMasterButtonState();
            }
        }

        private void UpdateSetMasterButtonState()
        {
            if (lvAccounts.SelectedItems.Count == 0)
            {
                btnSetMaster.Enabled = false;
                return;
            }

            var selectedId = lvAccounts.SelectedItems[0].Text;
            btnSetMaster.Enabled = !string.IsNullOrEmpty(currentMasterId) && selectedId != currentMasterId;
        }

        private async void BtnSetMaster_Click(object? sender, EventArgs e)
        {
            if (lvAccounts.SelectedItems.Count == 0)
                return;

            var selectedId = lvAccounts.SelectedItems[0].Text;
            if (string.IsNullOrEmpty(selectedId) || selectedId == currentMasterId)
                return;

            var newMaster = accounts.FirstOrDefault(a => a.AccountId == selectedId);
            var currentMaster = accounts.FirstOrDefault(a => a.AccountId == currentMasterId);

            if (newMaster == null || currentMaster == null)
                return;

            if (!Directory.Exists(currentMaster.AssetPath) || JunctionHelper.IsReparsePoint(currentMaster.AssetPath))
            {
                MessageBox.Show("Current master does not have a physical data directory (0000)!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var mdProcesses = Process.GetProcessesByName("masterduel");
            if (mdProcesses.Length > 0)
            {
                MessageBox.Show("Please close Yu-Gi-Oh! Master Duel before changing master data.", "Game Running", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show(
                $"Are you sure you want to transfer master data from [{currentMaster.AccountId}] to [{newMaster.AccountId}]?\n\n" +
                $"• Physical game data will be moved to [{newMaster.AccountId}].\n" +
                $"• [{currentMaster.AccountId}] and all other accounts will link to [{newMaster.AccountId}].",
                "Confirm Master Switch",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (confirm != DialogResult.Yes)
                return;

            btnSync.Enabled = false;
            btnSetMaster.Enabled = false;
            btnRefresh.Enabled = false;
            lblStatus.Text = $"Transferring master data to [{newMaster.AccountId}]...";

            bool success = false;
            string errorMessage = string.Empty;

            await Task.Run(() =>
            {
                try
                {
                    // 1. Remove junction or dummy data in newMaster's folders
                    JunctionHelper.DeleteDirectoryOrJunction(newMaster.AssetPath);
                    JunctionHelper.DeleteDirectoryOrJunction(newMaster.LocalSavePath);

                    // 2. Move physical 0000 data from currentMaster to newMaster
                    Directory.Move(currentMaster.AssetPath, newMaster.AssetPath);

                    // 3. Move physical localsave if it exists
                    if (Directory.Exists(currentMaster.LocalSavePath) && !JunctionHelper.IsReparsePoint(currentMaster.LocalSavePath))
                    {
                        Directory.Move(currentMaster.LocalSavePath, newMaster.LocalSavePath);
                    }
                    else if (Directory.Exists(currentMaster.LocalSavePath))
                    {
                        JunctionHelper.DeleteDirectoryOrJunction(currentMaster.LocalSavePath);
                    }

                    // 4. Create junction from old master to new master
                    JunctionHelper.CreateJunction(currentMaster.AssetPath, newMaster.AssetPath);
                    if (Directory.Exists(newMaster.LocalSavePath))
                    {
                        JunctionHelper.CreateJunction(currentMaster.LocalSavePath, newMaster.LocalSavePath);
                    }

                    // 5. Re-link all other accounts to new master
                    var others = accounts.Where(a => a.AccountId != newMaster.AccountId && a.AccountId != currentMaster.AccountId);
                    foreach (var other in others)
                    {
                        JunctionHelper.CreateJunction(other.AssetPath, newMaster.AssetPath);
                        if (Directory.Exists(newMaster.LocalSavePath))
                        {
                            JunctionHelper.CreateJunction(other.LocalSavePath, newMaster.LocalSavePath);
                        }
                    }

                    success = true;
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                }
            });

            if (success)
            {
                lblStatus.Text = $"Master data transferred to [{newMaster.AccountId}].";
                MessageBox.Show($"Successfully set [{newMaster.AccountId}] as the new Master account!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                lblStatus.Text = $"Error transferring master: {errorMessage}";
                MessageBox.Show($"Failed to transfer master data: {errorMessage}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            ScanAccountsAsync();
        }

        private async void BtnSync_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(currentMasterId))
            {
                MessageBox.Show("No master account with downloaded data found!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var masterAcc = accounts.FirstOrDefault(a => a.AccountId == currentMasterId);
            if (masterAcc == null || !Directory.Exists(masterAcc.AssetPath))
            {
                MessageBox.Show("Master data directory (0000) does not exist!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var targets = accounts.Where(a => a.AccountId != currentMasterId).ToList();
            if (targets.Count == 0)
            {
                MessageBox.Show("No secondary accounts found in LocalData to sync.\n(Log into your new Steam account and launch the game once to create its folder first).", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            btnSync.Enabled = false;
            btnRefresh.Enabled = false;
            lblStatus.Text = "Synchronizing data...";

            int success = 0;
            int failed = 0;

            await Task.Run(() =>
            {
                foreach (var target in targets)
                {
                    try
                    {
                        // 1. Link 0000 (game assets)
                        var (assetOk, _) = JunctionHelper.CreateJunction(target.AssetPath, masterAcc.AssetPath);
                        if (!assetOk)
                        {
                            failed++;
                            continue;
                        }

                        // 2. Link localsave (settings) silently
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
                lblStatus.Text = $"Synchronized {success} account(s) successfully.";
                MessageBox.Show($"Successfully synchronized {success} account(s)!\nYou can now launch the game.", "Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                lblStatus.Text = $"Sync finished ({success} succeeded, {failed} failed).";
                MessageBox.Show($"{failed} account(s) failed to link.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
