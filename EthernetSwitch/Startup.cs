using EthernetSwitch.BackgroundWorkers;
using EthernetSwitch.Data;
using EthernetSwitch.Infrastructure.Bash;
using EthernetSwitch.Infrastructure.Ethernet;
using EthernetSwitch.Infrastructure.Settings;
using EthernetSwitch.Infrastructure.SNMP;
using EthernetSwitch.Infrastructure.Users;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EthernetSwitch
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Env = env;
            Configuration = configuration;
        }

        public IWebHostEnvironment Env;
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddSingleton<IBashCommand, BashCommand>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<SNMPServices>();
            services.AddScoped<ISettingsRepository, SettingsRepository>();
            services.AddScoped<ITrapUsersRepository, TrapUsersRepository>();
            services.AddScoped<EthernetServices>();
            services.AddScoped<LLDPServices>();
            services.AddEntityFrameworkSqlite().AddDbContext<EthernetSwitchContext>();
            services.AddHostedService<QueuedHostedService>();
            services.AddHostedService<TrapReciverHostedService>();
            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme,
                    options =>
                    {
                        options.Cookie.HttpOnly = true;
                        options.LoginPath = "/User/Login";
                    });

            if (Env.IsDevelopment())
            {
                services
                    .AddRazorPages()
                    .AddRazorRuntimeCompilation();
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            if (Env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            
            app.UseStaticFiles();
            app.UseRouting();
            
            app.UseAuthorization();
            app.UseAuthentication();
            app.UseCookiePolicy();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Ethernet}/{action=Index}/{id?}");
            });
        }
    }
}