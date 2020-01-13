from django.contrib.auth.decorators import login_required
from django.http import HttpResponseRedirect
from django.shortcuts import render
from django.urls import reverse
from django_injector import request_scope
from django_injector import inject
from typing import List
from switch.models import Interface

from Services.InterfaceService import InterfaceService


@login_required(redirect_field_name='next', login_url='/login')
@inject
def index(request, interface_service: InterfaceService):
    interfaces = interface_service.get_non_default_interfaces()

    context = {
        'content': 'Hello',
        'title': 'Ethernet switch',
        'ports': interfaces
    }

    return render(request, 'switch/index.html', context)


@login_required(redirect_field_name='next', login_url='/login')
@inject
def submit(request, interface_service: InterfaceService):
    ports: List[Interface] = []
    print(request.POST)
    print(request.POST.getlist('names'))
    for portName in request.POST.getlist('names'):
        value = int(request.POST[f'{portName}-value'])
        enabled = portName in request.POST.getlist('enabled-ports')
        tagged = portName in request.POST.getlist('tagged-ports')
        ports.append(Interface(name=portName, value=value, enabled=enabled, tagged=tagged))

    # Port.objects.create()
    interface_service.apply_ports_settings(ports)
    print(ports)

    return HttpResponseRedirect(reverse(''))
