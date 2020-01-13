from typing import List

from django.contrib.auth.models import User
from django.shortcuts import render
from django_injector import inject

from Services.InterfaceService import InterfaceService
from configuration.models import Settings, HiddenInterfaces
from configuration.view_models import ConfigurationViewModel


@inject
def index(request, interface_service: InterfaceService):
    template = 'configuration/index.html'

    if request.method == 'POST':
        return None
    else:
        hidden_interface_names: List[str] = [interface.name for interface in HiddenInterfaces.objects.all()]
        users = User.objects.all()
        settings, creates = Settings.objects.get_or_create()
        interfaces = interface_service.get_all_interfaces()

        for interface in interfaces:
            if interface.name in hidden_interface_names:
                interface.hide()

        form = ConfigurationViewModel(settings, users, interfaces)

    return render(request, template, {'form': form})
