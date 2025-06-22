# Use the official .NET SDK image for build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and restore as distinct layers
COPY website/website.sln ./website.sln
COPY website/*.csproj ./website/
RUN dotnet restore ./website/website.sln

# Copy everything else and build
COPY website/. ./website/
WORKDIR /src/website
RUN dotnet publish -c Release -o /app --no-restore

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app .

# Expose port 8080 for the app
EXPOSE 8080

# Set environment variables if needed
# ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "WebApp.dll"]
