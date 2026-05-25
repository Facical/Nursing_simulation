# 12. 중간보고 발표자료 초안 — 구현 진행 현황

> 기준일: 2026-05-24  
> 목적: 현재 구현 상태를 중간보고 발표자료로 옮길 수 있도록 정리한다.  
> 주의: 아래 진행률은 코드 라인 수나 테스트 커버리지 기반의 정밀 산출값이 아니라, `docs/07-roadmap.md`의 Phase별 산출물 기준으로 본 보수적 추정치다.

## 1. 한 줄 요약

KABONE 핵심기본간호술 #3 근육주사 1개 시나리오를 대상으로, 절차 기준 문서화와 데이터 드리븐 런타임 구조, 13개 시뮬레이션 단계 구현은 완료된 상태다. 현재는 UX, 3D 손 조작, 오디오, 디브리핑 화면, QA 및 빌드 검증을 진행해야 하는 단계다.

## 2. 전체 진행률 요약

| 구분 | 진행률 | 현재 상태 | 근거 |
|---|---:|---|---|
| Phase 0. 프로젝트 셋업 | 약 90% | Unity 프로젝트, 폰트, 패키지, KABONE 기준 문서화 완료. 일부 에셋 정리와 라이선스 기록은 계속 관리 필요 | `ProjectVersion.txt`, `docs/02`, `docs/09`, `Assets/_Project/Art` |
| Phase 1. 프로토타입 | 100% | 손위생과 물품 준비 중심의 최소 플레이 루프 구현 | `InteractionManager`, `FeedbackBus`, `ScenarioRunner`, Phase 1 wiring |
| Phase 2. 시나리오 풀 구현 | 100% | 근육주사 13개 SO 단계, 7개 step 타입 컨트롤러, 채점 및 저장 구조 구현 | `docs/02 §3`, `Phase2WiringTool`, `Gameplay/*StepController.cs` |
| Phase 3. UX·오디오·디브리핑 | 약 25% | MainMenu, 최근 기록, 설정, 로딩 overlay, 3D 손위생 spike 일부 구현. 오디오와 최종 Debriefing UX는 미완료 | `Phase3MainMenuTool`, `Phase3WiringTool`, `UI/*Binder.cs` |
| Phase 4. QA·빌드 | 약 10% | 일부 EditMode 테스트가 추가되었으나, 전체 회귀 테스트와 Win/Mac 빌드는 아직 미완료 | `Assets/_Project/Tests/EditMode` |
| **MVP 전체** | **약 65%** | 핵심 절차와 런타임은 구현됨. 제품 발표/배포 수준의 UX polish, QA, 빌드 검증이 남음 | 로드맵 산출물 기준 추정 |

## 3. 현재까지 구현된 핵심 내용

### 3.1 의료 절차 기준 정리

- KABONE 핵심기본간호술 평가항목 프로토콜 제4.1판의 #3 근육주사를 1차 기준으로 고정했다.
- KABONE 21단계를 시뮬레이터용 13개 ScriptableObject 단계로 매핑했다.
- 핵심항목 6개를 critical gate로 구분했다.
- KABONE에 없는 임상 best-practice는 평가 규칙에 추가하지 않는 방향으로 정리했다.

### 3.2 런타임 구조

- 시나리오를 코드 하드코딩이 아니라 `NursingScenario`와 `ScenarioStep` ScriptableObject로 구성했다.
- `ScenarioRunner`가 단계 순서를 진행하고, step 타입에 맞는 controller를 호출한다.
- step controller, UI, 저장, 채점은 `FeedbackBus` 이벤트 채널을 통해 연결된다.
- 현재 구현된 step 타입은 다음 7종이다.
  - `ChecklistStep`
  - `ToolInteractionStep`
  - `DialogueStep`
  - `SelectionStep`
  - `LandmarkPickStep`
  - `SequenceStep`
  - `ToggleGroupStep`

### 3.3 근육주사 시나리오 구현

- MVP 대상은 근육주사 1개 시나리오로 제한했다.
- 현재 13개 SO 단계가 생성 가능하다.
- 주요 단계는 다음 흐름으로 구성되어 있다.
  1. 처방 확인
  2. 손위생
  3. 약물 준비
  4. 물품 준비
  5. 환자 접근 및 자기소개
  6. 환자 식별
  7. 설명, 의문사항 확인, 사생활 보호
  8. 주사 부위 선정
  9. 랜드마크 촉진
  10. 손위생 및 피부 소독
  11. 주사 시퀀스
  12. 발침, 마사지, 자세 정리, 후 설명
  13. 폐기, 손위생, 기록

### 3.4 채점 및 저장

- 각 단계는 `weight`, `feedbackTiming`, `isCriticalGate` 값을 가진다.
- 단계별 결과는 `StepResult`로 저장된다.
- 시나리오 완료 시 총점, 치명 실수 수, 단계별 결과를 포함하는 `DebriefingReport`를 만든다.
- `SaveService`가 최근 플레이 기록을 `playhistory.json`에 저장하는 구조를 갖췄다.

### 3.5 UI 및 UX 준비

- 플레이 중 HUD, 체크리스트, 선택지, 시퀀스 미니게임, 토스트, 완료 배너용 binder가 구현되어 있다.
- MainMenu, 설정 modal, 최근 기록 modal, 로딩 overlay를 생성하는 Phase 3 도구가 추가되어 있다.
- 단, 최종 발표용 수준의 화면 polish와 Debriefing 화면 완성은 아직 남아 있다.

### 3.6 3D 손 조작 spike

- 데스크톱 환경에서 양손 조작을 위한 `Hand3DController` 계층이 추가되었다.
- 손소독제 펌프 누르기와 손 비비기 이벤트를 3D 방식으로 처리하는 실험 구현이 들어가 있다.
- 현재는 손위생 중심의 spike이며, 약물 준비, 주사기 조작, 랜드마크 촉진, 폐기까지 전부 3D 물리 조작으로 대체된 상태는 아니다.

### 3.7 테스트 준비

- EditMode 테스트가 일부 추가되어 있다.
- 현재 테스트 범위는 다음 정도다.
  - `DeductionReason` 문서와 코드 enum 동기화 검증
  - 주사 시퀀스 critical fail 판정 검증
  - 3D hand event 전달 검증
  - 손 rig, finger curl, IK 동작 일부 검증
- 전체 시나리오 E2E 테스트, PlayMode smoke test, 빌드 테스트는 아직 남아 있다.

## 4. 발표용 정량 지표

| 항목 | 현재 값 |
|---|---:|
| 대상 술기 | 1개: 근육주사 |
| KABONE 원 절차 | 21단계 |
| 시뮬레이터 SO 단계 | 13단계 |
| Critical gate | 6개 |
| Step 타입 | 7종 |
| FeedbackBus 이벤트 채널 | 6종 |
| 주요 씬 파일 | MainMenu, Briefing, Simulation_IMInjection, Debriefing |
| 저장 이력 | 최근 10회 보관 구조 |
| EditMode 테스트 파일 | 4개 |

## 5. 현재 데모 가능 범위

- Unity 6000.0.73f1에서 프로젝트를 열 수 있다.
- `Tools > Nursing Sim > Phase 2 > 1. Create Full KABONE Scenario`로 13단계 시나리오 에셋을 생성할 수 있다.
- `Tools > Nursing Sim > Phase 2 > 2. Wire Simulation Scene (Full)`로 시뮬레이션 씬에 runner, controller, UI를 배치할 수 있다.
- Play Mode에서 핵심 절차 흐름을 확인하는 데모가 가능하다.
- 데모 시에는 그래픽, 사운드, 디브리핑 화면, 일부 3D 손 조작이 최종 품질이 아니라는 점을 먼저 설명하는 것이 안전하다.

## 6. 아직 완료되지 않은 내용

| 영역 | 남은 작업 |
|---|---|
| UX | MainMenu → Briefing → Simulation → Debriefing 전체 흐름 polish |
| Debriefing | 단계별 결과표, 학습 포인트, 재도전 흐름 UI 완성 |
| 오디오 | SFX, 병실 ambience, 음량 설정 연동 |
| 3D 조작 | 손위생 외 약물 준비, 주사, 폐기 단계의 물리 조작 확대 |
| 그래픽 | 처치실, 도구, 환자 모델 배치와 카메라 연출 정리 |
| QA | KABONE 자체 점검 9개 항목 회귀 확인 |
| 테스트 | PlayMode smoke test, 전체 시나리오 E2E 검증 |
| 빌드 | Windows x64, macOS Universal 빌드 검증 |
| 산출물 관리 | 에셋 라이선스와 미커밋 변경 정리 |

## 7. 중간보고에서 강조할 점

- 범위를 넓히지 않고 근육주사 1개 술기에 집중했다.
- 일반 의학 지식으로 임의 규칙을 추가하지 않고 KABONE 문서 기준으로 절차와 평가를 맞췄다.
- 시나리오를 ScriptableObject로 분리해 이후 단계 추가나 수정이 가능하도록 만들었다.
- 기능 구현은 핵심 루프까지 도달했지만, 아직 완성 빌드나 최종 사용자 테스트가 끝난 상태는 아니다.
- 현재 발표는 “완성 제품 시연”이 아니라 “핵심 절차 구현과 향후 polish/QA 계획 보고”로 잡는 것이 적절하다.

## 8. 발표자료 슬라이드 구성 제안

### Slide 1. 제목

- 간호 술기 시뮬레이터 중간보고
- 대상 술기: KABONE #3 근육주사
- 플랫폼: Unity 기반 PC 시뮬레이터

### Slide 2. 프로젝트 목표

- 간호대 학부생이 기본간호학 술기를 PC에서 반복 연습할 수 있는 3D 시뮬레이터 개발
- 1차 MVP는 근육주사 1개 시나리오로 제한
- 절차 학습, 즉시 피드백, 단계별 채점, 디브리핑을 목표로 함

### Slide 3. 기준 문서와 범위

- KABONE 핵심기본간호술 제4.1판 #3 근육주사를 기준으로 사용
- KABONE 21단계를 시뮬레이터 13개 단계로 매핑
- MVP에서는 다른 술기, VR, 멀티플레이, 생성형 시나리오는 제외

### Slide 4. 시스템 구조

- `NursingScenario` ScriptableObject가 시나리오 데이터를 보관
- `ScenarioRunner`가 단계 진행을 관리
- 각 step controller가 입력과 평가를 처리
- `FeedbackBus`가 UI, 채점, 저장을 이벤트로 연결

### Slide 5. 구현 진행률

- 전체 MVP 진행률: 약 65%
- Phase 0~2: 핵심 구현 완료
- Phase 3: UX와 3D 손 조작 일부 진행
- Phase 4: QA와 빌드 검증은 초기 단계

### Slide 6. 구현된 기능

- 13개 근육주사 단계 구현
- 7종 step controller 구현
- critical gate 기반 핵심항목 평가
- 점수 집계와 최근 플레이 기록 저장
- 일부 MainMenu, 설정, 최근 기록 UI
- 손위생 3D 조작 spike

### Slide 7. 데모 범위

- Unity Editor에서 시나리오 생성 및 씬 자동 wiring 가능
- Play Mode에서 근육주사 절차 흐름 확인 가능
- 현재 데모는 기능 확인용이며, 최종 그래픽/오디오/QA 버전은 아님

### Slide 8. 남은 작업

- Debriefing 화면 완성
- MainMenu, Briefing, Simulation, Debriefing 전체 흐름 polish
- 오디오와 접근성 옵션 정리
- PlayMode smoke test와 전체 회귀 테스트
- Windows/macOS 빌드 검증

### Slide 9. 다음 일정

- 1순위: Debriefing UI와 전체 scene flow 연결
- 2순위: 손위생 외 3D 조작 확대 여부 결정
- 3순위: KABONE 자체 점검 체크리스트 기반 회귀 테스트
- 4순위: Windows/macOS 빌드 및 데모 패키징

## 9. 발표 멘트 초안

현재 프로젝트는 근육주사 술기 1개를 대상으로 범위를 좁혀 구현하고 있습니다. 절차와 평가 기준은 KABONE 핵심기본간호술 제4.1판을 기준으로 정리했고, 이 21개 절차를 시뮬레이터에서는 13개 단계로 나누어 구현했습니다.

기술적으로는 시나리오를 코드에 직접 박지 않고 ScriptableObject 데이터로 구성했습니다. `ScenarioRunner`가 각 단계를 순서대로 실행하고, step controller가 입력과 평가를 처리하며, UI와 저장은 `FeedbackBus` 이벤트로 연결됩니다.

현재 핵심 플레이 루프와 13단계 시나리오, 채점, 저장 구조는 구현되어 있습니다. 다만 최종 제품 수준의 UX, 오디오, 디브리핑 화면, 전체 QA와 빌드 검증은 아직 남아 있기 때문에, 전체 MVP 기준으로는 약 65% 정도 진행된 상태로 보는 것이 적절합니다.

다음 단계에서는 Debriefing 화면과 전체 씬 흐름을 먼저 완성하고, 이후 KABONE 자체 점검 체크리스트를 기준으로 회귀 테스트와 Windows/macOS 빌드 검증을 진행할 계획입니다.
