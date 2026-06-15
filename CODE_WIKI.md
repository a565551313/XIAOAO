# 笑傲西游网关 - Code Wiki

> **项目名称**: 笑傲西游网关 (Xiao Ao Xi You Gateway)  
> **技术栈**: C# / .NET Framework 4.7.2 / Windows Forms  
> **项目类型**: MMORPG 游戏服务器网关代理工具  
> **版权**: Copyright 2020  
> **版本**: 1.0.0.0  

---

## 目录

1. [项目概述](#1-项目概述)
2. [整体架构](#2-整体架构)
3. [项目结构](#3-项目结构)
4. [核心模块详解](#4-核心模块详解)
5. [关键类与函数说明](#5-关键类与函数说明)
6. [协议与数据格式](#6-协议与数据格式)
7. [依赖关系](#7-依赖关系)
8. [配置说明](#8-配置说明)
9. [项目运行方式](#9-项目运行方式)
10. [GM管理功能](#10-gm管理功能)
11. [数据管理系统](#11-数据管理系统)

---

## 1. 项目概述

本项目是一个**中国风MMORPG游戏服务器的网关代理工具**，名为"笑傲西游"。它作为游戏客户端与后端游戏服务器 (`ggeserver.exe`) 之间的中间层，承担以下核心职责：

- **网络代理转发**: 双向透明转发客户端与服务端之间的网络数据包
- **协议解析与验证**: 解析自定义二进制协议，验证客户端版本和连接合法性
- **在线玩家管理**: 实时追踪和管理所有连接的客户端会话
- **GM管理工具**: 提供丰富的管理员操作界面（踢人、发公告、赠送道具等）
- **CDK激活码系统**: 生成和管理游戏内兑换码
- **游戏数据管理**: Excel数据表导入、加密、解密和实体代码自动生成

---

## 2. 整体架构

### 2.1 网关代理架构

```
游戏客户端  <-->  [Server]  <-->  [ClientSocket]  <-->  [笑傲西游网关]  <-->  [Client]  <-->  ggeserver.exe (游戏服务端)
                  ^                                                        ^
                  | 监听客户端连接                                          | 连接上游服务端
              (默认端口 8084)                                          (默认端口 9016)
```

### 2.2 三层网络模型

| 层级 | 组件 | 职责 |
|------|------|------|
| **接入层** | `Server` 类 | 监听客户端连接，接受TCP请求，分配 `ClientSocket` 实例 |
| **会话层** | `ClientSocket` 类 | 管理单个客户端会话，解析客户端协议，维护玩家状态 |
| **转发层** | `Client` 类 | 保持与服务端的持久连接，转发服务端响应到对应客户端 |

### 2.3 数据流向

**上行（客户端 -> 服务端）**:
```
客户端发送原始数据 -> ClientSocket.DataProcessing() -> 封装为网关协议 -> Client.SendMsg() -> 服务端
```

**下行（服务端 -> 客户端）**:
```
服务端返回数据 -> Client.ReceiveCalBback() -> Client.DataProcessing() -> 按ID路由 -> ClientSocket.SendMsg() -> 客户端
```

---

## 3. 项目结构

```
笑傲西游网关/
├── 哈哈哈哈.sln                    # Visual Studio 2019 解决方案文件
├── 哈哈呵哦.csproj                 # MSBuild 项目文件 (.NET 4.7.2)
├── App.config                      # .NET 运行时配置
├── packages.config                 # NuGet 包管理 (旧格式)
│
├── Program.cs                      # 应用程序入口点
├── Server.cs                       # TCP服务端 - 监听客户端连接
├── Client.cs                       # TCP客户端 - 连接游戏服务端
├── ClientSocket.cs                 # 客户端会话对象 - 管理单个玩家连接
├── Form1.cs                        # 主GUI界面 (1964行) - GM管理面板
├── Form1.Designer.cs               # 主窗体设计器代码
├── Form2.cs                        # 副GUI界面 - 玩家信息浏览器
├── Form2.Designer.cs               # 副窗体设计器代码
├── JosnConvert.cs                  # JSON数据传输对象类
├── shop.cs                         # 占位类 (空)
│
├── Common/
│   ├── MMO_MemoryStream.cs         # 二进制序列化/反序列化工具流
│   ├── GameDataTableParser.cs      # 游戏数据表(.data)解析器
│   ├── IniFile.cs                  # INI配置文件读写 (kernel32 API封装)
│   └── ZlibHelper.cs              # zlib压缩/解压缩工具
│
├── zlib_NET_104/                   # zlib完整.NET移植 (14个文件)
│   ├── Adler32.cs
│   ├── Deflate.cs
│   ├── Inflate.cs
│   ├── InfBlocks.cs
│   ├── InfCodes.cs
│   ├── InfTree.cs
│   ├── StaticTree.cs
│   ├── SupportClass.cs
│   ├── Tree.cs
│   ├── ZInputStream.cs
│   ├── Zlib.cs
│   ├── ZOutputStream.cs
│   ├── ZStream.cs
│   └── ZStreamException.cs
│
├── Properties/
│   ├── AssemblyInfo.cs             # 程序集元数据
│   ├── Resources.resx              # 嵌入式资源
│   ├── Settings.settings           # 应用程序设置
│   └── DataSources/                # 数据绑定源 (12个文件)
│
└── bin/Debug/                      # 构建输出目录
    ├── 笑傲西游.exe                  # 网关主程序
    ├── config.ini                   # 运行时配置文件
    ├── data/                        # 游戏数据文件 (.data)
    ├── map/                         # 地图文件 (~140个 .map)
    ├── 玩家信息/                     # 玩家存档目录
    ├── log/                         # 日志目录 (按日期组织)
    ├── 数据表/                      # Excel源数据 (.xls)
    ├── Lib/                         # 外部依赖DLL
    │   ├── HPSocket.dll             # 网络通信库
    │   ├── lua51.dll                # Lua 5.1 运行时
    │   ├── GGEE.dll                 # 游戏引擎
    │   └── ... (其他Lua/C扩展库)
    └── ggeserver.exe                # 上游游戏服务端
```

---

## 4. 核心模块详解

### 4.1 Server.cs - TCP服务端监听

**文件**: `Server.cs` (75行)

**职责**: 监听来自游戏客户端的连接请求，为每个新连接创建 `ClientSocket` 会话对象。

**关键实现**:
- 采用**单例模式** (`Server.ServerMsg`)
- 在后台线程中异步接受连接 (`ListenClientCallback`)
- 通过 `Socket.Accept()` 阻塞等待新连接
- 根据全局IP过滤已连接的客户端
- 支持两种模式：显示IP列表 或 自动分配ID创建会话

**核心方法**:

| 方法 | 行号 | 描述 |
|------|------|------|
| `start(string m_ServerIP)` | L21-32 | 初始化Socket，绑定地址和端口，启动监听线程 |
| `ListenClientCallback()` | L39-73 | 无限循环接受客户端连接，创建ClientSocket |
| `ServerMsg` (静态属性) | L13-16 | 单例访问入口 |

**网络参数**:
- 监听端口从 `config.ini` 的 `[mainconfig]port` 读取 (默认 8084)
- 最大监听队列: 1000 (`Listen(1000)`)

---

### 4.2 Client.cs - 服务端连接代理

**文件**: `Client.cs` (479行)

**职责**: 保持与上游游戏服务器 (`ggeserver.exe`) 的持久TCP连接，负责双向数据转发和协议解析。

**关键实现**:
- 采用**单例模式** (`Client.ClientMsg`)
- 独立的接收线程 (`ReceiveMsg`) 处理服务端推送
- 发送队列 + 异步发送模式，防止阻塞
- **粘包处理**: 循环解析 `MMO_MemoryStream` 中的完整数据包
- 根据消息序号 (`num`) 分发到不同处理逻辑

**核心方法**:

| 方法 | 行号 | 描述 |
|------|------|------|
| `Connect()` | L51-84 | 建立与服务端的TCP连接，启动接收线程 |
| `MakeData(int num, string msg)` | L108-142 | 封装数据包：写入flag(14138)+seq+长度+payload |
| `Send(byte[] buffer)` | L149-163 | 异步发送数据到服务端 |
| `SendMsg(int num, string msg)` | L182-197 | 将消息加入发送队列，触发异步发送 |
| `ReceiveMsg()` | L203-207 | 发起异步接收 |
| `ReceiveCalBback(IAsyncResult ar)` | L212-336 | 接收回调，处理粘包，解析并分发消息 |
| `DataProcessing(int num, int ID, string msg)` | L340-477 | 按消息序号分发处理逻辑 |

**消息处理分支**:

| 消息序号 (num) | 处理逻辑 | 行号 |
|----------------|----------|------|
| 997 | Lua表解析 - 提取角色名称，更新玩家状态为"登入游戏" | L408-431 |
| 998 | 服务端主动断开 - 通知客户端并记录断开方式 | L432-447 |
| default | 按玩家ID路由转发到对应ClientSocket | L461-474 |

---

### 4.3 ClientSocket.cs - 客户端会话管理

**文件**: `ClientSocket.cs` (642行)

**职责**: 代表一个已连接的游戏客户端，管理其生命周期、数据收发、状态追踪和请求频率限制。

**关键实现**:
- 每个活跃客户端对应一个 `ClientSocket` 实例
- 独立的接收线程处理该客户端的所有数据
- **请求频率限制**: 超过阈值自动断连 (防刷机制)
- **封禁检测**: 检查封禁IP列表和封禁封包列表
- 两个定时器：1秒重置请求计数，30秒超时检测未登录连接

**核心属性**:

| 属性 | 类型 | 描述 |
|------|------|------|
| `编号` | int | 客户端唯一编号 (自增) |
| `IP` | string | 客户端IP地址 |
| `端口` | int | 客户端端口 |
| `链接时间` | string | 连接建立时间 |
| `状态` | State枚举 | 连接/验证/登入/管理 四种状态 |
| `账号` | string | 登录账号 |
| `ID` | int | 游戏角色ID |
| `请求数量` | int | 当前周期内请求次数 |

**State枚举**:
```csharp
enum State { 连接成功, 验证成功, 登入游戏, 管理模式 }
```

**核心方法**:

| 方法 | 行号 | 描述 |
|------|------|------|
| `ReceiveMsg()` | L98-112 | 发起异步接收客户端数据 |
| `ReceiveCalBback(IAsyncResult ar)` | L117-242 | 接收回调，粘包处理，分发到DataProcessing |
| `MakeData(int num, string msg)` | L274-309 | 封装网关协议数据包 |
| `Send(byte[] buffer)` | L316-320 | 异步发送数据 |
| `SendMsg(int num, string msg)` | L348-366 | 加入发送队列 |
| `DataProcessing(int nub, string msg)` | L369-616 | 协议解析和分发 |

**客户端协议处理**:

| 消息序号 (nub) | 处理逻辑 | 行号 |
|----------------|----------|------|
| 1 | 登录 - 解析版本/账号/密码/硬件信息，转发给服务端 | L473-520 |
| 2 | 断开通知 - 发送断开原因给服务端 | L529-534 |
| 3 | 创建角色 - 解析模型/名称/染色ID | L535-541 |
| 4 | 角色登录 - 分配玩家ID，进入游戏世界 | L542-548 |
| else | 透传 - 注入编号和IP信息后转发 | L577-614 |

---

### 4.4 Form1.cs - GM管理主界面

**文件**: `Form1.cs` (1964行)

**职责**: 提供完整的图形化管理界面，包含玩家管理、GM指令、CDK系统、数据管理等功能。

**关键实现**:
- 使用 `SynchronizationContext` 实现跨线程UI更新
- `BindingList<ClientSocket>` + `Dictionary<int, ClientSocket>` 双数据结构管理在线玩家
- ListView展示地图玩家/怪物数量实时统计
- DES加密/解密管理网关密钥

**核心字段**:

| 字段 | 类型 | 描述 |
|------|------|------|
| `flag` | int | 协议魔数 = 14138 |
| `全局ip` | string | 网关服务器IP (DES解密得到) |
| `SyncContext` | SynchronizationContext | 线程同步上下文 |
| `AllUser` | BindingList<ClientSocket> | 绑定到DataGridView的在线玩家列表 |
| `AllUserTable` | Dictionary<int, ClientSocket> | 按编号索引的玩家字典 |
| `CDK` | List<string> | 内存中的激活码列表 |
| `RequestsNumber` | int | 请求频率阈值 |
| `debug` | bool | 调试模式标志 |

**核心方法分类**:

#### 4.4.1 连接管理

| 方法 | 触发 | 描述 |
|------|------|------|
| `button1_Click` | 启动网关按钮 | DES解密key -> 启动Server监听 -> 连接服务端 |
| `ConnectServer()` | 内部 | 调用 `Client.ClientMsg.Connect()` |
| `StartServer()` | 内部 | 检测并启动 `ggeserver.exe` 进程 |
| `Disconnect()` | 内部 | 连接断开时的恢复逻辑 |

#### 4.4.2 玩家操作

| 方法 | 消息序号 | 描述 |
|------|----------|------|
| `button13_Click` | 1001 | 封禁玩家 |
| `button39_Click` | 1002 | 调整经验倍率 |
| `button34_Click` | 1003 | 赠送称谓 |
| `button31_Click` | 1004 | 充值业务处理 |
| `button26_Click` | 1005 | 定制装备 |
| `button29_Click` | 1006 | 定制灵饰 |
| `button46_Click` | 1007 | 定制宠物 |
| `button49_Click_1` | 1008 | 地图操作 (添加假人/查看统计) |
| `button56_Click` | 1009 | CDK兑换 (指定玩家) |
| `button5_Click` | 1002 | 设置等级上限 |
| `button37_Click` | 1010 | 物品发放 (指定玩家) |
| `button75_Click` | 1010 | 物品发放 (全服) |
| `button57_Click` | 1003 | 自定义称谓赠送 |
| `button71_Click` | 1013 | 定制宠物 (新版) |

#### 4.4.3 公告与广播

| 方法 | 消息序号 | 描述 |
|------|----------|------|
| `button23_Click` | 直接发送 | 发送公告给所有在线玩家 |
| `button15_Click` | 99997 | 广播消息 |
| `button42_Click` | 1000 | 全服发送 |

#### 4.4.4 CDK系统

| 方法 | 描述 |
|------|------|
| `生成CDK(int count)` | 随机生成指定数量的激活码，写入文件 |
| `提取CDK(int count)` | 按前缀筛选并输出激活码 |
| `兑换CDK(object state)` | 处理玩家兑换请求，根据前缀分发奖励类型 |
| `CreateAndCheckCode(Random, string)` | 生成16位随机激活码 |

**CDK奖励类型映射**:

| 前缀 | 类型 |
|------|------|
| CJP00 | 普通抽奖 |
| CJZ00 | 中等抽奖 |
| CJJ00 | 极品抽奖 |
| Y0001-Y0020 | 银子 1E-20E |
| M0005-M1000 | 仙玉 5元-1000元 |

#### 4.4.5 数据管理

| 方法 | 描述 |
|------|------|
| `GetExcelFirstTableName()` | 通过OleDb获取Excel首个工作表名 |
| `ReadData()` | 读取Excel数据表 |
| `CreateData()` | 生成加密的 `.data` 文件 |
| `CreateEntity()` | 生成C#实体类和Lua实体代码 |
| `CreateDBModel()` | 生成C#数据管理类和Lua DBModel |
| `btnSelectData_Click` | 读取并显示 `.data` 文件内容 |

#### 4.4.6 加密工具

| 方法 | 描述 |
|------|------|
| `DesEncrypt()` | DES加密 (密钥: qq381990860, IV固定) |
| `DesDecrypt()` | DES解密 |

---

### 4.5 Form2.cs - 玩家信息浏览器

**文件**: `Form2.cs` (81行)

**职责**: 以树形结构浏览 `玩家信息/` 目录下的玩家存档数据。

**核心方法**:

| 方法 | 描述 |
|------|------|
| `LoadTree()` | 递归加载目录树到TreeView |

---

### 4.6 JosnConvert.cs - 数据传输对象

**文件**: `JosnConvert.cs` (141行)

**职责**: 定义各种JSON序列化/反序列化使用的DTO类。

**关键类**:

| 类名 | 用途 |
|------|------|
| `Login` | 登录消息 (账号/密码/卡洛) |
| `Root` | 根消息 (账号/密码/IP/编号) |
| `vVerify` | 版本验证 (空了/皮皮/版本) |
| `Register` / `RegisterRole` | 注册/创建角色 |
| `DataMsg` / `DataMsgs` | 通用数据消息 |
| `Usermsg` | 用户消息 (User/Name/id) |
| `MapUpdata` | 地图更新 (角色/怪物/模型/站位) |

---

## 5. 关键类与函数说明

### 5.1 MMO_MemoryStream (二进制序列化流)

**文件**: `Common/MMO_MemoryStream.cs` (315行)

**职责**: 扩展 `MemoryStream`，提供类型安全的二进制读写方法，是项目的**核心序列化层**。

**读写方法总览**:

| 方法 | 字节数 | 描述 |
|------|--------|------|
| `ReadShort()` / `WriteShort()` | 2 | signed/unsigned short |
| `ReadUShort()` / `WriteUShort()` | 2 | unsigned short |
| `ReadInt()` / `WriteInt()` | 4 | 32-bit integer |
| `ReadUInt()` / `WriteUInt()` | 4 | unsigned 32-bit |
| `ReadLong()` / `WriteLong()` | 8 | 64-bit integer |
| `ReadULong()` / `WriteULong()` | 8 | unsigned 64-bit |
| `ReadFloat()` / `WriteFloat()` | 4 | 32-bit float |
| `ReadDouble()` / `WriteDouble()` | 8 | 64-bit double |
| `ReadBool()` / `WriteBool()` | 1 | boolean (0/1) |
| `ReadASCIIString()` / `WriteASCIIString()` | 变长 | 2字节长度前缀 + 默认编码 |
| `ReadUTF8String()` / `WriteUTF8String()` | 变长 | 2字节长度前缀 + UTF-8 |
| `ReadDefaultString(int)` | 指定长度 | 按指定字节数读取 |
| `WriteDefaultString()` | 变长 | 2字节长度前缀 + UTF-8 |

---

### 5.2 GameDataTableParser (数据表解析器)

**文件**: `Common/GameDataTableParser.cs` (180行)

**职责**: 解析游戏加密数据文件 (`.data`)，提供逐行字段访问接口。

**解析流程**:
1. 读取二进制文件
2. XOR解密 (37字节循环密钥表 `xorScale`)
3. 读取行数和列数
4. 第一行: 字段名
5. 第二行: 字段类型
6. 第三行: 字段描述
7. 第四行起: 实际数据

**核心API**:

| 成员 | 描述 |
|------|------|
| `FieldName` | 字段名数组 |
| `Eof` | 是否到达文件末尾 |
| `Next()` | 移动到下一行 |
| `GetFieldValue(string)` | 获取当前行指定字段的值 |

---

### 5.3 IniFile (配置文件读写)

**文件**: `Common/IniFile.cs` (78行)

**职责**: 封装Windows kernel32 API，提供INI配置文件的读写操作。

**核心API**:

| 方法 | 描述 |
|------|------|
| `writeIni(section, key, value)` | 写入指定section的key |
| `readIni(section, key)` | 读取指定key的值 |
| `existINIFile()` | 检查文件是否存在 |

---

### 5.4 ZlibHelper (压缩工具)

**文件**: `Common/ZlibHelper.cs` (159行)

**职责**: 封装ComponentAce zlib库，提供数据压缩/解压缩功能。

**核心API**:

| 方法 | 描述 |
|------|------|
| `CompressBytes(byte[], int)` | 压缩字节数组 |
| `DeCompressBytes(byte[])` | 解压缩字节数组 |
| `CompressString(string, int)` | 压缩字符串 (Base64编码) |
| `DecompressString(string)` | 解压字符串 |

---

## 6. 协议与数据格式

### 6.1 网关协议格式

所有通过网络传输的数据包遵循以下固定格式：

```
+-----------+-----------+-----------+------------------+
| Flag (4B) | Seq  (4B) | Len  (4B) | Payload (N Bytes)|
+-----------+-----------+-----------+------------------+
  魔数:14138   序号:0      包体长度      实际消息数据
```

- **Flag**: 固定值 `14138`，用于数据包校验
- **Seq**: 序列号，当前固定为 `0`
- **Len**: 后续Payload的字节长度
- **Payload**: 业务数据

### 6.2 Lua Table 消息格式

网关与服务端之间的业务消息使用 **Lua table字符串序列化** 格式（非真实Lua执行）：

```lua
do local ret={["序号"]=1,["内容"]="消息文本",["ID"]=12345} return ret end
```

解析使用 `LuaTableToCsSharp.SharpluaTable` 库，转换为 `Dictionary<string, object>`。

### 6.3 客户端-网关分隔符

客户端与网关之间使用自定义分隔符：
- 字段分隔: `*-*` (`fgf`)
- 子字段分隔: `@+@` (`fgc`)

例如登录消息格式:
```
版本号@+@账号@+@密码@+@硬盘序列号
```

### 6.4 数据文件加密

**.data 文件加密流程**:
1. 原始数据写入 `MMO_MemoryStream` (行/列头 + ASCII字符串)
2. XOR加密: `buffer[i] ^= xorScale[i % 37]`
3. (可选) zlib压缩
4. 写入 `.data` 文件

**解密流程**相反: 读取 -> XOR解密 -> (可选)解压 -> 解析

XOR密钥表 (`xorScale`, 37字节):
```
{99, 77, 66, 138, 55, 23, 254, 109, 165, 90, 19, 41, 145, 201, 58,
 55, 37, 254, 185, 165, 169, 19, 171, 38, 1, 99, 9, 86, 12, 74,
 1, 215, 88, 64, 56, 22, 56}
```

### 6.5 DES加密

用于加密网关IP地址，存储在 `config.ini` 中：

- **密钥**: `"qq381990860"` 截取前8位
- **IV**: `{0x12, 0x34, 0x56, 0x68, 0x90, 0xAB, 0xCD, 0xEF}`
- **模式**: DES CBC

---

## 7. 依赖关系

### 7.1 NuGet 包依赖

| 包名 | 版本 | 用途 |
|------|------|------|
| Newtonsoft.Json | 12.0.3 | JSON序列化/反序列化 |
| LuaTableToCSharp | 1.0.3 | Lua table字符串解析 |
| System.Data.Common | 4.3.0 | 数据库连接 |
| System.Private.DataContractSerialization | 4.3.0 | 序列化支持 |
| System.Reflection.TypeExtensions | 4.1.1 | 反射扩展 |

### 7.2 外部DLL依赖 (bin/Debug/)

| DLL | 用途 |
|-----|------|
| HPSocket.dll | 高性能TCP网络通信库 |
| lua51.dll | Lua 5.1 运行时引擎 |
| GGEE.dll / Galaxy2d.dll | 游戏2D引擎 |
| Logger.dll / Logger_C.dll | 日志库 |
| bass.dll / fmod.dll | 音频库 |
| cjson.dll / lcurl.dll / lfs.lua / luasql.dll | Lua C扩展 |
| minizip.dll | 压缩库 |
| mir.dll | 图形渲染 |

### 7.3 进程依赖

| 进程 | 说明 |
|------|------|
| `ggeserver.exe` | 上游游戏服务端，网关必须连接此进程 |
| `g2d.exe` | 游戏客户端登入器 |

### 7.4 依赖关系图

```
                    +------------------+
                    |    Form1.cs      |  <--- 主GUI (协调所有模块)
                    |   (1964行)       |
                    +--------+---------+
                             |
          +------------------+------------------+
          |                  |                  |
          v                  v                  v
    +-----------+      +-----------+      +------------+
    |  Server   |      |   Client  |      | ClientSocket|
    | (监听层)   |      | (转发层)  |      | (会话层)    |
    +-----+-----+      +-----+-----+      +------+------+
          |                  |                  |
          +------------------+------------------+
                             |
                             v
                    +------------------+
                    | MMO_MemoryStream |  <--- 二进制序列化
                    +--------+---------+
                             |
              +--------------+--------------+
              |              |              |
              v              v              v
        +------------+  +-----------+  +-------------+
        |   DES加密   |  | XOR加密   |  |  Lua Table  |
        | (kernel32) |  |(xorScale)|  |  解析器      |
        +------------+  +-----------+  +-------------+
```

---

## 8. 配置说明

### 8.1 config.ini

**路径**: `bin/Debug/config.ini` (或 `../服务端/config.ini` 在调试模式下)

**格式**:
```ini
[mainconfig]
key=X+G3xQ+IwiVTSrjIHHhUnQ==   ; DES加密的服务端IP
ip=127.0.0.1                     ; 网关服务器IP
ver=1.2                          ; 客户端版本号
lv=69                            ; 等级上限
port=8084                        ; 网关监听端口
serPort=9016                     ; 服务端连接端口
id=23073                         ; 网关实例ID
```

### 8.2 其他配置文件

| 文件 | 路径 | 用途 |
|------|------|------|
| `CDK.txt` | 根目录 | 激活码列表 |
| `公告内容.txt` | 根目录 | 游戏公告文本 |
| `封禁IP.txt` | 根目录 | 封禁IP列表 |

### 8.3 玩家数据目录

**路径**: `bin/Debug/玩家信息/`

按账号ID分目录存储:
```
玩家信息/
├── {accountId}/
│   ├── role.data      ; 角色数据
│   ├── pet.data       ; 宠物数据
│   └── item.data      ; 道具数据
```

### 8.4 游戏数据文件

**路径**: `bin/Debug/data/`

| 文件 | 内容 |
|------|------|
| `npc.data` | NPC数据 |
| `teleport.data` | 传送点数据 |
| `skill.data` | 技能数据 |
| `item.data` | 道具数据 |
| `monster.data` | 怪物数据 |
| `pet.data` | 宠物数据 |
| `shop.data` | 商店数据 |
| `ringquest.data` | 环任务数据 |
| `title.data` | 称谓数据 |
| ... | 共17+个.data文件 |

**路径**: `bin/Debug/map/`

约140个 `.map` 地图文件。

---

## 9. 项目运行方式

### 9.1 环境要求

| 要求 | 版本 |
|------|------|
| IDE | Visual Studio 2019 |
| 框架 | .NET Framework 4.7.2 |
| 平台 | Windows |
| 架构 | x86 (Release) / AnyCPU (Debug) |

### 9.2 构建步骤

1. 用 Visual Studio 2019 打开 `哈哈哈哈.sln`
2. 还原 NuGet 包 (如有需要)
3. 选择配置: Debug 或 Release
4. 生成解决方案 (Ctrl+Shift+B)
5. 编译输出: `bin/Debug/笑傲西游.exe` 或 `bin/Release/笑傲西游.exe`

### 9.3 运行步骤

1. **准备服务端**: 确保 `ggeserver.exe` 在同一目录下运行
2. **配置**: 编辑 `config.ini` 设置端口和密钥
3. **启动网关**: 运行 `笑傲西游.exe`
4. **连接服务端**: 在GUI中点击"启动网关"按钮
   - 系统会自动解密key获取服务端IP
   - 启动Server监听客户端连接
   - 连接上游服务端
5. **客户端连接**: 游戏客户端配置网关IP和端口 (默认8084)

### 9.4 调试模式

- 如果检测不到 `ggeserver.exe`，自动进入调试模式
- 调试模式下文件路径前缀变为 `../服务端/`
- 可通过 `checkBox14` 手动切换调试模式

### 9.5 自动化功能

| 功能 | 触发条件 | 描述 |
|------|----------|------|
| 自动重启服务端 | `checkBox3` 勾选 | Timer每秒检测并重启ggeserver |
| 自动重连 | `checkBox4` 勾选 | 连接断开后自动重连服务端 |
| 未登录踢出 | 连接30秒未登录 | Timer1自动断开未进入游戏的连接 |
| 请求频率限制 | 超过阈值 | 自动封禁频繁请求的客户端 |

---

## 10. GM管理功能

### 10.1 消息序号速查表

| 序号 | 功能 | 说明 |
|------|------|------|
| 1000 | 开关控制 | 监听开关/全服发送 |
| 1001 | 封禁玩家 | 封禁指定玩家 |
| 1002 | 经验调整 | 设置经验倍率 |
| 1002 | 等级上限 | 设置最大等级 |
| 1003 | 赠送称谓 | 给玩家添加称号 |
| 1004 | 充值处理 | 处理充值业务 |
| 1005 | 定制装备 | 创建自定义装备 |
| 1006 | 定制灵饰 | 创建自定义饰品 |
| 1007 | 定制宠物 | 创建自定义召唤兽 |
| 1008 | 地图操作 | 添加假人/查看统计 |
| 1009 | CDK兑换 | 指定玩家兑换 |
| 1010 | 物品发放 | 指定玩家/全服发放 |
| 1011 | CDK错误 | 兑换码无效反馈 |
| 1012 | CDK奖励 | 根据类型发放奖励 |
| 1013 | 定制宠物(新) | 新版宠物创建 |
| 21 | 消息推送 | 向GM推送日志/消息 |
| 999 | 警告 | 请求异常警告 |
| 998 | 断开通知 | 服务端请求断开 |
| 99997 | 广播 | 全服广播 |

### 10.2 监听功能

通过 `checkBox5`~`checkBox10` 控制不同类型的消息监听：

| 复选框 | 功能 |
|--------|------|
| checkBox5 | 系统监听 |
| checkBox6 | 队伍监听 |
| checkBox7 | 世界聊天监听 |
| checkBox8 | 帮派监听 |
| checkBox9 | 传闻监听 |
| checkBox10 | 门派监听 |

### 10.3 地图管理

内置50+中国风地图名称，通过ListView展示各地图的玩家/怪物数量：

地府、建邺城、昆仑仙境、蟠桃园、东海湾、东海海底、东海岩洞、傲来国、女儿村、花果山、北俱芦洲、天宫、长寿村、方寸山、大唐境外、狮驼岭、魔王寨、盘丝岭、五庄观、长安城、江南野外、普陀山、灵台宫、西梁女国、宝象国、朱紫国、凌波城、神木林、无底洞等。

---

## 11. 数据管理系统

### 11.1 Excel -> .data 转换流程

```
Excel (.xls)
    |
    v
OleDbConnection 读取数据
    |
    v
MMO_MemoryStream 序列化
    |  (写入行/列数 + ASCII字符串)
    v
XOR 加密 (xorScale)
    |
    v
(可选) zlib 压缩
    |
    v
.data 文件输出
```

### 11.2 代码自动生成

系统可从Excel数据表自动生成三种代码:

1. **C# Entity** (`Create/{Name}Entity.cs`): 实体类定义
2. **Lua Entity** (`CreateLua/{Name}Entity.lua`): Lua实体类
3. **C# DBModel** (`Create/{Name}DBModel.cs`): 数据管理类
4. **Lua DBModel** (`CreateLua/{Name}DBModel.lua`): Lua数据访问类

### 11.3 .data 文件读取

通过 `GameDataTableParser` 可读取加密的 `.data` 文件并在UI中展示内容，用于数据验证和调试。

---

## 附录

### A. 程序入口点

```csharp
// Program.cs
static void Main() {
    Application.EnableVisualStyles();
    frm1 = new Form1();
    Application.Run(frm1);
}
```

注: 单例锁代码 (`Mutex`) 被注释掉，允许多实例运行。

### B. 命名空间

所有业务代码位于 `笑傲西游` 命名空间。  
公共工具类位于 `GameServerApp.Common` 命名空间 (MMO_MemoryStream, GameDataTableParser)。

### C. 关键设计模式

| 模式 | 应用位置 | 说明 |
|------|----------|------|
| 单例模式 | Server, Client | 全局唯一的服务器/客户端实例 |
| 生产者-消费者 | ClientSocket, Client | 发送队列 + 异步消费 |
| 观察者模式 | System.Timers.Timer | 定时触发状态检查和清理 |
| 策略模式 | DataProcessing switch | 按消息序号分发不同处理逻辑 |
| 适配器模式 | MMO_MemoryStream | 适配 .NET MemoryStream 为二进制协议格式 |

### D. 线程模型

```
主线程 (UI)
  |
  +-- SyncContext.Post -> 安全更新UI控件
  |
  +-- Server.ListenClientCallback (后台线程)
  |       |
  |       +-- ClientSocket (每个客户端独立后台线程)
  |
  +-- Client.ReceiveMsg (后台线程)
          |
          +-- DataProcessing -> SyncContext.Post -> UI更新
```

所有跨线程UI操作均通过 `SynchronizationContext.Post` 调度到主线程执行。
