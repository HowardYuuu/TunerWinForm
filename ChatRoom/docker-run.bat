@echo off
REM ChatRoom Docker 快速啟動腳本 (Windows)
REM 此腳本幫助快速建置和啟動 ChatRoom 容器

setlocal enabledelayedexpansion

REM 檢查 Docker 是否安裝
where docker >nul 2>nul
if %errorlevel% neq 0 (
    echo [錯誤] Docker 未安裝
    echo 請先安裝 Docker: https://docs.docker.com/get-docker/
    exit /b 1
)

where docker-compose >nul 2>nul
if %errorlevel% neq 0 (
    echo [錯誤] Docker Compose 未安裝
    echo 請先安裝 Docker Compose: https://docs.docker.com/compose/install/
    exit /b 1
)

REM 進入腳本所在目錄
cd /d "%~dp0"

if "%1"=="" goto :help
if "%1"=="build" goto :build
if "%1"=="start" goto :start
if "%1"=="dev" goto :dev
if "%1"=="stop" goto :stop
if "%1"=="restart" goto :restart
if "%1"=="logs" goto :logs
if "%1"=="status" goto :status
if "%1"=="clean" goto :clean
if "%1"=="help" goto :help
goto :help

:build
echo [建置] 正在建置 ChatRoom Docker 映像...
docker-compose build
if %errorlevel% equ 0 (
    echo [成功] 建置完成
) else (
    echo [錯誤] 建置失敗
    exit /b 1
)
goto :eof

:start
echo [啟動] 正在啟動 ChatRoom 容器（生產模式）...
call :build
docker-compose up -d
if %errorlevel% equ 0 (
    echo [成功] 容器已啟動
    echo [資訊] 應用程式運行於: http://localhost:5180
) else (
    echo [錯誤] 啟動失敗
    exit /b 1
)
goto :eof

:dev
echo [啟動] 正在啟動 ChatRoom 容器（開發模式）...
call :build
docker-compose -f docker-compose.dev.yml up -d
if %errorlevel% equ 0 (
    echo [成功] 容器已啟動
    echo [資訊] 應用程式運行於: http://localhost:5180
) else (
    echo [錯誤] 啟動失敗
    exit /b 1
)
goto :eof

:stop
echo [停止] 正在停止 ChatRoom 容器...
docker-compose down
docker-compose -f docker-compose.dev.yml down 2>nul
echo [成功] 容器已停止
goto :eof

:restart
echo [重啟] 正在重啟 ChatRoom 容器...
call :stop
call :start
goto :eof

:logs
echo [日誌] 顯示 ChatRoom 容器日誌...
docker-compose logs -f
goto :eof

:status
echo [狀態] ChatRoom 容器狀態:
docker-compose ps
goto :eof

:clean
echo [清理] 正在清理 ChatRoom 容器和映像...
docker-compose down -v
docker-compose -f docker-compose.dev.yml down -v 2>nul
docker rmi chatroom-chatroom 2>nul
echo [成功] 清理完成
goto :eof

:help
echo 使用方式: %~nx0 [command]
echo.
echo 可用指令:
echo   build      - 建置 Docker 映像
echo   start      - 啟動容器（生產模式）
echo   dev        - 啟動容器（開發模式）
echo   stop       - 停止容器
echo   restart    - 重啟容器
echo   logs       - 查看容器日誌
echo   status     - 查看容器狀態
echo   clean      - 清理容器和映像
echo   help       - 顯示此說明
echo.
goto :eof

endlocal
