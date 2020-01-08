from __future__ import annotations

from abc import ABC, abstractmethod
from typing import Tuple


class IBashPipeCommandBuilder(ABC):
    @abstractmethod
    def begin(self) -> IBashPipeCommandBuilder: raise NotImplementedError

    @abstractmethod
    def command(self, command: str) -> IBashPipeCommandBuilder: raise NotImplementedError

    @abstractmethod
    def execute(self) -> Tuple[str, str]: raise NotImplementedError
