import subprocess
from shlex import split
from subprocess import Popen, PIPE
from typing import List, Tuple

from Services.IBashPipeCommandBuilder import IBashPipeCommandBuilder


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
