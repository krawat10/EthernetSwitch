from django.http import HttpResponse, HttpResponseRedirect
from django.shortcuts import render, redirect

# Create your views here.
from django.urls import reverse

from switch.models import Port


def index(request):
    context = {
        'content': 'Hello',
        'title': 'Ethernet switch',
        'ports': [
            Port('1', 0, False),
            Port('2', 0, False),
            Port('3', 0, False),
            Port('4', 0, False),
        ]
    }
    return render(request, 'switch/index.html', context)


def submit(request):
    print(request.POST)
    print(request.POST.getlist('names'))
    for portName in request.POST.getlist('names'):
        enabled = portName in request.POST.getlist('enabled-ports')
        value = request.POST['port-' + portName + '-value']
        print(portName + ' ' + enabled.__str__() + ' ' + value)

    return HttpResponseRedirect(reverse('index'))
