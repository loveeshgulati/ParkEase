FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ParkEase.Notification/ParkEase.Notification.csproj", "ParkEase.Notification/"]
RUN dotnet restore "ParkEase.Notification/ParkEase.Notification.csproj"
COPY . .
WORKDIR "/src/ParkEase.Notification"
RUN dotnet build "ParkEase.Notification.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ParkEase.Notification.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ParkEase.Notification.dll"]
