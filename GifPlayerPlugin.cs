using OsuRTDataProvider;
using OsuRTDataProvider.BeatmapInfo;
using OsuRTDataProvider.Listen;
using OsuRTDataProvider.Mods;
using Sync.Plugins;
using Sync.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimationGifPlayer
{
    [SyncPluginID("4eaf2dca-1d49-4f0c-b9b7-c220db5feab0", PLGUIN_VERSION)]
    public class GifPlayerPlugin : Plugin
    {
        public const string PLUGIN_NAME = "GifPlayer";
        public const string PLUGIN_AUTHOR = "Deliay";
        public const string PLGUIN_VERSION = "0.1";
        private PluginConfigurationManager ConfigManager { get; set; }
        private Configuration Config { get; set; }
        private GifPlayerApp App { get; set; }
        public GifPlayerPlugin() : base(PLUGIN_NAME, PLUGIN_AUTHOR) { }

        public override void OnEnable()
        {
            App = new GifPlayerApp();
            App.Start();
            ConfigManager = new PluginConfigurationManager(this);
            Config = new Configuration(App);
            ConfigManager.AddItem(Config);
            this.EventBus.BindEvent<PluginEvents.InitCommandEvent>((@event) =>
            {
                @event.Commands.Dispatch.bind("gif", _ =>
                {
                    App.ShowForm();
                    return true;
                }, "Show or hide gif player window");
            });

            this.EventBus.BindEvent<PluginEvents.ProgramReadyEvent>((@event) =>
            {
                var ortdp = getHoster().EnumPluings().FirstOrDefault(p => p.Name == "OsuRTDataProvider") as OsuRTDataProviderPlugin;
                ortdp.ListenerManager.OnPlayingTimeChanged += ListenerManager_OnPlayingTimeChanged;
                ortdp.ListenerManager.OnBeatmapChanged += ListenerManager_OnBeatmapChanged;
                ortdp.ListenerManager.OnModsChanged += ListenerManager_OnModsChanged;
                ortdp.ListenerManager.OnStatusChanged += ListenerManager_OnStatusChanged;
                App.LoadImage(Config.ImagePath);
            });

        }

        private void ListenerManager_OnStatusChanged(OsuListenerManager.OsuStatus last_status, OsuListenerManager.OsuStatus status)
        {
            // apply Half-Time/Double-Time modifier when osu! in playing state
            App.SetCurrentState(status);
        }

        private void ListenerManager_OnModsChanged(ModsInfo mods)
        {
            App.SetCurrentMods(mods);
        }

        private void ListenerManager_OnBeatmapChanged(Beatmap map)
        {
            App.SetBeatmap(File.ReadAllLines(map.FilenameFull));
        }

        private void ListenerManager_OnPlayingTimeChanged(int ms)
        {
            App.SetCurrentPlaytime(ms);
        }
    }
}
