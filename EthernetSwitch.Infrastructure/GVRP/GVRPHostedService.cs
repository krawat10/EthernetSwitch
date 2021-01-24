using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EthernetSwitch.Infrastructure.Ethernet;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EthernetSwitch.Infrastructure.GVRP
{
    public class GVRPHostedService : BackgroundService
    {
        private readonly GVRPActivePortsSingleton _gvrpActivePortsSingleton = GVRPActivePortsSingleton.Instance;

        private readonly IDictionary<string, bool> _interfaceActiveTasks;
        private readonly ILogger<GVRPHostedService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public GVRPHostedService(ILogger<GVRPHostedService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _interfaceActiveTasks = new Dictionary<string, bool>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Queued Hosted Service is running");

            await BackgroundProcessing(stoppingToken);
        }

        private async Task BackgroundProcessing(CancellationToken stoppingToken)
        {
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;

            while (!stoppingToken.IsCancellationRequested)
            {
                var interfaceStates = _gvrpActivePortsSingleton.InterfaceStates;

                foreach (var interfaceName in interfaceStates.Keys)
                {

                    if (interfaceStates[interfaceName] == InterfaceState.Listening)
                    {
                        _interfaceActiveTasks.TryGetValue(interfaceName, out var isRunningTask);

                        if (!isRunningTask)
                        {
                            _interfaceActiveTasks[interfaceName] = true;

                            _logger.LogInformation($"GVRP started listening {interfaceName}");

                            await Task.Factory
                                .StartNew(async () => await FrameReader.StartCapturing(interfaceName, _serviceProvider, token), stoppingToken);
                            //.ContinueWith(task => _interfaceActiveTasks[interfaceName] = false, stoppingToken);
                        }
                    }
                    if (interfaceStates[interfaceName] == InterfaceState.Off && _interfaceActiveTasks[interfaceName] == true)
                    {
                        _logger.LogInformation($"Interface state off and active task {interfaceName}"); // throw cancelation request
                        source.Cancel();
                        source.Dispose();
                        _interfaceActiveTasks[interfaceName] = false;
                        source = new CancellationTokenSource();
                        token = source.Token;
                        foreach (var interfaceName2 in interfaceStates.Keys)
                        {
                            if (interfaceStates[interfaceName2] == InterfaceState.Listening)
                            {
                                _interfaceActiveTasks[interfaceName2] = true;
                                await Task.Factory
                               .StartNew(async () => await FrameReader.StartCapturing(interfaceName2, _serviceProvider, token), stoppingToken);
                            }
                        }
                    } 
                }
                await Task.Delay(5000, stoppingToken);
            }
            source.Dispose();
        }


        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Queued Hosted Service is stopping.");

            await base.StopAsync(stoppingToken);
        }
    }
}