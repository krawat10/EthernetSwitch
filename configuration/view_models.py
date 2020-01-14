from typing import List

from django import forms
from django.contrib.auth.models import User
from django.db.models import QuerySet
from django_injector import inject

from configuration.models import Settings, HiddenInterface
from switch.models import Interface


class ConfigurationForm(forms.Form):
    allow_bridges = forms.BooleanField(label='Allow Bridges', initial=True, required=False)
    allow_anonymous = forms.BooleanField(label='Allow Anonymous', initial=False, required=False)
    users_view = forms.ModelMultipleChoiceField(label='Users with view permission',
                                                queryset=User.objects.all(),
                                                initial=User.objects.all(),
                                                widget=forms.CheckboxSelectMultiple,
                                                required=False)
    users_edit = forms.ModelMultipleChoiceField(label='Users with edit permission',
                                                queryset=User.objects.all(),
                                                initial=User.objects.all(),
                                                widget=forms.CheckboxSelectMultiple,
                                                required=False)
    hidden_interfaces = forms.ModelMultipleChoiceField(label='Hidden interfaces',
                                                       queryset=Interface.objects.all(),
                                                       widget=forms.CheckboxSelectMultiple,
                                                       required=False)
