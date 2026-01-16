#!/bin/bash

# Development ortamını durdurma scripti
# Kullanım: ./scripts/dev-stop.sh

echo "🛑 Development ortamı durduruluyor..."
echo ""

# Development container'ını durdur
docker compose -f docker-compose.dev.yml down

echo ""
echo "✅ Development ortamı durduruldu!"
