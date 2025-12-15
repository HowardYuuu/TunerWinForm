# ChatRoom - 即時聊天室應用程式

基於 ASP.NET Core 8.0 和 SignalR 的即時聊天室 Web 應用程式。

## 功能特色

### 核心功能
- ✅ 用戶連線管理（加入/離開聊天室）
- ✅ 即時訊息傳送與接收
- ✅ 在線用戶列表顯示
- ✅ 在線人數統計
- ✅ 系統通知訊息

### 進階功能
- ✅ 訊息右側顯示（自己的訊息在右側，他人在左側）
- ✅ 訊息已讀狀態（顯示「傳送中...」→「已讀 N」）
- ✅ Emoji 選擇器（32 個常用表情符號）
- ✅ 圖片上傳與顯示
- ✅ 貼上圖片功能（Ctrl+V）

## 技術規格

- **框架**: ASP.NET Core 8.0
- **即時通訊**: SignalR
- **前端**: HTML5, CSS3, JavaScript (Vanilla JS)
- **執行緒安全**: ConcurrentDictionary
- **安全性**: XSS 防護（使用 textContent）

## 快速開始

### 方法 1：使用 Docker（推薦）

#### 環境需求
- Docker 20.10 或更新版本
- Docker Compose 1.29 或更新版本

#### 執行步驟

**Linux / macOS:**
```bash
cd ChatRoom
./docker-run.sh start
```

**Windows:**
```cmd
cd ChatRoom
docker-run.bat start
```

應用程式將在 `http://localhost:5180` 上運行。

詳細的 Docker 使用說明請參考 [DOCKER.md](./DOCKER.md)

### 方法 2：本地執行

#### 環境需求
- .NET 8.0 SDK
- 現代瀏覽器（Chrome, Firefox, Edge）

#### 執行步驟

1. **還原套件**
```bash
cd ChatRoom
dotnet restore
```

2. **建置專案**
```bash
dotnet build
```

3. **執行應用程式**
```bash
dotnet run
```

4. **開啟瀏覽器**
```
http://localhost:5180
```

## 使用說明

### 加入聊天室
1. 開啟應用程式
2. 輸入您的暱稱（1-20 個字元，必須唯一）
3. 點擊「加入聊天室」按鈕

### 發送訊息
- **文字訊息**: 在輸入框輸入文字，按 Enter 或點擊「發送」
- **Emoji**: 點擊 😊 按鈕，選擇表情符號
- **圖片**: 點擊 📷 按鈕選擇圖片，或直接 Ctrl+V 貼上

### 查看狀態
- **在線用戶**: 右側欄顯示所有在線用戶
- **已讀狀態**: 自己的訊息旁顯示已讀人數
- **系統通知**: 用戶加入/離開時自動顯示

### 離開聊天室
- 點擊「離開聊天室」按鈕
- 或直接關閉瀏覽器分頁

## 安全性特性

- **XSS 防護**: 使用 DOM API 和 textContent 安全渲染內容
- **輸入驗證**: 暱稱和訊息長度限制
- **圖片限制**: 最大 5MB，僅支援圖片格式
- **暱稱唯一性**: 防止重複暱稱
- **CORS 設定**: 開發環境寬鬆，生產環境嚴格

## 程式碼結構

```
ChatRoom/
├── Program.cs                      # 應用程式入口
├── Hubs/
│   └── ChatHub.cs                  # SignalR Hub
├── Models/
│   ├── UserInfo.cs                 # 用戶模型
│   └── ChatMessage.cs              # 訊息模型
├── Services/
│   └── UserConnectionService.cs    # 用戶連線管理服務
└── wwwroot/
    ├── index.html                  # 主頁面
    ├── css/
    │   └── styles.css              # 樣式表
    ├── js/
    │   └── chat.js                 # 聊天邏輯
    └── lib/
        └── signalr/                # SignalR 客戶端庫
```

## API 文件

### SignalR Hub 方法

#### 客戶端調用（Client → Server）

**JoinChat**
```csharp
Task JoinChat(string nickname)
```
加入聊天室

**SendMessage**
```csharp
Task SendMessage(string message)
```
發送文字訊息

**SendImageMessage**
```csharp
Task SendImageMessage(string imageData)
```
發送圖片訊息

**MarkMessageAsRead**
```csharp
Task MarkMessageAsRead(string messageId)
```
標記訊息為已讀

**LeaveChat**
```csharp
Task LeaveChat()
```
離開聊天室

#### 服務端推送（Server → Client）

**ReceiveMessage**
```javascript
connection.on("ReceiveMessage", (messageId, sender, content, timestamp, type, imageData) => {})
```
接收訊息

**UserJoined**
```javascript
connection.on("UserJoined", (nickname, onlineCount) => {})
```
用戶加入通知

**UserLeft**
```javascript
connection.on("UserLeft", (nickname, onlineCount) => {})
```
用戶離開通知

**SystemMessage**
```javascript
connection.on("SystemMessage", (message) => {})
```
系統訊息

**UpdateOnlineUsers**
```javascript
connection.on("UpdateOnlineUsers", (users) => {})
```
更新在線用戶列表

**MessageReadStatusUpdated**
```javascript
connection.on("MessageReadStatusUpdated", (messageId, readCount) => {})
```
訊息已讀狀態更新

## 疑難排解

### 問題：無法連線到 SignalR Hub
**解決方案**: 確認應用程式正在運行，檢查瀏覽器控制台是否有錯誤訊息

### 問題：圖片無法上傳
**解決方案**: 檢查圖片大小是否超過 5MB，確認檔案格式為圖片

### 問題：暱稱已被使用
**解決方案**: 選擇一個唯一的暱稱

## 授權

此專案為示範用途。

## 貢獻者

開發團隊與 GitHub Copilot
