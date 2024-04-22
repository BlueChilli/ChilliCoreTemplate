#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
ENV DOTNET_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Debug
WORKDIR /src
COPY ["ChilliCoreTemplate.Web/ChilliCoreTemplate.Web.csproj", "ChilliCoreTemplate.Web/"]
COPY ["ChilliCoreTemplate.Models/ChilliCoreTemplate.Models.csproj", "ChilliCoreTemplate.Models/"]
COPY ["ChilliCoreTemplate.Service/ChilliCoreTemplate.Service.csproj", "ChilliCoreTemplate.Service/"]
COPY ["ChilliCoreTemplate.Data/ChilliCoreTemplate.Data.csproj", "ChilliCoreTemplate.Data/"]
RUN dotnet restore "./ChilliCoreTemplate.Web/ChilliCoreTemplate.Web.csproj"
COPY . .
WORKDIR "/src/ChilliCoreTemplate.Web"
RUN dotnet build "./ChilliCoreTemplate.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Debug
RUN dotnet publish "./ChilliCoreTemplate.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ChilliCoreTemplate.Web.dll"]