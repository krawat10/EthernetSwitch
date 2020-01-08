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
