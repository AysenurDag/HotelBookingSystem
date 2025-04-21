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

# Sadece yayınlanan dosyaları al
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "auth-user-service.dll"]
