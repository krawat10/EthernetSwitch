from __future__ import annotations

import subprocess
from shlex import split
from typing import List, Tuple

from psutil import Popen

from Services.IBashPipeCommandBuilder import IBashPipeCommandBuilder


class BashPipeCommandBuilder(IBashPipeCommandBuilder):
    in_command: subprocess = None

    def begin(self) -> IBashPipeCommandBuilder:
        self.in_command = None
        return self

    def command(self, *command: List[str]) -> IBashPipeCommandBuilder:
        if self.in_command is None:
            p1 = Popen(split(command), stdout=subprocess.PIPE)
        else:
            p1 = Popen(command, stdin=self.in_command, stdout=subprocess.PIPE)

        self.in_command = p1
        return self

    def execute(self) -> Tuple[str, str]:
        stdout, _ = self.in_command.communicate()
        return stdout, _
