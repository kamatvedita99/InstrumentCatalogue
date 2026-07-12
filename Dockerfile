# ==========================
# Build Stage
# ==========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY InstrumentCatalogue.sln .
COPY InstrumentCatalogue.API/*.csproj InstrumentCatalogue.API/
COPY InstrumentCatalogue.Application/*.csproj InstrumentCatalogue.Application/
COPY InstrumentCatalogue.Core/*.csproj InstrumentCatalogue.Core/
COPY InstrumentCatalogue.Infrastructure/*.csproj InstrumentCatalogue.Infrastructure/

RUN dotnet restore InstrumentCatalogue.API/InstrumentCatalogue.API.csproj

COPY . .

RUN dotnet publish InstrumentCatalogue.API/InstrumentCatalogue.API.csproj \
    -c Release \
    --no-restore \
    -o /app/publish

# ==========================
# Runtime Stage
# ==========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

USER app
ENTRYPOINT ["dotnet", "InstrumentCatalogue.API.dll"]
