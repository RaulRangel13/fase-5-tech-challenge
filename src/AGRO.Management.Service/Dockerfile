FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/AGRO.Management.Service/AGRO.Management.Service.csproj", "src/AGRO.Management.Service/"]
RUN dotnet restore "./src/AGRO.Management.Service/AGRO.Management.Service.csproj"
COPY . .
WORKDIR "/src/src/AGRO.Management.Service"
RUN dotnet build "./AGRO.Management.Service.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AGRO.Management.Service.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AGRO.Management.Service.dll"]
