import json
import os
import re
from typing import List

from django.db import models

# Create your models here.
from django.forms import BooleanField


class Port:
    def __init__(self, name, value=0, enabled=True, tagged=False):
        self.tagged = tagged
        self.enabled = enabled
        self.name = name
        self.value = value

    @property
    def enabledBooleanField(self):
        return BooleanField(initial=self.enabled)

    @property
    def className(self):
        return 'table-success' if self.enabled else 'table-danger'


def get_ports_from_config(config: str) -> List[Port]:
    matches = re.findall(r'(auto eth([0-9]+)\n)?iface (eth([0-9]+)) .*(\n\W+[a-z 0-9.]+)*', config)

    if matches:
        for match in matches:
            name = re.search(r'iface (\w+)', match).group(1)
            yield Port(name)

    return []


def save_ports(ports: List[Port]):
    dumps = json.dumps(ports)
    stream = open(InterfacesConfig.ports_array_path, 'w+')
    stream.write(dumps)
    stream.close()


def load_ports() -> List[Port]:
    with open(InterfacesConfig.ports_array_path) as input_file:
        json_array = json.load(input_file)
        ports: List[Port] = []

        for item in json_array:
            ports.append(Port(item['name'], item['value'], item['enabled'], item['tagged']))

        return ports


class InterfacesConfig:
    site_root = os.path.dirname(os.path.realpath(__name__))
    interface_config_path = os.path.join(site_root, 'interfaces.txt')  # to /etc/network/interaces
    initial_config_path = os.path.join(site_root, 'interfaces_default.txt')
    ports_array_path = os.path.join(site_root, 'ports.json')


class Interfaces:
    separator: str = ' '
    text: str = ''
    modify_stamp = '#custom-ethernet-config - DO NOT REMOVE!'

    def __init__(self, bridge_name, ports):
        self.bridge_name = bridge_name
        self.ports = ports

    @property
    def enabled_ports(self):
        return list(filter(lambda x: x.enabled, self.ports))

    @staticmethod
    def init(bridge_name, ports):
        return Interfaces(bridge_name, ports)

    def add_modify_stamp(self):
        self.text += double_new_line(self.modify_stamp)
        return self

    def up_interfaces(self):
        self.text += double_new_line(f'auto lo eth0 {self.bridge_name}')
        return self

    def add_default_interface(self):
        initial_config = ''

        with open(InterfacesConfig.initial_config_path) as f:
            for line in f:
                initial_config += line

        eth1_config = re.search(r'(auto eth1\n)?iface (eth1) .*(\n\W+[a-z 0-9.]+)*', initial_config)
        if eth1_config:
            self.text += double_new_line(eth1_config.group())

        return self

    def add_loopback(self):
        self.text += double_new_line('iface lo inet loopback')
        return self

    def allow_vagrant(self):
        self.text += double_new_line(new_line('allow-hotplug eth0') +
                                     new_line('iface eth0 inet dhcp'))
        return self

    def add_no_tag_vlan(self):
        port_names = (port.name for port in self.enabled_ports)

        self.text += double_new_line(
            f'iface {self.bridge_name} inet static\n'
            f'  address 192.168.0.1\n'
            f'  broadcast 192.168.0.255\n'
            f'  netmask 255.255.255.0\n'
            f'  bridge_ports {self.separator.join(port_names)}\n'
            f'  bridge_stp on\n')
        return self

    def create_config(self):
        return self.text


def double_new_line(text):
    return text + '\n\n'


def new_line(text):
    return text + '\n'
