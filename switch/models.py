from django.db import models


# Create your models here.

class Port:
    def __init__(self, name, value, enabled):
        self.enabled = enabled
        self.name = name
        self.value = value

    @property
    def className(self):
        return 'table-success' if self.enabled else 'table-danger'

