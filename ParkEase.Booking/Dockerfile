FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ParkEase.Booking/ParkEase.Booking.csproj", "ParkEase.Booking/"]
RUN dotnet restore "ParkEase.Booking/ParkEase.Booking.csproj"
COPY . .
WORKDIR "/src/ParkEase.Booking"
RUN dotnet build "ParkEase.Booking.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ParkEase.Booking.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ParkEase.Booking.dll"]
