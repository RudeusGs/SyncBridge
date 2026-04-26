FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/SyncBridge.SourceMockApi/SyncBridge.SourceMockApi.csproj", "src/SyncBridge.SourceMockApi/"]
RUN dotnet restore "src/SyncBridge.SourceMockApi/SyncBridge.SourceMockApi.csproj"

COPY . .
WORKDIR "/src/src/SyncBridge.SourceMockApi"
RUN dotnet publish "SyncBridge.SourceMockApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SyncBridge.SourceMockApi.dll"]