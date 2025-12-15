# 使用官方 .NET 8.0 SDK 映像作為建置階段
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 複製專案檔案並還原相依性
COPY ["ChatRoom/ChatRoom.csproj", "ChatRoom/"]
RUN dotnet restore "ChatRoom/ChatRoom.csproj"

# 複製所有原始碼並建置應用程式
COPY . .
WORKDIR "/src/ChatRoom"
RUN dotnet build "ChatRoom.csproj" -c Release -o /app/build

# 發佈應用程式
FROM build AS publish
RUN dotnet publish "ChatRoom.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 使用官方 .NET 8.0 Runtime 映像作為最終階段
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# 安裝必要的系統相依性
RUN apt-get update && apt-get install -y \
    curl \
    && rm -rf /var/lib/apt/lists/*

# 複製發佈的應用程式
COPY --from=publish /app/publish .

# 暴露應用程式使用的埠號
EXPOSE 8080
EXPOSE 8081

# 設定環境變數
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# 設定健康檢查
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:8080/ || exit 1

# 啟動應用程式
ENTRYPOINT ["dotnet", "ChatRoom.dll"]
