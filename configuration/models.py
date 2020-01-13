from django.db import models


# Create your models here.
class Settings(models.Model):
    allow_anonymous: bool = False
    allow_bridges: bool = True


class HiddenInterfaces(models.Model):
    name: str = ''
