using OsuRTDataProvider.Mods;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static OsuRTDataProvider.Listen.OsuListenerManager;

namespace AnimationGifPlayer
{
    public class GifPlayerApp
    {
        private GifPlayer UIForm;
        private Thread UIThread;
        private BeatmapTiming Timings;
        private ModsInfo CurrentMods = default;
        public void Start()
        {
            UIThread = new Thread(ShowForm);
            UIThread.SetApartmentState(ApartmentState.STA);
            UIThread.Name = "STAThreadForm";
            UIThread.Start();
        }

        public void SetCurrentState(OsuStatus state)
        {
            UIForm.SetCurrentState(state);
        }

        public void SetCurrentMods(ModsInfo mods)
        {
            this.CurrentMods = mods;
            UIForm.SetCurrentMods(mods);
        }

        public void SetBeatmap(string[] raw)
        {
            Timings = new BeatmapTiming(raw);
        }

        public void SetCurrentPlaytime(double time)
        {
            if (Timings == null) return; 
            UIForm.SetAnimationDelay(Timings.GetBeatDelay(time), Timings.GetReadlineMeter(time));
            UIForm.SetCurrentPlaytime(time);
        }

        public void LoadImage(string path)
        {
            if (UIForm != null)
            {
                UIForm.LoadImage(path);
            }
        }

        [STAThread]
        public void ShowForm()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            UIForm = new GifPlayer();
            Application.Run(UIForm);
        }
    }
}
