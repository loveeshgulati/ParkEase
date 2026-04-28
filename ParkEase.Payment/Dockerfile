FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ParkEase.Payment/ParkEase.Payment.csproj", "ParkEase.Payment/"]
RUN dotnet restore "ParkEase.Payment/ParkEase.Payment.csproj"
COPY . .
WORKDIR "/src/ParkEase.Payment"
RUN dotnet build "ParkEase.Payment.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ParkEase.Payment.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ParkEase.Payment.dll"]
