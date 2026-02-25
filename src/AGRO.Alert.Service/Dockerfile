FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/AGRO.Alert.Service/AGRO.Alert.Service.csproj", "src/AGRO.Alert.Service/"]
RUN dotnet restore "./src/AGRO.Alert.Service/AGRO.Alert.Service.csproj"
COPY . .
WORKDIR "/src/src/AGRO.Alert.Service"
RUN dotnet build "./AGRO.Alert.Service.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AGRO.Alert.Service.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AGRO.Alert.Service.dll"]
