# Build aşaması (Maven + Java 21)
FROM maven:3.9.6-eclipse-temurin-21-alpine AS build

WORKDIR /app
COPY . .
RUN mvn clean install -DskipTests

# Runtime aşaması (yalnızca JDK)
FROM eclipse-temurin:21-jdk-alpine

WORKDIR /app
COPY --from=build /app/target/buse-booking-service-0.0.1-SNAPSHOT.jar app.jar

EXPOSE 8080
ENTRYPOINT ["java", "-jar", "app.jar"]
