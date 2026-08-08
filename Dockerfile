FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

COPY . .

RUN dotnet restore "ArtemisBankingApi/ArtemisBankingApi.csproj"

RUN dotnet publish \
    "ArtemisBankingApi/ArtemisBankingApi.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore


FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

WORKDIR /app

EXPOSE 8080

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "ArtemisBankingApi.dll"]