# ── Stage 1: Build ───────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files first for better layer caching
COPY Enterprise.TaskFlow.sln .
COPY src/Enterprise.TaskFlow.Domain/*.csproj             src/Enterprise.TaskFlow.Domain/
COPY src/Enterprise.TaskFlow.Application/*.csproj        src/Enterprise.TaskFlow.Application/
COPY src/Enterprise.TaskFlow.Infrastructure/*.csproj     src/Enterprise.TaskFlow.Infrastructure/
COPY src/Enterprise.TaskFlow.API/*.csproj                src/Enterprise.TaskFlow.API/

RUN dotnet restore

# Copy all source and publish
COPY . .
RUN dotnet publish src/Enterprise.TaskFlow.API/Enterprise.TaskFlow.API.csproj \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

# ── Stage 2: Runtime ────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Enterprise.TaskFlow.API.dll"]
