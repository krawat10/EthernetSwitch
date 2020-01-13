from abc import ABC, abstractmethod
from typing import List

from switch.models import Interface


class INetworkService(ABC):
    @abstractmethod
    def create_bridge(self, name: str) -> bool: raise NotImplementedError

    @abstractmethod
    def up_port(self, port: Interface): raise NotImplementedError

    @abstractmethod
    def down_port(self, interface: Interface): raise NotImplementedError

    @abstractmethod
    def tag_interface(self, interface: Interface): raise NotImplementedError

    @abstractmethod
    def untag_interface(self, interface: Interface): raise NotImplementedError

    @abstractmethod
    def delete_bridge(self, name): raise NotImplementedError

    @abstractmethod
    def get_bridges(self) -> List[str]: raise NotImplementedError
