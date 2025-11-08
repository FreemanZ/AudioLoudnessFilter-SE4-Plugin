# GitHub 上传指南

## 项目简介

音频响度过滤器插件 - 一个为 SubtitleEdit 4.0.13 开发的插件，用于根据音频响度自动删除低响度的字幕行。

## 准备工作

### 1. 安装 Git

如果您尚未安装 Git，请从以下地址下载并安装：
- 官方网站：https://git-scm.com/download/win
- 推荐使用 Git for Windows，安装时选择默认选项即可

### 2. 配置 Git

首次使用 Git 需要配置用户信息：

```bash
git config --global user.name "您的GitHub用户名"
git config --global user.email "您的GitHub邮箱"
```

### 3. 创建 GitHub 账号

如果您还没有 GitHub 账号：
1. 访问 https://github.com
2. 点击 "Sign up" 注册账号
3. 验证邮箱地址

## 文件排除说明

项目已创建 `.gitignore` 文件，自动排除以下内容：

### ✅ 会上传到 GitHub 的文件
- 核心源代码文件 (`.cs`)
- 项目配置文件 (`.csproj`)
- 安装脚本 (`install-*.bat`)
- 文档文件 (`.md`)
- README 和说明文件

### ❌ 不会上传到 GitHub 的文件
- **编译输出**：`bin/`, `obj/` 文件夹及其内容
- **SE4 源代码**：整个 `SE4/` 文件夹（约 1000+ 个文件）
- **测试文件**：所有 `test*.bat`, `test*.wav` 等测试相关文件
- **调试脚本**：`debug-*.bat`, `diagnose-*.bat`, `cleanup-*.bat` 等
- **临时文件**：`.dll`, `.pdb`, `.log`, `.tmp` 等
- **IDE 配置**：`.vs/`, `.vscode/`, `.idea/` 等
- **NuGet 包缓存**：`packages/`, `*.nupkg` 等

## 创建 GitHub 仓库

### 方法一：在 GitHub 网站创建（推荐新手）

1. **登录 GitHub**
   - 访问 https://github.com
   - 使用您的账号登录

2. **创建新仓库**
   - 点击右上角的 "+" 号
   - 选择 "New repository"

3. **配置仓库信息**
   - **Repository name**：`AudioLoudnessFilter-SE4-Plugin`
   - **Description**：`SubtitleEdit 音频响度过滤器插件 - 根据音频响度自动删除低响度字幕行`
   - **Public/Private**：根据需要选择公开或私有
   - **⚠️ 重要**：不要勾选 "Add a README file"（我们本地已有）
   - **⚠️ 重要**：不要勾选 "Add .gitignore"（我们本地已有）
   - 点击 "Create repository"

4. **记录仓库地址**
   - 创建后会显示仓库地址，例如：
   - `https://github.com/您的用户名/AudioLoudnessFilter-SE4-Plugin.git`

### 方法二：使用 GitHub CLI（推荐熟练用户）

```bash
# 安装 GitHub CLI
# 访问 https://cli.github.com/ 下载安装

# 登录
gh auth login

# 创建仓库
gh repo create AudioLoudnessFilter-SE4-Plugin --public --description "SubtitleEdit 音频响度过滤器插件"
```

## 上传项目到 GitHub

### 第一次上传（初始化仓库）

打开命令提示符或 PowerShell，导航到项目目录：

```powershell
# 1. 进入项目目录
cd D:\001_NETDev

# 2. 初始化 Git 仓库
git init

# 3. 添加所有文件（.gitignore 会自动排除不需要的文件）
git add .

# 4. 查看将要提交的文件（可选，用于确认）
git status

# 5. 提交到本地仓库
git commit -m "初始提交：音频响度过滤器插件"

# 6. 重命名分支为 main（GitHub 新标准）
git branch -M main

# 7. 添加远程仓库地址（替换为您的实际地址）
git remote add origin https://github.com/您的用户名/AudioLoudnessFilter-SE4-Plugin.git

# 8. 推送到 GitHub
git push -u origin main
```

### 首次推送时的认证

推送时会要求输入 GitHub 凭据：

#### 方案 A：使用 Personal Access Token（推荐）

1. 访问 https://github.com/settings/tokens
2. 点击 "Generate new token" → "Generate new token (classic)"
3. 设置权限：
   - Note: `Git 操作`
   - Expiration: `90 days` 或更长
   - 勾选 `repo` 权限
4. 点击 "Generate token"
5. **⚠️ 重要**：复制生成的 token（只显示一次）
6. 推送时使用 token 作为密码

#### 方案 B：使用 GitHub Desktop（最简单）

1. 下载安装 GitHub Desktop：https://desktop.github.com/
2. 登录 GitHub 账号
3. 选择 "Add Local Repository"
4. 选择项目文件夹 `D:\001_NETDev`
5. 点击 "Publish repository" 即可自动上传

#### 方案 C：配置 SSH 密钥

```powershell
# 1. 生成 SSH 密钥
ssh-keygen -t ed25519 -C "您的邮箱"

# 2. 添加公钥到 GitHub
# 复制 C:\Users\您的用户名\.ssh\id_ed25519.pub 的内容
# 访问 https://github.com/settings/keys
# 点击 "New SSH key"，粘贴公钥

# 3. 测试连接
ssh -T git@github.com

# 4. 修改远程地址为 SSH
git remote set-url origin git@github.com:您的用户名/AudioLoudnessFilter-SE4-Plugin.git

# 5. 推送
git push -u origin main
```

## 后续更新代码

每次修改代码后，使用以下命令更新到 GitHub：

```powershell
# 1. 查看修改了哪些文件
git status

# 2. 添加修改的文件
git add .

# 3. 提交更改（请用有意义的提交信息）
git commit -m "修复：解决音频加载问题"

# 4. 推送到 GitHub
git push
```

### 常用提交信息示例

```bash
# 新功能
git commit -m "新增：支持 MP4 格式音频文件"

# 修复 Bug
git commit -m "修复：响度计算精度问题"

# 优化改进
git commit -m "优化：提升大文件处理性能"

# 文档更新
git commit -m "文档：更新安装说明"

# 重构代码
git commit -m "重构：简化音频分析逻辑"
```

## 常见问题解决

### 问题 1：推送时提示 "failed to push"

**原因**：远程仓库有本地没有的更新

**解决**：
```powershell
# 先拉取远程更改
git pull origin main

# 如果有冲突，解决后再提交
git add .
git commit -m "合并远程更改"

# 再次推送
git push
```

### 问题 2：不小心提交了不该提交的文件

**解决**：
```powershell
# 从 Git 跟踪中移除（但保留本地文件）
git rm --cached bin/Release/net48/*.dll

# 确保 .gitignore 包含该文件
# 然后提交删除操作
git commit -m "移除：删除误提交的 DLL 文件"
git push
```

### 问题 3：忘记添加 .gitignore 导致大量文件被跟踪

**解决**：
```powershell
# 1. 创建或更新 .gitignore 文件

# 2. 清除 Git 缓存
git rm -r --cached .

# 3. 重新添加文件（会应用新的 .gitignore）
git add .

# 4. 提交更改
git commit -m "更新：应用 .gitignore 规则"

# 5. 推送
git push
```

### 问题 4：本地和远程仓库完全不同步

**解决**（⚠️ 慎用，会覆盖远程仓库）：
```powershell
git push -f origin main
```

## 协作开发

如果有其他人参与开发：

### 克隆仓库

```powershell
git clone https://github.com/您的用户名/AudioLoudnessFilter-SE4-Plugin.git
cd AudioLoudnessFilter-SE4-Plugin
```

### 创建分支进行开发

```powershell
# 创建并切换到新分支
git checkout -b feature/新功能名称

# 开发并提交
git add .
git commit -m "新增：某个功能"

# 推送分支到 GitHub
git push origin feature/新功能名称
```

### 合并分支

在 GitHub 网站上：
1. 进入仓库页面
2. 点击 "Pull requests"
3. 点击 "New pull request"
4. 选择要合并的分支
5. 填写说明并创建 Pull Request
6. 审查后点击 "Merge pull request"

## 查看仓库状态

```powershell
# 查看当前状态
git status

# 查看提交历史
git log --oneline

# 查看远程仓库地址
git remote -v

# 查看文件修改详情
git diff

# 查看某个文件的修改历史
git log --follow -- 文件名
```

## 忽略已跟踪文件的本地修改

如果某个文件已经被 Git 跟踪，但您想忽略本地修改：

```powershell
# 忽略某个文件的本地修改
git update-index --assume-unchanged 文件名

# 取消忽略
git update-index --no-assume-unchanged 文件名
```

## 回退操作

### 撤销本地修改

```powershell
# 撤销某个文件的修改
git checkout -- 文件名

# 撤销所有修改
git checkout -- .
```

### 撤销提交

```powershell
# 撤销最后一次提交（保留修改）
git reset --soft HEAD^

# 撤销最后一次提交（不保留修改）
git reset --hard HEAD^
```

## 项目结构说明

```
D:\001_NETDev\
├── .gitignore                              # Git 排除配置（已创建）
├── GitHub上传指南.md                        # 本文档
├── 项目说明.md                              # 项目说明
├── AudioLoudnessFilter_SE4_Net48.cs        # 主插件源代码
├── AudioLoudnessFilter_SE4_Net48.csproj    # 项目配置文件
├── IPlugin_SE4_Official.cs                 # 插件接口定义
├── TestPlugin_SE4_Official.cs              # 测试插件
├── install-*.bat                           # 安装脚本（保留）
└── （其他文件会被 .gitignore 排除）
```

## 推荐 README 内容

建议在项目根目录创建 `README.md`，内容参考如下：

````markdown
# SubtitleEdit 音频响度过滤器插件

## 项目简介

这是一个为 SubtitleEdit 4.0.13 开发的音频响度过滤器插件，可以根据音频响度自动删除低响度的字幕行。

## 主要功能

- ✅ 支持多种音频格式（WAV、MP3、AAC、FLAC、OGG）
- ✅ 可配置响度阈值（默认 -45dB）
- ✅ 实时处理日志显示
- ✅ 预览删除的字幕行
- ✅ 完全中文界面

## 技术规格

- **目标框架**：.NET Framework 4.8
- **音频分析**：RMS 算法计算响度
- **音频库**：NAudio 2.1.0

## 快速开始

### 1. 编译插件

```bash
dotnet build AudioLoudnessFilter_SE4_Net48.csproj --configuration Release
```

### 2. 安装插件

运行安装脚本：
```bash
install-net48-plugin.bat
```

### 3. 使用插件

1. 启动 SubtitleEdit 4.0.13
2. 打开字幕文件
3. 在"工具"菜单中点击"音频响度过滤器"
4. 加载音频文件并处理

## 依赖项

- SubtitleEdit 4.0.13
- .NET Framework 4.8
- NAudio 2.1.0

## 许可证

[在此添加您的许可证信息]

## 贡献

欢迎提交 Issue 和 Pull Request！

## 联系方式

[在此添加您的联系方式]
````

## 额外建议

### 1. 添加许可证文件

在项目根目录创建 `LICENSE` 文件，常用许可证：
- MIT License（最宽松）
- GPL v3（开源）
- Apache 2.0（企业友好）

访问 https://choosealicense.com/ 选择合适的许可证。

### 2. 添加 .editorconfig

统一代码格式：

```ini
# .editorconfig
root = true

[*]
charset = utf-8
indent_style = space
indent_size = 4
end_of_line = crlf
trim_trailing_whitespace = true
insert_final_newline = true

[*.{cs,csproj}]
indent_size = 4

[*.{md,bat}]
trim_trailing_whitespace = false
```

### 3. 启用 GitHub Actions（CI/CD）

创建 `.github/workflows/build.yml` 实现自动构建（可选）。

### 4. 添加 CHANGELOG.md

记录版本更新历史：

```markdown
# 更新日志

## [1.0.0] - 2025-11-08
### 新增
- 初始版本发布
- 支持基于音频响度过滤字幕
```

## 最佳实践

1. **频繁提交**：每完成一个小功能就提交一次
2. **有意义的提交信息**：清楚说明做了什么改动
3. **定期推送**：至少每天推送一次到 GitHub
4. **使用分支**：新功能在单独分支开发，测试通过后合并
5. **编写文档**：保持 README 和文档更新
6. **代码审查**：重要更改通过 Pull Request 进行审查
7. **版本标签**：发布时打 tag：`git tag -a v1.0.0 -m "版本 1.0.0"`

## 学习资源

- **Git 官方文档**：https://git-scm.com/doc
- **GitHub 使用指南**：https://docs.github.com/cn
- **Pro Git 中文版**：https://git-scm.com/book/zh/v2
- **可视化学习 Git**：https://learngitbranching.js.org/?locale=zh_CN
- **GitHub 使用教程视频**：B站搜索"GitHub 教程"

## 完成检查清单

- [ ] 安装 Git
- [ ] 配置 Git 用户信息
- [ ] 创建 GitHub 账号
- [ ] 创建 GitHub 仓库
- [ ] 本地初始化 Git 仓库
- [ ] 添加并提交文件
- [ ] 配置认证（Token/SSH）
- [ ] 推送到 GitHub
- [ ] 验证文件已正确上传（检查是否排除了不需要的文件）
- [ ] 创建 README.md
- [ ] 添加 LICENSE 文件

---

**祝您上传顺利！如有问题，请参考上述常见问题解决方案或在 GitHub 社区寻求帮助。**
````

