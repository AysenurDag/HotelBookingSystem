# 1. Base Aşaması: ASP.NET Core Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# 2. Build Aşaması: .NET SDK kullanarak derleme işlemleri
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
# Proje dosyanızı kopyalayın (csproj dosyasının adı projenize göre ayarlanmalı)
COPY ["auth-user-service.csproj", "./"]
RUN dotnet restore "./auth-user-service.csproj"
# Tüm kaynak kodu kopyalayın
COPY . .
WORKDIR "/src/."
RUN dotnet build "auth-user-service.csproj" -c Release -o /app/build

# 3. Publish Aşaması: Uygulamayı yayınlayın
FROM build AS publish
RUN dotnet publish "auth-user-service.csproj" -c Release -o /app/publish

# 4. Final Aşaması: Yayınlanan dosyaları runtime imajına kopyalayın
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "auth-user-service.dll"]
