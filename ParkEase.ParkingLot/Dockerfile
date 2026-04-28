FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ParkEase.ParkingLot/ParkEase.ParkingLot.csproj", "ParkEase.ParkingLot/"]
RUN dotnet restore "ParkEase.ParkingLot/ParkEase.ParkingLot.csproj"
COPY . .
WORKDIR "/src/ParkEase.ParkingLot"
RUN dotnet build "ParkEase.ParkingLot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ParkEase.ParkingLot.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ParkEase.ParkingLot.dll"]
