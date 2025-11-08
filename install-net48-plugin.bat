@echo off
chcp 65001 >nul
echo ========================================
echo 安装.NET Framework 4.8版本插件
echo ========================================
echo.

set "PluginPath=%USERPROFILE%\AppData\Roaming\Subtitle Edit\Plugins"
set "SourceDll=bin\Release\net48\AudioLoudnessFilter.dll"
set "TargetDll=%PluginPath%\AudioLoudnessFilter.dll"

echo 1. 检查源文件...
if exist "%SourceDll%" (
    echo ✅ .NET Framework 4.8版本插件存在: %SourceDll%
    echo 文件大小:
    dir "%SourceDll%" | findstr "AudioLoudnessFilter.dll"
) else (
    echo ❌ .NET Framework 4.8版本插件不存在: %SourceDll%
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
echo 4. 安装.NET Framework 4.8版本插件...
copy "%SourceDll%" "%TargetDll%" /Y
if %ERRORLEVEL% EQU 0 (
    echo ✅ .NET Framework 4.8版本插件安装成功
) else (
    echo ❌ .NET Framework 4.8版本插件安装失败
    pause
    exit /b 1
)

echo.
echo 5. 创建正确的插件配置...
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
powershell -Command "try { $assembly = [System.Reflection.Assembly]::LoadFrom('%TargetDll%'); Write-Host '✅ .NET Framework 4.8插件可以加载'; $types = $assembly.GetTypes(); $pluginTypes = $types | Where-Object { $_.GetInterfaces().Name -contains 'IPlugin' }; Write-Host '找到插件类型数量:' $pluginTypes.Length; foreach ($type in $pluginTypes) { Write-Host '  -' $type.Name; $nameProp = $type.GetProperty('Name'); if ($nameProp) { $name = $nameProp.GetValue($null); Write-Host '    名称:' $name } } } catch { Write-Host '❌ .NET Framework 4.8插件加载失败:' $_.Exception.Message }"

echo.
echo 7. 检查核心功能...
echo 检查AudioLoudnessAnalyzer类...
powershell -Command "try { $assembly = [System.Reflection.Assembly]::LoadFrom('%TargetDll%'); $analyzerType = $assembly.GetType('Nikse.SubtitleEdit.PluginLogic.AudioLoudnessAnalyzer'); if ($analyzerType) { Write-Host '✅ AudioLoudnessAnalyzer类已找到' } else { Write-Host '❌ AudioLoudnessAnalyzer类未找到' } } catch { Write-Host '❌ AudioLoudnessAnalyzer检查失败:' $_.Exception.Message }"

echo.
echo 8. 检查窗体类...
echo 检查AudioLoudnessForm类...
powershell -Command "try { $assembly = [System.Reflection.Assembly]::LoadFrom('%TargetDll%'); $formType = $assembly.GetType('Nikse.SubtitleEdit.PluginLogic.AudioLoudnessForm'); if ($formType) { Write-Host '✅ AudioLoudnessForm类已找到' } else { Write-Host '❌ AudioLoudnessForm类未找到' } } catch { Write-Host '❌ AudioLoudnessForm检查失败:' $_.Exception.Message }"

echo.
echo ========================================
echo .NET Framework 4.8版本插件安装完成！
echo ========================================
echo.
echo 特点:
echo 1. 使用.NET Framework 4.8，与SubtitleEdit完全兼容
echo 2. 基于FFmpeg的音频响度分析
echo 3. 现代化的UI界面
echo 4. 完整的字幕处理功能
echo 5. 详细的日志记录
echo.
echo 下一步:
echo 1. 重启SubtitleEdit
echo 2. 在插件管理器中应该能看到"Audio Loudness Filter"
echo 3. 在"工具"菜单中应该能找到该插件
echo 4. 测试插件功能是否正常
echo.
pause


