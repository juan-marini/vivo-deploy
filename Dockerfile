# Dockerfile apenas para BACKEND
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project file
COPY backend/backend.csproj backend/
RUN dotnet restore backend/backend.csproj

# Copy source code
COPY backend/ backend/
WORKDIR /src/backend
RUN dotnet build backend.csproj -c Release -o /app/build

FROM build AS publish
RUN dotnet publish backend.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "backend.dll"]