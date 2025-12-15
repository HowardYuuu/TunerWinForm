# ChatRoom Docker 容器化指南

## 概述

此文件說明如何使用 Docker 容器化 ChatRoom 即時聊天室應用程式。

## 前置需求

- [Docker](https://www.docker.com/get-started) 20.10 或更新版本
- [Docker Compose](https://docs.docker.com/compose/install/) 1.29 或更新版本

## 檔案結構

```
ChatRoom/
├── Dockerfile                    # Docker 映像建置檔案
├── .dockerignore                 # Docker 忽略檔案
├── docker-compose.yml            # 生產環境 Docker Compose 配置
├── docker-compose.dev.yml        # 開發環境 Docker Compose 配置
└── DOCKER.md                     # 本文件
```

## 快速開始

### 方法 1：使用 Docker Compose（推薦）

#### 生產環境

```bash
# 進入 ChatRoom 目錄
cd ChatRoom

# 建置並啟動容器
docker-compose up -d

# 查看日誌
docker-compose logs -f

# 停止容器
docker-compose down
```

應用程式將在 `http://localhost:5180` 上運行。

#### 開發環境

```bash
# 使用開發環境配置
docker-compose -f docker-compose.dev.yml up -d

# 查看日誌
docker-compose -f docker-compose.dev.yml logs -f

# 停止容器
docker-compose -f docker-compose.dev.yml down
```

### 方法 2：直接使用 Docker

#### 建置映像

```bash
# 在專案根目錄執行
cd /path/to/TunerWinForm

# 建置 Docker 映像
docker build -t chatroom:latest -f ChatRoom/Dockerfile .
```

#### 執行容器

```bash
# 執行容器
docker run -d \
  --name chatroom-app \
  -p 5180:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  chatroom:latest

# 查看日誌
docker logs -f chatroom-app

# 停止容器
docker stop chatroom-app

# 移除容器
docker rm chatroom-app
```

## 配置選項

### 環境變數

| 變數名稱 | 預設值 | 說明 |
|---------|--------|------|
| `ASPNETCORE_ENVIRONMENT` | `Production` | 執行環境（Development/Production） |
| `ASPNETCORE_URLS` | `http://+:8080` | 應用程式監聽的 URL |
| `ASPNETCORE_HTTP_PORTS` | `8080` | HTTP 埠號 |

### 埠號映射

- `5180:8080` - HTTP 連接埠（可在 docker-compose.yml 中修改）
- `5181:8081` - HTTPS 連接埠（預留）

### 卷（Volumes）

- `chatroom-logs` - 持久化應用程式日誌

## 進階使用

### 自訂配置

編輯 `docker-compose.yml` 或 `docker-compose.dev.yml` 來自訂配置：

```yaml
services:
  chatroom:
    environment:
      # 新增或修改環境變數
      - CUSTOM_SETTING=value
    ports:
      # 修改埠號映射
      - "8080:8080"
```

### 健康檢查

容器包含內建的健康檢查機制：

```bash
# 檢查容器健康狀態
docker ps

# 查看健康檢查詳細資訊
docker inspect chatroom-app | grep -A 10 Health
```

### 查看容器資訊

```bash
# 查看運行中的容器
docker ps

# 查看所有容器（包括已停止的）
docker ps -a

# 進入容器 shell
docker exec -it chatroom-app /bin/bash

# 查看容器資源使用情況
docker stats chatroom-app
```

### 清理資源

```bash
# 停止並移除容器
docker-compose down

# 同時移除卷
docker-compose down -v

# 移除映像
docker rmi chatroom:latest

# 清理未使用的 Docker 資源
docker system prune -a
```

## 多階段建置說明

Dockerfile 使用多階段建置來優化映像大小：

1. **Build 階段**：使用 SDK 映像建置應用程式
2. **Publish 階段**：發佈應用程式
3. **Final 階段**：使用輕量的 Runtime 映像執行應用程式

這種方式可以顯著減少最終映像的大小。

## 疑難排解

### 容器無法啟動

```bash
# 查看詳細錯誤訊息
docker logs chatroom-app

# 檢查容器狀態
docker inspect chatroom-app
```

### 埠號衝突

如果埠號 5180 已被佔用：

```bash
# 修改 docker-compose.yml 中的埠號映射
ports:
  - "5280:8080"  # 改用 5280
```

### 連線到容器內的應用程式

```bash
# 從容器內部測試
docker exec chatroom-app curl http://localhost:8080
```

### 重建映像

如果修改了程式碼，需要重建映像：

```bash
# 停止並移除舊容器
docker-compose down

# 重建映像並啟動
docker-compose up -d --build
```

## 生產環境部署建議

### 1. 使用環境變數檔案

建立 `.env` 檔案：

```env
ASPNETCORE_ENVIRONMENT=Production
CHATROOM_PORT=5180
```

在 docker-compose.yml 中引用：

```yaml
services:
  chatroom:
    ports:
      - "${CHATROOM_PORT}:8080"
```

### 2. 設定 HTTPS

如果需要 HTTPS：

```yaml
services:
  chatroom:
    environment:
      - ASPNETCORE_URLS=https://+:8081;http://+:8080
      - ASPNETCORE_Kestrel__Certificates__Default__Path=/https/certificate.pfx
      - ASPNETCORE_Kestrel__Certificates__Default__Password=your_password
    volumes:
      - ./https:/https:ro
```

### 3. 使用反向代理

建議在生產環境中使用 Nginx 或 Traefik 作為反向代理：

```yaml
services:
  nginx:
    image: nginx:alpine
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./nginx.conf:/etc/nginx/nginx.conf:ro
    depends_on:
      - chatroom
```

### 4. 監控和日誌

建議整合日誌收集系統（如 ELK Stack）和監控工具（如 Prometheus）。

## 效能優化

### 映像大小優化

目前的 Dockerfile 已經使用多階段建置，進一步優化建議：

1. 使用 Alpine 基礎映像（如果相容）
2. 清理不必要的檔案
3. 合併 RUN 指令

### 執行時優化

```yaml
services:
  chatroom:
    deploy:
      resources:
        limits:
          cpus: '1.0'
          memory: 512M
        reservations:
          cpus: '0.5'
          memory: 256M
```

## 安全性考量

1. **不要在映像中包含敏感資訊**：使用環境變數或 Docker secrets
2. **定期更新基礎映像**：確保使用最新的安全補丁
3. **最小權限原則**：容器以非 root 使用者執行
4. **網路隔離**：使用 Docker 網路隔離容器

## CI/CD 整合

### GitHub Actions 範例

```yaml
- name: Build and push Docker image
  run: |
    docker build -t chatroom:${{ github.sha }} -f ChatRoom/Dockerfile .
    docker tag chatroom:${{ github.sha }} chatroom:latest
```

### 建議的工作流程

1. 推送程式碼到 Git
2. CI 系統自動建置 Docker 映像
3. 執行測試
4. 推送映像到 Container Registry
5. 部署到目標環境

## 相關資源

- [Docker 官方文件](https://docs.docker.com/)
- [Docker Compose 文件](https://docs.docker.com/compose/)
- [ASP.NET Core Docker 文件](https://docs.microsoft.com/aspnet/core/host-and-deploy/docker/)
- [ChatRoom README](./README.md)

## 支援

如有問題，請參考：
1. ChatRoom 專案的 README.md
2. Docker 官方文件
3. ASP.NET Core 部署指南
