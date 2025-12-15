# Playwright 測試截圖

此資料夾用於存放 Playwright 自動化測試的截圖結果。

## 截圖說明

### ChatRoom 專案測試截圖

以下是 ChatRoom 即時聊天室專案的功能測試截圖：

#### 1. 登入介面
- `chatroom-login-page.png` - 聊天室登入頁面

#### 2. 聊天介面
- `chatroom-new-interface.png` - 新版聊天介面（包含 emoji 和圖片按鈕）
- `chatroom-chat-interface.png` - 基本聊天介面

#### 3. 訊息功能
- `chatroom-own-message.png` - 自己的訊息顯示在右側
- `chatroom-with-message.png` - 訊息發送示例
- `chatroom-multi-users-chat.png` - 多用戶聊天畫面

#### 4. Emoji 功能
- `chatroom-emoji-picker.png` - Emoji 選擇器
- `chatroom-emoji-working.png` - Emoji 正常顯示
- `chatroom-emoji-fixed.png` - 修復後的 Emoji 顯示

#### 5. 用戶管理
- `chatroom-two-users.png` - 兩個用戶在線
- `chatroom-user-left.png` - 用戶離開聊天室

#### 6. 已讀狀態
- `chatroom-read-status.png` - 訊息已讀狀態顯示

#### 7. 系統訊息
- `chatroom-system-message-centered.png` - 系統訊息置中顯示

## 使用說明

當執行 Playwright 測試時，截圖會自動保存到此資料夾。

測試執行指令範例：
```bash
# 執行測試並保存截圖
dotnet test --logger "console;verbosity=detailed"
```

## 注意事項

- 截圖檔案應使用描述性的檔名
- 建議使用 PNG 格式以保持圖片品質
- 定期清理舊的測試截圖以節省空間
- 不要將敏感資訊的截圖提交到版本控制
