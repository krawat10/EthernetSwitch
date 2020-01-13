from __future__ import annotations

from subprocess import PIPE, Popen
from typing import List, Tuple

import shlex

from Services.IBashPipeCommandBuilder import IBashPipeCommandBuilder


class BashPipeCommandBuilder(IBashPipeCommandBuilder):
    def execute(self, cmd: str) -> Tuple[str, str, int]:
        if "|" in cmd:
            cmd_parts = cmd.split('|')
        else:
            cmd_parts = [cmd]
        i = 0
        p = {}
        for cmd_part in cmd_parts:
            cmd_part = cmd_part.strip()
            if i == 0:
                p[i] = Popen(shlex.split(cmd_part), stdin=None, stdout=PIPE, stderr=PIPE, universal_newlines=True)
            else:
                p[i] = Popen(shlex.split(cmd_part), stdin=p[i - 1].stdout, stdout=PIPE, stderr=PIPE,
                             universal_newlines=True)
            i = i + 1

        (output, err) = p[i - 1].communicate()
        exit_code = p[0].wait()
        return str(output), str(err), exit_code
