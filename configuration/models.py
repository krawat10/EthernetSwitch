from django.db import models


class Settings(models.Model):
    allow_anonymous = models.BooleanField(default=False)
    allow_bridges = models.BooleanField(default=True)


class HiddenInterface(models.Model):
    name = models.CharField(max_length=30, default='')
