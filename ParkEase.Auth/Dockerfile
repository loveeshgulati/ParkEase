FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ParkEase.Auth/ParkEase.Auth.csproj", "ParkEase.Auth/"]
RUN dotnet restore "ParkEase.Auth/ParkEase.Auth.csproj"
COPY ParkEase.Auth/ ParkEase.Auth/
WORKDIR "/src/ParkEase.Auth"
RUN dotnet build "ParkEase.Auth.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ParkEase.Auth.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "ParkEase.Auth.dll"]
