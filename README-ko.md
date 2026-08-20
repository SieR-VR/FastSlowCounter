# FastSlowCounter

한국어 | [English](README.md)

Beat Saber의 [Counters+](https://github.com/NuggoDEV/CountersPlus) 커스텀 카운터 애드온입니다.
노트를 처리한 타이밍이 **빨랐는지(FAST)** 느렸는지(SLOW) 한눈에 보여주는 가로 막대를 게임 내에 표시합니다.

## 기능

막대의 중앙 = 완벽한 타이밍(0ms), 양 끝 = 미스 타이밍. 각 노트 처리 시 점이 찍히고, 과거 점은 서서히 사라집니다.

| 케이스 | 처리 | 표시 |
|---|---|---|
| 정확한 손 + 정확한 타이밍 | 컷 | `timeDeviation` 기준 중앙 근처에 점 |
| 정확한 손 + 빠른 타이밍 | 미스 | 세이버 궤적 기록으로 판정 → 왼쪽(FAST) 가장자리 |
| 정확한 손 + 느린 타이밍 | 미스 | 세이버 궤적 기록으로 판정 → 오른쪽(SLOW) 가장자리 |
| 어긋난 손 | 컷(잘못된 세이버) | 표시 안 함 |

미스 판정은 노트가 스폰된 순간부터 컷 평면(`jumpStartPos + moveVec`) 도달 시점(tPerfect)과 올바른 손 세이버가 레인에 정렬된 시점(tSaber)을 매 프레임 기록한 뒤 비교해 빠름/느림을 결정합니다.

## 요구 사항

- Beat Saber 1.40.8
- BSIPA 4.2+
- [Counters+](https://github.com/NuggoDEV/CountersPlus) 2.3+
- BeatSaberMarkupLanguage 1.12+
- SiraUtil 3.1+

## 설치

릴리스 `FastSlowCounter.dll`(및 `.pdb`)을 Beat Saber 설치 폴더의 `Plugins/`에 넣습니다.
Counters+ 설정 메뉴에서 **"FastSlow Counter"** 를 켜고 위치를 지정합니다.

## 설정 (Counters+ 설정 → FastSlow Counter)

- **Timing Mode** — `Exact`: 중앙을 0ms에 고정 / `Moving Average`: 최근 평균 타이밍(EMA)으로 중앙 이동
- **Center Color** — 중앙 마커·타이밍이 맞은 점의 색
- **Edge Color** — 양 끝 마커·미스 점의 색
- **Point Count** — 동시에 표시할 최대 점 수(4–24)
- **Fade Strength** — 새 점이 추가될 때 기존 점이 감쇠하는 세기(0–100%, 지수 감쇠)

## 빌드

.NET Framework 4.8.1 타겟팅팩과 Visual Studio 2022 Build Tools의 MSBuild 사용:

```powershell
& "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe" FastSlowCounter.csproj -t:Restore
& "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe" FastSlowCounter.csproj -t:Build -p:Configuration=Release
```

Beat Saber 설치 경로는 `FastSlowCounter.csproj.user`의 `BeatSaberDir`로 지정합니다.

## 튜닝 / 참고

`FastSlowCounterController.cs` 상단 상수(인게임 테스트 후 조정 가능):

- `BarHalfWidth` — 막대 반폭(로컬 단위)
- `MaxDeviation` — 막대 양 끝이 나타내는 타이밍 편차(초, 기본 0.15)
- `SaberAlignThreshold` — 미스 판정 시 세이버가 레인에 접근한 것으로 볼 임계(기본 0.5)
- `EmaAlpha` — Moving Average 평활 계수

부호 규칙: `timeDeviation < 0`(빠름)을 왼쪽(FAST)으로 가정했습니다. 인게임에서 반대로 나타나면 부호만 뒤집으면 됩니다.

## 라이선스

MIT
