from abc import ABC, abstractmethod
from typing import List

from switch.models import Interface


class IBridgeServices(ABC):
    @abstractmethod
    def up_interface(self, interface: Interface): raise NotImplementedError

    @abstractmethod
    def down_interface(self, interface: Interface): raise NotImplementedError

    @abstractmethod
    def create_bridge(self, name: str) -> bool: raise NotImplementedError

    @abstractmethod
    def create_bridge(self, name: str) -> bool: raise NotImplementedError

    @abstractmethod
    def set_tagged_interface(self, interface: Interface) -> bool: raise NotImplementedError

    @abstractmethod
    def set_interface_not_tagged(self, interface: Interface) -> bool: raise NotImplementedError

    @abstractmethod
    def delete_bridge(self, name) -> bool: raise NotImplementedError

    @abstractmethod
    def get_bridges(self) -> List[str]: raise NotImplementedError
