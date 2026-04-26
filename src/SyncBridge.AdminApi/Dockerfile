FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/SyncBridge.AdminApi/SyncBridge.AdminApi.csproj", "src/SyncBridge.AdminApi/"]
COPY ["src/SyncBridge.Shared/SyncBridge.Shared.csproj", "src/SyncBridge.Shared/"]
RUN dotnet restore "./src/SyncBridge.AdminApi/SyncBridge.AdminApi.csproj"
COPY . .
WORKDIR "/src/src/SyncBridge.AdminApi"
RUN dotnet build "./SyncBridge.AdminApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SyncBridge.AdminApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SyncBridge.AdminApi.dll"]
