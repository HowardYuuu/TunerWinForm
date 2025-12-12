using ChatRoom.Hubs;
using ChatRoom.Services;

var builder = WebApplication.CreateBuilder(args);

// 新增 SignalR 服務
builder.Services.AddSignalR();

// 註冊單例服務
builder.Services.AddSingleton<UserConnectionService>();

// 新增 CORS 設定
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// 啟用靜態檔案服務
app.UseDefaultFiles();
app.UseStaticFiles();

// 啟用 CORS
app.UseCors();

// 設定 SignalR Hub 路由
app.MapHub<ChatHub>("/chatHub");

app.Run();
