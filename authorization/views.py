from django.contrib.auth import authenticate, login
from django.shortcuts import render, redirect

from authorization.forms import LoginForm


def login_view(request):
    template = 'authorization/Login.html'
    form = LoginForm
    if request.method == 'POST':
        username = request.POST.get('username', '')
        password = request.POST.get('password', '')
        user = authenticate(username=username, password=password)
        if user is not None:
            if user.is_active:
                login(request, user)
                # messages.success(request, "You have logged in!")
                return redirect(request.POST.get('next', ''))
            else:
                # messages.warning(request, "Your account is disabled!")
                return redirect('/login')
        else:
            # messages.warning(request, "The username or password are not valid!")
            return redirect('/login')
    context = {'form': form}
    return render(request, template, context)


def post(request):
    username = request.POST['Username']
    password = request.POST['Password']
    user = authenticate(request, username=username, password=password)
    if user is not None:
        login(request, user)

        # Redirect to a success page.
        ...
    else:
        # Return an 'invalid login' error message.
        ...


def get(request):
    context = {}
    return render(request, 'authorization/Login.html', context)
