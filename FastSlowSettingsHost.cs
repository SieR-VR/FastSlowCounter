using System.Collections.Generic;
using System.Linq;
using BeatSaberMarkupLanguage.Attributes;
using FastSlowCounter.Configuration;
using UnityEngine;
using Zenject;

namespace FastSlowCounter
{
    public class FastSlowSettingsHost
    {
        public const string CounterName = "FastSlow Counter";

        [Inject] private FastSlowConfig config;

        [UIValue("TimingMode")]
        public FastSlowTimingMode TimingMode
        {
            get => config.TimingMode;
            set => config.TimingMode = value;
        }

        [UIValue("TimingModes")]
        public List<object> TimingModes => new List<object> { FastSlowTimingMode.Exact, FastSlowTimingMode.MovingAverage };

        [UIAction("TimingModeFormat")]
        public string TimingModeFormat(FastSlowTimingMode mode) => mode.ToString();

        [UIValue("CenterColor")]
        public Color CenterColor
        {
            get => config.CenterColor;
            set => config.CenterColor = value;
        }

        [UIValue("EdgeColor")]
        public Color EdgeColor
        {
            get => config.EdgeColor;
            set => config.EdgeColor = value;
        }

        [UIValue("PointCount")]
        public int PointCount
        {
            get => config.PointCount;
            set => config.PointCount = value;
        }

        [UIValue("PointCounts")]
        public List<object> PointCounts => Enumerable.Range(4, 21).Cast<object>().ToList();

        [UIValue("FadeStrength")]
        public int FadeStrength
        {
            get => Mathf.RoundToInt(config.FadeStrength * 100f);
            set => config.FadeStrength = Mathf.Clamp01(value / 100f);
        }

        [UIValue("FadeStrengths")]
        public List<object> FadeStrengths => Enumerable.Range(0, 21).Select(i => i * 5).Cast<object>().ToList();

        [UIAction("FadeStrengthFormat")]
        public string FadeStrengthFormat(int percent) => percent + "%";
    }
}
