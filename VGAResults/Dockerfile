#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
ENV LC_ALL=en_US.UTF-8 \
    LANG=en_US.UTF-8
WORKDIR /src
COPY ["VGAResults/VGAResults.csproj", "VGAResults/"]
COPY ["Shared/VgaUI.Shared.csproj", "Shared/"]
COPY ["VGARazorLib/VGARazorLib.csproj", "VGARazorLib/"]
RUN dotnet restore "./VGAResults/./VGAResults.csproj"
COPY . .
WORKDIR "/src/VGAResults"
RUN dotnet build "./VGAResults.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ENV LC_ALL=en_US.UTF-8 \
    LANG=en_US.UTF-8
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./VGAResults.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
ENV LC_ALL=en_US.UTF-8 \
    LANG=en_US.UTF-8
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VGAResults.dll"]