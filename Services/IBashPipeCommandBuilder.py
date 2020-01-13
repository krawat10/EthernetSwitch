from __future__ import annotations

from abc import ABC, abstractmethod
from typing import Tuple


class IBashPipeCommandBuilder(ABC):
    @abstractmethod
    def execute(self, cmd: str) -> Tuple[str, str, int]: raise NotImplementedError
