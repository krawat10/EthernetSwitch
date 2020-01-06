#!/bin/bash
set -x #echo on
sudo apt-get update && sudo apt-get -y upgrade
sudo apt-get install python3
sudo apt-get install -y python3-pip
pip3 install django
pip3 install virtualenv
pip3 install -r requirements.txt
virtualenv env
. env/bin/activate
python3 manage.py runserver 8000