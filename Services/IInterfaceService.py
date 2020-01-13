from abc import ABC, abstractmethod
from typing import List

from switch.models import Interface


class IInterfaceServices(ABC):

    @abstractmethod
    def get_non_default_interfaces(self) -> List[Interface]: raise NotImplementedError

    @abstractmethod
    def get_all_interfaces(self) -> List[Interface]: raise NotImplementedError

    @abstractmethod
    def get_default_interface_name(self) -> str: raise NotImplementedError
