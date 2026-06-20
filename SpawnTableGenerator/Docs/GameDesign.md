# 게임 디자인 문서 (Living Document)

탑다운 전술 생존 게임. 플레이어는 맵 위에서 몬스터 웨이브를 처리하며 목표를 달성한다.
이 문서는 게임의 모든 시스템 기획을 기록하고, 구현 진행 상황을 추적한다.

> **룰**: 기획이 바뀌면 이 문서를 먼저 수정하고, 코드는 그다음에 따라간다.
> 구현 완료된 항목은 ✅, 진행 중은 🔧, 미착수는 📋, 보류/제거는 ❌로 표시.

---

## 1. 맵 시스템

### 1.1 맵 생성
- ✅ `MapGenerator` — 평면 바닥 + 경계 벽 + 절차적 장애물
- ✅ NavMesh 자동 베이크
- ✅ `SpawnDirector.mapHalfSize` — 스폰 위치 맵 경계 클램핑

### 1.2 맵-포그 크기 연동
- 🔧 `MapGenerator.Generate()` 시 `VolumetricFog` 볼륨 크기를 맵 크기에 맞게 자동 동기화
  - 포그 GO를 씬에서 찾아 `transform.localScale = (width, fogHeight, length)`
  - `fogOfWarSize = (width, 0, length)`
  - `SpawnDirector.mapHalfSize = Mathf.Max(width, length) * 0.5f`

---

## 2. 시야 / 안개 시스템

> ⏸️ **현재 볼류메트릭 안개(FoW) 비활성** — `VolumetricFog2` 사용을 일시 보류했다. 씬에서 `VolumetricFogManager`/`VolumetricFog_FoW` GO는 inactive, `FogReveal`/`FogMapSync`/`FogDayNightResponder` 컴포넌트는 disable. 맵을 또렷하게 보기 위한 조치이며 컴포넌트만 꺼서 되돌리기 쉽다. 아래 2.1은 보류된 구현의 기록.

### 2.1 현재 상태 (VolumetricFog2 FoW)
- ✅ `VolumetricFogAndMist2` 볼류메트릭 포그 씬에 배치
- ✅ `FogReveal.cs` — 이동 임계값(0.5u) 기반 업데이트, Awake에서 초기화
- ✅ `fogOfWarBlur=true`, `fogOfWarTextureWidth=512` (복구 아티팩트 감소)
- ✅ 포그 albedo=어두운 청회색(0.04,0.04,0.08), density=2.5 → 진한 어둠 연출
- ✅ 도메인 리로드 후 NullReferenceException 수정 (Awake에서 FogOfWarInit 강제 실행)
- ✅ `FogMapSync.cs` — `MapGenerator.onGenerated` 구독, fogOfWarSize 자동 동기화
- ✅ `MapGenerator.fogVolumeTransform` + `spawnDirector` 씬에 연결

### 2.2 남은 시각 이슈
- 복구 패턴이 원형 마스크여서 벽 차단(시야 폴리곤) 없음 → §2.3 장기 계획
- 현재는 원형 시야로 충분하며 재생 중 오류 없음

### 2.3 장기 계획 — 시야각 + 벽 차단 시스템
- 📋 플레이어에서 N개 레이(Physics.Raycast)를 쏘아 벽에 가로막힌 시야 폴리곤 생성
  - 참조: "2D visibility / shadow casting" 알고리즘 (Unity PolygonCollider2D나 메시 기반)
  - 단, 이 프로젝트는 3D 탑다운 → XZ 평면 레이캐스트 + 결과를 메시로 마스킹
- 📋 벽 너머는 어두운 안개, 이미 본 곳은 반투명 안개(기억 영역), 현재 시야는 완전 밝음
- ✅ 시야 범위: 낮=24, 밤=10 (§3.2 낮/밤 연동) — `FogDayNightResponder`가 `FogReveal.revealRadius` 조절 (※ 원형 마스크 기준. 벽 차단 폴리곤은 위 항목대로 장기 과제)

---

## 3. 낮/밤 사이클

### 3.1 기획 의도
- 하루를 게임 내 시간 단위로 순환 (예: 1 사이클 = 5분 실시간)
- 낮: 밝고 시야가 넓음 → 플레이어 유리
- 밤: 어둡고 시야 좁음 + 몬스터 활동량 증가 → 긴장도 상승

### 3.2 구현 (✅ 완료 — Observer + 순수 시임, [Architecture.md](Architecture.md) §3)
| 항목 | 낮 값 | 밤 값 | 컴포넌트 (반응자) |
|------|-------|-------|---------|
| Directional Light 강도 | 1.0 | 0.05 | `SunLightResponder` → `Light.intensity` |
| Directional Light 색온도 | 5500K (흰/노랑) | 2800K (주황/파랑) | `SunLightResponder` → `Light.colorTemperature` |
| **태양 각도(회전)** | 정오 머리 위 | 자정 지평선 아래 | `SunLightResponder` → `transform.rotation`(하루 X축 1회전, 그림자 각도 변화) |
| **맵 색조(색 필터)** | 중성/밝음 | 청색/어둠/탈색 | `PostFxDayNightResponder` → URP `ColorAdjustments`(filter·exposure·saturation) |
| Ambient Light | 밝은 회색 | 매우 어두운 남색 | `AmbientLightResponder` → `RenderSettings.ambientLight` |
| 시야 반경 | 24 | 10 | `FogDayNightResponder` → `FogReveal.revealRadius` *(현재 비활성 — §2 참고)* |
| 스폰 간격 | 정상 | 50% 단축 | `SpawnRateResponder` → `SpawnDirector.spawnIntervalScale` |

- ✅ `DayNightModel` (순수 static 시임): `Evaluate(normalizedTime, DayNightConfig) → DayNightState`. 씬/시간 무관 → EditMode 테스트(`DayNightModelTests`). 태양 회전도 순수 함수 `SunRotation(t, yaw)`로 분리(정오 수직·일출/일몰 수평·자정 지평선 아래).
- ✅ `DayNightController` MonoBehaviour: `cycleSeconds = 300f`(5분), `[0,1]` normalized time을 매 프레임 진행시켜 모델로 상태 계산 후 등록 반응자에 푸시. 새벽/황혼은 `Daylight01`(코사인 곡선)로 부드럽게 lerp.
- ✅ `IDayNightResponder` + `DayNightResponderBehaviour`(자가 등록): 새 반응 추가 시 컨트롤러 수정 불필요(OCP). 태양 회전·맵 색조가 그 예 — 색조는 URP 의존이라 Assembly-CSharp의 `PostFxDayNightResponder`로 경계 넘어 추가.

### 3.3 분위기 포인트 라이팅 (✅ 완료 — `AtmosphereLightPlacer`)
- ✅ 맵 장애물 일부 위에 따뜻한 포인트 라이트를 배치(`MapGenerator.onGenerated` 구독 + 사전 생성 맵용 Start() 1회 배치)
  - 개수: `Mathf.RoundToInt(obstacleCount * 0.2f)` (장애물의 20%, seed 고정 셔플)
  - 강도: 0–2.5(`Darkness01` 비례), 범위: 6–10, 색: 따뜻한 주황 (#FF8A3D)
- ✅ 낮에는 끔(`enabled=false`, darkness≤0.02), 밤일수록 밝게 — `IDayNightResponder`로 낮/밤 연동

---

## 4. 전투 시스템

### 4.1 플레이어 무기
- ✅ `WeaponSO` abstract 베이스 → `MeleeWeaponSO` / `RangedWeaponSO` 서브클래스
- ✅ 1키/2키: 무기 슬롯 스왑 (근접 ↔ 원거리)
- ✅ 좌클릭 홀드: 현재 슬롯 무기 연속 발사

| 슬롯 | 무기 | 동작 | 특성 |
|------|------|------|------|
| 1 (근접) | WP_Melee | 120° 부채꼴 AoE | damage=40, Piercing, cd=0.45s, radius=4.5 |
| 2 (원거리) | WP_Ranged | 레이저 투사체 | damage=14, Normal, cd=0.22s, speed=75, range=40 |

- ✅ 무기 교체 UI (현재 슬롯 HUD 표시) — MVP `WeaponSlotView` (좌하단 `무기 [n] 이름`), [Architecture.md](Architecture.md) §4

### 4.2 LaserProjectile
- ✅ `Rigidbody(kinematic) + SphereCollider(trigger)` 물리 피격 판정
- ✅ 벽·장애물 충돌 시 소멸
- ✅ 몬스터 타격 시 데미지 + 소멸
- ✅ LineRenderer 시각 효과
- ✅ **프리팹 기반 발사** — `Assets/Prefabs/Combat/LaserProjectile.prefab`(Rigidbody+SphereCollider+Point Light+`LaserProjectile`). `RangedWeaponSO.projectilePrefab`에 연결(`WP_Ranged`). `LaserProjectile.Spawn(prefab, …)`이 인스턴스화하며, 프리팹이 비면 코드 생성으로 폴백.
- ✅ **글로우 Point Light** — 탄에 부착된 라이트가 탄 색(HDR은 최대 채널로 정규화)으로 빛난다. `ConfigureGlow`가 프리팹 라이트를 쓰거나 없으면 런타임 부착.
- ✅ **Muzzle에서 발사** — 플레이어 얼굴의 `Muzzle` 트랜스폼에서 발사. `PlayerCombat.muzzle`(비우면 `transform.Find("Muzzle")` 자동 탐색), 없으면 몸통 위쪽 폴백.

### 4.3 몬스터 근접 공격
- ✅ `MonsterAttack.cs` — `AttackKind.Melee` 처리
- ✅ 부채꼴 AoE + 플레이어 넉백
- ✅ 빨간 MeleeArcVfx 시각 효과

### 4.4 몬스터 원거리 공격
- ✅ `MonsterAttack.cs` — `AttackKind.Projectile` 처리
- ✅ `AP_RangedShot` 프로필: damage=5, range=18, cd=2s, speed=18
- ✅ `MD_Ranged_Small`에 `AP_RangedShot` 연결됨
- ✅ `LaserProjectile.SpawnRaw(hitsPlayer:true)` — 플레이어 피격

### 4.5 피격 판정 / 방어
- ✅ `Health.TakeDamage(damage, DamageType)`
- ✅ `DamageResolver` — 중장갑은 Piercing/WeakPoint만 통과
- ✅ 플레이어 HP/피격 UI — MVP `HealthBarView`(좌하단 채움 바 + `HP cur/max`, 색 Low(빨강)→High(초록) lerp). `Health.Changed` 이벤트 → `HealthPresenter` → View, [Architecture.md](Architecture.md) §4

---

## 5. 몬스터 로스터

자세한 내용은 [Monsters.md](Monsters.md) 참조.

| 이름 | 종류 | 상태 |
|------|------|------|
| MD_Melee_Small | 소형 근접 | ✅ |
| MD_Melee_Medium | 중형 근접 | ✅ |
| MD_Melee_LargeHeavy | 대형 중장갑 | ✅ |
| MD_Melee_SmallJumper | 소형 도약 | ✅ |
| MD_Melee_MediumBurrower | 중형 잠복 | ✅ |
| MD_Ranged_Small | 소형 원거리 | ✅ |
| MD_Ranged_MediumExplosive | 중형 폭발 | ✅ |
| MD_Ranged_LargeArtillery | 대형 포대 | ✅ |

---

## 6. 스폰 시스템

자세한 내용은 [SpawnSystem-Design.md](SpawnSystem-Design.md) 참조.

- ✅ `SpawnDirector` — 긴장도 기반 연속 스폰
- ✅ `SpawnTable ST_Sample` — 근접 + 원거리 혼합 웨이브
- ✅ 맵 경계 클램핑 (`mapHalfSize=42`)
- ✅ 원거리 몬스터(MD_Ranged_Small) 포함

---

## 7. 이슈 트래킹

| # | 이슈 | 상태 | 메모 |
|---|------|------|------|
| 1 | VolumetricFog2 FoW 복구 아티팩트 | ✅ 해결 | blur+임계값+고해상도로 개선 |
| 2 | 맵 크기 변경 시 포그 볼륨 자동 동기화 안됨 | ✅ 해결 | MapGenerator.onGenerated + FogMapSync |
| 3 | FoW 도메인 리로드 후 NullReferenceException | ✅ 해결 | FogReveal.Awake()에서 강제 FogOfWarInit |
| 4 | SpawnDirector 백그라운드 저FPS (에디터 비포커스) | 📋 참고 | 에디터 포커스 시 정상. 실제 빌드는 문제 없음 |
| 5 | 플레이어 HP UI 없음 | ✅ 해결 | MVP HUD(HP/무기/시계) + 낮/밤 사이클 동시 구현. [Architecture.md](Architecture.md) |
| 6 | FoW 벽 차단 시야 없음 (원형만) | 📋 장기 | §2.3 장기 계획 — 레이캐스트 시야 폴리곤 |
