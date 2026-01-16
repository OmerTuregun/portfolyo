#!/bin/bash

# Production ortamını durdurma scripti
# Kullanım: ./scripts/prod-stop.sh

echo "🛑 Production ortamı durduruluyor..."
echo "⚠️  DİKKAT: Bu canlı siteyi durdurur!"
echo ""

read -p "Devam etmek istediğinize emin misiniz? (y/N): " -n 1 -r
echo ""

if [[ ! $REPLY =~ ^[Yy]$ ]]
then
    echo "❌ İşlem iptal edildi."
    exit 1
fi

# Production container'ını durdur
docker compose down

echo ""
echo "✅ Production ortamı durduruldu!"
