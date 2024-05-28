module DI.Providers

open Microsoft.Extensions.DependencyInjection

open Model
open DI.Interfaces
open Brokers
open Services

let ServiceProviderBuild (options : ArgumentOptions) =

    ServiceCollection()
        .AddSingleton<IProcessBroker, ProcessBroker>()
        .AddSingleton<IIpBroker, IpBroker>(
            fun services ->
                IpBroker(services.GetRequiredService<IProcessBroker>(),
                         options.PingTimeOut |> PingTimeOut.create,
                         options.Retries |> Retries.create,
                         options.NameLookUpTimeOut |> NameLookupTimeOut.create))
        .AddSingleton<INetworkBroker, NetworkBroker>()
        .AddSingleton<IHelpTextBroker, HelpTextBroker>()
        .AddSingleton<IExceptionBroker, ExceptionBroker>()
        .AddSingleton<IMetricsBroker, MetricsBroker>()
        .AddSingleton<IMacBlacklistBroker, MacBlacklistBroker>(
            fun _ -> MacBlacklistBroker(FileName.create options.MacBlackListFileName))
        .AddSingleton<IIpBlacklistBroker, IpBlacklistBroker>(
            fun _ -> IpBlacklistBroker(FileName.create options.IpBlackListFileName))

        .AddSingleton<IIpService, IpService>()
        .AddSingleton<IHelpTextService, HelpTextService>()
        .AddSingleton<IExceptionService, ExceptionService>()
        .AddSingleton<IMetricsService, MetricsService>()
        .BuildServiceProvider()
