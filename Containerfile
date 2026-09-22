# ==============================================================================
# Stage 1: Runtime Base (Lightweight environment for executing the app)
# ==============================================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# ==============================================================================
# Stage 2: Build (SDK image, has compilers/tools)
# ==============================================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["CSharpApp.sln", "./"]
COPY ["CSharpApp.Api/CSharpApp.Api.csproj", "CSharpApp.Api/"]
COPY ["CSharpApp.Application/CSharpApp.Application.csproj", "CSharpApp.Application/"]
COPY ["CSharpApp.Core/CSharpApp.Core.csproj", "CSharpApp.Core/"]
COPY ["CSharpApp.Infrastructure/CSharpApp.Infrastructure.csproj", "CSharpApp.Infrastructure/"]
COPY ["CSharpApp.UnitTests/CSharpApp.UnitTests.csproj", "CSharpApp.UnitTests/"]

RUN dotnet restore "CSharpApp.sln"

COPY . .

WORKDIR "/src/CSharpApp.Api"
RUN dotnet build "CSharpApp.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CSharpApp.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .

USER $APP_UID

ENTRYPOINT ["dotnet", "CSharpApp.Api.dll"]