using System.Collections.Generic;
using BeatSaberMarkupLanguage;
using CountersPlus.Counters.Interfaces;
using CountersPlus.Custom;
using CountersPlus.Utils;
using FastSlowCounter.Configuration;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace FastSlowCounter
{
    public class FastSlowCounterController : MonoBehaviour, ICounter
    {
        private const float MaxDeviation = 0.15f;
        private const float BarHalfWidth = 20f;
        private const float BarHeight = 1.5f;
        private const float TickHeight = 1.5f;
        private const float TickWidth = 1f;
        private const float PointWidth = 2.5f;
        private const float SaberAlignThreshold = 0.5f;
        private const float MinAlpha = 0.03f;
        private const float EmaAlpha = 0.2f;

        [Inject] private CanvasUtility canvasUtility;
        [Inject] private CustomConfigModel settings;
        [InjectOptional] private BeatmapObjectManager beatmapObjectManager;
        [InjectOptional] private SaberManager saberManager;

        private RectTransform root;
        private Image centerTick;
        private Image leftTick;
        private Image rightTick;

        private static Sprite whiteSprite;

        private readonly List<PointData> points = new List<PointData>();
        private readonly List<NoteTracker> trackers = new List<NoteTracker>();
        private float average;

        private class PointData
        {
            public float dev;
            public float alpha;
            public Image img;
        }

        private class NoteTracker
        {
            public NoteController controller;
            public Vector3 cutPoint;
            public Saber correctSaber;
            public bool cutPointSet;
            public float minNoteDist = float.MaxValue;
            public float tPerfect;
            public float minSaberXY = float.MaxValue;
            public float tSaber;
        }

        public void CounterInit()
        {
            BuildUI();
            if (beatmapObjectManager != null)
            {
                beatmapObjectManager.noteWasSpawnedEvent += OnNoteSpawned;
                beatmapObjectManager.noteWasDespawnedEvent += OnNoteDespawned;
                beatmapObjectManager.noteWasCutEvent += OnNoteCut;
                beatmapObjectManager.noteWasMissedEvent += OnNoteMissed;
            }
        }

        public void CounterDestroy()
        {
            if (beatmapObjectManager != null)
            {
                beatmapObjectManager.noteWasSpawnedEvent -= OnNoteSpawned;
                beatmapObjectManager.noteWasDespawnedEvent -= OnNoteDespawned;
                beatmapObjectManager.noteWasCutEvent -= OnNoteCut;
                beatmapObjectManager.noteWasMissedEvent -= OnNoteMissed;
            }
            trackers.Clear();
            foreach (var p in points)
            {
                if (p.img != null) Destroy(p.img.gameObject);
            }
            points.Clear();
            if (root != null) Destroy(root.gameObject);
        }

        private void Update()
        {
            if (saberManager == null || root == null) return;

            for (int i = trackers.Count - 1; i >= 0; i--)
            {
                var t = trackers[i];
                if (t == null || t.controller == null || !t.controller.isActiveAndEnabled)
                {
                    trackers.RemoveAt(i);
                    continue;
                }

                if (!t.cutPointSet)
                {
                    Vector3 mv = t.controller.moveVec;
                    if (mv.sqrMagnitude > 0.0001f)
                    {
                        t.cutPoint = t.controller.jumpStartPos + mv;
                        t.cutPointSet = true;
                        t.correctSaber = ResolveCorrectSaber(t.controller.noteData.colorType);
                    }
                }

                if (!t.cutPointSet || t.correctSaber == null) continue;

                Vector3 notePos = t.controller.noteTransform.position;
                float nd = (notePos - t.cutPoint).sqrMagnitude;
                if (nd < t.minNoteDist)
                {
                    t.minNoteDist = nd;
                    t.tPerfect = Time.time;
                }

                Vector3 sb = (t.correctSaber.saberBladeBottomPos + t.correctSaber.saberBladeTopPos) * 0.5f;
                float xy = (t.cutPoint.x - sb.x) * (t.cutPoint.x - sb.x) + (t.cutPoint.y - sb.y) * (t.cutPoint.y - sb.y);
                if (xy < t.minSaberXY)
                {
                    t.minSaberXY = xy;
                    t.tSaber = Time.time;
                }
            }

            RenderPoints();
        }

        private void OnNoteSpawned(NoteController controller)
        {
            if (controller == null) return;
            NoteData data = controller.noteData;
            if (data == null || data.colorType == ColorType.None || data.isArcTail) return;
            for (int i = 0; i < trackers.Count; i++)
            {
                if (trackers[i].controller == controller) return;
            }
            trackers.Add(new NoteTracker { controller = controller });
        }

        private void OnNoteDespawned(NoteController controller)
        {
            RemoveTracker(controller);
        }

        private void OnNoteCut(NoteController controller, in NoteCutInfo info)
        {
            RemoveTracker(controller);
            if (info.noteData == null || info.noteData.colorType == ColorType.None) return;
            if (!info.saberTypeOK) return;
            AddPoint(info.timeDeviation);
        }

        private void OnNoteMissed(NoteController controller)
        {
            NoteTracker t = null;
            for (int i = 0; i < trackers.Count; i++)
            {
                if (trackers[i].controller == controller)
                {
                    t = trackers[i];
                    trackers.RemoveAt(i);
                    break;
                }
            }
            if (t == null || !t.cutPointSet || t.correctSaber == null) return;
            if (t.minSaberXY > SaberAlignThreshold * SaberAlignThreshold) return;
            AddPoint(t.tSaber - t.tPerfect);
        }

        private void RemoveTracker(NoteController controller)
        {
            for (int i = 0; i < trackers.Count; i++)
            {
                if (trackers[i].controller == controller)
                {
                    trackers.RemoveAt(i);
                    return;
                }
            }
        }

        private Saber ResolveCorrectSaber(ColorType color)
        {
            if (saberManager == null) return null;
            if (color == ColorType.None) return null;
            SaberType expected = color == ColorType.ColorA ? SaberType.SaberA : SaberType.SaberB;
            if (saberManager.leftSaber != null && saberManager.leftSaber.saberType == expected) return saberManager.leftSaber;
            if (saberManager.rightSaber != null && saberManager.rightSaber.saberType == expected) return saberManager.rightSaber;
            return null;
        }

        private void AddPoint(float deviation)
        {
            FastSlowConfig cfg = FastSlowConfig.Instance;
            float fade = Mathf.Clamp01(cfg != null ? cfg.FadeStrength : 0f);
            int maxPoints = Mathf.Clamp(cfg != null ? cfg.PointCount : 12, 1, 64);

            for (int i = points.Count - 1; i >= 0; i--)
            {
                points[i].alpha *= 1f - fade;
                if (points[i].alpha < MinAlpha)
                {
                    if (points[i].img != null) Destroy(points[i].img.gameObject);
                    points.RemoveAt(i);
                }
            }

            average += (deviation - average) * EmaAlpha;

            Image img = MakeImage("point", 0f, PointWidth, TickHeight, Color.white);
            points.Add(new PointData { dev = deviation, alpha = 1f, img = img });

            while (points.Count > maxPoints)
            {
                var p = points[0];
                if (p.img != null) Destroy(p.img.gameObject);
                points.RemoveAt(0);
            }

            RenderPoints();
        }

        private void RenderPoints()
        {
            FastSlowConfig cfg = FastSlowConfig.Instance;
            if (cfg == null || root == null) return;

            if (centerTick != null) centerTick.color = cfg.CenterColor;
            if (leftTick != null) leftTick.color = cfg.EdgeColor;
            if (rightTick != null) rightTick.color = cfg.EdgeColor;

            float centerOffset = cfg.TimingMode == FastSlowTimingMode.MovingAverage ? average : 0f;

            foreach (var p in points)
            {
                if (p.img == null) continue;
                float dx = p.dev - centerOffset;
                float clamped = Mathf.Clamp(dx, -MaxDeviation, MaxDeviation);
                float nx = clamped / MaxDeviation * BarHalfWidth;
                p.img.rectTransform.anchoredPosition = new Vector2(nx, 0f);
                float t = Mathf.Clamp(Mathf.Abs(clamped) / MaxDeviation, 0f, 1f);
                Color c = Color.Lerp(cfg.CenterColor, cfg.EdgeColor, t);
                c.a = p.alpha;
                p.img.color = c;
                p.img.gameObject.SetActive(p.alpha > MinAlpha);
            }
        }

        private void BuildUI()
        {
            if (canvasUtility == null || settings == null) return;
            Canvas canvas = canvasUtility.GetCanvasFromID(settings.CanvasID);
            if (canvas == null) return;

            float posScale = 10f;
            var canvasSettings = canvasUtility.GetCanvasSettingsFromID(settings.CanvasID);
            if (canvasSettings != null) posScale = canvasSettings.PositionScale;
            Vector3 anchor = canvasUtility.GetAnchoredPositionFromConfig(settings);

            EnsureWhiteSprite();

            root = new GameObject("FastSlowBar").AddComponent<RectTransform>();
            root.SetParent(canvas.transform, false);
            root.anchorMin = root.anchorMax = root.pivot = Vector2.one * 0.5f;
            root.localScale = Vector3.one;
            root.anchoredPosition3D = new Vector3(anchor.x, anchor.y, anchor.z) * posScale;
            root.gameObject.layer = canvas.gameObject.layer;

            MakeImage("bg", 0f, BarHalfWidth * 2f, BarHeight, new Color(1f, 1f, 1f, 0.15f));
            centerTick = MakeImage("center", 0f, TickWidth, TickHeight, Color.white);
            leftTick = MakeImage("left", -BarHalfWidth, TickWidth, TickHeight, Color.white);
            rightTick = MakeImage("right", BarHalfWidth, TickWidth, TickHeight, Color.white);
        }

        private Image MakeImage(string name, float x, float width, float height, Color color)
        {
            var go = new GameObject(name);
            var rt = go.AddComponent<RectTransform>();
            rt.SetParent(root, false);
            rt.anchorMin = rt.anchorMax = rt.pivot = Vector2.one * 0.5f;
            rt.anchoredPosition = new Vector2(x, 0f);
            rt.sizeDelta = new Vector2(width, height);
            if (root != null) go.layer = root.gameObject.layer;
            var img = go.AddComponent<Image>();
            img.sprite = whiteSprite;
            img.type = Image.Type.Simple;
            img.color = color;
            return img;
        }

        private static void EnsureWhiteSprite()
        {
            if (whiteSprite != null) return;
            var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply(false);
            whiteSprite = Sprite.Create(tex, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
