# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["WorkoutTracker.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

# Create a data directory for the SQLite database
RUN mkdir -p /app/data

EXPOSE 5065
ENTRYPOINT ["dotnet", "WorkoutTracker.dll"]