from __future__ import annotations
import subprocess
from abc import ABCMeta, abstractmethod, ABC

from shlex import split
from subprocess import Popen, PIPE

import netifaces as netifaces
import psutil
from typing import List, Tuple


class IInterfaceServices(ABC):

    @abstractmethod
    def get_all_interfaces(self) -> List[str]: raise NotImplementedError

    @abstractmethod
    def get_additional_interfaces(self) -> List[str]: raise NotImplementedError

    @abstractmethod
    def get_default_iface_name(self) -> str: raise NotImplementedError

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

    def get_default_iface_name(self) -> str:
        return netifaces.gateways()['default'][netifaces.AF_INET][1]


class IBashPipeCommandBuilder(ABC):
    @abstractmethod
    def begin(self, command: str) -> IBashPipeCommandBuilder: raise NotImplementedError

    @abstractmethod
    def command(self, command: str) -> IBashPipeCommandBuilder: raise NotImplementedError

    @abstractmethod
    def execute(self) -> Tuple[str, str]: raise NotImplementedError


class BashPipeCommandBuilder(IBashPipeCommandBuilder):
    in_command: subprocess = None

    def begin(self, command: str) -> IBashPipeCommandBuilder:
        self.in_command = None

    def command(self, *command: List[str]) -> IBashPipeCommandBuilder:
        if self.in_command is None:
            p1 = Popen(split(command), stdout=PIPE)
        else:
            p1 = Popen(command, stdin=self.in_command, stdout=PIPE)

        self.in_command = p1

    def execute(self) -> Tuple[str, str]:
        stdout, _ = self.in_command.communicate()
        return stdout, _
