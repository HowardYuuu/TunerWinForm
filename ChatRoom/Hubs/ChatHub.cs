using Microsoft.AspNetCore.SignalR;
using ChatRoom.Services;
using ChatRoom.Models;

namespace ChatRoom.Hubs;

public class ChatHub : Hub
{
    private const int MaxNicknameLength = 20;
    private const int MaxMessageLength = 500;
    
    private readonly UserConnectionService _userConnectionService;

    public ChatHub(UserConnectionService userConnectionService)
    {
        _userConnectionService = userConnectionService;
    }

    public async Task JoinChat(string nickname)
    {
        // 驗證暱稱
        if (string.IsNullOrWhiteSpace(nickname) || nickname.Length > MaxNicknameLength)
        {
            await Clients.Caller.SendAsync("Error", $"暱稱不可為空或超過{MaxNicknameLength}個字元");
            return;
        }

        // 不需要在後端編碼，因為前端使用 textContent 來安全顯示
        var trimmedNickname = nickname.Trim();

        // 新增用戶
        if (_userConnectionService.AddUser(Context.ConnectionId, trimmedNickname))
        {
            // 通知該用戶連線成功
            await Clients.Caller.SendAsync("JoinedSuccessfully", trimmedNickname);

            // 取得在線人數
            var onlineCount = _userConnectionService.GetOnlineCount();

            // 通知所有用戶有新用戶加入
            await Clients.All.SendAsync("UserJoined", trimmedNickname, onlineCount);

            // 發送系統訊息
            await Clients.All.SendAsync("SystemMessage", $"{trimmedNickname} 加入了聊天室");

            // 更新在線用戶列表
            var users = _userConnectionService.GetAllUsers();
            await Clients.All.SendAsync("UpdateOnlineUsers", users);
        }
        else
        {
            await Clients.Caller.SendAsync("Error", "暱稱已被使用，請選擇其他暱稱");
        }
    }

    public async Task SendMessage(string message)
    {
        // 驗證訊息
        if (string.IsNullOrWhiteSpace(message) || message.Length > MaxMessageLength)
        {
            await Clients.Caller.SendAsync("Error", $"訊息不可為空或超過{MaxMessageLength}個字元");
            return;
        }

        var userInfo = _userConnectionService.GetUser(Context.ConnectionId);
        if (userInfo != null)
        {
            // 不需要在後端編碼，因為前端使用 textContent 來安全顯示
            var trimmedMessage = message.Trim();

            // 解析 @ 提及
            var mentionedUsers = ParseMentions(trimmedMessage);

            var chatMessage = new ChatMessage
            {
                MessageId = Guid.NewGuid().ToString(),
                Sender = userInfo.Nickname,
                Content = trimmedMessage,
                Timestamp = DateTime.Now,
                Type = MessageType.Text,
                MentionedUsers = mentionedUsers
            };

            // 儲存訊息
            _userConnectionService.AddMessage(chatMessage);

            // 廣播訊息給所有用戶
            await Clients.All.SendAsync("ReceiveMessage", 
                chatMessage.MessageId,
                chatMessage.Sender, 
                chatMessage.Content, 
                chatMessage.Timestamp,
                chatMessage.Type.ToString(),
                null, // imageData
                chatMessage.MentionedUsers);
            
            // 通知被提及的用戶
            foreach (var mentionedUser in mentionedUsers)
            {
                var mentionedUserInfo = _userConnectionService.GetUserByNickname(mentionedUser);
                if (mentionedUserInfo != null && mentionedUserInfo.Nickname != userInfo.Nickname)
                {
                    await Clients.Client(mentionedUserInfo.ConnectionId)
                        .SendAsync("UserMentioned", userInfo.Nickname, chatMessage.MessageId);
                }
            }
        }
        else
        {
            await Clients.Caller.SendAsync("Error", "請先加入聊天室");
        }
    }

    public async Task SendImageMessage(string imageData)
    {
        var userInfo = _userConnectionService.GetUser(Context.ConnectionId);
        if (userInfo != null)
        {
            var chatMessage = new ChatMessage
            {
                MessageId = Guid.NewGuid().ToString(),
                Sender = userInfo.Nickname,
                Content = "",
                Timestamp = DateTime.Now,
                Type = MessageType.Image,
                ImageData = imageData
            };

            // 儲存訊息
            _userConnectionService.AddMessage(chatMessage);

            // 廣播圖片訊息給所有用戶
            await Clients.All.SendAsync("ReceiveMessage", 
                chatMessage.MessageId,
                chatMessage.Sender, 
                chatMessage.Content, 
                chatMessage.Timestamp,
                chatMessage.Type.ToString(),
                chatMessage.ImageData,
                chatMessage.MentionedUsers);
        }
        else
        {
            await Clients.Caller.SendAsync("Error", "請先加入聊天室");
        }
    }

    public async Task MarkMessageAsRead(string messageId)
    {
        if (_userConnectionService.MarkMessageAsRead(messageId, Context.ConnectionId))
        {
            var message = _userConnectionService.GetMessage(messageId);
            if (message != null)
            {
                // 通知訊息發送者已讀狀態更新
                await Clients.All.SendAsync("MessageReadStatusUpdated", messageId, message.ReadBy.Count);
            }
        }
    }

    public async Task LeaveChat()
    {
        var userInfo = _userConnectionService.GetUser(Context.ConnectionId);
        if (userInfo != null)
        {
            await HandleUserDisconnection(userInfo);
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userInfo = _userConnectionService.GetUser(Context.ConnectionId);
        if (userInfo != null)
        {
            await HandleUserDisconnection(userInfo);
        }

        await base.OnDisconnectedAsync(exception);
    }

    private List<string> ParseMentions(string message)
    {
        var mentions = new List<string>();
        var words = message.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var word in words)
        {
            if (word.StartsWith('@') && word.Length > 1)
            {
                // 移除 @ 符號並清除末尾標點符號
                var username = word.Substring(1).TrimEnd(',', '.', '!', '?', ';', ':', ')', ']', '}', '"', '\'', '>', '…');
                
                // 驗證用戶是否存在
                var user = _userConnectionService.GetUserByNickname(username);
                if (user != null && !mentions.Contains(username))
                {
                    mentions.Add(username);
                }
            }
        }
        
        return mentions;
    }

    private async Task HandleUserDisconnection(UserInfo userInfo)
    {
        _userConnectionService.RemoveUser(Context.ConnectionId);

        // 通知所有用戶該用戶離開
        var onlineCount = _userConnectionService.GetOnlineCount();
        await Clients.All.SendAsync("UserLeft", userInfo.Nickname, onlineCount);

        // 發送系統訊息
        await Clients.All.SendAsync("SystemMessage", $"{userInfo.Nickname} 離開了聊天室");

        // 更新在線用戶列表
        var users = _userConnectionService.GetAllUsers();
        await Clients.All.SendAsync("UpdateOnlineUsers", users);
    }
}
