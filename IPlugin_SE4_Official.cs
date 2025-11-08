using System;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    /// <summary>
    /// SubtitleEdit 4.0.13 官方插件接口
    /// </summary>
    public interface IPlugin
    {
        /// <summary>
        /// 插件名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 插件显示文本
        /// </summary>
        string Text { get; }

        /// <summary>
        /// 插件描述
        /// </summary>
        string Description { get; }

        /// <summary>
        /// 插件版本
        /// </summary>
        decimal Version { get; }

        /// <summary>
        /// 插件操作类型 (video, subtitle, etc.)
        /// </summary>
        string ActionType { get; }

        /// <summary>
        /// 快捷键
        /// </summary>
        string Shortcut { get; }

        /// <summary>
        /// 执行插件操作
        /// </summary>
        /// <param name="parentForm">父窗体</param>
        /// <param name="subtitle">字幕内容</param>
        /// <param name="frameRate">帧率</param>
        /// <param name="listViewLineSeparatorString">列表视图行分隔符</param>
        /// <param name="subtitleFileName">字幕文件名</param>
        /// <param name="videoFileName">视频文件名</param>
        /// <param name="audioFileName">音频文件名</param>
        /// <returns>处理后的字幕内容</returns>
        string DoAction(Form parentForm, string subtitle, double frameRate, string listViewLineSeparatorString, string subtitleFileName, string videoFileName, string audioFileName);
    }
}
