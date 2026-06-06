FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/EventOrganizer.Domain/EventOrganizer.Domain.csproj", "src/EventOrganizer.Domain/"]
COPY ["src/EventOrganizer.Application/EventOrganizer.Application.csproj", "src/EventOrganizer.Application/"]
COPY ["src/EventOrganizer.Infrastructure/EventOrganizer.Infrastructure.csproj", "src/EventOrganizer.Infrastructure/"]
COPY ["src/EventOrganizer.API/EventOrganizer.API.csproj", "src/EventOrganizer.API/"]
RUN dotnet restore "src/EventOrganizer.API/EventOrganizer.API.csproj"

COPY . .
RUN dotnet publish "src/EventOrganizer.API/EventOrganizer.API.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "EventOrganizer.API.dll"]
