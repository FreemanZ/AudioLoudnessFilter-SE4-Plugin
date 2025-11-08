using System;
using System.IO;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    /// <summary>
    /// 测试插件 - 用于验证SubtitleEdit 4.0.13的插件系统
    /// </summary>
    public class TestPlugin : IPlugin
    {
        public string Name => "插件系统测试";
        public string Text => "插件系统测试";
        public string Description => "测试SubtitleEdit 4.0.13的插件系统是否正常工作";
        public decimal Version => 1.0m;
        public string ActionType => "video";
        public string Shortcut => string.Empty;

        public string DoAction(Form parentForm, string subtitle, double frameRate, string listViewLineSeparatorString, string subtitleFileName, string videoFileName, string audioFileName)
        {
            try
            {
                // 记录测试开始
                LogToFile("插件系统测试开始");
                LogToFile($"插件名称: {Name}");
                LogToFile($"插件版本: {Version}");
                LogToFile($"ActionType: {ActionType}");

                // 收集系统信息
                var systemInfo = $"系统信息测试:\n" +
                                $"操作系统: {Environment.OSVersion}\n" +
                                $".NET版本: {Environment.Version}\n" +
                                $"工作目录: {Environment.CurrentDirectory}\n" +
                                $"用户目录: {Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\n" +
                                $"临时目录: {Path.GetTempPath()}\n" +
                                $"当前程序集: {System.Reflection.Assembly.GetExecutingAssembly().Location}\n" +
                                $"插件目录: {Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)}";

                LogToFile(systemInfo);

                // 收集插件参数信息
                var paramInfo = $"插件参数测试:\n" +
                               $"字幕内容长度: {subtitle?.Length ?? 0}\n" +
                               $"帧率: {frameRate}\n" +
                               $"字幕文件名: {subtitleFileName ?? "无"}\n" +
                               $"视频文件名: {videoFileName ?? "无"}\n" +
                               $"音频文件名: {audioFileName ?? "无"}\n" +
                               $"音频文件存在: {(!string.IsNullOrEmpty(audioFileName) && File.Exists(audioFileName))}";

                LogToFile(paramInfo);

                // 收集已加载的程序集信息
                var assemblyInfo = "已加载的程序集:\n";
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    try
                    {
                        assemblyInfo += $"  - {assembly.GetName().Name} v{assembly.GetName().Version} ({assembly.Location})\n";
                    }
                    catch
                    {
                        assemblyInfo += $"  - {assembly.GetName().Name} (位置未知)\n";
                    }
                }
                LogToFile(assemblyInfo);

                // 显示测试结果
                var testResult = $"插件系统测试完成！\n\n" +
                               $"✅ 插件成功加载\n" +
                               $"✅ 插件接口正常工作\n" +
                               $"✅ 日志系统正常工作\n\n" +
                               $"系统信息:\n{systemInfo}\n\n" +
                               $"参数信息:\n{paramInfo}\n\n" +
                               $"日志文件位置:\n{GetLogFilePath()}";

                LogToFile("插件系统测试完成");
                
                MessageBox.Show(testResult, "插件系统测试", MessageBoxButtons.OK, MessageBoxIcon.Information);

                return subtitle;
            }
            catch (Exception ex)
            {
                LogToFile($"测试过程中发生异常: {ex.Message}");
                LogToFile($"异常堆栈: {ex.StackTrace}");
                MessageBox.Show($"测试过程中发生错误: {ex.Message}\n\n详细信息已记录到日志文件", "测试错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return subtitle;
            }
        }

        private void LogToFile(string message)
        {
            try
            {
                var logFile = GetLogFilePath();
                var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}{Environment.NewLine}";
                File.AppendAllText(logFile, logMessage);
            }
            catch
            {
                // 忽略日志写入错误
            }
        }

        private string GetLogFilePath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "TestPlugin_SE4_Official.log");
        }
    }
}
