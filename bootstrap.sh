sudo apt-get update && sudo apt-get -y upgrade
sudo apt-get install python3
sudo apt-get install -y python3-pip
pip3 install -r requirements.txt
pip3 install virtualenv
virtualenv env
. env/bin/activate
python3 manage.py runserver 8000