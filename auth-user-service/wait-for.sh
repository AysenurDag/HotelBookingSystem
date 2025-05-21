#!/bin/bash

echo "⏳ MSSQL port kontrolü başlatılıyor..."
until nc -z -v -w30 mssql 1433
do
  echo "❌ MSSQL henüz hazır değil. Tekrar deneniyor..."
  sleep 2
done

echo "✅ MSSQL hazır. Servis başlatılıyor..."
exec "$@"
