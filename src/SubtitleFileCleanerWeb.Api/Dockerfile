FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/SubtitleFileCleanerWeb.Api/SubtitleFileCleanerWeb.Api.csproj", "src/SubtitleFileCleanerWeb.Api/"]
COPY ["src/SubtitleFileCleanerWeb.Application/SubtitleFileCleanerWeb.Application.csproj", "src/SubtitleFileCleanerWeb.Application/"]
COPY ["src/SubtitleFileCleanerWeb.Domain/SubtitleFileCleanerWeb.Domain.csproj", "src/SubtitleFileCleanerWeb.Domain/"]
COPY ["src/SubtitleFileCleanerWeb.Infrastructure/SubtitleFileCleanerWeb.Infrastructure.csproj", "src/SubtitleFileCleanerWeb.Infrastructure/"]
RUN dotnet restore "src/SubtitleFileCleanerWeb.Api/SubtitleFileCleanerWeb.Api.csproj"
COPY . .
WORKDIR "/src/src/SubtitleFileCleanerWeb.Api"
RUN dotnet build "SubtitleFileCleanerWeb.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
RUN dotnet publish "SubtitleFileCleanerWeb.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SubtitleFileCleanerWeb.Api.dll"]