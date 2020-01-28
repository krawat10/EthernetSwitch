EthernetSwitch - 2 OSI layer switch
===================================

## Intro
EthernetSwitch application allows users to manage connected network interfaces and to create virtual LANs. The application uses the second OSI layer (data-link) to manage interfaces.

Features:
- Turning on/off-network interfaces 
- Adding Virtual LANs
- Creating tagged VLANs
- Private VLANs

## Installation
#### (**Vagrant**) Self-deployment VM enviroment
Example self-deployment enviroment (by Vagrant) is available on https://github.com/mp-mp/Vagrant repository. To run the VM and the application, simply go to **/Vagrant/DEBIAN_SWITH/** directory and type in terminal (Vagrant is required):
    
    vagrant up
    
#### (**DEBIAN**) Instal and run using deploy.sh script 
To run and deploy an application using one script open terminal and go to EthernetSwitch's project directory and type:
    
    sudo bash deploy.sh


#### Bundles
Repository contains 2 version of bundled application:
 - EthernetSwitch/publish/linux-x64.zip ***(debian tested)***
 - EthernetSwitch/publish/linux-arm.zip ***(not tested)***

Before executing an application please install the following packages:

    apt-get update  
    apt-get install -y aptitude
    aptitude install -y tcpdump
    aptitude install -y bridge-utils
    aptitude install -y vlan
    aptitude install -y gnupg
    aptitude install -y ebtables
    aptitude install -y unzip

Then run the application:
	cd publish
	unzip linux-{version}.zip
	chmod +x EthernetSwitch    # make file executable
	sudo ./EthernetSwitch


#### Getting started
After executing an application terminal should return the following output:
	
	info: Microsoft.Hosting.Lifetime[0]
	    Now listening on: http://[::]:80
	info: Microsoft.Hosting.Lifetime[0]
	    Application started. Press Ctrl+C to shut down.
	info: Microsoft.Hosting.Lifetime[0]
	    Hosting environment: Production
	info: Microsoft.Hosting.Lifetime[0]
	    Content root path: /home/vagrant/EthernetSwitch/publish

Now the application is hosted on 80 port (http://localhost).


