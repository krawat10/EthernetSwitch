#!/bin/bash
sudo apt-get update

wget https://packages.microsoft.com/config/debian/10/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb

sudo apt-get update; \
  sudo apt-get install -y apt-transport-https && \
  sudo apt-get update && \
  sudo apt-get install -y dotnet-sdk-3.1

sudo apt-get update; \
  sudo apt-get install -y apt-transport-https && \
  sudo apt-get update && \
  sudo apt-get install -y aspnetcore-runtime-3.1

sudo apt-get install -y dotnet-runtime-3.1


sudo apt-get install -y aptitude
sudo aptitude install -y tcpdump
sudo aptitude install -y bridge-utils
sudo aptitude install -y vlan
sudo aptitude install -y tmux
sudo aptitude install -y git
sudo aptitude install -y gnupg
sudo aptitude install snmpd snmp libsnmp-dev -y

#SNMP
sudo systemctl start snmpd

export DEBIAN_FRONTEND=noninteractive

sudo git clone --depth 1 https://github.com/krawat10/EthernetSwitch.git
sudo dotnet publish -c Release EthernetSwitch/EthernetSwitch

dir=$`pwd`/
sudo sed -i 's/PATH/${dir//\//\\/}/g' EthernetSwitch/EthernetSwitch/ethernet-switch.service
sudo cp EthernetSwitch/EthernetSwitch/ethernet-switch.service /etc/systemd/system/ethernet-switch.service
sudo systemctl daemon-reload
sudo systemctl enable ethernet-switch.service
sudo systemctl start ethernet-switch.service