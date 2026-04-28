FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ParkEase.Vehicle/ParkEase.Vehicle.csproj", "ParkEase.Vehicle/"]
RUN dotnet restore "ParkEase.Vehicle/ParkEase.Vehicle.csproj"
COPY . .
WORKDIR "/src/ParkEase.Vehicle"
RUN dotnet build "ParkEase.Vehicle.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ParkEase.Vehicle.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ParkEase.Vehicle.dll"]
