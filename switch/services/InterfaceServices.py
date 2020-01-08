from __future__ import annotations

from abc import abstractmethod, ABC
from typing import List

import netifaces
import psutil

from EthernetSwitch.ServiceFactory import si
from switch.models import Port
from switch.services.BashPipeCommandBuilder import IBashPipeCommandBuilder


class IInterfaceServices(ABC):

    @abstractmethod
    def get_non_default_interfaces(self) -> List[str]: raise NotImplementedError

    @abstractmethod
    def get_all_interfaces(self) -> List[str]: raise NotImplementedError

    @abstractmethod
    def get_default_iface_name(self) -> str: raise NotImplementedError


class IBridgeServices(ABC):
    @abstractmethod
    def up_port(self, port: Port): raise NotImplementedError

    @abstractmethod
    def down_port(self, port: Port): raise NotImplementedError

    @abstractmethod
    def create_bridge(self, name: str) -> bool: raise NotImplementedError

    @abstractmethod
    def create_bridge(self, name: str) -> bool: raise NotImplementedError

    @abstractmethod
    def set_tagged_port(self, port: Port) -> bool: raise NotImplementedError

    @abstractmethod
    def set_port_not_tagged(self, port: Port) -> bool: raise NotImplementedError

    @abstractmethod
    def delete_bridge(self, name) -> bool: raise NotImplementedError

    @abstractmethod
    def get_bridges(self) -> List[str]: raise NotImplementedError


class BridgeServices(IBridgeServices):
    def __init__(self, command_builder: IBashPipeCommandBuilder):
        self.cmd = command_builder

    def create_bridge(self, name: str) -> bool:
        self.cmd.begin().command(f'ip link add name {name} type bridge').execute()

        self.cmd \
            .begin() \
            .command(f'ip link set {name} up') \
            .execute()

        return True

    def up_port(self, port: Port):
        self.cmd.begin().command(f'ip link set {port.name} up').execute()
        port.enabled = True

    def down_port(self, port: Port):
        self.cmd.begin().command(f'ip link set {port.name} down').execute()
        port.enabled = False

    def set_tagged_port(self, port: Port):
        if not port.tagged:
            raise AttributeError(f'Port {port.name} is not tagged')

        if not port.enabled:
            raise AttributeError(f'Port {port.name} is down')

        self.cmd.begin().command(f'ip link set {port.name} master {port.value}').execute()

    def set_port_not_tagged(self, port: Port):
        if not port.tagged:
            raise AttributeError(f'Port {port.name} is still tagged')

        if not port.enabled:
            raise AttributeError(f'Port {port.name} is down')

        self.cmd.begin().command(f'ip link set {port.name} nomaster').execute()

    def delete_bridge(self, name):
        self.cmd.begin().command(f'ip link delete {name} type bridge').execute()

    def get_bridges(self) -> List[str]:
        out = self.cmd \
            .begin() \
            .command('brctl show') \
            .command("awk 'NF>1 && NR>1 {print $1}' ") \
            .execute()

        for name in out:
            yield name


@si.register(name="InterfaceServices")
class InterfaceServices(IInterfaceServices):
    def __init__(self, network_services: IBridgeServices):

        self.network_services = network_services

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


        def get_all_interfaces(self) -> List[Port]:
            ports: List[Port] = []
            interfaces = psutil.net_if_stats()

            for key in interfaces.keys():
                ports.append(Port(name=key, enabled=interfaces[key].isup))

            return ports

        def get_non_default_interfaces(self) -> List[Port]:
            interfaces = self.get_all_interfaces()
            default = self.get_default_iface_name()

            if 'lo' in interfaces:
                interfaces.remove('lo')

            if default in interfaces:
                interfaces.remove(default)

            return interfaces

        def get_default_iface_name(self) -> str:
            return netifaces.gateways()['default'][netifaces.AF_INET][1]
