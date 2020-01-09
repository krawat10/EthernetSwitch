from abc import ABC, abstractmethod
from typing import List

from switch.models import Port


class IInterfaceServices(ABC):

    @abstractmethod
    def get_non_default_interfaces(self) -> List[Port]: raise NotImplementedError

    @abstractmethod
    def get_all_interfaces(self) -> List[Port]: raise NotImplementedError

    @abstractmethod
    def get_default_iface_name(self) -> str: raise NotImplementedError
