# Use the official .NET SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
RUN apt-get update && apt-get install
WORKDIR /src

# Copy the solution file and restore dependencies
COPY ["RegulusProject.sln", "."]
COPY ["src/Web/Web.csproj", "src/Web/"]
COPY ["src/Application/Application.csproj", "src/Application/"]
COPY ["src/Domain/Domain.csproj", "src/Domain/"]
COPY ["src/Infrastructure/Infrastructure.csproj", "src/Infrastructure/"]

COPY ["tests/Application.FunctionalTests/Application.FunctionalTests.csproj", "tests/Application.FunctionalTests/"]
COPY ["tests/Application.UnitTests/Application.UnitTests.csproj", "tests/Application.UnitTests/"]
COPY ["tests/Domain.UnitTests/Domain.UnitTests.csproj", "tests/Domain.UnitTests/"]
COPY ["tests/Infrastructure.IntegrationTests/Infrastructure.IntegrationTests.csproj", "tests/Infrastructure.IntegrationTests/"]

COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["global.json", "."]
COPY [".editorconfig", "."]

RUN dotnet restore

# Copy the rest of the application source code
COPY . .

# Build the application
RUN dotnet build "src/Web/Web.csproj" -c Release -o /app/build

# Publish the application
RUN dotnet publish "src/Web/Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the official .NET runtime image to run the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
RUN apt-get update && apt-get install -y libgdiplus

# COPY ["src/Infrastructure/fonts/", "//usr/share/fonts/truetype/"]
# WORKDIR /fonts/
# RUN fc-cache -f

WORKDIR /app
COPY --from=build /app/publish .

# Expose the port the application runs on
EXPOSE 80

# Define the entry point for the container
ENTRYPOINT ["dotnet", "RegulusProject.Web.dll"]
