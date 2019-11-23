import os
import re

import json
from django.http import HttpResponse, HttpResponseRedirect
from django.shortcuts import render, redirect

# Create your views here.
from django.urls import reverse

from switch.models import Port, Interfaces, InterfacesConfig, get_ports_from_config, save_ports


def index(request):
    ports = []
    is_initial_config = True

    initial_config_content = ''

    with open(InterfacesConfig.initial_config_path) as initial_config:
        for line in initial_config:
            initial_config_content += line

    with open(InterfacesConfig.interface_config_path) as config:
        for idx, line in enumerate(config):
            if idx == 0 and re.search('#custom-ethernet-config', line):
                is_initial_config = False
                break

    ports = get_ports_from_config(initial_config_content)
    save_ports(ports)

    if is_initial_config:
        with open(InterfacesConfig.initial_config_path, 'w+') as defaultConfig:
            defaultConfig.write(content)

    context = {
        'content': 'Hello',
        'title': 'Ethernet switch',
        'ports': ports
    }

    return render(request, 'switch/index.html', context)


def submit(request):
    ports = []
    print(request.POST)
    print(request.POST.getlist('names'))
    for portName in request.POST.getlist('names'):
        value = int(request.POST['port-' + portName + '-value'])
        enabled = portName in request.POST.getlist('enabled-ports')
        tagged = portName in request.POST.getlist('tagged-ports')
        ports.append(Port(portName, value, enabled, tagged))

    print(ports)
    config = Interfaces \
        .init('mybridge', ports) \
        .add_modify_stamp() \
        .add_default_interface() \
        .up_interfaces() \
        .add_loopback() \
        .allow_vagrant() \
        .add_no_tag_vlan() \
        .create_config()

    with open(InterfacesConfig.interface_config_path, 'w+') as f:
        f.write(config)

    return HttpResponseRedirect(reverse('index'))
