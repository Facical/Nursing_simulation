# 처치실 Environment Map

작성일: 2026-05-21  
대상 씬: `NursingSimulation/Assets/_Project/Scenes/Simulation_IMInjection.unity`  
좌표 기준: Unity world space, `position/bounds center/bounds size`는 `(x,y,z)` 단위 meter 기준으로 기록한다.

## 목적

`Simulation_IMInjection` 처치실은 외부 room mesh가 `Room/Main` 아래의 `Main_*` 파트로 분할되어 있어 이름만 보고 용도를 알기 어렵다. 핸드 시뮬레이션, 카메라, 물품 배치 작업 때 매번 Scene View에서 다시 찾지 않도록, 현재 확인된 주요 파트와 좌표를 기준표로 둔다.

## 씬 루트 구조

| Root | 용도 | Bounds center / size | 비고 |
|---|---|---:|---|
| `Room` | 처치실 환경 전체 | `(3.85,2.01,-2.36)` / `(18.60,6.55,40.21)` | 실제 환경 mesh. 하위 `Main`에 `Main_*` 파트가 있음 |
| `Patient_Placeholder` | 환자 placeholder | `(-1.82,0.96,1.33)` / `(1.39,0.46,1.85)` | 침대 위 환자 |
| `Tray_Placeholder` | 기존 tray/tools placeholder | `(1.99,-0.44,-3.78)` / `(1.34,2.72,9.80)` | bounds가 넓게 잡힘. 세부 물품은 별도 검증 필요 |
| `HandSanitizerPump` | 손소독제 펌프 | `(0.48,1.05,-2.66)` / `(0.11,0.26,0.11)` | `Main_63m_0` 처치 테이블 위 |
| `FaucetWaterZone` | 수도꼭지 물 접촉/이펙트 zone | `(1.15,1.38,-2.71)` / radius `0.24` | 손이 stream 가까이에 오면 물줄기/파티클 표시 |
| `FirstPersonHandCameraRig` | 손 조작용 1인칭 rig | `(1.14,2.02,-0.92)` | `LeftShoulderAnchor`, `RightShoulderAnchor` 포함. 손위생 step 시작 시에만 Main Camera에 적용 |
| `PlayerHand_Left` | 왼팔/손 rig | `(1.38,1.76,-0.89)` | parent: `LeftShoulderAnchor` |
| `PlayerHand_Right` | 오른팔/손 rig | `(0.90,1.76,-0.90)` | parent: `RightShoulderAnchor` |
| `SharpsContainer` | 폐기통 placeholder | `(1.80,0.30,0.00)` / `(0.15,0.21,0.15)` | 폐기 step 확장 후보 |

## 손위생 기준 영역

| Object | 판정된 용도 | Bounds center / size | 작업 시 기준 |
|---|---|---:|---|
| `Main_23m_0` | 세면대 | `(1.15,0.85,-2.71)` / `(0.64,0.88,0.60)` | 사용자 확인 기준 세면대 |
| `Main_63m_0` | 세면대 옆 처치 테이블 | `(-0.64,0.47,-2.63)` / `(2.57,0.89,0.64)` | 손소독제 배치 기준 |
| `HandSanitizerPump` | 손소독제 | `(0.48,1.05,-2.66)` / `(0.11,0.26,0.11)` | 테이블 상단에 놓인 pump. root rotation `(270,0,0)` 유지 |
| `FaucetWaterZone` | 수도꼭지 물줄기 | start `(1.15,1.38,-2.71)`, end `(1.15,0.81,-2.71)` | `FaucetWaterInteraction`; active hand palm이 radius `0.24` 안에 들어오면 물 이펙트와 물 접촉 이벤트 발생 |
| `FirstPersonHandCameraRig` | 손위생 시점 | pos `(1.14,2.02,-0.92)` | Scene View pose 캡처 기준. Play 시작 시 자동 적용하지 않음 |

손소독제 자동 배치 규칙:
- table: `Main_63m_0`
- sink: `Main_23m_0`
- pump x: table의 sink 방향 끝에서 안쪽으로 약 `0.16`
- pump z: sink 중심 z를 table 범위 안으로 clamp
- pump y: table bounds max.y + pump bottom 보정
- pump rotation: `(270,0,0)`

## 침대/환자 영역

| Object | 판정된 용도 | Bounds center / size | 비고 |
|---|---|---:|---|
| `bed_frame` | 침대 프레임 | `(-0.02,0.39,1.25)` / `(4.89,0.40,2.61)` | Room/Main mesh |
| `bed_frame2` | 침대 프레임/상부 | `(-0.02,0.56,1.25)` / `(5.00,1.08,2.63)` | Room/Main mesh |
| `bed_sheet` | 침대 시트 | `(-0.02,0.66,1.27)` / `(4.86,0.21,2.55)` | 로그 기준 이름은 `bed_sheet` |
| `pillow` | 베개 | `(-0.05,0.79,2.02)` / `(4.36,0.20,0.50)` | 침대 위 |
| `Patient_Placeholder` | 환자 | `(-1.82,0.96,1.33)` / `(1.39,0.46,1.85)` | 랜드마크/주사 부위 작업 기준 |

## 주요 Room/Main 파트 후보

아래 파트는 직접 용도 확정이 필요한 mesh 조각이다. 작업 전 Scene View에서 한 번만 시각 확인한 뒤 이 문서를 갱신한다.

| Object | 추정/관찰 | Bounds center / size |
|---|---|---:|
| `Main_4m_0` | 바닥 또는 큰 수평면 | `(2.50,0.02,-2.69)` / `(15.06,0.01,33.76)` |
| `Main_1m_0` | 천장 또는 상부 수평면 | `(3.30,4.46,-2.65)` / `(15.59,0.01,33.86)` |
| `Main_2m_0` | 큰 벽/외곽 구조 | `(8.52,2.01,-2.36)` / `(9.27,6.55,40.21)` |
| `Main_5m_0` | 좌측/후면 큰 벽 또는 캐비닛군 | `(-0.62,1.84,0.02)` / `(9.58,2.87,5.44)` |
| `Main_35m_0` | 침대 후면 벽장/패널군 | `(1.03,1.21,0.97)` / `(7.04,1.02,3.53)` |
| `Main_68m_0` | 긴 벽/환경 구조 | `(0.67,1.18,-4.12)` / `(10.46,1.52,24.31)` |
| `Main_62m_0` | 세면대 주변 수납/기기 후보 | `(-2.51,0.53,-2.42)` / `(0.47,1.03,0.63)` |
| `Main_51m_0` | 테이블 위 소품 후보 | `(0.37,0.96,-2.74)` / `(0.21,0.28,0.20)` |
| `Main_48m_0` | 테이블 위 소품 후보 | `(0.36,1.19,-2.70)` / `(0.19,0.25,0.25)` |
| `Main_32m_0` | 테이블 위 소품 후보 | `(0.07,1.02,-2.66)` / `(0.24,0.39,0.17)` |
| `Main_31m_0` | 테이블 위 소품 후보 | `(-0.24,0.89,-2.58)` / `(0.37,0.13,0.22)` |

## 핸드/카메라 리그 기준

| Object | 위치 | 부모 | 용도 |
|---|---:|---|---|
| `FirstPersonHandCameraRig` | `(1.22,1.63,-1.88)` | root | 손위생용 1인칭 rig |
| `LeftShoulderAnchor` | rig 하위 | `FirstPersonHandCameraRig` | 왼쪽 어깨 고정점 |
| `RightShoulderAnchor` | rig 하위 | `FirstPersonHandCameraRig` | 오른쪽 어깨 고정점 |
| `PlayerHand_Left` | `(1.57,1.12,-2.31)` | `LeftShoulderAnchor` | 왼팔 shoulder-root |
| `PlayerHand_Right` | `(0.89,1.12,-2.32)` | `RightShoulderAnchor` | 오른팔 shoulder-root |

주의:
- 손/팔 이동은 `PlayerHand_*` root가 아니라 각 prefab 내부 `HandTarget`을 움직이는 방식이어야 한다.
- `PlayerHand_*` root는 shoulder anchor로 간주하고, 손위생/도구 조작 중 world position이 흔들리면 안 된다.
- 손위생 카메라는 `HandHygieneAnimator.Begin()`에서 Main Camera pose를 `FirstPersonHandCameraRig.CameraPose`로 옮기고, `End()`에서 복구한다.
- `LeftShoulderAnchor`/`RightShoulderAnchor` 기본 local offset은 각각 `(-0.34,-0.26,0.62)`, `(0.34,-0.26,0.62)`이다. 팔이 GameView 아래에 안 보이면 z를 키우고, 너무 아래면 y를 0에 가깝게 올린다.
- 손위생 카메라 위치가 어긋나면 Scene View를 원하는 구도로 맞춘 뒤 `Tools > Nursing Sim > Phase 3 > 2. Set Hand Hygiene Camera From Scene View`를 실행해 현재 Scene View pose를 저장한다.
- Step 2 물·비누 손위생은 `HandSanitizerPump` 2회 입력 후 `FaucetWaterZone` 접촉이 있어야 비비기 시간이 카운트된다.

## 후속 작업 규칙

- 새 물품을 처치대에 둘 때는 우선 `Main_63m_0` bounds를 기준으로 배치한다.
- 세면대와 관련된 상호작용은 `Main_23m_0` bounds를 기준으로 한다.
- 환자/주사 부위는 `Patient_Placeholder`와 침대 bounds를 기준으로 별도 landmark marker를 만든다.
- 폐기 step은 `SharpsContainer` root를 먼저 사용하고, 감염성/일반 폐기통이 필요하면 별도 prefab으로 분리한다.
- `Main_*` 이름의 의미를 새로 확인하면 이 문서의 “주요 Room/Main 파트 후보”를 갱신한다.
