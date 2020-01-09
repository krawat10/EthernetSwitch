from typing import List

from django_injector import inject

from Services.BashPipeCommandBuilder import BashPipeCommandBuilder
from Services.IBridgeServices import IBridgeServices
from switch.models import Interface


class BridgeService(IBridgeServices):
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

    def up_interface(self, interface: Interface):
        self.cmd.begin().command(f'ip link set {interface.name} up').execute()
        interface.enabled = True

    def down_interface(self, interface: Interface):
        self.cmd.begin().command(f'ip link set {interface.name} down').execute()
        interface.enabled = False

    def set_tagged_interface(self, interface: Interface):
        if not interface.tagged:
            raise AttributeError(f'Interface {interface.name} is not tagged')

        if not interface.enabled:
            raise AttributeError(f'Interface {interface.name} is down')

        self.cmd.begin().command(f'ip link set {interface.name} master {interface.value}').execute()

    def set_interface_not_tagged(self, interface: Interface):
        if not interface.tagged:
            raise AttributeError(f'Interface {interface.name} is still tagged')

        if not interface.enabled:
            raise AttributeError(f'Interface {interface.name} is down')

        self.cmd.begin().command(f'ip link set {interface.name} nomaster').execute()

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
