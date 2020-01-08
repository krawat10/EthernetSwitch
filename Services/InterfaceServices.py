from __future__ import annotations

from typing import List

import netifaces
import psutil
from django_injector import inject
from Services.BridgeServices import BridgeServices
from Services.IInterfaceServices import IInterfaceServices
from switch.models import Port


class InterfaceServices(IInterfaceServices):

    @inject
    def __init__(self, network_services: BridgeServices):
        self.network_services = network_services

    def get_all_interfaces(self) -> List[Port]:
        ports: List[Port] = []
        interfaces = psutil.net_if_stats()

        for key in interfaces.keys():
            ports.append(Port(name=key, enabled=interfaces[key].isup))

        return ports

    def get_default_iface_name(self) -> str:
        return netifaces.gateways()['default'][netifaces.AF_INET][1]

    def apply_ports_settings(self, ports: List[Port]):
        for old_bridge in self.network_services.get_bridges():
            self.network_services.delete_bridge(old_bridge)

        for new_bridge in list(set([port.value for port in ports])):
            self.network_services.create_bridge(new_bridge)

        for port in ports:
            if port.enabled:
                self.network_services.up_port(port)
                if port.tagged:
                    self.network_services.set_tagged_port(port)
                else:
                    self.network_services.set_port_not_tagged(port)
            else:
                self.network_services.down_port(port)

    def get_non_default_interfaces(self) -> List[Port]:
        interfaces = self.get_all_interfaces()
        default = self.get_default_iface_name()

        if 'lo' in interfaces:
            interfaces.remove('lo')

        if default in interfaces:
            interfaces.remove(default)

        return interfaces
