class ServiceFactory:
    def __init__(self):
        self.__services = {}

    def register(self, name, service_class):
        self.__services[name] = service_class

    def create(self, name, *args, **kwargs):
        return self.__services[name](*args, **kwargs)

factory = ServiceFactory()