#!/bin/bash
set -x #echo on
sudo apt-get install --yes --force-yes --keep-current-confs python3
sudo apt-get install --yes --force-yes --keep-current-confs python3-pip
sudo pip3 install django
sudo pip3 install virtualenv
sudo pip3 install -r requirements.txt
sudo virtualenv env
. env/bin/activate
sudo python3 manage.py runserver 0:997