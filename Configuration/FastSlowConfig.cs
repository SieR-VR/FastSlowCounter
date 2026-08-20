using IPA.Config.Stores.Attributes;
using IPA.Config.Stores.Converters;
using UnityEngine;

namespace FastSlowCounter.Configuration
{
    public class FastSlowConfig
    {
        public static FastSlowConfig Instance { get; set; }

        public virtual FastSlowTimingMode TimingMode { get; set; } = FastSlowTimingMode.MovingAverage;

        [UseConverter(typeof(HexColorConverter))]
        public virtual Color CenterColor { get; set; } = new Color(0.25f, 1f, 0.45f, 1f);

        [UseConverter(typeof(HexColorConverter))]
        public virtual Color EdgeColor { get; set; } = new Color(1f, 0.28f, 0.32f, 1f);

        public virtual int PointCount { get; set; } = 12;

        public virtual float FadeStrength { get; set; } = 0.15f;

        public virtual void OnReload() { }

        public virtual void Changed() { }

        public virtual void CopyFrom(FastSlowConfig other)
        {
            TimingMode = other.TimingMode;
            CenterColor = other.CenterColor;
            EdgeColor = other.EdgeColor;
            PointCount = other.PointCount;
            FadeStrength = other.FadeStrength;
        }
    }

    public enum FastSlowTimingMode
    {
        Exact,
        MovingAverage
    }
}
