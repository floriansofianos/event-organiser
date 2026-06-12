FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
RUN apt-get update && apt-get install -y --no-install-recommends libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/EventOrganizer.Domain/EventOrganizer.Domain.csproj", "src/EventOrganizer.Domain/"]
COPY ["src/EventOrganizer.Application/EventOrganizer.Application.csproj", "src/EventOrganizer.Application/"]
COPY ["src/EventOrganizer.Infrastructure/EventOrganizer.Infrastructure.csproj", "src/EventOrganizer.Infrastructure/"]
COPY ["src/EventOrganizer.API/EventOrganizer.API.csproj", "src/EventOrganizer.API/"]
RUN dotnet restore "src/EventOrganizer.API/EventOrganizer.API.csproj"
COPY . .
WORKDIR "/src/src/EventOrganizer.API"
RUN dotnet build "EventOrganizer.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "EventOrganizer.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EventOrganizer.API.dll"]
