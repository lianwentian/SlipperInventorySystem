# Slipper Inventory System

## 项目概述

拖鞋库存管理系统是一个综合的企业级库存和销售管理平台，专为拖鞋制造和销售业务设计。

## 🩴 主要功能

### 1. 产品管理
- ✅ 产品信息维护（编码、名称、规格、价格）
- ✅ 库存跟踪和管理
- ✅ **产品图片预览** - 批量导入和搜索产品图片
- ✅ 库存预警系统
- ✅ 产品分类管理

### 2. 销售管理
- ✅ 销售订单创建和管理
- ✅ 客户管理
- ✅ 报价单管理
- ✅ 订单状态跟踪
- ✅ 支付状态管理

### 3. 报表分析
- ✅ 销售报表
- ✅ 库存报表
- ✅ 客户分析
- ✅ 收入统计
- ✅ PDF/Excel导出

### 4. 用户管理
- ✅ 用户账户管理
- ✅ 角色和权限管理
- ✅ 操作日志记录
- ✅ 访问控制

### 5. 产品图片预览功能 🖼️

#### 工作流程
1. **准备图片文件夹**
   - 创建一个文件夹 `D:\ProductImages`
   - 将产品图片放入，按产品编码命名
   - 命名规则：`P001.jpg`, `P001_1.jpg`, `P001_2.jpg`

2. **导入图片**
   - 打开软件 → 点击"产品图片"菜单
   - 点击"导入图片"按钮
   - 选择包含图片的文件夹
   - 系统自动识别产品并导入

3. **搜索和预览**
   - 在搜索框输入产品编码或产品名称
   - 点击"🔍 搜索"
   - 在左侧列表选择图片
   - 右侧显示高质量预览

#### 支持的图片格式
- JPG / JPEG
- PNG
- BMP
- GIF
- WEBP

#### 限制条件
- 单个图片最大：10 MB
- 支持无限数量的图片导入

## 🛠️ 技术栈

- **框架**: .NET 8.0
- **UI**: WPF (Windows Presentation Foundation)
- **数据库**: SQLite
- **ORM**: Entity Framework Core 8.0
- **报表**: iText7, EPPlus
- **认证**: Role-based Access Control (RBAC)

## 📋 系统要求

- Windows 10 或更高版本
- .NET 8.0 SDK（开发环境）
- 200MB 硬盘空间

## 🚀 快速开始

### 1. 克隆仓库
```bash
git clone https://github.com/lianwentian/SlipperInventorySystem.git
cd SlipperInventorySystem
```

### 2. 运行构建脚本

**Windows (PowerShell)**
```powershell
.\setup.ps1
```

**Windows (Command Prompt)**
```cmd
setup.bat
```

**Linux/Mac**
```bash
chmod +x setup.sh
./setup.sh
```

### 3. 启动应用
- Windows: 双击 `publish\SlipperIS.UI.exe`
- Linux/Mac: 运行 `./publish/SlipperIS.UI`

## 👤 默认登录账户

| 角色 | 用户名 | 密码 | 权限 |
|------|--------|------|------|
| Admin | admin | admin123 | 完全访问 |
| Sales | sales | sales123 | 销售和报价 |
| Warehouse | warehouse | warehouse123 | 产品和库存 |

## 📦 项目结构

```
SlipperInventorySystem/
├── src/
│   ├── SlipperIS.Core/              # 核心业务逻辑
│   │   ├── Models/                  # 数据模型
│   │   ├── Interfaces/              # 服务接口
│   │   ├── Services/                # 业务服务
│   │   └── Constants/               # 常量定义
│   ├── SlipperIS.Data/              # 数据访问层
│   │   ├── DbContext/               # 数据库上下文
│   │   └── Repositories/            # 数据仓储
│   ├── SlipperIS.UI/                # WPF用户界面
│   │   ├── Views/                   # XAML视图
│   │   ├── ViewModels/              # 视图模型
│   │   └── Resources/               # 资源文件
│   ├── SlipperIS.Reports/           # 报表生成
│   └── SlipperIS.Common/            # 通用工具
├── setup.ps1                        # PowerShell构建脚本
├── setup.bat                        # Batch构建脚本
├── setup.sh                         # Shell构建脚本
└── README.md                        # 本文档
```

## 🔑 主要特性

### 产品管理
- 完整的产品信息管理
- 多价格体系（成本价、销售价、VIP价）
- 库存自动跟踪
- 产品分类和规格管理
- **产品图片管理和预览**

### 销售流程
- 从报价到订单的完整流程
- 灵活的折扣管理
- 多种支付状态跟踪
- 订单历史记录

### 客户管理
- 客户信息维护
- VIP客户标识
- 信用额度管理
- 应收账款跟踪

### 报表和分析
- 实时销售仪表板
- 库存分析
- 客户排行
- 收入趋势
- 导出为PDF或Excel

## 🔐 主要特性

### 产品管理
- 完整的产品信息管理
- 多价格体系（成本价、销售价、VIP价）
- 库存自动跟踪
- 产品分类和规格管理

### 销售流程
- 从报价到订单的完整流程
- 灵活的折扣管理
- 多种支付状态跟踪
- 订单历史记录

### 客户管理
- 客户信息维护
- VIP客户标识
- 信用额度管理
- 应收账款跟踪

### 报表和分析
- 实时销售仪表板
- 库存分析
- 客户排行
- 收入趋势
- 导出为PDF或Excel

## 💾 数据库

系统使用SQLite数据库，无需额外配置。数据库文件自动创建在应用程序目录。

### 数据库文件
- 位置：`slippers_inventory.db`
- 自动初始化种子数据

## 🔒 安全性

- 密码使用SHA256加密存储
- 基于角色的访问控制 (RBAC)
- 操作日志记录
- 权限验证机制

## 📝 使用说明

### 产品图片导入

1. **文件夹准备**
   ```
   D:\ProductImages\
   ├── P001.jpg
   ├── P001_1.jpg
   ├── P002.jpg
   └── P003.jpg
   ```

2. **打开应用**
   - 启动系统 → 登录 → 点击"产品图片"

3. **导入过程**
   - 点击"导入图片"按钮
   - 选择图片文件夹
   - 等待导入完成

4. **搜索预览**
   - 在搜索框输入产品编码
   - 选择图片查看预览

### 创建销售订单

1. 点击"销售"菜单
2. 点击"新建订单"
3. 选择客户
4. 添加产品和数量
5. 设置价格和折扣
6. 保存订单

### 生成报表

1. 点击"报表"菜单
2. 选择报表类型
3. 设置筛选条件
4. 点击"导出"保存为PDF或Excel

## 🐛 故障排除

### 构建失败
- 确保已安装 .NET 8.0 SDK
- 检查网络连接（用于下载NuGet包）
- 删除 `bin` 和 `obj` 文件夹后重试

### 数据库连接错误
- 确保有写入权限
- 检查磁盘空间
- 删除旧的 `slippers_inventory.db` 文件

### 图片无法导入
- 确认产品编码与文件名匹配
- 检查图片格式是否支持
- 确保文件没有被其他程序占用

## 📞 支持

如有问题，请提交 Issue 或联系开发团队。

## 📄 许可证

本项目采用 MIT 许可证。详见 LICENSE 文件。

## 🎉 贡献

欢迎提交 Pull Request 和报告Bug！

---

**最后更新**: 2026年7月20日

**版本**: 1.0.0
