#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["EthernetSwitch/EthernetSwitch.csproj", "EthernetSwitch/"]
RUN dotnet restore "EthernetSwitch/EthernetSwitch.csproj"
COPY . .
WORKDIR "/src/EthernetSwitch"
RUN dotnet build "EthernetSwitch.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EthernetSwitch.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EthernetSwitch.dll"]