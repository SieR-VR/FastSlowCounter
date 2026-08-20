using FastSlowCounter.Configuration;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using SiraUtil.Zenject;
using IPALogger = IPA.Logging.Logger;

namespace FastSlowCounter
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        internal static IPALogger Log { get; private set; }

        [Init]
        public void Init(IPALogger logger, Config conf, Zenjector zenjector)
        {
            Log = logger;
            FastSlowConfig.Instance = conf.Generated<FastSlowConfig>();
            Log.Info("FastSlowCounter initialized.");

            zenjector.Install(Location.App, container =>
            {
                container.Bind<FastSlowConfig>().FromInstance(FastSlowConfig.Instance);
                container.Bind<FastSlowSettingsHost>().WithId(FastSlowSettingsHost.CounterName).AsSingle();
            });
        }
    }
}
