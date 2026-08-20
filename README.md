# FastSlowCounter

[한국어](README-ko.md) | English

A custom counter addon for [Counters+](https://github.com/NuggoDEV/CountersPlus) in Beat Saber.
It draws an in-game horizontal bar that shows at a glance whether your note-hit timing was **fast (FAST)** or **slow (SLOW)**.

## Features

The bar's center = perfect timing (0 ms), the edges = miss timing. Each handled note drops a point on the bar, and older points fade out over time.

| Case | Outcome | Display |
|---|---|---|
| Correct hand + on-time | Cut | Point near center, from `timeDeviation` |
| Correct hand + early | Miss | Resolved from saber trajectory → left (FAST) edge |
| Correct hand + late | Miss | Resolved from saber trajectory → right (SLOW) edge |
| Wrong hand | Cut (wrong saber) | Not shown |

For misses, from the moment a note spawns the counter records (each frame) the time the note reaches its cut plane (`jumpStartPos + moveVec`, `tPerfect`) and the time the correct-hand saber aligns with the lane (`tSaber`). Comparing those determines fast vs slow.

## Requirements

- Beat Saber 1.40.8
- BSIPA 4.2+
- [Counters+](https://github.com/NuggoDEV/CountersPlus) 2.3+
- BeatSaberMarkupLanguage 1.12+
- SiraUtil 3.1+

## Installation

Drop the release `FastSlowCounter.dll` (and `.pdb`) into your Beat Saber `Plugins/` folder.
In the Counters+ settings menu, enable **"FastSlow Counter"** and position it.

## Configuration (Counters+ settings → FastSlow Counter)

- **Timing Mode** — `Exact`: keep the center at 0 ms / `Moving Average`: recenter the bar on your recent average timing (EMA)
- **Center Color** — color of the center marker and on-time points
- **Edge Color** — color of the edge markers and miss-timed points
- **Point Count** — max number of points shown at once (4–24)
- **Fade Strength** — how strongly older points fade each time a new point is added (0–100%, exponential decay)

## Build

Uses the .NET Framework 4.8.1 targeting pack and MSBuild from Visual Studio 2022 Build Tools:

```powershell
& "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe" FastSlowCounter.csproj -t:Restore
& "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe" FastSlowCounter.csproj -t:Build -p:Configuration=Release
```

Set your Beat Saber install path via `BeatSaberDir` in `FastSlowCounter.csproj.user`.

## Tuning / Notes

Constants at the top of `FastSlowCounterController.cs` (tweak after in-game testing):

- `BarHalfWidth` — half-width of the bar (local units)
- `MaxDeviation` — timing deviation the bar edges represent, in seconds (default 0.15)
- `SaberAlignThreshold` — threshold to consider the saber aligned with the lane for miss detection (default 0.5)
- `EmaAlpha` — smoothing factor for Moving Average

Sign convention: `timeDeviation < 0` (early) is treated as left (FAST). If it appears reversed in-game, just flip the sign.

## License

MIT
