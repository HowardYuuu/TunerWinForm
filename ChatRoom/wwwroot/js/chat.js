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
const emojiBtn = document.getElementById('emojiBtn');
const emojiPicker = document.getElementById('emojiPicker');
const emojiList = document.getElementById('emojiList');
const imageBtn = document.getElementById('imageBtn');
const imageInput = document.getElementById('imageInput');
const imagePreview = document.getElementById('imagePreview');
const previewImage = document.getElementById('previewImage');
const sendImageBtn = document.getElementById('sendImageBtn');
const cancelPreviewBtn = document.getElementById('cancelPreviewBtn');

// 當前預覽的圖片數據
let currentImageData = null;

// Emoji 列表
const emojis = ['😊', '😂', '😍', '🥰', '😎', '🤔', '😮', '😢', '😡', '👍', '👎', '👏', '🙏', '💪', '🎉', '❤️', '💯', '🔥', '⭐', '✨', '🌟', '💡', '📷', '🎵', '🎮', '⚽', '🍕', '🍔', '🎂', '☕', '🌈', '🌸'];

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
    connection.on("ReceiveMessage", (messageId, sender, message, timestamp, type, imageData) => {
        addMessage(messageId, sender, message, timestamp, sender === currentUser, type, imageData);
        
        // 如果不是自己的訊息，標記為已讀
        if (sender !== currentUser) {
            setTimeout(() => {
                connection.invoke("MarkMessageAsRead", messageId).catch(err => {
                    console.error("標記已讀錯誤:", err);
                });
            }, 1000);
        }
    });

    // 訊息已讀狀態更新
    connection.on("MessageReadStatusUpdated", (messageId, readCount) => {
        updateMessageReadStatus(messageId, readCount);
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

// 發送圖片
async function sendImage(imageData) {
    try {
        await connection.invoke("SendImageMessage", imageData);
    } catch (err) {
        console.error("發送圖片錯誤:", err);
        showError("圖片發送失敗，請稍後再試");
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
function addMessage(messageId, sender, content, timestamp, isOwn = false, type = 'Text', imageData = null) {
    const messageDiv = document.createElement('div');
    messageDiv.className = `message ${isOwn ? 'own' : ''}`;
    messageDiv.dataset.messageId = messageId;

    const time = formatTime(new Date(timestamp));

    // 建立訊息標題
    const headerDiv = document.createElement('div');
    headerDiv.className = 'message-header';
    
    const senderSpan = document.createElement('span');
    senderSpan.className = 'message-sender';
    senderSpan.textContent = sender;
    
    const timeSpan = document.createElement('span');
    timeSpan.className = 'message-time';
    timeSpan.textContent = time;
    
    headerDiv.appendChild(senderSpan);
    headerDiv.appendChild(timeSpan);

    // 建立訊息內容包裝器
    const wrapperDiv = document.createElement('div');
    wrapperDiv.className = 'message-content-wrapper';

    // 建立訊息內容
    const contentDiv = document.createElement('div');
    contentDiv.className = 'message-content';
    
    if (type === 'Image' && imageData) {
        const img = document.createElement('img');
        img.src = imageData;
        img.alt = '圖片';
        img.onclick = () => window.open(img.src);
        contentDiv.appendChild(img);
    } else {
        // 使用 textContent 來顯示內容，這樣可以正確顯示 emoji
        contentDiv.textContent = content;
    }
    
    wrapperDiv.appendChild(contentDiv);

    // 如果是自己的訊息，添加狀態指示
    if (isOwn) {
        const statusSpan = document.createElement('span');
        statusSpan.className = 'message-status';
        statusSpan.dataset.status = '';
        statusSpan.textContent = '傳送中...';
        wrapperDiv.appendChild(statusSpan);
    }

    messageDiv.appendChild(headerDiv);
    messageDiv.appendChild(wrapperDiv);
    messagesList.appendChild(messageDiv);
    scrollToBottom();
}

// 更新訊息已讀狀態
function updateMessageReadStatus(messageId, readCount) {
    const messageDiv = document.querySelector(`[data-message-id="${messageId}"]`);
    if (messageDiv) {
        const statusSpan = messageDiv.querySelector('[data-status]');
        if (statusSpan) {
            if (readCount > 0) {
                statusSpan.textContent = `已讀 ${readCount}`;
                statusSpan.style.color = '#4A90E2';
            } else {
                statusSpan.textContent = '已送出';
            }
        }
    }
}

// 新增系統訊息
function addSystemMessage(message) {
    const messageDiv = document.createElement('div');
    messageDiv.className = 'message system';
    
    const contentDiv = document.createElement('div');
    contentDiv.className = 'message-content';
    contentDiv.textContent = message;
    
    messageDiv.appendChild(contentDiv);
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
        
        const nameSpan = document.createElement('span');
        nameSpan.className = 'user-name';
        nameSpan.textContent = user.nickname;
        
        const timeSpan = document.createElement('span');
        timeSpan.className = 'user-time';
        timeSpan.textContent = time;
        
        li.appendChild(nameSpan);
        li.appendChild(timeSpan);
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

// 初始化 Emoji 選擇器
function initializeEmojiPicker() {
    emojis.forEach(emoji => {
        const emojiSpan = document.createElement('span');
        emojiSpan.className = 'emoji-item';
        emojiSpan.textContent = emoji;
        emojiSpan.onclick = () => {
            messageInput.value += emoji;
            emojiPicker.style.display = 'none';
            messageInput.focus();
        };
        emojiList.appendChild(emojiSpan);
    });
}

// 顯示圖片預覽
function showImagePreview(imageData) {
    currentImageData = imageData;
    previewImage.src = imageData;
    imagePreview.style.display = 'block';
    emojiPicker.style.display = 'none';
}

// 隱藏圖片預覽
function hideImagePreview() {
    imagePreview.style.display = 'none';
    currentImageData = null;
    imageInput.value = '';
}

// 發送預覽的圖片
async function sendPreviewedImage() {
    if (currentImageData) {
        await sendImage(currentImageData);
        hideImagePreview();
    }
}

// 處理圖片上傳
function handleImageUpload(event) {
    const file = event.target.files[0];
    if (file) {
        // 檢查檔案大小（限制 5MB）
        if (file.size > 5 * 1024 * 1024) {
            showError('圖片大小不可超過 5MB');
            imageInput.value = '';
            return;
        }

        // 檢查檔案類型
        if (!file.type.startsWith('image/')) {
            showError('只能上傳圖片檔案');
            imageInput.value = '';
            return;
        }

        const reader = new FileReader();
        reader.onload = (e) => {
            showImagePreview(e.target.result);
        };
        reader.readAsDataURL(file);
    }
}

// 處理貼上圖片
function handlePaste(event) {
    const items = event.clipboardData.items;
    for (let i = 0; i < items.length; i++) {
        if (items[i].type.indexOf('image') !== -1) {
            event.preventDefault();
            const blob = items[i].getAsFile();
            
            // 檢查檔案大小（限制 5MB）
            if (blob.size > 5 * 1024 * 1024) {
                showError('圖片大小不可超過 5MB');
                return;
            }
            
            const reader = new FileReader();
            reader.onload = (e) => {
                showImagePreview(e.target.result);
            };
            reader.readAsDataURL(blob);
            break;
        }
    }
}

// 事件監聽器
document.addEventListener('DOMContentLoaded', () => {
    // 初始化連線
    initializeConnection();
    
    // 初始化 Emoji 選擇器
    initializeEmojiPicker();

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

    // 貼上圖片
    messageInput.addEventListener('paste', handlePaste);

    // Emoji 按鈕
    emojiBtn.addEventListener('click', () => {
        emojiPicker.style.display = emojiPicker.style.display === 'none' ? 'block' : 'none';
    });

    // 點擊其他地方關閉 Emoji 選擇器
    document.addEventListener('click', (e) => {
        if (!emojiBtn.contains(e.target) && !emojiPicker.contains(e.target)) {
            emojiPicker.style.display = 'none';
        }
    });

    // 圖片按鈕
    imageBtn.addEventListener('click', () => {
        imageInput.click();
    });

    // 圖片上傳
    imageInput.addEventListener('change', handleImageUpload);

    // 圖片預覽 - 發送按鈕
    sendImageBtn.addEventListener('click', sendPreviewedImage);

    // 圖片預覽 - 取消按鈕
    cancelPreviewBtn.addEventListener('click', hideImagePreview);

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
