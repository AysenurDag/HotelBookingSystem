#!/bin/bash

/opt/mssql/bin/sqlservr &

# SQL Server'ın hazır hale gelmesini bekle
echo "SQL Server başlatılıyor, hazır olması bekleniyor..."
for i in {1..30}; do
  /opt/mssql-tools/bin/sqlcmd -S 127.0.0.1 -U SA -P 'saUser.123' -Q "SELECT 1" &> /dev/null && break
  echo "Hazır değil, tekrar denenecek..."
  sleep 2
done

# init.sql çalıştır
echo "✅ MSSQL bağlantısı kuruldu. init.sql çalıştırılıyor..."
/opt/mssql-tools/bin/sqlcmd -S 127.0.0.1 -U SA -P 'saUser.123' -i /init.sql

# SQL Server foreground'da çalışmaya devam etsin
wait
