# 03. 기술 명세

## 1. 엔진 및 파이프라인

| 항목 | 선택 | 근거 |
|---|---|---|
| Unity | **Unity 6 LTS** (대안: 2022.3 LTS) | 최신 URP·Shader Graph 기능, 장기 지원. (버전 선택 상세는 이 문서 말미 §10) |
| Render Pipeline | **URP (Universal RP)** | Semi-realistic + 포토리얼 도구 혼합 아트 방향에 적합. HDRP는 과함 |
| Color Space | Linear | PBR 전제 |
| Scripting Backend | IL2CPP (빌드) / Mono (개발) | 빌드는 최적화, 개발은 컴파일 속도 |
| .NET | .NET Standard 2.1 |

## 1.1 아트 방향 (결정됨)

- **인물(환자·의료진)·환경**: Semi-realistic. 1차 에셋 원천은 [Microsoft Rocketbox (MIT)](https://github.com/microsoft/Microsoft-Rocketbox). 자세한 임포트 절차는 `docs/06-asset-plan.md §2.2`.
- **의료 도구**(주사기·바이알·바늘·알콜솜 등): **포토리얼 지향**. 학생이 클로즈업으로 가장 오래 보는 대상이며, 라벨·눈금·게이지 색상이 학습에 직결되므로 품질 투자 집중.
- **얼굴 클로즈업 회피**: Rocketbox 얼굴 디테일은 현대 포토리얼 대비 약하므로, 환자 얼굴 클로즈업 컷을 카메라 연출에서 제외. 상호작용 줌은 의료 도구·주사 부위 중심으로.
- **왜 이 방향**: (1) 학습 몰입에는 semi-realistic로 충분, (2) uncanny valley 회피, (3) 에셋 비용 0원, (4) 포토리얼 캐릭터 개발 리스크(R1/R5) 제거.

## 2. 핵심 패키지

| 패키지 | 용도 | 필수/선택 |
|---|---|:---:|
| `com.unity.inputsystem` (New Input System) | 마우스·키보드 입력 | 필수 |
| `com.unity.textmeshpro` | 한글 폰트 렌더링 (동적 Atlas 또는 사전 생성) | 필수 |
| `com.unity.cinemachine` | 상호작용 줌, 카메라 전환 | 필수 |
| `com.unity.render-pipelines.universal` | URP | 필수 |
| `com.unity.postprocessing` (URP 내장) | 포스트 이펙트 (Bloom, DoF, Color) | 필수 |
| `com.unity.addressables` | 에셋 지연 로딩 (Phase 3 이후) | 선택 |
| `com.unity.localization` | i18n 구조만 준비 (한국어 전용이지만 확장성 위해) | 선택 |
| `DOTween` (에셋스토어) | 트윈·시퀀스 애니메이션 | 선택 (권장) |
| `Odin Inspector` (유료) | 복잡한 SO 에디터 편집성 | 선택 |

## 3. 아키텍처 개요

```
┌─────────────────────────────────────────────────────┐
│                  Presentation                       │
│  Scenes, Prefabs, UI(uGUI), Cinemachine             │
└───────────▲─────────────────────▲───────────────────┘
            │ events              │ commands
┌───────────┴─────────────────────┴───────────────────┐
│                   Runtime                           │
│  ScenarioRunner (FSM)  InteractionManager           │
│  FeedbackBus (Event Ch) ScoreTracker                │
│  DebriefingBuilder  SaveService (JSON)              │
└───────────▲─────────────────────▲───────────────────┘
            │ reads               │
┌───────────┴─────────────────────┴───────────────────┐
│                 Data (ScriptableObject)             │
│  NursingScenario │ ScenarioStep (추상)              │
│   ├ ChecklistStep   ├ ToolInteractionStep           │
│   ├ DialogueStep    ├ LandmarkPickStep              │
│   └ SelectionStep   └ SequenceStep                  │
└─────────────────────────────────────────────────────┘
```

### 3.1 시나리오 런타임 (ScenarioRunner)
- 상태: `Idle → StepActive → StepEvaluating → NextStep / StepActive(재시도) → Completed`
- 한 번에 하나의 `ScenarioStep`이 활성. 현재 스텝의 `IStepController` 구현체가 인스턴스화되어 씬에서 상호작용을 받는다.
- 스텝 종료 시 `StepResult`(pass/partial/fail + 감점 사유 리스트)를 `FeedbackBus`에 발행.

### 3.2 상호작용 (InteractionManager)
- 카메라에서 `Physics.Raycast`로 `IInteractable` 레이어를 질의.
- 하이라이트: URP `Renderer Feature`의 Outline 셰이더 또는 `QuickOutline` 에셋.
- 드래그: 첫 클릭 지점에서 카메라 평면 투영, 매 프레임 위치 업데이트, 릴리즈 시 단계 검증 이벤트.

### 3.3 이벤트 채널 (FeedbackBus)
- `ScriptableObject` 기반 이벤트 채널 패턴 (Unity Open Project 방식).
- 이벤트 타입: `StepStarted`, `StepProgress(float 0..1)`, `StepCompleted(StepResult)`, `InstantFeedback(FeedbackKind, string msg)`, `ScoreChanged(int)`, `ScenarioCompleted(DebriefingReport)`.
- UI·사운드·애니메이션 컴포넌트가 각자 구독 — 결합도 낮춤.

### 3.4 저장 (SaveService)
- `Application.persistentDataPath/playhistory.json`에 직렬화.
- 스키마: `{ version, plays: [{ scenarioId, startedAt, endedAt, totalScore, stepResults[] }] }`
- PlayerPrefs는 설정 값(볼륨·자막)만.

### 3.5 폰트
- 본문: 나눔고딕/프리텐다드(OFL) — SDF Atlas 사전 생성 권장 (KS X 1001 2,350자 + 자주 쓰는 한자/기호).
- 제목: Noto Sans KR Bold.
- Atlas는 `Assets/Art/Fonts/` 하위에 버전 고정.

## 4. 폴더 구조 (Unity 프로젝트 생성 후)

```
Assets/
├── _Project/                # 프로젝트 전용 에셋(서드파티와 분리)
│   ├── Scripts/
│   │   ├── Core/            # Runner, Interaction, FeedbackBus
│   │   ├── Data/            # SO 정의
│   │   ├── UI/
│   │   ├── Gameplay/        # Step Controllers
│   │   └── Utils/
│   ├── Prefabs/
│   ├── Scenes/
│   │   ├── MainMenu.unity
│   │   ├── Briefing.unity
│   │   ├── Simulation_IMInjection.unity
│   │   └── Debriefing.unity
│   ├── Data/                # SO 에셋 (.asset)
│   │   └── Scenarios/IMInjection/
│   ├── Art/
│   │   ├── Models/
│   │   ├── Textures/
│   │   ├── Materials/
│   │   ├── Fonts/
│   │   └── UI/
│   ├── Audio/
│   └── Localization/
├── ThirdParty/              # 에셋 스토어 패키지
└── Plugins/
```

## 5. 코딩 컨벤션

- 네임스페이스: `NursingSim.Core`, `NursingSim.Data`, `NursingSim.Gameplay.IMInjection` 등 기능 단위.
- 클래스명: PascalCase; 인터페이스 `I` 접두.
- 직렬화 필드: `[SerializeField] private`, 접근은 프로퍼티.
- ScriptableObject 파일명: `SO_{타입}_{식별자}` (예: `SO_Scenario_IMInjection`).
- 이벤트 채널: `Channel_{이벤트명}` (예: `Channel_ScoreChanged`).
- MonoBehaviour는 가능한 한 **얇게** — 로직은 POCO/SO에서. 테스트 용이성 확보.

## 6. 테스트 전략 (경량)

- Edit Mode 테스트: `ScenarioRunner` 상태 전이, 채점 로직, SaveService 직렬화 — NUnit.
- Play Mode 테스트: 핵심 인터랙션 스모크(클릭 → 하이라이트 → 선택 이벤트 발행).
- 손 검증: 각 Phase 끝에 "IM 시나리오 한 번 완주" 체크리스트. 감수자 리뷰는 별도.

## 7. 빌드 타깃

| 플랫폼 | 사양 | 비고 |
|---|---|---|
| Windows x64 | DX11/DX12, 8GB RAM, GTX 1650 이상 권장 | 학교 PC실 주력 |
| macOS | Apple Silicon + Intel Universal | 학생 개인 노트북 |

패키징: Windows는 zip + 설치 가이드, macOS는 .dmg 또는 .zip(공증 여부는 추후 결정).

## 8. 버전 관리

- Git + Git LFS (`*.fbx`, `*.psd`, `*.wav`, `*.png` over threshold).
- 브랜치: `main`(릴리즈) / `dev`(통합) / `feature/*`
- `.gitattributes`를 프로젝트 생성 직후 추가 (Unity YAML merge, LFS 룰).
