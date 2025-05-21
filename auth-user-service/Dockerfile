# 1) Build + Publish Aşaması
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Önce sadece proje dosyasını kopyalayıp restore et
COPY ["auth-user-service.csproj", "./"]
RUN dotnet restore "./auth-user-service.csproj"

# Tüm kodu kopyala ve publish et
COPY . .
RUN dotnet publish "auth-user-service.csproj" -c Release -o /app/publish

# 2) Runtime Aşaması
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 80

# ❗ netcat (nc) kurulumu
RUN apt-get update && apt-get install -y netcat-openbsd

# Yayınlanan dosyaları ve bekleme scriptini kopyala
COPY --from=build /app/publish .
COPY wait-for.sh .
RUN chmod +x wait-for.sh

ENTRYPOINT ["./wait-for.sh", "host.docker.internal:1433", "dotnet", "auth-user-service.dll"]
