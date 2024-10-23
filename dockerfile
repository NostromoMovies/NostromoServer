# Base SDK for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# Copy everything needed for the build
COPY . .

# Restore dependencies
RUN dotnet restore "Nostromo.CLI/Nostromo.CLI.csproj"

# Build and publish
RUN dotnet publish "Nostromo.CLI/Nostromo.CLI.csproj" -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app

# Copy the published app
COPY --from=build /app/publish .

# Copy dependencies
COPY Dependencies/ ./Dependencies/

# Set the entrypoint
ENTRYPOINT ["dotnet", "Nostromo.CLI.dll"]