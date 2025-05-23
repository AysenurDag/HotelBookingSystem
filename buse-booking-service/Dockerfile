# Java 21 kullanıyorsak Temurin image
FROM eclipse-temurin:21-jdk-alpine

# JAR dosyasını container'a kopyala
COPY target/buse-booking-service-0.0.1-SNAPSHOT.jar app.jar

# Port aç
EXPOSE 8080

# Uygulama başlat
ENTRYPOINT ["java", "-jar", "/app.jar"]
