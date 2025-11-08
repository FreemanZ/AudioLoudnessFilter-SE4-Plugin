using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;

namespace Nikse.SubtitleEdit.PluginLogic
{
    /// <summary>
    /// .NET Framework 4.8版本的音频响度过滤器插件
    /// </summary>
    public class AudioLoudnessFilter : IPlugin
    {
        public string Name => "音频响度过滤器";
        public string Text => "音频响度过滤器";
        public decimal Version => 2.0m;
        public string Description => "基于FFmpeg的音频响度过滤器，删除低响度字幕行";
        public string ActionType => "Tool";
        public string Shortcut => string.Empty;

        public string DoAction(Form parentForm, string subtitle, double frameRate, string listViewLineSeparatorString, string subtitleFileName, string videoFileName, string audioFileName)
        {
            try
            {
                // 记录插件启动信息
                LogToFile($"插件启动: {Name} v{Version}");
                LogToFile($"字幕文件: {subtitleFileName}");
                LogToFile($"视频文件: {videoFileName}");
                LogToFile($"音频文件: {audioFileName}");

                // 检查FFmpeg是否可用（只在初次使用时提示）
                LogToFile("检查FFmpeg可用性...");
                if (!IsFFmpegAvailable())
                {
                    LogToFile("FFmpeg不可用，尝试自动安装");
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
                        if (!FFmpegInstaller.ShowInstallDialog(parentForm))
                        {
                            LogToFile("用户取消FFmpeg安装");
                            return subtitle; // 用户取消安装
                        }
                        LogToFile("FFmpeg安装完成");
                        
                        // 重新检查FFmpeg是否可用
                        if (!IsFFmpegAvailable())
                        {
                            LogToFile("FFmpeg安装后仍然不可用");
                            return subtitle;
                        }
                    }
                    else
                    {
                        LogToFile("用户拒绝安装FFmpeg");
                        return subtitle;
                    }
                }
                else
                {
                    LogToFile("FFmpeg已可用");
                }

                // 显示插件窗口
                using (var form = new AudioLoudnessForm(subtitle, subtitleFileName, videoFileName, audioFileName))
                {
                    if (form.ShowDialog(parentForm) == DialogResult.OK)
                    {
                        return form.ModifiedSubtitle;
                    }
                }

                return subtitle;
            }
            catch (Exception ex)
            {
                LogToFile($"插件异常: {ex.Message}");
                LogToFile($"异常堆栈: {ex.StackTrace}");
                
                MessageBox.Show(
                    $"插件发生异常: {ex.Message}\n\n" +
                    $"详细信息已记录到日志文件。",
                    "插件异常",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                
                return subtitle;
            }
        }

        private void LogToFile(string message)
        {
            try
            {
                var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Subtitle Edit", "Plugins", "AudioLoudnessFilter_Net48.log");
                var logDir = Path.GetDirectoryName(logPath);
                
                if (!Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }

                var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
                File.AppendAllText(logPath, logEntry + Environment.NewLine);
            }
            catch
            {
                // 忽略日志写入错误
            }
        }

        private bool IsFFmpegAvailable()
        {
            try
            {
                var ffmpegPath = GetFFmpegExecutablePath();
                
                // 如果返回的是"ffmpeg"，说明在PATH中
                if (ffmpegPath == "ffmpeg")
                {
                    var processInfo = new ProcessStartInfo
                    {
                        FileName = "ffmpeg",
                        Arguments = "-version",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };

                    using (var process = new Process { StartInfo = processInfo })
                    {
                        process.Start();
                        process.WaitForExit(5000); // 5秒超时
                        return process.ExitCode == 0;
                    }
                }
                else
                {
                    // 检查具体路径的文件是否存在
                    return File.Exists(ffmpegPath);
                }
            }
            catch
            {
                return false;
            }
        }

        private string GetFFmpegExecutablePath()
        {
            // 首先尝试系统PATH中的ffmpeg
            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = "-version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (var process = new Process { StartInfo = processInfo })
                {
                    process.Start();
                    process.WaitForExit(1000); // 1秒超时
                    if (process.ExitCode == 0)
                    {
                        return "ffmpeg";
                    }
                }
            }
            catch { }

            // 如果系统PATH中没有，使用本地安装
            var localFFmpeg = FFmpegInstaller.GetFFmpegPath();
            if (!string.IsNullOrEmpty(localFFmpeg) && File.Exists(localFFmpeg))
            {
                return localFFmpeg;
            }

            // 尝试常见的安装路径
            var commonPaths = new[]
            {
                @"C:\ffmpeg\bin\ffmpeg.exe",
                @"C:\Program Files\ffmpeg\bin\ffmpeg.exe",
                @"C:\Program Files (x86)\ffmpeg\bin\ffmpeg.exe",
                @"C:\tools\ffmpeg\bin\ffmpeg.exe",
                @"C:\bin\ffmpeg.exe"
            };

            foreach (var path in commonPaths)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }

            return "ffmpeg"; // 默认返回
        }
    }

    /// <summary>
    /// 音频响度分析器 - .NET Framework 4.8版本
    /// </summary>
    public class AudioLoudnessAnalyzer
    {
        private static readonly object lockObject = new object();

        public double GetLoudness(string audioFilePath, double startTimeMs, double endTimeMs)
        {
            if (string.IsNullOrEmpty(audioFilePath) || !File.Exists(audioFilePath))
            {
                throw new FileNotFoundException($"音频文件不存在: {audioFilePath}");
            }

            if (startTimeMs < 0 || endTimeMs <= startTimeMs)
            {
                throw new ArgumentException("时间参数无效");
            }

            try
            {
                // 首先尝试使用SubtitleEdit的波形数据
                var loudness = GetLoudnessFromWaveformData(audioFilePath, startTimeMs, endTimeMs);
                if (loudness != -60.0) // 如果成功获取到有效数据
                {
                    return loudness;
                }

                // 如果波形数据不可用，回退到FFmpeg
                return GetLoudnessWithFFmpeg(audioFilePath, startTimeMs, endTimeMs);
            }
            catch (Exception ex)
            {
                throw new Exception($"音频响度分析失败: {ex.Message}", ex);
            }
        }

        private double GetLoudnessFromWaveformData(string audioFilePath, double startTimeMs, double endTimeMs)
        {
            try
            {
                // 使用反射尝试访问SubtitleEdit的波形数据
                var coreAssembly = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == "Nikse.SubtitleEdit.Core");

                if (coreAssembly == null)
                {
                    return -60.0; // SubtitleEdit.Core程序集未找到
                }

                // 获取WavePeakGenerator类型
                var wavePeakGeneratorType = coreAssembly.GetType("Nikse.SubtitleEdit.Core.Common.WavePeakGenerator");
                if (wavePeakGeneratorType == null)
                {
                    return -60.0; // WavePeakGenerator类型未找到
                }

                // 调用GetPeakWaveFileName静态方法
                var getPeakWaveFileNameMethod = wavePeakGeneratorType.GetMethod("GetPeakWaveFileName", 
                    new[] { typeof(string) });
                if (getPeakWaveFileNameMethod == null)
                {
                    return -60.0; // 方法未找到
                }

                var peakFileName = (string)getPeakWaveFileNameMethod.Invoke(null, new object[] { audioFilePath });
                if (!File.Exists(peakFileName))
                {
                    return -60.0; // 波形文件不存在
                }

                // 获取WavePeakData类型
                var wavePeakDataType = coreAssembly.GetType("Nikse.SubtitleEdit.Core.Common.WavePeakData");
                if (wavePeakDataType == null)
                {
                    return -60.0; // WavePeakData类型未找到
                }

                // 调用FromDisk静态方法
                var fromDiskMethod = wavePeakDataType.GetMethod("FromDisk", new[] { typeof(string) });
                if (fromDiskMethod == null)
                {
                    return -60.0; // 方法未找到
                }

                var wavePeakData = fromDiskMethod.Invoke(null, new object[] { peakFileName });
                if (wavePeakData == null)
                {
                    return -60.0; // 波形数据无效
                }

                // 获取Peaks属性
                var peaksProperty = wavePeakDataType.GetProperty("Peaks");
                if (peaksProperty == null)
                {
                    return -60.0; // Peaks属性未找到
                }

                var peaks = peaksProperty.GetValue(wavePeakData);
                if (peaks == null)
                {
                    return -60.0; // Peaks为空
                }

                // 获取SampleRate属性
                var sampleRateProperty = wavePeakDataType.GetProperty("SampleRate");
                if (sampleRateProperty == null)
                {
                    return -60.0; // SampleRate属性未找到
                }

                var sampleRate = (int)sampleRateProperty.GetValue(wavePeakData);

                // 计算时间段内的峰值
                var startSeconds = startTimeMs / 1000.0;
                var endSeconds = endTimeMs / 1000.0;

                // 计算对应的样本索引
                var startSampleIndex = (int)(startSeconds * sampleRate);
                var endSampleIndex = (int)(endSeconds * sampleRate);

                // 获取Peaks集合的Count
                var countProperty = peaks.GetType().GetProperty("Count");
                if (countProperty == null)
                {
                    return -60.0; // Count属性未找到
                }

                var peakCount = (int)countProperty.GetValue(peaks);

                // 确保索引在有效范围内
                startSampleIndex = Math.Max(0, Math.Min(startSampleIndex, peakCount - 1));
                endSampleIndex = Math.Max(startSampleIndex, Math.Min(endSampleIndex, peakCount - 1));

                if (startSampleIndex >= endSampleIndex)
                {
                    return -60.0; // 时间段太短
                }

                // 计算该时间段内的平均峰值
                double totalPeak = 0.0;
                int sampleCount = 0;

                // 获取索引器
                var indexer = peaks.GetType().GetProperty("Item");
                if (indexer == null)
                {
                    return -60.0; // 索引器未找到
                }

                for (int i = startSampleIndex; i <= endSampleIndex; i++)
                {
                    var peak = indexer.GetValue(peaks, new object[] { i });
                    if (peak != null)
                    {
                        // 获取Abs属性
                        var absProperty = peak.GetType().GetProperty("Abs");
                        if (absProperty != null)
                        {
                            var absValue = (int)absProperty.GetValue(peak);
                            totalPeak += absValue;
                            sampleCount++;
                        }
                    }
                }

                if (sampleCount == 0)
                {
                    return -60.0;
                }

                var averagePeak = totalPeak / sampleCount;

                // 将峰值转换为dB值
                // 假设最大峰值为32767 (16位音频的最大值)
                var maxPeak = 32767.0;
                var normalizedPeak = averagePeak / maxPeak;

                // 转换为dB，避免log(0)
                if (normalizedPeak <= 0)
                {
                    return -60.0; // 静音
                }

                var dbValue = 20.0 * Math.Log10(normalizedPeak);
                return Math.Max(-60.0, dbValue); // 限制最小值为-60dB
            }
            catch
            {
                return -60.0; // 发生错误时返回默认值
            }
        }

        private double GetLoudnessWithFFmpeg(string filePath, double startTimeMs, double endTimeMs)
        {
            lock (lockObject)
            {
                // 检查ffmpeg是否可用
                if (!IsFFmpegAvailable())
                {
                    throw new Exception($"FFmpeg未找到，请确保FFmpeg已安装并在PATH中，或使用插件的自动安装功能");
                }

                // 转换时间为秒
                var startTimeSeconds = startTimeMs / 1000.0;
                var durationSeconds = (endTimeMs - startTimeMs) / 1000.0;

                // 确保时间段至少为0.1秒
                if (durationSeconds < 0.1)
                {
                    durationSeconds = 0.1;
                }

                // 首先尝试使用astats滤镜（更可靠）
                var loudness = GetLoudnessWithAstats(filePath, startTimeMs, endTimeMs);
                if (loudness != -60.0)
                {
                    return loudness;
                }

                // 如果astats失败，尝试使用loudnorm
                return GetLoudnessWithLoudnorm(filePath, startTimeMs, endTimeMs);
            }
        }

        private double GetLoudnessWithLoudnorm(string filePath, double startTimeMs, double endTimeMs)
        {
            try
            {
                // 转换时间为秒
                var startTimeSeconds = startTimeMs / 1000.0;
                var durationSeconds = (endTimeMs - startTimeMs) / 1000.0;

                // 确保时间段至少为0.1秒
                if (durationSeconds < 0.1)
                {
                    durationSeconds = 0.1;
                }

                // 使用ffmpeg的loudnorm滤镜
                var arguments = $"-ss {startTimeSeconds:F3} -i \"{filePath}\" -t {durationSeconds:F3} -af loudnorm=print_format=json -f null -";

                var ffmpegPath = GetFFmpegExecutablePath();
                var processInfo = new ProcessStartInfo
                {
                    FileName = ffmpegPath,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (var process = new Process { StartInfo = processInfo })
                {
                    process.Start();
                    var output = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (process.ExitCode != 0)
                    {
                        return -60.0;
                    }

                    // 解析loudnorm输出
                    return ParseFFmpegLoudnormOutput(output);
                }
            }
            catch
            {
                return -60.0;
            }
        }

        private double GetLoudnessWithAstats(string filePath, double startTimeMs, double endTimeMs)
        {
            try
            {
                // 转换时间为秒
                var startTimeSeconds = startTimeMs / 1000.0;
                var durationSeconds = (endTimeMs - startTimeMs) / 1000.0;

                // 确保时间段至少为0.1秒
                if (durationSeconds < 0.1)
                {
                    durationSeconds = 0.1;
                }

                // 使用ffmpeg的astats滤镜
                var arguments = $"-ss {startTimeSeconds:F3} -i \"{filePath}\" -t {durationSeconds:F3} -af astats -f null -";

                var ffmpegPath = GetFFmpegExecutablePath();
                var processInfo = new ProcessStartInfo
                {
                    FileName = ffmpegPath,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (var process = new Process { StartInfo = processInfo })
                {
                    process.Start();
                    var output = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (process.ExitCode != 0)
                    {
                        return -60.0;
                    }

                    // 解析astats输出
                    return ParseAstatsOutput(output);
                }
            }
            catch
            {
                return -60.0;
            }
        }

        private double ParseAstatsOutput(string output)
        {
            try
            {
                if (string.IsNullOrEmpty(output))
                {
                    return -60.0;
                }

                // 查找RMS level dB（更精确的正则表达式）
                var rmsPattern = @"RMS level dB:\s*([-\d\.]+)";
                var match = Regex.Match(output, rmsPattern);
                
                if (match.Success && match.Groups.Count > 1)
                {
                    var rmsValue = match.Groups[1].Value.Trim();
                    
                    if (double.TryParse(rmsValue, out double rms))
                    {
                        return rms;
                    }
                }

                // 如果没有找到RMS，尝试查找Peak level dB
                var peakPattern = @"Peak level dB:\s*([-\d\.]+)";
                match = Regex.Match(output, peakPattern);
                
                if (match.Success && match.Groups.Count > 1)
                {
                    var peakValue = match.Groups[1].Value.Trim();
                    
                    if (double.TryParse(peakValue, out double peak))
                    {
                        return peak;
                    }
                }

                return -60.0;
            }
            catch
            {
                return -60.0;
            }
        }

        private bool IsFFmpegAvailable()
        {
            try
            {
                var ffmpegPath = GetFFmpegExecutablePath();
                
                // 如果返回的是"ffmpeg"，说明在PATH中
                if (ffmpegPath == "ffmpeg")
                {
                    var processInfo = new ProcessStartInfo
                    {
                        FileName = "ffmpeg",
                        Arguments = "-version",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };

                    using (var process = new Process { StartInfo = processInfo })
                    {
                        process.Start();
                        process.WaitForExit(5000); // 5秒超时
                        return process.ExitCode == 0;
                    }
                }
                else
                {
                    // 检查具体路径的文件是否存在
                    return File.Exists(ffmpegPath);
                }
            }
            catch
            {
                return false;
            }
        }

        private string GetFFmpegExecutablePath()
        {
            // 首先尝试系统PATH中的ffmpeg
            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = "-version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (var process = new Process { StartInfo = processInfo })
                {
                    process.Start();
                    process.WaitForExit(1000); // 1秒超时
                    if (process.ExitCode == 0)
                    {
                        return "ffmpeg";
                    }
                }
            }
            catch { }

            // 如果系统PATH中没有，使用本地安装
            var localFFmpeg = FFmpegInstaller.GetFFmpegPath();
            if (!string.IsNullOrEmpty(localFFmpeg) && File.Exists(localFFmpeg))
            {
                return localFFmpeg;
            }

            // 尝试常见的安装路径
            var commonPaths = new[]
            {
                @"C:\ffmpeg\bin\ffmpeg.exe",
                @"C:\Program Files\ffmpeg\bin\ffmpeg.exe",
                @"C:\Program Files (x86)\ffmpeg\bin\ffmpeg.exe",
                @"C:\tools\ffmpeg\bin\ffmpeg.exe",
                @"C:\bin\ffmpeg.exe"
            };

            foreach (var path in commonPaths)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }

            return "ffmpeg"; // 默认返回
        }

        private double ParseFFprobeLoudnessOutput(string output)
        {
            try
            {
                if (string.IsNullOrEmpty(output))
                {
                    return -60.0;
                }

                // ffprobe JSON输出格式解析
                // 查找 "lavfi.astats.Overall.RMS_level" 字段
                var rmsLevelPattern = "\"lavfi\\.astats\\.Overall\\.RMS_level\"\\s*:\\s*\"([^\"]+)\"";
                var match = System.Text.RegularExpressions.Regex.Match(output, rmsLevelPattern);
                
                if (match.Success && match.Groups.Count > 1)
                {
                    var rmsValue = match.Groups[1].Value.Trim();
                    
                    // 处理-inf值
                    if (rmsValue.Equals("-inf", StringComparison.OrdinalIgnoreCase))
                    {
                        return -60.0; // 静音
                    }
                    
                    // 尝试解析数值
                    if (double.TryParse(rmsValue, out double rms))
                    {
                        // RMS值已经是dB，直接返回
                        return rms;
                    }
                }

                // 如果没有找到JSON格式，尝试解析CSV格式（向后兼容）
                var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length > 0)
                {
                    var firstLine = lines[0].Trim();
                    
                    // 处理-inf值
                    if (firstLine.Equals("-inf", StringComparison.OrdinalIgnoreCase))
                    {
                        return -60.0; // 静音
                    }
                    
                    // 尝试解析数值
                    if (double.TryParse(firstLine, out double rms))
                    {
                        return rms;
                    }
                }

                // 如果没有找到有效值，返回默认值
                return -60.0;
            }
            catch
            {
                return -60.0;
            }
        }

        private double ParseFFmpegLoudnormOutput(string output)
        {
            try
            {
                if (string.IsNullOrEmpty(output))
                {
                    return -60.0;
                }

                // ffmpeg loudnorm JSON输出格式解析
                // 查找 "input_i" 字段（输入响度）
                var inputIPattern = "\"input_i\"\\s*:\\s*\"([^\"]+)\"";
                var match = Regex.Match(output, inputIPattern);
                
                if (match.Success && match.Groups.Count > 1)
                {
                    var inputIValue = match.Groups[1].Value.Trim();
                    
                    // 尝试解析数值
                    if (double.TryParse(inputIValue, out double inputI))
                    {
                        // input_i是响度值（dB），直接返回
                        return inputI;
                    }
                }

                // 如果没有找到input_i，尝试查找input_tp（真峰值）
                var inputTpPattern = "\"input_tp\"\\s*:\\s*\"([^\"]+)\"";
                match = Regex.Match(output, inputTpPattern);
                
                if (match.Success && match.Groups.Count > 1)
                {
                    var inputTpValue = match.Groups[1].Value.Trim();
                    
                    if (double.TryParse(inputTpValue, out double inputTp))
                    {
                        // 使用真峰值作为响度参考
                        return inputTp;
                    }
                }

                // 如果没有找到有效值，返回默认值
                return -60.0;
            }
            catch
            {
                // 记录错误但不使用LogToFile（因为不在主类中）
                return -60.0;
            }
        }
    }

    /// <summary>
    /// 音频响度过滤器窗体 - .NET Framework 4.8版本
    /// </summary>
    public class AudioLoudnessForm : Form
    {
        private Label statusLabel;
        private Button loadAudioButton;
        private Button testButton;
        private Button processButton;
        private Button pauseButton;
        private Button stopButton;
        private NumericUpDown thresholdNumericUpDown;
        private Label thresholdLabel;
        private ListBox logListBox;
        private ProgressBar progressBar;
        private CheckBox previewCheckBox;

        // 处理控制变量
        private bool isPaused = false;
        private bool shouldStop = false;

        private string originalSubtitle;
        private string subtitleFileName;
        private string videoFileName;
        private string audioFileName;
        private string currentAudioSource;

        public string ModifiedSubtitle { get; private set; }

        public AudioLoudnessForm(string subtitle, string subtitleFileName, string videoFileName, string audioFileName)
        {
            this.originalSubtitle = subtitle ?? string.Empty;
            this.subtitleFileName = subtitleFileName ?? string.Empty;
            this.videoFileName = videoFileName ?? string.Empty;
            this.audioFileName = audioFileName ?? string.Empty;
            this.ModifiedSubtitle = this.originalSubtitle;

            InitializeComponent();
            SetupEventHandlers();
            LoadInitialAudioSource();
        }

        private void InitializeComponent()
        {
            this.Text = "音频响度过滤器 (.NET Framework 4.8)";
            this.Size = new System.Drawing.Size(650, 550);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // 状态标签
            statusLabel = new Label
            {
                Text = "准备就绪",
                Location = new System.Drawing.Point(20, 20),
                Size = new System.Drawing.Size(600, 20)
            };

            // 加载音频按钮
            loadAudioButton = new Button
            {
                Text = "加载音频文件",
                Location = new System.Drawing.Point(20, 50),
                Size = new System.Drawing.Size(120, 30)
            };

            // 阈值设置
            thresholdLabel = new Label
            {
                Text = "响度阈值 (dB):",
                Location = new System.Drawing.Point(160, 55),
                Size = new System.Drawing.Size(100, 20)
            };

            thresholdNumericUpDown = new NumericUpDown
            {
                Minimum = -80,
                Maximum = 0,
                Value = -45,
                Location = new System.Drawing.Point(270, 53),
                Size = new System.Drawing.Size(80, 20)
            };

            // 预览复选框
            previewCheckBox = new CheckBox
            {
                Text = "预览模式",
                Location = new System.Drawing.Point(370, 55),
                Size = new System.Drawing.Size(100, 20),
                Checked = true
            };

            // 测试按钮
            testButton = new Button
            {
                Text = "测试分析",
                Location = new System.Drawing.Point(20, 90),
                Size = new System.Drawing.Size(100, 30)
            };

            // 处理按钮
            processButton = new Button
            {
                Text = "处理字幕",
                Location = new System.Drawing.Point(130, 90),
                Size = new System.Drawing.Size(80, 30)
            };

            // 暂停按钮
            pauseButton = new Button
            {
                Text = "暂停",
                Location = new System.Drawing.Point(220, 90),
                Size = new System.Drawing.Size(60, 30),
                Enabled = false
            };

            // 停止按钮
            stopButton = new Button
            {
                Text = "停止",
                Location = new System.Drawing.Point(290, 90),
                Size = new System.Drawing.Size(60, 30),
                Enabled = false
            };

            // 日志列表框
            logListBox = new ListBox
            {
                Location = new System.Drawing.Point(20, 130),
                Size = new System.Drawing.Size(600, 300),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };

            // 进度条
            progressBar = new ProgressBar
            {
                Location = new System.Drawing.Point(20, 440),
                Size = new System.Drawing.Size(600, 20),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Style = ProgressBarStyle.Marquee,
                Visible = false
            };

            // 添加控件到窗体
            this.Controls.Add(statusLabel);
            this.Controls.Add(loadAudioButton);
            this.Controls.Add(thresholdLabel);
            this.Controls.Add(thresholdNumericUpDown);
            this.Controls.Add(previewCheckBox);
            this.Controls.Add(testButton);
            this.Controls.Add(processButton);
            this.Controls.Add(pauseButton);
            this.Controls.Add(stopButton);
            this.Controls.Add(logListBox);
            this.Controls.Add(progressBar);
        }

        private void SetupEventHandlers()
        {
            loadAudioButton.Click += LoadAudioButton_Click;
            testButton.Click += TestButton_Click;
            processButton.Click += ProcessButton_Click;
            pauseButton.Click += PauseButton_Click;
            stopButton.Click += StopButton_Click;
        }

        private void LoadInitialAudioSource()
        {
            // 优先使用视频文件中的音频，其次使用外部音频文件
            if (!string.IsNullOrEmpty(videoFileName) && File.Exists(videoFileName))
            {
                currentAudioSource = videoFileName;
                AddLog($"使用视频文件作为音频源: {Path.GetFileName(videoFileName)}");
            }
            else if (!string.IsNullOrEmpty(audioFileName) && File.Exists(audioFileName))
            {
                currentAudioSource = audioFileName;
                AddLog($"使用音频文件: {Path.GetFileName(audioFileName)}");
            }
            else
            {
                AddLog("未找到音频源，请手动加载音频文件");
            }
        }

        private void LoadAudioButton_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "音频/视频文件|*.mp3;*.wav;*.flac;*.aac;*.mp4;*.avi;*.mkv;*.mov|所有文件|*.*";
                openFileDialog.Title = "选择音频或视频文件";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    currentAudioSource = openFileDialog.FileName;
                    AddLog($"已加载音频源: {Path.GetFileName(currentAudioSource)}");
                    statusLabel.Text = $"音频源: {Path.GetFileName(currentAudioSource)}";
                }
            }
        }

        private void TestButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(currentAudioSource) || !File.Exists(currentAudioSource))
            {
                MessageBox.Show("请先加载音频文件", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                AddLog("开始测试音频分析功能...");
                AddLog($"测试文件: {Path.GetFileName(currentAudioSource)}");

                var analyzer = new AudioLoudnessAnalyzer();

                // 测试几个时间点
                var testTimes = new[]
                {
                    new { Name = "开始", Start = 0.0, End = 2000.0 },
                    new { Name = "中间", Start = 10000.0, End = 12000.0 },
                    new { Name = "结束", Start = 20000.0, End = 22000.0 }
                };

                foreach (var test in testTimes)
                {
                    try
                    {
                        var loudness = analyzer.GetLoudness(currentAudioSource, test.Start, test.End);
                        var status = loudness <= -50 ? "静音" : loudness <= -30 ? "低音量" : loudness <= -10 ? "中等音量" : "高音量";
                        AddLog($"  {test.Name}: {loudness:F2} dB ({status})");
                    }
                    catch (Exception ex)
                    {
                        AddLog($"  {test.Name}: 分析失败 - {ex.Message}");
                    }
                }

                AddLog("测试完成");
            }
            catch (Exception ex)
            {
                AddLog($"测试过程中发生错误: {ex.Message}");
                MessageBox.Show($"测试过程中发生错误: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ProcessButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(currentAudioSource) || !File.Exists(currentAudioSource))
            {
                MessageBox.Show("请先加载音频文件", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(originalSubtitle))
            {
                MessageBox.Show("没有字幕内容可处理", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var threshold = (double)thresholdNumericUpDown.Value;
                AddLog($"开始处理字幕，响度阈值: {threshold} dB");

                // 解析字幕
                var subtitleLines = ParseSubtitle(originalSubtitle);
                if (subtitleLines.Count == 0)
                {
                    MessageBox.Show("无法解析字幕内容", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                AddLog($"找到 {subtitleLines.Count} 行字幕");

                var analyzer = new AudioLoudnessAnalyzer();
                var linesToRemove = new List<int>();

                progressBar.Visible = true;
                progressBar.Maximum = subtitleLines.Count;

                // 开始处理
                processButton.Text = "停止处理";
                pauseButton.Enabled = true;
                stopButton.Enabled = true;

                // 使用多线程分析每行字幕的响度
                var tasks = new List<Task>();
                var results = new Dictionary<int, double>();
                var lockObject = new object();
                const int batchSize = 5; // 每批处理5行

                for (int i = 0; i < subtitleLines.Count; i++)
                {
                    if (shouldStop) break;

                    int lineIndex = i; // 捕获循环变量
                    var line = subtitleLines[i];

                    var task = Task.Run(() =>
                    {
                        if (shouldStop) return;

                        try
                        {
                            var loudness = analyzer.GetLoudness(currentAudioSource, line.StartTime, line.EndTime);
                            
                            lock (lockObject)
                            {
                                results[lineIndex] = loudness;
                            }
                        }
                        catch
                        {
                            lock (lockObject)
                            {
                                results[lineIndex] = -60.0; // 默认值
                            }
                        }
                    });

                    tasks.Add(task);

                    // 每处理batchSize行或达到末尾时，等待并更新UI
                    if (tasks.Count >= batchSize || i == subtitleLines.Count - 1)
                    {
                        // 等待当前批次完成
                        Task.WaitAll(tasks.ToArray(), 30000); // 30秒超时
                        
                        // 更新UI
                        foreach (var kvp in results.OrderBy(x => x.Key))
                        {
                            if (shouldStop) break;
                            
                            while (isPaused && !shouldStop)
                            {
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(100);
                            }
                            
                            if (shouldStop) break;

                            var lineIdx = kvp.Key;
                            var loudness = kvp.Value;
                            var subtitleLine = subtitleLines[lineIdx];
                            
                            AddLog($"行 {lineIdx + 1}: {loudness:F2} dB - {subtitleLine.Text.Substring(0, Math.Min(30, subtitleLine.Text.Length))}...");
                            
                            if (loudness < threshold)
                            {
                                linesToRemove.Add(lineIdx);
                            }
                            
                            progressBar.Value = lineIdx + 1;
                            Application.DoEvents();
                        }
                        
                        // 清理
                        tasks.Clear();
                        results.Clear();
                    }
                }

                // 重置按钮状态
                processButton.Text = "处理字幕";
                pauseButton.Text = "暂停";
                pauseButton.Enabled = false;
                stopButton.Enabled = false;

                progressBar.Visible = false;

                if (linesToRemove.Count == 0)
                {
                    AddLog("没有找到需要删除的字幕行");
                    MessageBox.Show("没有找到需要删除的字幕行", "处理完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 确认删除
                var result = MessageBox.Show(
                    $"找到 {linesToRemove.Count} 行响度低于 {threshold} dB 的字幕行。\n\n" +
                    $"是否删除这些字幕行？",
                    "确认删除",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // 删除字幕行
                    var modifiedLines = new List<SubtitleLine>();
                    for (int i = 0; i < subtitleLines.Count; i++)
                    {
                        if (!linesToRemove.Contains(i))
                        {
                            modifiedLines.Add(subtitleLines[i]);
                        }
                    }

                    // 重新生成字幕
                    ModifiedSubtitle = GenerateSubtitle(modifiedLines);
                    AddLog($"已删除 {linesToRemove.Count} 行字幕，剩余 {modifiedLines.Count} 行");

                    MessageBox.Show($"处理完成！\n\n删除了 {linesToRemove.Count} 行字幕\n剩余 {modifiedLines.Count} 行", "处理完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                progressBar.Visible = false;
                AddLog($"处理过程中发生错误: {ex.Message}");
                MessageBox.Show($"处理过程中发生错误: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PauseButton_Click(object sender, EventArgs e)
        {
            if (isPaused)
            {
                // 恢复处理
                isPaused = false;
                pauseButton.Text = "暂停";
                AddLog("处理已恢复");
            }
            else
            {
                // 暂停处理
                isPaused = true;
                pauseButton.Text = "继续";
                AddLog("处理已暂停");
            }
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            shouldStop = true;
            AddLog("正在停止处理...");
        }

        private List<SubtitleLine> ParseSubtitle(string subtitle)
        {
            var lines = new List<SubtitleLine>();
            if (string.IsNullOrEmpty(subtitle))
                return lines;

            try
            {
                var subtitleLines = subtitle.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                SubtitleLine currentLine = null;

                foreach (var line in subtitleLines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    // 检查是否是时间戳行
                    if (line.Contains("-->"))
                    {
                        if (currentLine != null)
                        {
                            lines.Add(currentLine);
                        }

                        currentLine = new SubtitleLine();
                        var timeParts = line.Split(new[] { " --> " }, StringSplitOptions.None);
                        if (timeParts.Length == 2)
                        {
                            currentLine.StartTime = ParseTimeToMilliseconds(timeParts[0]);
                            currentLine.EndTime = ParseTimeToMilliseconds(timeParts[1]);
                        }
                    }
                    else if (currentLine != null)
                    {
                        // 这是文本行，清理HTML标签和多余内容
                        var cleanLine = CleanSubtitleText(line);
                        if (!string.IsNullOrEmpty(cleanLine))
                        {
                            if (!string.IsNullOrEmpty(currentLine.Text))
                            {
                                currentLine.Text += "\n";
                            }
                            currentLine.Text += cleanLine;
                        }
                    }
                }

                if (currentLine != null)
                {
                    lines.Add(currentLine);
                }
            }
            catch (Exception ex)
            {
                AddLog($"解析字幕时发生错误: {ex.Message}");
            }

            return lines;
        }

        private double ParseTimeToMilliseconds(string timeString)
        {
            try
            {
                // 格式: 00:00:01,000 或 00:00:01.000
                var parts = timeString.Replace(',', '.').Split(':');
                if (parts.Length == 3)
                {
                    var hours = int.Parse(parts[0]);
                    var minutes = int.Parse(parts[1]);
                    var secondsParts = parts[2].Split('.');
                    var seconds = int.Parse(secondsParts[0]);
                    var milliseconds = secondsParts.Length > 1 ? int.Parse(secondsParts[1].PadRight(3, '0').Substring(0, 3)) : 0;

                    return (hours * 3600 + minutes * 60 + seconds) * 1000 + milliseconds;
                }
            }
            catch
            {
                // 解析失败，返回0
            }

            return 0;
        }

        private string GenerateSubtitle(List<SubtitleLine> lines)
        {
            var result = new System.Text.StringBuilder();
            for (int i = 0; i < lines.Count; i++)
            {
                result.AppendLine((i + 1).ToString());
                result.AppendLine(FormatTime(lines[i].StartTime) + " --> " + FormatTime(lines[i].EndTime));
                result.AppendLine(lines[i].Text);
                if (i < lines.Count - 1) // 只在不是最后一行时添加空行
                {
                    result.AppendLine();
                }
            }

            return result.ToString();
        }

        private string FormatTime(double milliseconds)
        {
            var totalSeconds = (int)(milliseconds / 1000);
            var hours = totalSeconds / 3600;
            var minutes = (totalSeconds % 3600) / 60;
            var seconds = totalSeconds % 60;
            var ms = (int)(milliseconds % 1000);

            return $"{hours:D2}:{minutes:D2}:{seconds:D2},{ms:D3}";
        }

        private string CleanSubtitleText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            // 移除HTML标签
            text = System.Text.RegularExpressions.Regex.Replace(text, @"<[^>]+>", "");
            
            // 移除多余的行号（如 "1", "2" 等单独的数字）
            text = System.Text.RegularExpressions.Regex.Replace(text, @"^\d+$", "");
            
            // 移除多余的空白字符
            text = text.Trim();
            
            return text;
        }

        private void AddLog(string message)
        {
            var logEntry = $"[{DateTime.Now:HH:mm:ss}] {message}";
            logListBox.Items.Add(logEntry);
            logListBox.TopIndex = logListBox.Items.Count - 1;
        }
    }

    /// <summary>
    /// 字幕行类
    /// </summary>
    public class SubtitleLine
    {
        public double StartTime { get; set; }
        public double EndTime { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}
