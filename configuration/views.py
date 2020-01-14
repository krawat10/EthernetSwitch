from typing import List

from django.contrib.auth.models import User
from django.shortcuts import render
from django_injector import inject

from Services.InterfaceService import InterfaceService
from configuration.models import Settings, HiddenInterface
from configuration.view_models import ConfigurationForm
from switch.models import Interface


@inject
def index(request, interface_service: InterfaceService):
    template = 'configuration/index.html'
    form = ConfigurationForm

    hidden_interface_names: List[str] = [interface.name for interface in HiddenInterface.objects.all()]
    users = User.objects.all()
    settings, creates = Settings.objects.get_or_create()
    interfaces = interface_service.get_all_interfaces()
    form = ConfigurationForm(request.POST)
    if request.method == 'POST' and form.is_valid():
        hidden_interfaces = [iface.name for iface in form.cleaned_data['hidden_interfaces']]
        users_edit = form.cleaned_data['users_edit']
        users_view = form.cleaned_data['users_view']
        allow_anonymous = form.cleaned_data['allow_anonymous']
        allow_bridges = form.cleaned_data['allow_bridges']

        return None
    else:
        Interface.objects.all().delete()
        for interface in interfaces:
            if interface.name in hidden_interface_names:
                interface.hide()
            interface.save()

        form = ConfigurationForm(data={
            'allow_bridges': settings.allow_bridges,
            'allow_anonymous': settings.allow_anonymous,
            'hidden_interfaces': [interface for interface in interfaces if interface.is_hidden()],
            'users_edit': users,
            'users_view': users
        })

        # form.fields['hidden_interfaces'].choices = interfaces
        # form.fields['hidden_interfaces'].initial = interfaces
        users_ = [(user.username, user.username) for user in users]
        valid = form.is_valid()
        # form.fields['users_view'].choices = users_
        # form.fields['users_edit'].choices = users_

    return render(request, template, {'form': form})
