@echo off
chcp 65001 >nul
echo ========================================
echo 安装最终版本插件
echo ========================================
echo.

set "PluginPath=%USERPROFILE%\AppData\Roaming\Subtitle Edit\Plugins"
set "SourceDll=bin\Release\net48\AudioLoudnessFilter.dll"
set "TargetDll=%PluginPath%\AudioLoudnessFilter.dll"

echo 1. 检查源文件...
if exist "%SourceDll%" (
    echo ✅ 最终版本插件存在: %SourceDll%
    echo 文件信息:
    dir "%SourceDll%"
) else (
    echo ❌ 最终版本插件不存在: %SourceDll%
    pause
    exit /b 1
)

echo.
echo 2. 检查.NET Framework版本...
powershell -Command "try { $version = [System.Environment]::Version; Write-Host '✅ 当前.NET版本:' $version; $runtimeInfo = [System.Runtime.InteropServices.RuntimeInformation]::FrameworkDescription; Write-Host '✅ 运行时信息:' $runtimeInfo } catch { Write-Host '❌ .NET版本检查失败' }"

echo.
echo 3. 备份现有插件...
if exist "%TargetDll%" (
    copy "%TargetDll%" "%TargetDll%.backup" >nul 2>&1
    echo ✅ 现有插件已备份
) else (
    echo ℹ️ 没有现有插件需要备份
)

echo.
echo 4. 安装最终版本插件...
copy "%SourceDll%" "%TargetDll%" /Y
if %ERRORLEVEL% EQU 0 (
    echo ✅ 最终版本插件安装成功
) else (
    echo ❌ 最终版本插件安装失败
    pause
    exit /b 1
)

echo.
echo 5. 创建插件配置文件...
set "PluginConfig=%PluginPath%\AudioLoudnessFilter.plugin"
echo [Plugin] > "%PluginConfig%"
echo Name=Audio Loudness Filter >> "%PluginConfig%"
echo Description=Delete subtitle lines based on audio loudness using FFmpeg >> "%PluginConfig%"
echo Version=2.0 >> "%PluginConfig%"
echo ActionType=Tool >> "%PluginConfig%"
echo Assembly=AudioLoudnessFilter.dll >> "%PluginConfig%"
echo Class=Nikse.SubtitleEdit.PluginLogic.AudioLoudnessFilter_SE4_Net48 >> "%PluginConfig%"

echo ✅ 插件配置文件已创建

echo.
echo 6. 测试插件加载...
powershell -Command "try { $assembly = [System.Reflection.Assembly]::LoadFrom('%TargetDll%'); Write-Host '✅ 最终版本插件可以加载'; $types = $assembly.GetTypes(); $pluginTypes = $types | Where-Object { $_.GetInterfaces().Name -contains 'IPlugin' }; Write-Host '找到插件类型数量:' $pluginTypes.Length; foreach ($type in $pluginTypes) { Write-Host '  -' $type.Name } } catch { Write-Host '❌ 最终版本插件加载失败:' $_.Exception.Message }"

echo.
echo 7. 检查FFmpeg可用性...
ffmpeg -version >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo ✅ FFmpeg已安装并可用
    ffmpeg -version | findstr "ffmpeg version"
) else (
    echo ⚠️ FFmpeg未找到，请确保FFmpeg已安装并在PATH中
    echo 下载地址: https://ffmpeg.org/download.html
)

echo.
echo ========================================
echo 最终版本插件安装完成！
echo ========================================
echo.
echo 插件特点:
echo 1. 基于.NET Framework 4.8，与SubtitleEdit完全兼容
echo 2. 使用FFmpeg进行真实的音频响度分析
echo 3. 现代化的UI界面，操作简单直观
echo 4. 完整的字幕处理功能，支持SRT格式
echo 5. 详细的日志记录，便于问题诊断
echo 6. 自动检测音频源（视频文件或音频文件）
echo 7. 可调节的响度阈值设置
echo 8. 预览模式，处理前可查看结果
echo.
echo 使用说明:
echo 1. 重启SubtitleEdit
echo 2. 在"工具"菜单中找到"音频响度过滤器"
echo 3. 加载音频/视频文件
echo 4. 设置响度阈值（默认-45dB）
echo 5. 点击"测试分析"验证功能
echo 6. 点击"处理字幕"执行删除操作
echo.
echo 注意事项:
echo - 需要安装FFmpeg并添加到系统PATH
echo - 支持常见音频格式：MP3, WAV, FLAC, AAC等
echo - 支持常见视频格式：MP4, AVI, MKV, MOV等
echo - 处理前会显示确认对话框
echo.
pause


