using Sync.Tools;
using Sync.Tools.ConfigurationAttribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimationGifPlayer
{
    public class Configuration : IConfigurable
    {
        private GifPlayerApp App { get; }
        public Configuration(GifPlayerApp app)
        {
            this.App = app;
        }
        [Path(IsDirectory = false)]
        public ConfigurationElement ImagePath { get; set; }

        public void onConfigurationLoad()
        {
            App.LoadImage(ImagePath);
        }

        public void onConfigurationReload()
        {
            App.LoadImage(ImagePath);
        }

        public void onConfigurationSave()
        {
            App.LoadImage(ImagePath);
        }
    }
}
