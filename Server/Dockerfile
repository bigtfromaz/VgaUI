#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Server/VgaUI.Server.csproj", "Server/"]
COPY ["Client/VgaUI.Client.csproj", "Client/"]
COPY ["ExcelInterface/ExcelInterface.csproj", "ExcelInterface/"]
COPY ["Shared/VgaUI.Shared.csproj", "Shared/"]
COPY ["VGARazorLib/VGARazorLib.csproj", "VGARazorLib/"]
RUN dotnet restore "./Server/./VgaUI.Server.csproj"
COPY . .
WORKDIR "/src/Server"
RUN dotnet build "./VgaUI.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./VgaUI.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VgaUI.Server.dll"]