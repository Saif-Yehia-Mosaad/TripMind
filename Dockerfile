FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["TripMind.API/TripMind.API.csproj", "TripMind.API/"]
COPY ["TripMind.Application/TripMind.Application.csproj", "TripMind.Application/"]
COPY ["TripMind.Domain/TripMind.Domain.csproj", "TripMind.Domain/"]
COPY ["TripMind.Infrastructure/TripMind.Infrastructure.csproj", "TripMind.Infrastructure/"]
RUN dotnet restore "TripMind.API/TripMind.API.csproj"
COPY . .
WORKDIR "/src/TripMind.API"
RUN dotnet build "TripMind.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TripMind.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TripMind.API.dll"]
