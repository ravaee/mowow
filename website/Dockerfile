# Use the official .NET SDK image for build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and restore as distinct layers
COPY website.sln ./website.sln
COPY *.csproj ./
RUN dotnet restore ./website.sln

# Copy everything else and build
COPY . .
WORKDIR /src
RUN dotnet publish WebApp.csproj -c Release -o /app --no-restore

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app .

# Expose port 8080 for the app
EXPOSE 8080

# Set environment variables if needed
# ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "WebApp.dll"]
