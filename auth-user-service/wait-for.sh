#!/bin/sh

HOST="$1"
shift
CMD="$@"

echo "👉 Bağlantı bekleniyor: $HOST"

until nc -z $(echo "$HOST" | cut -d: -f1) $(echo "$HOST" | cut -d: -f2); do
  echo "⏳ Bekliyor..."
  sleep 2
done

echo "✅ Servis hazır: $HOST"

exec $CMD
