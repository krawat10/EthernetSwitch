from typing import List

from django_injector import inject

from Services.BashPipeCommandBuilder import BashPipeCommandBuilder
from Services.INetworkService import INetworkService
from switch.models import Interface


class NetworkService(INetworkService):
    @inject
    def __init__(self, command_builder: BashPipeCommandBuilder):
        self.cmd = command_builder

    def create_bridge(self, name: str) -> bool:
        self.cmd.execute(f'ip link add name {name} type bridge')

        self.cmd.execute(f'ip link set {name} up')

        return True

    def up_port(self, port: Interface):
        (out, err, exit_code) = self.cmd.execute(f'ip link set {port.name} up')
        if exit_code == 0:
            port.enabled = True

    def down_port(self, interface: Interface):
        (out, err, exit_code) = self.cmd.execute(f'ip link set {interface.name} down')
        if exit_code == 0:
            interface.enabled = False

    def tag_interface(self, interface: Interface):
        if not interface.tagged:
            raise AttributeError(f'Port {interface.name} is not tagged')

        if not interface.enabled:
            raise AttributeError(f'Port {interface.name} is down')

        self.cmd.execute(f'ip link set {interface.name} master {interface.value}')

    def untag_interface(self, interface: Interface):
        if interface.tagged:
            raise AttributeError(f'Port {interface.name} is still tagged')

        if not interface.enabled:
            raise AttributeError(f'Port {interface.name} is down')

        self.cmd.execute(f'ip link set {interface.name} nomaster')

    def delete_bridge(self, name):
        self.cmd.execute(f'ip link delete {name} type bridge')

    def get_bridges(self) -> List[str]:
        (out, err, exit_code) = self.cmd.execute("bridge link | sed -n '2,$ {s/ .*//; /./p}'")

        for line in out:
            bridge = line.strip()
            if bridge.isdecimal():
                yield int(bridge)
