#!/bin/bash

# ========================================
# Platheo API - Docker Helper Script
# ========================================

set -e

# Colores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Variables
IMAGE_NAME="platheo-api"
CONTAINER_NAME="platheo-api"
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"

# Funciones
print_usage() {
    echo -e "${GREEN}Uso:${NC}"
    echo "  ./docker-helper.sh [comando] [ambiente]"
    echo ""
    echo -e "${GREEN}Comandos:${NC}"
    echo "  build       - Construir imagen Docker"
    echo "  run         - Ejecutar contenedor"
    echo "  stop        - Detener contenedor"
    echo "  logs        - Ver logs del contenedor"
    echo "  restart     - Reiniciar contenedor"
    echo "  clean       - Limpiar contenedores e imágenes"
    echo "  shell       - Abrir shell en el contenedor"
    echo ""
    echo -e "${GREEN}Ambientes:${NC}"
    echo "  dev         - Development (default)"
    echo "  stage       - Stage"
    echo "  prod        - Production"
    echo ""
    echo -e "${GREEN}Ejemplos:${NC}"
    echo "  ./docker-helper.sh build dev"
    echo "  ./docker-helper.sh run stage"
    echo "  ./docker-helper.sh logs prod"
    echo ""
    echo -e "${YELLOW}Nota: Este script debe ejecutarse desde VectorStinger.Contaner/${NC}"
}

get_env_vars() {
    ENV=$1
    case $ENV in
        dev|development)
            echo "Development"
            ;;
        stage)
            echo "Stage"
            ;;
        prod|production)
            echo "Production"
            ;;
        *)
            echo "Development"
            ;;
    esac
}

get_port() {
    ENV=$1
    case $ENV in
        dev|development)
            echo "8034"
            ;;
        stage)
            echo "8035"
            ;;
        prod|production)
            echo "8080"
            ;;
        *)
            echo "8034"
            ;;
    esac
}

build_image() {
    ENV=$(get_env_vars $1)
    echo -e "${GREEN}?? Construyendo imagen para ambiente: $ENV${NC}"
    echo -e "${CYAN}?? Context: $ROOT_DIR${NC}"
    echo -e "${CYAN}?? Dockerfile: $SCRIPT_DIR/Dockerfile${NC}"
    
    cd "$ROOT_DIR"
    docker build \
        -f "$SCRIPT_DIR/Dockerfile" \
        -t ${IMAGE_NAME}:${ENV,,} \
        --build-arg ASPNETCORE_ENVIRONMENT=$ENV \
        .
    
    echo -e "${GREEN}? Imagen construida: ${IMAGE_NAME}:${ENV,,}${NC}"
}

run_container() {
    ENV=$(get_env_vars $1)
    PORT=$(get_port $1)
    TAG=${ENV,,}
    CONTAINER="${CONTAINER_NAME}-${TAG}"
    
    echo -e "${GREEN}?? Ejecutando contenedor para ambiente: $ENV${NC}"
    
    # Detener contenedor existente si existe
    docker stop ${CONTAINER} 2>/dev/null || true
    docker rm ${CONTAINER} 2>/dev/null || true
    
    # Source .env file if exists
    ENV_FILE="$SCRIPT_DIR/.env"
    if [ -f "$ENV_FILE" ]; then
        echo -e "${CYAN}?? Cargando variables desde .env${NC}"
        set -a
        source "$ENV_FILE"
        set +a
    fi
    
    # Crear carpeta de imágenes si no existe
    IMAGES_DIR="$ROOT_DIR/images"
    if [ ! -d "$IMAGES_DIR" ]; then
        echo -e "${YELLOW}?? Creando carpeta de imágenes: $IMAGES_DIR${NC}"
        mkdir -p "$IMAGES_DIR"
    fi
    
    docker run -d \
        --name ${CONTAINER} \
        -p ${PORT}:8080 \
        -e ASPNETCORE_ENVIRONMENT=$ENV \
        -e DB_PASSWORD="${DB_PASSWORD}" \
        -e PAYMENT_SECRET_KEY="${PAYMENT_SECRET_KEY}" \
        -e APPINSIGHTS_CONNECTION_STRING="${APPINSIGHTS_CONNECTION_STRING}" \
        -v "$IMAGES_DIR:/app/images" \
        ${IMAGE_NAME}:${TAG}
    
    echo -e "${GREEN}? Contenedor ejecutándose en puerto: $PORT${NC}"
    echo -e "${YELLOW}?? Ver logs: docker logs -f ${CONTAINER}${NC}"
    echo -e "${YELLOW}?? URL: http://localhost:$PORT${NC}"
    echo -e "${YELLOW}??  Health: http://localhost:$PORT/health${NC}"
}

stop_container() {
    ENV=$(get_env_vars $1)
    TAG=${ENV,,}
    CONTAINER="${CONTAINER_NAME}-${TAG}"
    
    echo -e "${YELLOW}?? Deteniendo contenedor: ${CONTAINER}${NC}"
    docker stop ${CONTAINER}
    echo -e "${GREEN}? Contenedor detenido${NC}"
}

show_logs() {
    ENV=$(get_env_vars $1)
    TAG=${ENV,,}
    CONTAINER="${CONTAINER_NAME}-${TAG}"
    
    echo -e "${GREEN}?? Mostrando logs de: ${CONTAINER}${NC}"
    docker logs -f ${CONTAINER}
}

restart_container() {
    ENV=$(get_env_vars $1)
    TAG=${ENV,,}
    CONTAINER="${CONTAINER_NAME}-${TAG}"
    
    echo -e "${YELLOW}?? Reiniciando contenedor: ${CONTAINER}${NC}"
    docker restart ${CONTAINER}
    echo -e "${GREEN}? Contenedor reiniciado${NC}"
}

clean_all() {
    echo -e "${RED}?? Limpiando contenedores e imágenes...${NC}"
    read -p "¿Estás seguro? (y/n) " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        docker stop $(docker ps -a -q --filter name=${CONTAINER_NAME}) 2>/dev/null || true
        docker rm $(docker ps -a -q --filter name=${CONTAINER_NAME}) 2>/dev/null || true
        docker rmi $(docker images -q ${IMAGE_NAME}) 2>/dev/null || true
        echo -e "${GREEN}? Limpieza completada${NC}"
    fi
}

open_shell() {
    ENV=$(get_env_vars $1)
    TAG=${ENV,,}
    CONTAINER="${CONTAINER_NAME}-${TAG}"
    
    echo -e "${GREEN}?? Abriendo shell en: ${CONTAINER}${NC}"
    docker exec -it ${CONTAINER} /bin/bash
}

# Main
case ${1} in
    build)
        build_image ${2:-dev}
        ;;
    run)
        run_container ${2:-dev}
        ;;
    stop)
        stop_container ${2:-dev}
        ;;
    logs)
        show_logs ${2:-dev}
        ;;
    restart)
        restart_container ${2:-dev}
        ;;
    clean)
        clean_all
        ;;
    shell)
        open_shell ${2:-dev}
        ;;
    *)
        print_usage
        exit 1
        ;;
esac
