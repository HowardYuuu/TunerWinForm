using Microsoft.AspNetCore.SignalR;
using ChatRoom.Services;
using ChatRoom.Models;
using System.Net;

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

        // 編碼暱稱以防止 XSS
        var sanitizedNickname = WebUtility.HtmlEncode(nickname.Trim());

        // 新增用戶
        if (_userConnectionService.AddUser(Context.ConnectionId, sanitizedNickname))
        {
            // 通知該用戶連線成功
            await Clients.Caller.SendAsync("JoinedSuccessfully", sanitizedNickname);

            // 取得在線人數
            var onlineCount = _userConnectionService.GetOnlineCount();

            // 通知所有用戶有新用戶加入
            await Clients.All.SendAsync("UserJoined", sanitizedNickname, onlineCount);

            // 發送系統訊息
            await Clients.All.SendAsync("SystemMessage", $"{sanitizedNickname} 加入了聊天室");

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
            // 編碼訊息以防止 XSS
            var sanitizedMessage = WebUtility.HtmlEncode(message.Trim());

            // 廣播訊息給所有用戶
            await Clients.All.SendAsync("ReceiveMessage", 
                userInfo.Nickname, 
                sanitizedMessage, 
                DateTime.Now);
        }
        else
        {
            await Clients.Caller.SendAsync("Error", "請先加入聊天室");
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
