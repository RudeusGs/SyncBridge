FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/SyncBridge.Worker/SyncBridge.Worker.csproj", "src/SyncBridge.Worker/"]
COPY ["src/SyncBridge.Shared/SyncBridge.Shared.csproj", "src/SyncBridge.Shared/"]
RUN dotnet restore "src/SyncBridge.Worker/SyncBridge.Worker.csproj"

COPY . .
WORKDIR "/src/src/SyncBridge.Worker"
RUN dotnet publish "SyncBridge.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SyncBridge.Worker.dll"]