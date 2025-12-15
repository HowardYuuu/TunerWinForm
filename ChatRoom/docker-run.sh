#!/bin/bash

# ChatRoom Docker 快速啟動腳本
# 此腳本幫助快速建置和啟動 ChatRoom 容器

set -e

# 顏色輸出
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# 顯示使用說明
show_usage() {
    echo "使用方式: $0 [command]"
    echo ""
    echo "可用指令:"
    echo "  build      - 建置 Docker 映像"
    echo "  start      - 啟動容器（生產模式）"
    echo "  dev        - 啟動容器（開發模式）"
    echo "  stop       - 停止容器"
    echo "  restart    - 重啟容器"
    echo "  logs       - 查看容器日誌"
    echo "  status     - 查看容器狀態"
    echo "  clean      - 清理容器和映像"
    echo "  help       - 顯示此說明"
    echo ""
}

# 檢查 Docker 是否安裝
check_docker() {
    if ! command -v docker &> /dev/null; then
        echo -e "${RED}錯誤: Docker 未安裝${NC}"
        echo "請先安裝 Docker: https://docs.docker.com/get-docker/"
        exit 1
    fi

    if ! command -v docker-compose &> /dev/null; then
        echo -e "${RED}錯誤: Docker Compose 未安裝${NC}"
        echo "請先安裝 Docker Compose: https://docs.docker.com/compose/install/"
        exit 1
    fi
}

# 建置映像
build() {
    echo -e "${GREEN}正在建置 ChatRoom Docker 映像...${NC}"
    cd "$(dirname "$0")"
    docker-compose build
    echo -e "${GREEN}✓ 建置完成${NC}"
}

# 啟動容器（生產模式）
start() {
    echo -e "${GREEN}正在啟動 ChatRoom 容器（生產模式）...${NC}"
    cd "$(dirname "$0")"
    docker-compose up -d
    echo -e "${GREEN}✓ 容器已啟動${NC}"
    echo -e "${YELLOW}應用程式運行於: http://localhost:5180${NC}"
}

# 啟動容器（開發模式）
dev() {
    echo -e "${GREEN}正在啟動 ChatRoom 容器（開發模式）...${NC}"
    cd "$(dirname "$0")"
    docker-compose -f docker-compose.dev.yml up -d
    echo -e "${GREEN}✓ 容器已啟動${NC}"
    echo -e "${YELLOW}應用程式運行於: http://localhost:5180${NC}"
}

# 停止容器
stop() {
    echo -e "${YELLOW}正在停止 ChatRoom 容器...${NC}"
    cd "$(dirname "$0")"
    docker-compose down
    docker-compose -f docker-compose.dev.yml down 2>/dev/null || true
    echo -e "${GREEN}✓ 容器已停止${NC}"
}

# 重啟容器
restart() {
    echo -e "${YELLOW}正在重啟 ChatRoom 容器...${NC}"
    stop
    start
}

# 查看日誌
logs() {
    echo -e "${GREEN}顯示 ChatRoom 容器日誌...${NC}"
    cd "$(dirname "$0")"
    docker-compose logs -f
}

# 查看狀態
status() {
    echo -e "${GREEN}ChatRoom 容器狀態:${NC}"
    cd "$(dirname "$0")"
    docker-compose ps
}

# 清理
clean() {
    echo -e "${YELLOW}正在清理 ChatRoom 容器和映像...${NC}"
    cd "$(dirname "$0")"
    docker-compose down -v
    docker-compose -f docker-compose.dev.yml down -v 2>/dev/null || true
    docker rmi chatroom-chatroom 2>/dev/null || true
    echo -e "${GREEN}✓ 清理完成${NC}"
}

# 主函式
main() {
    check_docker

    case "${1:-help}" in
        build)
            build
            ;;
        start)
            build
            start
            ;;
        dev)
            build
            dev
            ;;
        stop)
            stop
            ;;
        restart)
            restart
            ;;
        logs)
            logs
            ;;
        status)
            status
            ;;
        clean)
            clean
            ;;
        help|*)
            show_usage
            ;;
    esac
}

main "$@"
