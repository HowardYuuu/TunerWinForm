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
        // 開發環境設定：允許所有來源
        // 生產環境應改為：policy.WithOrigins("https://yourdomain.com").AllowCredentials()
        if (builder.Environment.IsDevelopment())
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
        else
        {
            // 生產環境：限制特定來源並啟用憑證
            policy.WithOrigins("https://yourdomain.com")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
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
