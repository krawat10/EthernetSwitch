from typing import List

from django_injector import inject

from Services.BashPipeCommandBuilder import BashPipeCommandBuilder
from Services.IBridgeServices import IBridgeServices
from switch.models import Port


class BridgeServices(IBridgeServices):
    @inject
    def __init__(self, command_builder: BashPipeCommandBuilder):
        self.cmd = command_builder

    def create_bridge(self, name: str) -> bool:
        self.cmd.execute(f'ip link add name {name} type bridge')

        self.cmd.execute(f'ip link set {name} up')
            

        return True

    def up_port(self, port: Port):
        (out, err, exit_code) = self.cmd.execute(f'ip link set {port.name} up')
        port.enabled = True

    def down_port(self, port: Port):
        self.cmd.execute(f'ip link set {port.name} down')
        port.enabled = False

    def set_tagged_port(self, port: Port):
        if not port.tagged:
            raise AttributeError(f'Port {port.name} is not tagged')

        if not port.enabled:
            raise AttributeError(f'Port {port.name} is down')

        self.cmd.execute(f'ip link set {port.name} master {port.value}')

    def set_port_not_tagged(self, port: Port):
        if port.tagged:
            raise AttributeError(f'Port {port.name} is still tagged')

        if not port.enabled:
            raise AttributeError(f'Port {port.name} is down')

        self.cmd.execute(f'ip link set {port.name} nomaster')

    def delete_bridge(self, name):
        self.cmd.execute(f'ip link delete {name} type bridge')

    def get_bridges(self) -> List[str]:
        (out, err, exit_code) = self.cmd.execute("bridge link | sed -n '2,$ {s/ .*//; /./p}'")

        for line in out:
            bridge = line.strip()
            if(bridge.isdecimal()):
                yield int(bridge)
