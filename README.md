# 音频响度过滤器插件项目

## 项目概述

这是一个为SubtitleEdit 4.0.13开发的音频响度过滤器插件，可以根据音频响度自动删除低响度的字幕行。**v2.0版本使用FFmpeg进行音频分析，提供更强大的格式支持和更精确的响度测量。**

## 项目结构

### 核心文件
- **`AudioLoudnessFilter_SE4_Net48.cs`** - 主插件文件（v2.0 FFmpeg版本）
- **`FFmpegInstaller.cs`** - FFmpeg自动下载安装器
- **`IPlugin_SE4_Official.cs`** - 官方插件接口定义
- **`AudioLoudnessFilter_SE4_Net48.csproj`** - 项目文件

### 安装脚本
- **`install-net48-plugin.bat`** - 插件安装脚本

### 文档
- **`使用说明.md`** - 详细使用说明（必读）
- **`项目说明.md`** - 本文件
- **`GitHub上传指南.md`** - GitHub仓库上传指引

### SE4源代码
- **`SE4/`** - SubtitleEdit 4.0.13完整源代码（Git排除）

## 插件特点

### 1. 基于FFmpeg的音频分析
- **主要方法**：使用FFmpeg的`astats`滤镜获取RMS响度
- **备用方法**：使用FFmpeg的`loudnorm`滤镜作为降级方案
- **优化方法**：优先使用SubtitleEdit内置波形数据（如果可用）
- **自动管理**：FFmpeg自动检测、下载、安装
- **精确测量**：dB（分贝）单位的专业响度测量

### 2. 工具菜单集成
- 插件出现在SubtitleEdit的"工具"菜单中
- 点击后显示独立的插件窗口
- 完整的用户交互界面

### 3. 功能完整
- 支持多种音频格式（WAV、MP3、AAC、FLAC、OGG等）
- 支持视频文件音频提取（MP4、AVI、MKV、MOV等）
- 可配置响度阈值（默认-45dB）
- 实时处理日志显示
- 预览删除的字幕行
- 自动解析SRT字幕格式
- 暂停/恢复/停止处理控制

### 4. 用户友好
- 完全中文界面
- 实时进度显示
- 详细的错误处理
- 完整的操作日志记录
- FFmpeg自动安装功能

## 技术规格

- **目标框架**: .NET Framework 4.8
- **音频引擎**: FFmpeg（loudnorm + astats 滤镜）
- **音频分析**: RMS响度计算（dB单位）
- **插件接口**: 符合SubtitleEdit 4.0.13官方规范
- **命名空间**: `Nikse.SubtitleEdit.PluginLogic`
- **版本**: 2.0

## 安装路径

插件安装到SubtitleEdit的默认路径：
- **主程序**: `C:\Program Files\Subtitle Edit\`- **主程序**:`C:\程序文件\字幕编辑\`
- **插件目录**: `C:\Program Files\Subtitle Edit\Plugins\`* * - * *插件目录:“C: \ Program Files \字幕编辑\ Plugins \ '

## 使用方法

1. **编译插件**：
   ```batch   ”“批
   dotnet build AudioLoudnessFilter_SE4_Official.csproj --configuration Releasedotnet构建AudioLoudnessFilter_SE4_Official。csproj——配置发布
   ```

2. **安装插件**：
   ```batch   ”“批
   install-tool-plugin.bat
   ```

3. **使用插件**：
   - 启动SubtitleEdit 4.0.13
   - 打开字幕文件
   - 在"工具"菜单中点击"音频响度过滤器"
   - 在插件窗口中加载音频文件并处理

## 开发环境

- **.NET Framework 4.8**   - * *。NET Framework 4.8**
- **FFmpeg** - 音频处理引擎（自动安装或手动安装）
- **System.Windows.Forms** - 用户界面
- **System.IO.Compression** - FFmpeg安装器所需

### FFmpeg获取方式
1. **自动安装**（推荐）：插件首次使用时自动下载安装
2. **包管理器**：`choco install ffmpeg` 或 `scoop install ffmpeg`
3. **手动下载**：https://ffmpeg.org/download.html

## 项目历史

### v2.0（当前版本）- 2025-11-08
- ✨ **重大更新**：使用FFmpeg替换NAudio
- 🚀 更强大的音频格式支持
- 📊 更精确的响度测量（astats + loudnorm）
- 🔧 FFmpeg自动下载安装功能
- ⚡ 多线程批处理优化
- 🎛️ 暂停/恢复/停止控制

### v1.0（历史版本）
- 初始版本，使用NAudio库
- 基本的响度过滤功能
- 工具菜单集成

## 注意事项

- 确保SubtitleEdit 4.0.13已正确安装
- 插件需要音频文件与字幕文件时间同步
- 建议在处理前备份原始字幕文件
