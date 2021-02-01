EthernetSwitch - 2 OSI layer switch
===================================

## Intro
EthernetSwitch application allows users to manage connected network interfaces and to create virtual LANs. The application uses the second OSI layer (data-link) to manage interfaces.

Features:
- Turning on/off-network interfaces 
- Adding Virtual LANs
- Creating tagged VLANs
- Private VLANs with Promiscous/Isolation mode
- SET/GET/TRAP v3 and WALK v1 SNMP Protocol
- LLDP Protocol
- GVRP protocol (automatic configuration of VLANs)

## Installation    
#### 1. (**DEBIAN 10**) Instal and run using deploy.sh script 
To run and deploy the application using one script type the following commands:
    
    git clone --depth 1 https://github.com/krawat10/EthernetSwitch.git
    cd EthernetSwitch
    sudo ./deploy.sh

Successful instalation should print the following lines:

    ● ethernet-switch.service - Ethernet Switch Application
       Loaded: loaded (/etc/systemd/system/ethernet-switch.service; enabled; vendor preset: enabled)
       Active: active (running) since Mon 2021-02-01 17:59:47 GMT; 11ms ago
     Main PID: 16371 (dotnet)
        Tasks: 1 (limit: 2359)
       Memory: 756.0K
       CGroup: /system.slice/ethernet-switch.service
               └─16371 /bin/dotnet EthernetSwitch.dll

Application now works as service.

#### 2. Bundles
Repository contains 1 version of bundled application (executable):
 - EthernetSwitch/publish/EthernetSwitch ***(Debian tested)***

Before executing the application please install the following packages:

    sudo apt-get update &&
    sudo apt-get install -y aptitude &&
    sudo aptitude install -y git &&
    sudo aptitude install -y tcpdump &&
    sudo aptitude install -y bridge-utils &&
    sudo aptitude install -y vlan &&
    sudo aptitude install -y tmux &&
    sudo aptitude install -y gnupg &&
    sudo aptitude install -y ebtables &&
    sudo aptitude install -y unzip &&
    sudo aptitude install -y snmpd &&
    sudo aptitude install -y snmp &&
    sudo aptitude install -y libsnmp-dev 

To run the executable, type the following commands:

    git clone --depth 1 https://github.com/krawat10/EthernetSwitch.git
    cd EthernetSwitch/publish
    sudo chmod 777 EthernetSwitch
    sudo ./EthernetSwitch

Successful run should print the following lines:

    info: EthernetSwitch.BackgroundWorkers.QueuedHostedService[0]
          Queued Hosted Service is running
    info: EthernetSwitch.Infrastructure.SNMP.TrapReceiverHostedService[0]
          Queued Hosted Service is running
    info: EthernetSwitch.Infrastructure.GVRP.GVRPHostedService[0]
          Queued Hosted Service is running
    info: Microsoft.Hosting.Lifetime[0]
          Now listening on: http://[::]:5000
    info: Microsoft.Hosting.Lifetime[0]
          Application started. Press Ctrl+C to shut down.
    info: Microsoft.Hosting.Lifetime[0]
          Hosting environment: Production
    info: Microsoft.Hosting.Lifetime[0]

#### 3. (**Vagrant**) Self-deployment VM enviroment
Running self-deployment enviroment (by Vagrant) is available on https://github.com/mp-mp/Vagrant repository. To run the application on VM, simply go to **/Vagrant/DEBIAN_SWITH/** directory and type in terminal (Vagrant is required):
    
    vagrant up

## Getting started
The application is hosted on 5000 port (http://localhost:5000). 

Go to http://localhost:5000 using your browser (if you are using a machine where the application is hosted). You should see a login panel. **The default password is admin/admin.** Please change it after login. 

![login panel](https://github.com/krawat10/EthernetSwitch/blob/master/LoginPanel.png?raw=true)

After login you should see the following dashboard (rows can be different based on connected interfaces):

![login panel](https://github.com/krawat10/EthernetSwitch/blob/master/Home.png?raw=true)

##### **Now you can modify your interfaces!**
***Project for Gdansk University of Technology (Engineering thesis) - 2021***
