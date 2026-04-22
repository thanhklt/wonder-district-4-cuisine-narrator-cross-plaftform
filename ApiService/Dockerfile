FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY AudioTravelling.slnx .
COPY AudioTravelling.Core/AudioTravelling.Core.csproj AudioTravelling.Core/
COPY AudioTravelling.Infrastructure/AudioTravelling.Infrastructure.csproj AudioTravelling.Infrastructure/
COPY AudioTravelling.API/AudioTravelling.API.csproj AudioTravelling.API/
RUN dotnet restore
COPY . .
RUN dotnet publish AudioTravelling.API/AudioTravelling.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .
EXPOSE 5000
ENTRYPOINT ["dotnet", "AudioTravelling.API.dll"]
