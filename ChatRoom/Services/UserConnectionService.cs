using System.Collections.Concurrent;
using ChatRoom.Models;

namespace ChatRoom.Services;

public class UserConnectionService
{
    private readonly ConcurrentDictionary<string, UserInfo> _connections = new();

    public bool AddUser(string connectionId, string nickname)
    {
        // 檢查暱稱是否已存在
        if (_connections.Values.Any(u => u.Nickname == nickname))
        {
            return false;
        }

        var userInfo = new UserInfo
        {
            ConnectionId = connectionId,
            Nickname = nickname,
            ConnectedAt = DateTime.Now
        };

        return _connections.TryAdd(connectionId, userInfo);
    }

    public bool RemoveUser(string connectionId)
    {
        return _connections.TryRemove(connectionId, out _);
    }

    public UserInfo? GetUser(string connectionId)
    {
        _connections.TryGetValue(connectionId, out var userInfo);
        return userInfo;
    }

    public List<UserInfo> GetAllUsers()
    {
        return _connections.Values.ToList();
    }

    public int GetOnlineCount()
    {
        return _connections.Count;
    }
}
