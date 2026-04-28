FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ParkEase.Spot/ParkEase.Spot.csproj", "ParkEase.Spot/"]
RUN dotnet restore "ParkEase.Spot/ParkEase.Spot.csproj"
COPY . .
WORKDIR "/src/ParkEase.Spot"
RUN dotnet build "ParkEase.Spot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ParkEase.Spot.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ParkEase.Spot.dll"]
