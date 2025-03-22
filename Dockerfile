# Use the official Node.js image to build the front-end assets
FROM node:18 AS node-build
WORKDIR /src

# Clone the repository and navigate to the project directory
RUN git clone https://github.com/BlueChilli/ChilliCoreTemplate.git .
WORKDIR /src/ChilliCoreTemplate.Web

# Install dependencies and build the front-end assets
RUN npm install
RUN npm install -g gulp
RUN gulp

# Use the official .NET 8 SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the cloned repository from the node-build stage
COPY --from=node-build /src /src

WORKDIR /src/ChilliCoreTemplate.Web
RUN dotnet restore
RUN dotnet publish -c Debug -o /app/publish

# Use the official .NET 8 ASP.NET Core runtime image to run the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
COPY --from=node-build /src/ChilliCoreTemplate.Web/wwwroot ./wwwroot

ENV ASPNETCORE_ENVIRONMENT Debug

EXPOSE 80
ENTRYPOINT ["dotnet", "ChilliCoreTemplate.Web.dll"]

