FROM mcr.microsoft.com/dotnet/sdk:8.0 AS c
WORKDIR /app

COPY ["TgStickerMakerBot/TgStickerMakerBot.csproj", "TgStickerMakerBot/"]
COPY ["TgStickerMaker/TgStickerMaker.csproj", "TgStickerMaker/"]

RUN dotnet restore ./TgStickerMakerBot/TgStickerMakerBot.csproj

COPY . .
WORKDIR "/app/TgStickerMakerBot"
RUN dotnet build "TgStickerMakerBot.csproj" -c Release -o /app/build

from c as publish
RUN dotnet publish "TgStickerMakerBot.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS base
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TgStickerMakerBot.dll"]