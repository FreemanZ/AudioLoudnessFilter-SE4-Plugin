using System;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    /// <summary>
    /// FFmpeg自动安装器
    /// </summary>
    public static class FFmpegInstaller
    {
        public const string FFmpegDownloadUrl = "https://github.com/BtbN/FFmpeg-Builds/releases/download/latest/ffmpeg-master-latest-win64-gpl.zip";
        public const string FFmpegDirectory = "FFmpeg";

        /// <summary>
        /// 检查FFmpeg是否已安装
        /// </summary>
        public static bool IsFFmpegInstalled()
        {
            try
            {
                // 检查系统PATH中是否有ffmpeg
                var processInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = "-version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (var process = new System.Diagnostics.Process { StartInfo = processInfo })
                {
                    process.Start();
                    process.WaitForExit(3000); // 3秒超时
                    return process.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 获取FFmpeg安装路径
        /// </summary>
        public static string GetFFmpegPath()
        {
            if (IsFFmpegInstalled())
            {
                return "ffmpeg"; // 在PATH中
            }

            // 检查本地安装目录
            var localPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Subtitle Edit", FFmpegDirectory);
            var ffmpegExe = Path.Combine(localPath, "ffmpeg.exe");
            
            if (File.Exists(ffmpegExe))
            {
                return ffmpegExe;
            }

            return null;
        }

        /// <summary>
        /// 显示安装对话框
        /// </summary>
        public static bool ShowInstallDialog(Form parentForm)
        {
            var result = MessageBox.Show(
                "FFmpeg未安装或未找到。\n\n" +
                "此插件需要FFmpeg来进行音频响度分析。\n" +
                "是否现在自动下载并安装FFmpeg？\n\n" +
                "注意：下载大小约50MB，需要网络连接。",
                "FFmpeg安装",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1);

            if (result == DialogResult.Yes)
            {
                return InstallFFmpeg(parentForm);
            }

            return false;
        }

        /// <summary>
        /// 安装FFmpeg
        /// </summary>
        private static bool InstallFFmpeg(Form parentForm)
        {
            try
            {
                var installForm = new FFmpegInstallForm();
                return installForm.ShowDialog(parentForm) == DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"安装FFmpeg时发生错误：\n{ex.Message}",
                    "安装失败",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
        }
    }

    /// <summary>
    /// FFmpeg安装进度窗体
    /// </summary>
    public partial class FFmpegInstallForm : Form
    {
        private ProgressBar progressBar;
        private Label statusLabel;
        private Button cancelButton;
        private WebClient webClient;
        private bool isCancelled = false;

        public FFmpegInstallForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "FFmpeg安装";
            this.Size = new System.Drawing.Size(400, 150);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            statusLabel = new Label
            {
                Text = "准备下载FFmpeg...",
                Location = new System.Drawing.Point(12, 12),
                Size = new System.Drawing.Size(360, 20)
            };

            progressBar = new ProgressBar
            {
                Location = new System.Drawing.Point(12, 40),
                Size = new System.Drawing.Size(360, 23),
                Style = ProgressBarStyle.Marquee
            };

            cancelButton = new Button
            {
                Text = "取消",
                Location = new System.Drawing.Point(297, 75),
                Size = new System.Drawing.Size(75, 23)
            };
            cancelButton.Click += CancelButton_Click;

            this.Controls.Add(statusLabel);
            this.Controls.Add(progressBar);
            this.Controls.Add(cancelButton);

            // 开始下载
            StartDownload();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            isCancelled = true;
            webClient?.CancelAsync();
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private string zipFilePath;

        private void StartDownload()
        {
            try
            {
                statusLabel.Text = "正在下载FFmpeg...";
                progressBar.Style = ProgressBarStyle.Continuous;
                progressBar.Value = 0;

                var tempFile = Path.GetTempFileName();
                zipFilePath = tempFile + ".zip";

                webClient = new WebClient();
                webClient.DownloadProgressChanged += WebClient_DownloadProgressChanged;
                webClient.DownloadFileCompleted += WebClient_DownloadFileCompleted;

                webClient.DownloadFileAsync(new Uri(FFmpegInstaller.FFmpegDownloadUrl), zipFilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"下载失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

        private void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (isCancelled) return;

            progressBar.Value = e.ProgressPercentage;
            statusLabel.Text = $"正在下载FFmpeg... {e.ProgressPercentage}% ({e.BytesReceived / 1024 / 1024}MB / {e.TotalBytesToReceive / 1024 / 1024}MB)";
        }

        private void WebClient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (isCancelled) return;

            if (e.Cancelled)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
                return;
            }

            if (e.Error != null)
            {
                MessageBox.Show($"下载失败：{e.Error.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.Cancel;
                this.Close();
                return;
            }

            try
            {
                statusLabel.Text = "正在解压FFmpeg...";
                progressBar.Style = ProgressBarStyle.Marquee;

                var installDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Subtitle Edit", FFmpegInstaller.FFmpegDirectory);

                // 创建安装目录
                if (Directory.Exists(installDir))
                {
                    Directory.Delete(installDir, true);
                }
                Directory.CreateDirectory(installDir);

                // 解压文件
                using (var archive = ZipFile.OpenRead(zipFilePath))
                {
                    foreach (var entry in archive.Entries)
                    {
                        // 提取所有.exe文件（ffmpeg.exe, ffprobe.exe等）
                        if (entry.Name.EndsWith(".exe"))
                        {
                            var extractPath = Path.Combine(installDir, entry.Name);
                            entry.ExtractToFile(extractPath, true);
                        }
                    }
                }

                // 清理临时文件
                if (File.Exists(zipFilePath))
                {
                    File.Delete(zipFilePath);
                }

                statusLabel.Text = "安装完成！";
                progressBar.Style = ProgressBarStyle.Continuous;
                progressBar.Value = 100;

                MessageBox.Show(
                    "FFmpeg安装成功！\n\n" +
                    "安装位置：" + installDir + "\n" +
                    "现在可以使用音频响度分析功能了。",
                    "安装完成",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"安装失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }
    }
}