from __future__ import annotations

from typing import List

import netifaces
import psutil
from django_injector import inject
from Services.NetworkService import NetworkService
from Services.IInterfaceService import IInterfaceServices
from switch.models import Interface


class InterfaceService(IInterfaceServices):

    @inject
    def __init__(self, network_services: NetworkService):
        self.network_services = network_services

    def get_all_interfaces(self) -> List[Interface]:
        ports: List[Interface] = []
        interfaces = psutil.net_if_stats()

        for key in interfaces.keys():
            ports.append(Interface(name=key, enabled=interfaces[key].isup))

        return ports

    def get_default_interface_name(self) -> str:
        return netifaces.gateways()['default'][netifaces.AF_INET][1]

    def apply_ports_settings(self, interfaces: List[Interface]):
        for old_bridge in self.network_services.get_bridges():
            self.network_services.delete_bridge(old_bridge)

        for new_bridge in list(set([port.value for port in interfaces])):
            if new_bridge > 0:
                self.network_services.create_bridge(str(new_bridge))

        for interface in interfaces:
            if interface.enabled:
                self.network_services.up_port(interface)
                if interface.tagged:
                    self.network_services.set_tagged_port(interface)
                else:
                    self.network_services.set_port_not_tagged(interface)
            else:
                self.network_services.down_port(interface)

    def get_non_default_interfaces(self) -> List[Interface]:
        interfaces = self.get_all_interfaces()
        default = self.get_default_interface_name()

        other_interfaces: List[Interface] = []

        for interface in interfaces:
            if interface.name == 'lo':
                continue

            if interface.name == default:
                continue

            other_interfaces.append(interface)

        return other_interfaces
