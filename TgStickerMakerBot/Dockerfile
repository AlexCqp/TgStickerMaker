FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Установка необходимых шрифтов, Fontconfig и ffmpeg
RUN apt-get update && apt-get install -y \
    ffmpeg \
    libfreetype6 \
    libfreetype6-dev \
    fontconfig \
    fonts-dejavu \
    && rm -rf /var/lib/apt/lists/*

COPY ["TgStickerMakerBot/TgStickerMakerBot.csproj", "TgStickerMakerBot/"]
COPY ["TgStickerMaker/TgStickerMaker.csproj", "TgStickerMaker/"]

RUN dotnet restore ./TgStickerMakerBot/TgStickerMakerBot.csproj

COPY . .
WORKDIR "/app/TgStickerMakerBot"
RUN dotnet build "TgStickerMakerBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TgStickerMakerBot.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

RUN apt-get update && apt-get install -y \
    ffmpeg \
    && rm -rf /var/lib/apt/lists/*

RUN which ffmpeg

# Копируем опубликованные файлы
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "TgStickerMakerBot.dll"]
