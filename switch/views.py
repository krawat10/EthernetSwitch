from django.contrib.auth.decorators import login_required
from django.http import HttpResponseRedirect
from django.shortcuts import render
from django.urls import reverse
from django_injector import request_scope
from django_injector import inject
from Services.InterfaceServices import InterfaceServices
from switch.models import Port


@login_required(redirect_field_name='next', login_url='/login')
@inject
def index(request, interface_service: InterfaceServices):
    interfaces = interface_service.get_all_interfaces()
    ports = [Port(interface) for interface in interfaces]

    context = {
        'content': 'Hello',
        'title': 'Ethernet switch',
        'ports': interfaces
    }

    return render(request, 'switch/index.html', context)


@login_required(redirect_field_name='next', login_url='/login')
def submit(request):
    ports = []
    print(request.POST)
    print(request.POST.getlist('names'))
    for portName in request.POST.getlist('names'):
        value = int(request.POST[f'{portName}-value'])
        enabled = portName in request.POST.getlist('enabled-ports')
        tagged = portName in request.POST.getlist('tagged-ports')
        ports.append(Port(name=portName, value=value, enabled=enabled, tagged=tagged))

    # Port.objects.create()

    print(ports)
    # config = Interfaces \
    #     .init('mybridge', ports) \
    #     .add_modify_stamp() \
    #     .add_default_interface() \
    #     .up_interfaces() \
    #     .add_loopback() \
    #     .allow_vagrant() \
    #     .add_no_tag_vlan() \
    #     .create_config()
    #
    # with open(InterfacesConfig.interface_config_path, 'w+') as f:
    #     f.write(config)

    return HttpResponseRedirect(reverse('index'))
