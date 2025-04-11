# Use the official Node.js image to build the front-end assets
FROM node:18 AS node-build
WORKDIR /src

# Clone the repository and navigate to the project directory
ADD https://api.github.com/repos/bluechilli/chillicoretemplate/git/ref/heads/net8 version.json
RUN git clone --branch net8 https://github.com/BlueChilli/ChilliCoreTemplate.git code

# Install dependencies and build the front-end assets
WORKDIR /src/code/ChilliCoreTemplate.Web
RUN npm install
RUN npm install -g gulp
RUN gulp

# Use the official .NET 8 SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src/code

# Copy the cloned repository from the node-build stage
COPY --from=node-build /src/code /src

# Copy the secrets file into the project directory
COPY ChilliCoreTemplate.Web/appsettings.Debug.json /src/ChilliCoreTemplate.Web/appsettings.Debug.json

WORKDIR /src/ChilliCoreTemplate.Web
RUN dotnet restore
RUN dotnet publish -c Debug -o /app/publish

# Use the official .NET 8 ASP.NET Core runtime image to run the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
COPY --from=node-build /src/code/ChilliCoreTemplate.Web/wwwroot ./wwwroot
COPY --from=node-build /src/code/ChilliCoreTemplate.Web/node_modules ./node_modules

ENV ASPNETCORE_ENVIRONMENT Debug

EXPOSE 8080
EXPOSE 8081
ENTRYPOINT ["dotnet", "ChilliCoreTemplate.Web.dll"]

