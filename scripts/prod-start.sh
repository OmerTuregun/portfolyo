#!/bin/bash

# Production ortamını başlatma scripti
# Kullanım: ./scripts/prod-start.sh

echo "🏭 Production ortamı başlatılıyor..."
echo "⚠️  DİKKAT: Bu canlı siteyi başlatır!"
echo ""

read -p "Devam etmek istediğinize emin misiniz? (y/N): " -n 1 -r
echo ""

if [[ ! $REPLY =~ ^[Yy]$ ]]
then
    echo "❌ İşlem iptal edildi."
    exit 1
fi

# Production container'ını başlat
docker compose up -d --build

echo ""
echo "✅ Production ortamı hazır!"
echo "📝 Site: http://localhost:5001"
echo "📊 Logları izlemek için: docker compose logs -f"
echo ""
