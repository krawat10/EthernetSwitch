from abc import ABC, abstractmethod
from typing import List

from switch.models import Port


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