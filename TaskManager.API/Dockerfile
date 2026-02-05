# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0.2-bookworm-slim AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# Health check for container orchestration
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
  CMD dotnet --version || exit 1


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0.2-bookworm AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TaskManager.API/TaskManager.API.csproj", "TaskManager.API/"]
RUN dotnet restore "./TaskManager.API/TaskManager.API.csproj"
COPY . .
WORKDIR "/src/TaskManager.API"
RUN dotnet build "./TaskManager.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TaskManager.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
LABEL maintainer="your-email@example.com" \
      description="TaskManager API - ASP.NET Core 10 REST API with Auth0 authentication" \
      version="1.0.0"

WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskManager.API.dll"]