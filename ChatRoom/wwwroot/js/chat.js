// 常數定義
const MAX_NICKNAME_LENGTH = 20;
const MAX_MESSAGE_LENGTH = 500;

// SignalR 連線設定
let connection = null;
let currentUser = null;

// 通知權限狀態
let notificationPermission = 'default';

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

// 在線用戶列表（用於 @ 提及自動完成）
let onlineUsers = [];

// @ 提及自動完成狀態
let mentionAutocomplete = {
    isActive: false,
    startPosition: -1,
    selectedIndex: -1,
    filteredUsers: []
};

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
    connection.on("ReceiveMessage", (messageId, sender, message, timestamp, type, imageData, mentionedUsers) => {
        const isMentioned = mentionedUsers && mentionedUsers.includes(currentUser);
        addMessage(messageId, sender, message, timestamp, sender === currentUser, type, imageData, isMentioned);
        
        // 如果不是自己的訊息，標記為已讀
        if (sender !== currentUser) {
            // 檢查網頁是否不可見，如果是則顯示通知
            if (document.hidden && notificationPermission === 'granted') {
                showDesktopNotification(sender, message, type, imageData, isMentioned);
            }
            
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

    // 用戶被提及
    connection.on("UserMentioned", (mentioner, messageId) => {
        showMentionNotification(mentioner);
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
        
        // 請求通知權限
        await requestNotificationPermission();
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
function addMessage(messageId, sender, content, timestamp, isOwn = false, type = 'Text', imageData = null, isMentioned = false) {
    const messageDiv = document.createElement('div');
    messageDiv.className = `message ${isOwn ? 'own' : ''} ${isMentioned ? 'mentioned' : ''}`;
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
        // 處理 @ 提及的高亮顯示
        if (content.includes('@')) {
            contentDiv.innerHTML = highlightMentions(escapeHtml(content));
        } else {
            contentDiv.textContent = content;
        }
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
    
    // 儲存在線用戶列表供 @ 提及使用
    onlineUsers = users.map(u => u.nickname);
    
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

// 請求桌面通知權限
async function requestNotificationPermission() {
    if (!('Notification' in window)) {
        console.log('瀏覽器不支援桌面通知');
        return;
    }
    
    if (Notification.permission === 'granted') {
        notificationPermission = 'granted';
        return;
    }
    
    if (Notification.permission !== 'denied') {
        const permission = await Notification.requestPermission();
        notificationPermission = permission;
    }
}

// 顯示桌面通知
function showDesktopNotification(sender, message, type, imageData, isMentioned = false) {
    if (notificationPermission !== 'granted') {
        return;
    }
    
    let notificationBody = message;
    let notificationIcon = null;
    
    if (type === 'Image') {
        notificationBody = '發送了一張圖片';
        // 使用圖片的縮略圖作為通知圖標（如果可用）
        if (imageData) {
            notificationIcon = imageData;
        }
    }
    
    // 如果被提及，在通知中顯示
    if (isMentioned) {
        notificationBody = `提及了你：${notificationBody}`;
    }
    
    const notification = new Notification(`💬 ${sender}`, {
        body: notificationBody,
        icon: notificationIcon || '/favicon.ico',
        badge: '/favicon.ico',
        tag: 'chat-message',
        requireInteraction: false,
        silent: false
    });
    
    // 點擊通知時聚焦到網頁
    notification.onclick = () => {
        window.focus();
        notification.close();
    };
    
    // 4秒後自動關閉通知
    setTimeout(() => {
        notification.close();
    }, 4000);
}

// 顯示提及通知
function showMentionNotification(mentioner) {
    addSystemMessage(`💬 ${mentioner} 提及了你`);
}

// 高亮顯示 @ 提及
function highlightMentions(text) {
    // 使用正則表達式匹配 @username，支援中文字符
    return text.replace(/@([\w\u4e00-\u9fff]+)/g, '<span class="mention">@$1</span>');
}

// 處理 @ 提及自動完成
function handleMentionAutocomplete(event) {
    const input = messageInput;
    const cursorPos = input.selectionStart;
    const textBeforeCursor = input.value.substring(0, cursorPos);
    
    // 檢查是否在輸入 @
    const atMatch = textBeforeCursor.match(/@([\w\u4e00-\u9fff]*)$/);
    
    if (atMatch) {
        const searchTerm = atMatch[1].toLowerCase();
        mentionAutocomplete.startPosition = cursorPos - atMatch[0].length;
        
        // 過濾符合的用戶（排除自己）
        mentionAutocomplete.filteredUsers = onlineUsers
            .filter(user => user !== currentUser && user.toLowerCase().includes(searchTerm));
        
        if (mentionAutocomplete.filteredUsers.length > 0) {
            mentionAutocomplete.isActive = true;
            mentionAutocomplete.selectedIndex = 0;
            showMentionDropdown();
        } else {
            hideMentionDropdown();
        }
    } else {
        hideMentionDropdown();
    }
}

// 顯示 @ 提及下拉選單
function showMentionDropdown() {
    let dropdown = document.getElementById('mentionDropdown');
    
    if (!dropdown) {
        dropdown = document.createElement('div');
        dropdown.id = 'mentionDropdown';
        dropdown.className = 'mention-dropdown';
        document.querySelector('.input-area').appendChild(dropdown);
    }
    
    dropdown.innerHTML = '';
    
    mentionAutocomplete.filteredUsers.forEach((user, index) => {
        const item = document.createElement('div');
        item.className = 'mention-item';
        if (index === mentionAutocomplete.selectedIndex) {
            item.classList.add('selected');
        }
        item.textContent = user;
        item.onclick = () => selectMentionUser(user);
        dropdown.appendChild(item);
    });
    
    dropdown.style.display = 'block';
}

// 隱藏 @ 提及下拉選單
function hideMentionDropdown() {
    mentionAutocomplete.isActive = false;
    mentionAutocomplete.selectedIndex = -1;
    const dropdown = document.getElementById('mentionDropdown');
    if (dropdown) {
        dropdown.style.display = 'none';
    }
}

// 選擇提及的用戶
function selectMentionUser(username) {
    const input = messageInput;
    const cursorPos = input.selectionStart;
    const textBefore = input.value.substring(0, mentionAutocomplete.startPosition);
    const textAfter = input.value.substring(cursorPos);
    
    // 插入選中的用戶名
    input.value = textBefore + '@' + username + ' ' + textAfter;
    
    // 設定游標位置
    const newCursorPos = textBefore.length + username.length + 2; // +2 for @ and space
    input.setSelectionRange(newCursorPos, newCursorPos);
    
    hideMentionDropdown();
    input.focus();
}

// 處理提及下拉選單的鍵盤導航
function handleMentionKeydown(event) {
    if (!mentionAutocomplete.isActive) {
        return false;
    }
    
    switch(event.key) {
        case 'ArrowDown':
            event.preventDefault();
            mentionAutocomplete.selectedIndex = 
                (mentionAutocomplete.selectedIndex + 1) % mentionAutocomplete.filteredUsers.length;
            showMentionDropdown();
            return true;
            
        case 'ArrowUp':
            event.preventDefault();
            mentionAutocomplete.selectedIndex = 
                (mentionAutocomplete.selectedIndex - 1 + mentionAutocomplete.filteredUsers.length) 
                % mentionAutocomplete.filteredUsers.length;
            showMentionDropdown();
            return true;
            
        case 'Tab':
        case 'Enter':
            if (mentionAutocomplete.filteredUsers.length > 0) {
                event.preventDefault();
                selectMentionUser(mentionAutocomplete.filteredUsers[mentionAutocomplete.selectedIndex]);
                return true;
            }
            break;
            
        case 'Escape':
            event.preventDefault();
            hideMentionDropdown();
            return true;
    }
    
    return false;
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

    // Enter 鍵發送訊息（但先檢查是否在選擇提及）
    messageInput.addEventListener('keydown', (e) => {
        // 處理提及下拉選單的鍵盤導航
        if (handleMentionKeydown(e)) {
            return;
        }
        
        if (e.key === 'Enter' && !mentionAutocomplete.isActive) {
            e.preventDefault();
            sendMessage();
        }
    });
    
    // 監聽輸入以觸發 @ 提及自動完成
    messageInput.addEventListener('input', handleMentionAutocomplete);

    // 貼上圖片
    messageInput.addEventListener('paste', handlePaste);

    // Emoji 按鈕
    emojiBtn.addEventListener('click', () => {
        emojiPicker.style.display = emojiPicker.style.display === 'none' ? 'block' : 'none';
    });

    // 點擊其他地方關閉 Emoji 選擇器和提及下拉選單
    document.addEventListener('click', (e) => {
        if (!emojiBtn.contains(e.target) && !emojiPicker.contains(e.target)) {
            emojiPicker.style.display = 'none';
        }
        
        const mentionDropdown = document.getElementById('mentionDropdown');
        if (mentionDropdown && !messageInput.contains(e.target) && !mentionDropdown.contains(e.target)) {
            hideMentionDropdown();
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
