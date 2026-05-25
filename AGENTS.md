# AGENTS.md

이 파일은 Codex가 본 저장소에서 작업할 때의 가이드라인이다. 응답·문서·메모리는 한국어가 기본이며, 코드와 커밋 메시지는 영어를 허용한다.

---

## 행동 원칙 (Behavioral Principles)

본 절은 [forrestchang/andrej-karpathy-skills](https://github.com/forrestchang/andrej-karpathy-skills)의 Karpathy-style AGENTS.md를 본 프로젝트에 맞춰 적응시킨 것이다. 사소한 작업에는 판단을 우선하되, 의료 절차·평가 로직처럼 정확성이 중요한 부분에서는 본 원칙을 엄격히 따른다.

### 1. 코딩 전에 생각하라
- 가정을 명시하라. 불확실하면 묻는다.
- 두 해석이 가능하면 양쪽을 제시하고 선택하지 말라.
- 더 단순한 접근이 있으면 말하라. 필요하면 사용자 의견에 반박한다.
- 불명확하면 멈춰라. 무엇이 헷갈리는지 명명하고 물어라.

### 2. 단순함이 우선
- 요구된 것 외 기능 금지.
- 단발성 코드에 추상화 금지.
- 요구되지 않은 "유연성"·"설정 가능성" 금지.
- 발생 불가능한 시나리오에 에러 처리 금지.
- 200줄을 50줄로 줄일 수 있으면 다시 써라.

### 3. 외과적 수정
- 수정해야 할 것만 만진다. 인접 코드의 "개선"·서식 변경 금지.
- 망가지지 않은 것을 리팩터링하지 말라.
- 기존 스타일과 일치시켜라 (다르게 작성하고 싶더라도).
- 이번 변경으로 고아가 된 import·변수·함수만 제거. 기존 dead code는 사용자가 요청하지 않는 한 건드리지 않는다.
- 변경된 모든 줄은 사용자 요청에서 직접 추적 가능해야 한다.

### 4. 목표 주도 실행
- 모호한 요청을 검증 가능한 목표로 변환:
  - "검증 추가" → "잘못된 입력에 대한 테스트를 작성하고 통과시킨다"
  - "버그 수정" → "버그를 재현하는 테스트를 작성하고 통과시킨다"
- 다단계 작업은 각 단계의 verify 기준을 짧게 명시.
- 강한 성공 기준이 있어야 독립 루프가 가능하다.

---

## 프로젝트 상태 (2026-05-06 기준)

본 저장소는 Unity 6 LTS 기반 한국 간호 술기 시뮬레이터 프로젝트다. **Phase 0~2 완료** 상태로, 13개 SO 단계 KABONE 시나리오가 코드와 데이터로 구현되어 플레이 가능하다 ([docs/07-roadmap.md](./docs/07-roadmap.md)).

| Phase | 상태 |
|---|---|
| 0. 셋업 (Unity 프로젝트, 폰트, Rocketbox, KABONE 그라운딩) | ✅ |
| 1. 프로토타입 (인터랙션, FeedbackBus, ScenarioRunner FSM, 2개 컨트롤러) | ✅ |
| 2. 시나리오 풀 구현 (5개 신규 컨트롤러, 13단계 SO, SaveService) | ✅ |
| 3. UX·오디오·디브리핑 | 미진행 |
| 4. QA·빌드 | 미진행 |

**스택**: Unity 6 LTS, URP, Linear color, New Input System, TextMeshPro(Pretendard SDF), Cinemachine. Win + macOS 스탠드얼론. 언어 한국어 전용.

---

## 의료 정확성 — 두 개의 진실의 출처

본 시뮬레이터의 절차·평가 로직은 두 문서가 함께 진실의 출처를 이룬다. **둘은 항상 정합해야 한다.**

1. **[docs/09-references.md](./docs/09-references.md)** — KABONE 핵심기본간호술 평가항목 프로토콜 **제4.1판**(2017-02-22) 항목 #3 근육주사의 21단계 원문 + 출처 ID(`REF-KABONE-3-N`).
2. **[docs/02-functional-spec.md](./docs/02-functional-spec.md)** — 위 출처를 시뮬레이터 13개 SO 단계로 매핑한 절차·배점·핵심항목(★)·감점 사유.

절차·배점·감점 사유를 변경할 때:
1. `docs/09-references.md`에서 KABONE 출처를 먼저 확인. 충돌 시 **KABONE이 우선**.
2. `docs/02-functional-spec.md` §3 매핑 표와 §6 자체 점검 체크리스트를 갱신.
3. `docs/05-data-model.md`의 `DeductionReason` enum과 `Assets/_Project/Scripts/Data/DeductionReason.cs`를 **동시에** 갱신 — 둘은 항상 1:1 동기화.
4. `docs/02 §6` 자체 점검 9개 항목으로 회귀 점검.

**KABONE에 명시되지 않은 임상 best-practice는 추가하지 않는다.** Z-track 기법, 둔부 배면 회피 정책, 부피별 부위 거부, 마사지 회피, 5분 후 관찰 등은 시험 기준 외이므로 시뮬레이터에 도입하지 말라. 일반 의학 지식에서 새 규칙을 발명하지 말라 — 불명확하면 묻는다.

**감수자 검증은 운영하지 않는다** (사용자 결정, 2026-05-06). R2(의료 정확성) 잔여 리스크는 사용자가 인지·수용했으며, 그라운딩은 KABONE 공식 PDF 직접 인용으로 마감한다. docs에 "감수자 리뷰" 표현을 도입하지 말라.

---

## 아키텍처

런타임은 **데이터 드리븐**: 시나리오는 코드가 아니라 ScriptableObject(`.asset`) 자산으로 작성된다.

```
NursingScenario (SO)
  └─ List<ScenarioStep>  (추상 SO, 7개 구체 타입)
       ChecklistStep, ToolInteractionStep, DialogueStep,
       SelectionStep, LandmarkPickStep, SequenceStep, ToggleGroupStep
```

`ScenarioRunner` FSM이 step을 하나씩 활성화하고 SO 타입에 매칭되는 `IStepController` MonoBehaviour로 디스패치한다. `InteractionManager`가 Raycast로 마우스 입력을 `IInteractable`에 전달한다. 모든 통신은 **`FeedbackBus`(SO 이벤트 채널)** 를 통한다 — step 컨트롤러가 `StepCompleted`/`InstantFeedback`/`ScoreChanged`를 발행하고, UI·오디오·채점·SaveService가 독립적으로 구독한다.

편집 시 지켜야 할 것:
- **MonoBehaviour에 시나리오 로직을 하드코딩하지 말라.** 로직은 SO 타입별 step 컨트롤러에, 데이터는 `.asset`에 둔다.
- **모든 step에 `weight`, `feedbackTiming`, `isCriticalGate`가 있다.** 채점·UX는 이 셋에서 분기하므로 새 step 타입을 추가할 때 보존하라.
- **Editor 와이어링 도구로 .asset을 빌드한다.** `Tools > Nursing Sim > Phase 2 > 1. Create Full KABONE Scenario`가 13단계 시나리오를 자동 생성하고 (`Editor/Phase2WiringTool.cs`), `Phase 2 > 2. Wire Simulation Scene`가 씬에 모든 컨트롤러+UI를 배치한다. 새 step 타입을 추가하면 이 도구의 `BuildStepN_*` + `BuildAllControllers` 둘 다 갱신.

전체 SO 스키마: [docs/05-data-model.md](./docs/05-data-model.md). 씬/UX 흐름: [docs/04-scene-and-ux-flow.md](./docs/04-scene-and-ux-flow.md).

---

## 폴더 레이아웃

```
NursingSimulation/Assets/_Project/
├── Data/
│   ├── Events/                    # FeedbackBus + 6개 채널 .asset
│   └── Scenarios/IMInjection/     # NursingScenario .asset (sub-asset 포함)
├── Scripts/
│   ├── Core/                      # Events, Interaction, Runner (FSM, SaveService)
│   ├── Data/                      # ScriptableObject 정의 + 열거형
│   ├── Editor/                    # Phase0/1/2 와이어링 도구, UiBuilder
│   ├── Gameplay/                  # 7개 IStepController + Interactable 컴포넌트
│   └── UI/                        # 6개 Binder (HUD, Checklist, Choice, Sequence, Toast, Completion)
├── Scenes/                        # Simulation_IMInjection 등
├── Art/Fonts/                     # Pretendard SDF + LICENSES.md
└── Prefabs/UI/                    # Toggle/Choice 버튼 prefab
```

`ThirdParty/`는 외부 에셋 전용. Microsoft Rocketbox는 별도 `Microsoft-Rocketbox/` 디렉터리.

---

## 작업 관행

- **스코프 규율**: MVP는 KABONE #3 근육주사 1종. 다른 술기, VR, 멀티플레이, 생성형 시나리오는 MVP 출시까지 거부 (R12).
- **아트 방향 고정**: Semi-realistic 인물·환경(Rocketbox, MIT) + 포토리얼 의료 도구(주사기·앰플·바늘). 환자/간호사 룩을 MetaHuman 수준으로 밀지 말라 — uncanny valley + 비용 회피 (R1, R5).
- **사용자 대면 텍스트는 한국어**. 코드·주석·커밋은 영어 허용. TMP atlas는 KS X 1001 2,350자 + 공통 기호 포함.
- **에셋 라이선스**: 모든 외부 에셋은 첫 반입 시 `Assets/_Project/Art/LICENSES.md`에 기록 ([docs/06-asset-plan.md](./docs/06-asset-plan.md) §5).
- **처치실 배치 기준**: `Simulation_IMInjection` 씬의 세면대, 처치 테이블, 침대, 핸드 rig, 손세정제 위치는 [docs/11-treatment-room-environment-map.md](./docs/11-treatment-room-environment-map.md)를 먼저 확인한다. 물품·카메라·핸드 interaction 배치를 변경하면 이 문서도 갱신한다.
- **이모지 사용 금지** (사용자가 요청하지 않는 한). 코드·문서·메모리·커밋 모두.

---

## 명령어

- 프로젝트 열기: Unity Hub에서 `ProjectSettings/ProjectVersion.txt`에 고정된 버전으로 오픈.
- Phase 1 데모 실행: `Tools > Nursing Sim > Phase 1 > 1. Create Channels + Bus` → `2. Create Mini Scenario` → `3. Wire Simulation Scene` → ▶ Play.
- Phase 2 풀 시나리오 실행: `Tools > Nursing Sim > Phase 2 > 1. Create Full KABONE Scenario` → `2. Wire Simulation Scene (Full)` → ▶ Play.
- 빌드: `File → Build Settings` → Windows x64 (IL2CPP) 또는 macOS Universal.
- 테스트: Unity Test Runner — Edit Mode(NUnit, FSM/채점/JSON), Play Mode(인터랙션 스모크). 현재 테스트 코드는 미작성 (Phase 4 예정).
- Unity MCP 운용: 다음 세션에서 Unity Editor 조작·검증이 필요하면 [docs/13-unity-mcp-operations.md](./docs/13-unity-mcp-operations.md)를 먼저 보고 `uLoop`와 `AI Game Developer MCP` 중 적절한 도구를 선택한다.

---

## 핵심 참조 파일

- `README.md` — 진입점, 문서 인덱스
- `docs/02-functional-spec.md` — IM 주사 절차 (KABONE 그라운딩, 13개 SO 단계 매핑)
- `docs/03-technical-spec.md` — Unity 버전·패키지·아키텍처·코딩 컨벤션
- `docs/05-data-model.md` — SO 스키마 + `DeductionReason` enum (코드와 동기화 필수)
- `docs/07-roadmap.md` — 단계별 작업·DoD
- `docs/08-risks-and-mitigation.md` — R1–R12 (R7 폐기됨)
- `docs/09-references.md` — KABONE 제4.1판 출처 인덱스 + 21단계 원문
- `docs/11-treatment-room-environment-map.md` — `Simulation_IMInjection` 처치실 오브젝트·좌표 기준표. 핸드·물품·카메라 배치 전 우선 확인
- `docs/13-unity-mcp-operations.md` — `uLoop`/`AI Game Developer MCP` 연결 확인, 도구 선택 기준, 검증 루프
