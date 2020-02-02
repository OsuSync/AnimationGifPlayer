using OsuRTDataProvider.Mods;
using Sync.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static OsuRTDataProvider.Listen.OsuListenerManager;

namespace AnimationGifPlayer
{
    public partial class GifPlayer : Form
    {
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);

        private Graphics Gdip { get; set; }
        private Image Image { get; set; }
        private FrameDimension Frames { get; set; }
        private int FrameCount { get; set; }
        private double ModifierDelayMultiply = 1.0D;
        private int CurrentFrame { get; set; } = 0;
        private bool RenderState { get; set; } = false;
        private string ImagePath;
        private Thread Looping;
        public GifPlayer()
        {
            InitializeComponent();
            Gdip = this.CreateGraphics();
            Looping = new Thread(() =>
            {
                while (true)
                {
                    if (Image != null && RenderState)
                    {
                        var isPlaying = CurrentState == OsuStatus.Playing;
                        var delay = (int)(Delay / (isPlaying ? ModifierDelayMultiply : 1));
                        // atleast 30bpm
                        if (delay > 2000) delay = 2000;
                        Thread.Sleep(delay);
                        if (CurrentFrame >= FrameCount)
                        {
                            CurrentFrame = 0;
                        }
                        Image.SelectActiveFrame(this.Frames, CurrentFrame++);
                        Gdip.DrawImage(Image, new Rectangle(0, 0, this.Width, this.Height));
                    }
                    Thread.Sleep(1);
                }
            });
            Looping.Start();
        }

        public double Delay { get; set; }
        public double CurrentTime { get; set; }
        public ModsInfo CurrentMods { get; set; } = ModsInfo.Empty;
        public OsuStatus CurrentState { get; set; } = OsuStatus.Idle;

        public delegate void Setter<T>(T value);
        public delegate void Setter<T, F>(T value, F value2);
        public void SetCurrentState(OsuStatus state)
        {
            Invoke(new Setter<OsuStatus>((value) =>
            {
                CurrentState = value;
            }), state);
        }
        public void SetAnimationDelay(double delay, int meter)
        {
            Invoke(new Setter<double, int>((value, value2) =>
            {
                IO.CurrentIO.Write($"[GIF Player] BPM: {60000 / value}, {value2}/4, Mods: {CurrentMods.ShortName}, State: {CurrentState}, Mul: {ModifierDelayMultiply}");
                // Convert to 4/4 beat
                this.Delay = value / value2;
            }), delay, meter);
        }
        public void SetCurrentPlaytime(double time)
        {
            Invoke(new Setter<double>((value) =>
            {
                CurrentTime = value;
                RenderState = true;
            }), time);
        }
        public void SetCurrentMods(ModsInfo mods)
        {
            Invoke(new Setter<ModsInfo>((value) =>
            {
                this.CurrentMods = value;
                IO.CurrentIO.Write($"[GIF Player] Set mods: {CurrentMods}");
                this.ModifierDelayMultiply = value.TimeRate;
            }), mods);
        }

        public delegate void LoadImageDelegate(string path);
        public void LoadImage(string path)
        {
            Invoke(new LoadImageDelegate((_p) => _LoadImage(_p)), path);
        }
        private void _LoadImage(string path)
        {
            if (path == null || path.Length == 0) return;
            if (!File.Exists(path)) return;
            if (ImagePath != path)
            {
                RenderState = false;
                this.Image = Image.FromFile(path);
                this.Frames = new FrameDimension(this.Image.FrameDimensionsList[0]);
                FrameCount = Image.GetFrameCount(this.Frames);
                CurrentFrame = 0;
                ImagePath = path;
                this.Width = Image.Width;
                this.Height = Image.Height;
                try
                {
                    Gdip.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    Gdip.DrawImage(Image, new Rectangle(0, 0, this.Width, this.Height));
                }
                catch { }
            }
        }

        private void onMouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(Handle, 0x0112, 0xF012, 0);
        }
    }
}
