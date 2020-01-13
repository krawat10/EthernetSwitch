from typing import List

from django import forms
from django.contrib.auth.models import User

from configuration.models import Settings
from switch.models import Interface


class ConfigurationViewModel(forms.Form):

    allow_bridges = forms.BooleanField(label='Allow Bridges', initial=True)
    allow_anonymous = forms.BooleanField(label='Allow Anonymous', initial=False)
    users_view: forms.ModelMultipleChoiceField(label='Users with view permission',
                                               queryset=None,
                                               widget=forms.SelectMultiple)
    users_edit: forms.ModelMultipleChoiceField(label='Users with edit permission',
                                               queryset=None,
                                               widget=forms.SelectMultiple)
    hidden_interface = forms.ModelMultipleChoiceField(label='Hidden interfaces',
                                                      queryset=None,
                                                      widget=forms.CheckboxSelectMultiple)

    def __init__(self, settings: Settings, users: User, interfaces: List[Interface]):
        super().__init__()
        self.fields['allow_anonymous'].initial = settings.allow_anonymous
        self.fields['allow_bridges'].initial = settings.allow_bridges
        self.fields['hidden_interface'].choices = [(interface.name, interface.name) for interface in interfaces]
        self.fields['hidden_interface'].initial = [interface.name for interface in interfaces if interface.is_hidden()]
