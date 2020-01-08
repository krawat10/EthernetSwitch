from __future__ import annotations

import netifaces
import psutil
from typing import List

from Services.IInterfaceServices import IInterfaceServices


class InterfaceServices(IInterfaceServices):

    def get_additional_interfaces(self) -> List[str]:
        return list(psutil.net_if_addrs().keys())

    def get_all_interfaces(self) -> List[str]:
        interfaces = self.get_additional_interfaces()
        default = self.get_default_iface_name()

        if 'lo' in interfaces:
            interfaces.remove('lo')

        if default in interfaces:
            interfaces.remove(default)

        return interfaces

    def get_default_interface_name(self) -> str:
        return netifaces.gateways()['default'][netifaces.AF_INET][1]
