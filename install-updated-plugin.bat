@echo off
chcp 65001 >nul
echo ========================================
echo 安装更新后的音频响度过滤器插件
echo ========================================
echo.

set "PluginPath=%USERPROFILE%\AppData\Roaming\Subtitle Edit\Plugins"
set "SourceFile=bin\Release\net48\AudioLoudnessFilter.dll"

echo 1. 检查源文件...
if not exist "%SourceFile%" (
    echo ❌ 源文件不存在: %SourceFile%
    echo 请先运行: dotnet build AudioLoudnessFilter_SE4_Official.csproj --configuration Release
    pause
    exit /b 1
)
echo ✅ 源文件存在

echo.
echo 2. 创建插件目录...
if not exist "%PluginPath%" (
    mkdir "%PluginPath%"
    echo ✅ 插件目录已创建
) else (
    echo ✅ 插件目录已存在
)

echo.
echo 3. 备份现有插件...
if exist "%PluginPath%\AudioLoudnessFilter.dll" (
    copy "%PluginPath%\AudioLoudnessFilter.dll" "%PluginPath%\AudioLoudnessFilter.dll.backup" /Y
    echo ✅ 现有插件已备份
)

echo.
echo 4. 安装新插件...
copy "%SourceFile%" "%PluginPath%\AudioLoudnessFilter.dll" /Y
if %ERRORLEVEL% EQU 0 (
    echo ✅ 新插件安装成功
) else (
    echo ❌ 插件安装失败
    pause
    exit /b 1
)

echo.
echo 5. 复制依赖文件...
if exist "bin\Release\net48\System.Resources.Extensions.dll" (
    copy "bin\Release\net48\System.Resources.Extensions.dll" "%PluginPath%\" /Y
    echo ✅ System.Resources.Extensions.dll 已复制
)

if exist "bin\Release\net48\System.Buffers.dll" (
    copy "bin\Release\net48\System.Buffers.dll" "%PluginPath%\" /Y
    echo ✅ System.Buffers.dll 已复制
)

if exist "bin\Release\net48\System.Memory.dll" (
    copy "bin\Release\net48\System.Memory.dll" "%PluginPath%\" /Y
    echo ✅ System.Memory.dll 已复制
)

echo.
echo 6. 测试插件加载...
powershell -Command "try { $assembly = [System.Reflection.Assembly]::LoadFrom('%PluginPath%\AudioLoudnessFilter.dll'); Write-Host '✅ 插件可以加载'; $types = $assembly.GetTypes(); $pluginTypes = $types | Where-Object { $_.GetInterfaces().Name -contains 'IPlugin' }; Write-Host '找到插件类型数量:' $pluginTypes.Length; foreach ($type in $pluginTypes) { Write-Host '  插件类型:' $type.Name; $instance = [System.Activator]::CreateInstance($type); Write-Host '    名称:' $instance.Name; Write-Host '    描述:' $instance.Description; Write-Host '    动作类型:' $instance.ActionType } } catch { Write-Host '❌ 插件加载失败:' $_.Exception.Message }"

echo.
echo ========================================
echo 安装完成！
echo ========================================
echo.
echo 下一步操作:
echo 1. 重启SubtitleEdit
echo 2. 在"工具"菜单中找到"音频响度过滤器"
echo 3. 点击打开插件窗口
echo 4. 加载音频/视频文件
echo 5. 点击"测试分析"按钮测试响度检测功能
echo.
pause
