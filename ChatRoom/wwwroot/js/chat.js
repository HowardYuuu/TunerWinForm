// 常數定義
const MAX_NICKNAME_LENGTH = 20;
const MAX_MESSAGE_LENGTH = 500;

// SignalR 連線設定
let connection = null;
let currentUser = null;

// DOM 元素
const loginSection = document.getElementById('loginSection');
const chatSection = document.getElementById('chatSection');
const nicknameInput = document.getElementById('nicknameInput');
const joinBtn = document.getElementById('joinBtn');
const messageInput = document.getElementById('messageInput');
const sendBtn = document.getElementById('sendBtn');
const leaveBtn = document.getElementById('leaveBtn');
const messagesList = document.getElementById('messagesList');
const usersList = document.getElementById('usersList');
const onlineCount = document.getElementById('onlineCount');
const errorMessage = document.getElementById('errorMessage');

// 初始化 SignalR 連線
function initializeConnection() {
    connection = new signalR.HubConnectionBuilder()
        .withUrl("/chatHub")
        .withAutomaticReconnect()
        .build();

    // 註冊事件處理器
    registerEventHandlers();
}

// 註冊 SignalR 事件處理器
function registerEventHandlers() {
    // 接收訊息
    connection.on("ReceiveMessage", (sender, message, timestamp) => {
        addMessage(sender, message, timestamp, sender === currentUser);
    });

    // 用戶加入
    connection.on("UserJoined", (nickname, count) => {
        updateOnlineCount(count);
    });

    // 用戶離開
    connection.on("UserLeft", (nickname, count) => {
        updateOnlineCount(count);
    });

    // 系統訊息
    connection.on("SystemMessage", (message) => {
        addSystemMessage(message);
    });

    // 更新在線用戶列表
    connection.on("UpdateOnlineUsers", (users) => {
        updateUsersList(users);
    });

    // 加入成功
    connection.on("JoinedSuccessfully", (nickname) => {
        currentUser = nickname;
        showChatSection();
        clearError();
    });

    // 錯誤訊息
    connection.on("Error", (error) => {
        showError(error);
    });

    // 重新連線
    connection.onreconnecting(() => {
        addSystemMessage("正在重新連線...");
    });

    connection.onreconnected(() => {
        addSystemMessage("已重新連線");
        // 重新加入聊天室
        if (currentUser) {
            connection.invoke("JoinChat", currentUser);
        }
    });

    connection.onclose(() => {
        addSystemMessage("連線已關閉");
    });
}

// 加入聊天室
async function joinChat() {
    const nickname = nicknameInput.value.trim();
    
    if (!nickname) {
        showError("請輸入暱稱");
        return;
    }

    if (nickname.length > MAX_NICKNAME_LENGTH) {
        showError(`暱稱不可超過${MAX_NICKNAME_LENGTH}個字元`);
        return;
    }

    try {
        // 啟動連線
        await connection.start();
        console.log("SignalR 已連線");

        // 加入聊天室
        await connection.invoke("JoinChat", nickname);
    } catch (err) {
        console.error("連線錯誤:", err);
        showError("連線失敗，請稍後再試");
    }
}

// 發送訊息
async function sendMessage() {
    const message = messageInput.value.trim();
    
    if (!message) {
        return;
    }

    if (message.length > MAX_MESSAGE_LENGTH) {
        showError(`訊息不可超過${MAX_MESSAGE_LENGTH}個字元`);
        return;
    }

    try {
        await connection.invoke("SendMessage", message);
        messageInput.value = '';
        messageInput.focus();
    } catch (err) {
        console.error("發送訊息錯誤:", err);
        showError("發送失敗，請稍後再試");
    }
}

// 離開聊天室
async function leaveChat() {
    if (confirm("確定要離開聊天室嗎？")) {
        try {
            await connection.invoke("LeaveChat");
            await connection.stop();
            resetToLogin();
        } catch (err) {
            console.error("離開錯誤:", err);
        }
    }
}

// 新增訊息到列表
function addMessage(sender, content, timestamp, isOwn = false) {
    const messageDiv = document.createElement('div');
    messageDiv.className = `message ${isOwn ? 'own' : ''}`;

    const time = formatTime(new Date(timestamp));

    messageDiv.innerHTML = `
        <div class="message-header">
            <span class="message-sender">${escapeHtml(sender)}</span>
            <span class="message-time">${time}</span>
        </div>
        <div class="message-content">${escapeHtml(content)}</div>
    `;

    messagesList.appendChild(messageDiv);
    scrollToBottom();
}

// 新增系統訊息
function addSystemMessage(message) {
    const messageDiv = document.createElement('div');
    messageDiv.className = 'message system';
    messageDiv.innerHTML = `<div class="message-content">${escapeHtml(message)}</div>`;
    messagesList.appendChild(messageDiv);
    scrollToBottom();
}

// 更新在線用戶列表
function updateUsersList(users) {
    usersList.innerHTML = '';
    
    users.forEach(user => {
        const li = document.createElement('li');
        li.className = 'user-item';
        
        const time = formatTime(new Date(user.connectedAt));
        
        li.innerHTML = `
            <span class="user-name">${escapeHtml(user.nickname)}</span>
            <span class="user-time">${time}</span>
        `;
        
        usersList.appendChild(li);
    });
}

// 更新在線人數
function updateOnlineCount(count) {
    onlineCount.textContent = count;
}

// 顯示聊天區域
function showChatSection() {
    loginSection.style.display = 'none';
    chatSection.style.display = 'flex';
    messageInput.focus();
}

// 重置到登入畫面
function resetToLogin() {
    chatSection.style.display = 'none';
    loginSection.style.display = 'flex';
    nicknameInput.value = '';
    messagesList.innerHTML = '';
    usersList.innerHTML = '';
    onlineCount.textContent = '0';
    currentUser = null;
}

// 顯示錯誤訊息
function showError(message) {
    // 清除舊的錯誤訊息與按鈕
    errorMessage.innerHTML = '';
    
    // 建立訊息文字
    const msgSpan = document.createElement('span');
    msgSpan.textContent = message;
    errorMessage.appendChild(msgSpan);
    
    // 建立關閉按鈕
    const closeBtn = document.createElement('button');
    closeBtn.textContent = '×';
    closeBtn.setAttribute('aria-label', '關閉錯誤訊息');
    closeBtn.style.marginLeft = '8px';
    closeBtn.style.cursor = 'pointer';
    closeBtn.style.border = 'none';
    closeBtn.style.background = 'none';
    closeBtn.style.fontSize = '20px';
    closeBtn.style.color = '#ff6b6b';
    closeBtn.addEventListener('click', clearError);
    errorMessage.appendChild(closeBtn);
    
    // 延長自動清除時間（15 秒），並避免多重 timeout
    if (window._errorTimeout) {
        clearTimeout(window._errorTimeout);
    }
    window._errorTimeout = setTimeout(() => {
        clearError();
    }, 15000);
}

// 清除錯誤訊息
function clearError() {
    errorMessage.innerHTML = '';
    if (window._errorTimeout) {
        clearTimeout(window._errorTimeout);
        window._errorTimeout = null;
    }
}

// 捲動到底部
function scrollToBottom() {
    messagesList.scrollTop = messagesList.scrollHeight;
}

// 格式化時間
function formatTime(date) {
    const hours = date.getHours().toString().padStart(2, '0');
    const minutes = date.getMinutes().toString().padStart(2, '0');
    return `${hours}:${minutes}`;
}

// HTML 編碼（防止 XSS）
function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

// 事件監聽器
document.addEventListener('DOMContentLoaded', () => {
    // 初始化連線
    initializeConnection();

    // 加入按鈕
    joinBtn.addEventListener('click', joinChat);

    // Enter 鍵加入
    nicknameInput.addEventListener('keypress', (e) => {
        if (e.key === 'Enter') {
            joinChat();
        }
    });

    // 發送按鈕
    sendBtn.addEventListener('click', sendMessage);

    // Enter 鍵發送訊息
    messageInput.addEventListener('keypress', (e) => {
        if (e.key === 'Enter') {
            sendMessage();
        }
    });

    // 離開按鈕
    leaveBtn.addEventListener('click', leaveChat);
});

// 處理頁面關閉
window.addEventListener('beforeunload', (e) => {
    if (connection && connection.state === signalR.HubConnectionState.Connected) {
        // 使用同步方式確保離開通知送達
        // 由於 SignalR 不支援同步調用，我們依賴 OnDisconnectedAsync 處理斷線
        // 瀏覽器關閉時會自動觸發 WebSocket 斷線，伺服器會收到通知
    }
});
