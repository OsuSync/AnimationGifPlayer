using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimationGifPlayer
{
    public class BeatmapTiming
    {
        private readonly Dictionary<double, double> RedLines = new Dictionary<double, double>();
        private readonly Dictionary<double, int> RedLinesMeters = new Dictionary<double, int>();
        private readonly List<double> Offsets = new List<double>();

        private void ParseLine(string raw)
        {
            var splitPos = raw.IndexOf(',');
            var secondSplitPos = raw.IndexOf(',', splitPos + 1);
            var thirdSplitPos = raw.IndexOf(',', secondSplitPos + 1);
            // [offset],[bpm],[meter],...
            double offset = double.Parse(raw.Substring(0, splitPos));
            double bpm = double.Parse(raw.Substring(splitPos + 1, secondSplitPos - splitPos - 1));
            int meter = int.Parse(raw.Substring(secondSplitPos + 1, thirdSplitPos - secondSplitPos - 1));
            // < 0 mean grean line
            if (bpm < 0)
            {
                return;
            }
            RedLines.Add(offset, bpm);
            RedLinesMeters.Add(offset, meter);
            Offsets.Add(offset);
        }
        private void Parse(string[] raw)
        {
            int index = 0;
            while (raw[index++].Trim() != "[TimingPoints]") { }
            while (raw[index].Length > 0 && char.IsNumber(raw[index][0]))
            {
                ParseLine(raw[index++]);
            }
        }
        public BeatmapTiming(string[] raw)
        {
            Parse(raw);
        }

        public double GetBeatDelay(double offset)
        {
            foreach (var redlineOffset in Offsets.OrderByDescending(_ => _))
            {
                if (offset >= redlineOffset) return RedLines[redlineOffset];
            }
            // default set to 60bpm (1e3 ms)
            return 1000;
        }

        public int GetReadlineMeter(double offset)
        {
            foreach (var redlineOffset in Offsets.OrderByDescending(_ => _))
            {
                if (offset >= redlineOffset) return RedLinesMeters[redlineOffset];
            }
            // default set to 60bpm (1e3 ms)
            return 4;
        }

        public double GetBeatPreMinute(double offset)
        {
            return 60000 / GetBeatDelay(offset);
        }
    }
}
