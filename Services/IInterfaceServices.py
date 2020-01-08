from abc import ABC, abstractmethod
from typing import List


class IInterfaceServices(ABC):

    @abstractmethod
    def get_all_interfaces(self) -> List[str]: raise NotImplementedError

    @abstractmethod
    def get_additional_interfaces(self) -> List[str]: raise NotImplementedError

    @abstractmethod
    def get_default_interface_name(self) -> str: raise NotImplementedError
