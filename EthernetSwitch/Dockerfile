#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build	

ARG DEBIAN_FRONTEND=noninteractive

RUN apt update \
	&& apt install -y tcpdump \
	&& apt install -y bridge-utils \
	&& apt install -y vlan \
	&& apt install -y tmux \ 
	&& apt install -y gnupg \
	&& apt install -y lldpd \
	&& apt install -y net-tools \
	&& apt install snmpd snmp libsnmp-dev -y 

RUN /etc/init.d/snmpd stop \
	&& net-snmp-create-v3-user -ro -A adminadmin -a SHA -X adminadmin -x AES snmpro \
	&& /etc/init.d/snmpd start 

WORKDIR /src
COPY ["EthernetSwitch/EthernetSwitch.csproj", "EthernetSwitch/"]
COPY ["EthernetSwitch.Data/EthernetSwitch.Data.csproj", "EthernetSwitch.Data/"]
COPY ["EthernetSwitch.Infrastructure/EthernetSwitch.Infrastructure.csproj", "EthernetSwitch.Infrastructure/"]
COPY ["EthernetSwitch.Seciurity/EthernetSwitch.Security.csproj", "EthernetSwitch.Seciurity/"]
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