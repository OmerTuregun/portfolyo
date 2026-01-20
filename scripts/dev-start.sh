#!/bin/bash

# Development ortamını başlatma scripti
# Kullanım: ./scripts/dev-start.sh

echo "🚀 Development ortamı başlatılıyor..."
echo ""

# Development container'ını başlat
docker compose -f docker-compose.dev.yml up -d --build

echo ""
echo "✅ Development ortamı hazır!"
echo "📝 Site: http://localhost:3000"
echo "📊 Logları izlemek için: docker compose -f docker-compose.dev.yml logs -f"
echo ""
