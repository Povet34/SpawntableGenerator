# 가용 에셋 카탈로그

프로젝트에 임포트된 서드파티 에셋 = **언제든 쓸 수 있는 수단**. 새 기능 만들 때 직접 짜기 전에 여기 먼저 본다.
(추가될 때마다 갱신.)

---

## 🗡️ VFX — Stylized Slash (`Assets/slash5-HungNguyen/`)
**"Stylizer Slash" (HungNguyen).** VFX Graph 기반 슬래시 이펙트. 근접 공격 연출용.

- **프리팹**(인스턴스화해서 재생): `prefab/slash/` 에 색상 5종 —
  `slash-green bolder`, `white-blue bolder`, `white-red bolder`, `white-black bolder`, `white-yellow bolder`.
- **VFX 프로파일**: `slash 5 visual vfx profiles/*.vfx` (색상별 `.vfx`).
- **셰이더/모델**: `slash shader/slash5.shadergraph`, `3D model/slash3.fbx`. 머티리얼 `material/`.
- 샘플 씬: `sample scene-slash5-HungNguyen.unity`. 설명: `Read me Stylizer Slash HungNguyen.pdf`.

**쓸 곳 / 아이디어**
- **플레이어 근접(1키)**: 공격 시 슬래시 프리팹을 공격 위치에 Instantiate → 조준 방향으로 회전 → 왼→오 휘두름. (지금의 발광 링 대체.) 녹색/파랑 추천.
- **적 근접**: 빨강 슬래시. 부채꼴 데미지 판정(80~130°)과 함께 연출.
- 프리팹은 VFX Graph라 재생 후 자동 종료. 풀링하거나 잠깐 띄우고 Destroy.
- 의존: `com.unity.visualeffectgraph`(설치됨).

## 🌫️ Fog / Fog of War — Volumetric Fog 2 (`Assets/VolumetricFog2/`)
**Kronnect "Volumetric Fog 2" (URP).** 볼류메트릭 포그 + **Fog of War** 내장.

- **핵심 컴포넌트**: `VolumetricFog`(Scripts/VolumetricFog.cs). FoW 부분은 `VolumetricFog.FoW.cs`.
- **Fog of War API**:
  - `enableFogOfWar` (bool) 켜기.
  - `SetFogOfWarAlpha(Vector3 worldPos, float radius, float alpha, ...)` — 한 지점 반경의 안개 알파 설정(alpha 0 = 걷힘/시야 확보).
  - `ResetFogOfWar(alpha=1)`, `GetFogOfWarAlpha(pos)`.
- **프리팹**: `Resources/Prefabs/FogVolume2D`, `FogSubVolume`. **프로파일 프리셋**: `Demo/Presets/` (Mist, Heavy Fog, Foggy Lake, Windy Mist 등).
- **렌더 피처**: `VolumetricFogRenderFeature` 를 URP Renderer 에 추가해야 보임.
- **데모**: `Demo/DemoFogOfWar/DemoFogOfWar.unity` + 헬퍼 스크립트
  `Demo/Scripts/ClearFogOfWarUnderGameObject.cs`(GO 아래 자동 걷힘 — **플레이어에 붙이면 바로 FoW**),
  `ClearFogOfWarInsideCollider.cs`, `ClearFogOfWarInsideBounds.cs`.

**쓸 곳 / 아이디어**
- **Fog of War**: FogVolume 깔고 `enableFogOfWar`, 매 프레임 `SetFogOfWarAlpha(player.pos, revealRadius, 0)` 로 플레이어 주변만 시야 확보. (또는 `ClearFogOfWarUnderGameObject` 를 플레이어에 부착.) 안 보이는 곳에서 스폰(설계 §7)·증원과 궁합 좋음 — 정찰/스텔스 감각.
- **분위기**: Mist/Heavy Fog 프리셋으로 탑다운 무드.
- 의존: URP Renderer 에 RenderFeature 등록 + FogManager.

## ⚔️ Sword Slash VFX PRO — Hovl Studio (`Assets/Hovl Studio/Sword slash VFX/`)
**Hovl Studio "Sword Slash VFX".** 파티클 기반 검 슬래시 이펙트. 프리팹 즉시 사용 가능.

- **프리팹 목록** (`Prefabs/`):
  - `Sword Slash 1~17` — 다양한 방향·색상 단순 슬래시 (17종).
  - `Sword Slash Combo 1~9` — 연속 콤보 슬래시 (9종).
  - `Prick 1~5` — 찌르기 이펙트 (5종).
  - `Slash wave`, `Spatial section`, `Spikes attack` — 특수 효과.
  - `Sword Slash mirror` — 좌우 미러 슬래시.
- 파티클 시스템 기반 — VFX Graph 아님. `Instantiate` 후 자동 재생·종료. `Destroy(go, 3f)` 로 정리.

**쓸 곳 / 아이디어**
- **플레이어 근접(1키)**: `Sword Slash 1~3` 또는 `Combo` 프리팹 → WeaponSO.slashVfxPrefab 에 할당 → `PlayerCombat.FireMelee()` 에서 자동 스폰.
- **적 근접**: `Sword Slash 5~8` (더 거친 느낌) → MonsterAttack 에서 별도 스폰 가능.
- 색상 커스터마이징: 프리팹 복제 후 파티클 색 변경.

## ⚙️ Infrastructure — Roslyn (`Assets/Plugins/Roslyn/`)
런타임/에디터 C# 컴파일러 DLL. MCP `execute_code`(in-editor C# 실행)·동적 컴파일 인프라. **게임 에셋 아님** — 건드릴 일 없음.

---

## 직접 만든 것 (참고)
- **빔 셰이더** `Assets/Shaders/Beam.shader` (URP unlit, HDR 코어+프레넬, 가산) — 광선검/블래스터 빔. Bloom(Volume) 과 함께 발광.
- (있음) URP Bloom 용 `Assets/GameData/PostFX.asset` Volume Profile + 씬 `GlobalVolume`.

## 메모 (다음에 이런 거 쓰면 될 것)
- 근접 슬래시 → **Stylized Slash 프리팹**(직접 만든 링/부채꼴 메시 대신 연출만 교체).
- 시야/스텔스/증원 연출 → **VolumetricFog2 FoW**.
- 레이저 총알(투사체) → 직접 만든 **Beam 셰이더** + LineRenderer 트레일.
- ❌ "Sword Slashes PRO"는 현재 프로젝트에 **없음**(Stylized Slash 만 임포트됨). 필요하면 임포트 후 여기 추가.
