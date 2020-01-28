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
#### 1. (**Vagrant**) Self-deployment VM enviroment
Running self-deployment enviroment (by Vagrant) is available on https://github.com/mp-mp/Vagrant repository. To run network the application on VM, simply go to **/Vagrant/DEBIAN_SWITH/** directory and type in terminal (Vagrant is required):
    
    vagrant up
    
#### 2. (**DEBIAN**) Instal and run using deploy.sh script 
To run and deploy the application using one script open terminal and go to EthernetSwitch Project directory and type:
    
    sudo bash deploy.sh


#### Bundles
Repository contains 2 version of bundled application:
 - EthernetSwitch/publish/linux-x64.zip ***(Debian tested)***
 - EthernetSwitch/publish/linux-arm.zip ***(not tested)***

Before executing the application please install the following packages:

    apt-get update  
    apt-get install -y aptitude
    aptitude install -y bridge-utils
    aptitude install -y gnupg
    aptitude install -y ebtables
    aptitude install -y unzip

Then run the application:

    cd publish
    unzip linux-{version}.zip
    chmod +x EthernetSwitch  # make file executable
    sudo ./EthernetSwitch


## Getting started
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

Go to http://localhost using your browser (if you are using a machine where the application is hosted). You should see a login panel. **The default password is admin/admin.** Please change it after login. 

![login panel](https://github.com/krawat10/EthernetSwitch/blob/master/img/LoginPanel.jpg?raw=true)

After login you should see the following dashboard (rows can be different based on connected interfaces):

![login panel](https://github.com/krawat10/EthernetSwitch/blob/master/img/Home.jpg?raw=true)

##### **Now you can modify your interfaces!**
***School project for Gdansk University of Technology (Group project) - 2020***
***Przelacznik Ethernet wykorzystujacy system Linux – PEWSL***
