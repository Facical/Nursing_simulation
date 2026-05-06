# 07. 로드맵

> 1인 중급 Unity 개발자 기준, 주당 약 15~20시간 투입 가정. 총 9~12주 MVP.
> 날짜는 예시 플레이스홀더(2026-04-22 기준)이며, 실제 착수 시 조정.

## 타임라인 개요

| Phase | 기간 | 목표 산출물 |
|---|---|---|
| 0. 셋업 | 1주 | Unity 프로젝트, VCS, 에셋 1차 조달, **KABONE 그라운딩 (docs/02·09)** |
| 1. 프로토타입 | 2~3주 | Simulation 씬 1단계 플레이 가능 (손위생 + 물품준비) |
| 2. 시나리오 풀 구현 | 3~4주 | IM injection 13단계 전부 플레이 가능, 채점 동작 |
| 3. UX·오디오·디브리핑 | 2주 | Briefing/Debriefing 씬, SFX, 폴리싱 |
| 4. QA·빌드 | 1~2주 | 자체 점검 통과, Win/Mac 빌드, 스모크 테스트 |

## Phase 0 — 프로젝트 셋업 (Week 1)

### 작업
- [x] `Nursing_simulation/` 아래 `git init`, 기획 문서 초기 커밋
- [x] Unity 6 LTS 프로젝트 생성 (URP, Linear 컬러), 폴더 구조 적용 ([03-technical-spec.md §4](./03-technical-spec.md))
- [x] 패키지 설치: Input System, TextMeshPro, Cinemachine
- [x] `.gitattributes` 작성 (Unity YAML merge, LFS)
- [x] 한글 SDF 폰트 Atlas 생성 (`Pretendard`)
- [ ] 에셋 1차 조달: 처치실 팩, 의료 도구 팩
- [x] **Microsoft Rocketbox clone** → `Medical_*` 직군 8종 + 환자 후보(`Adults/...`) Unity 임포트 검증 완료 (commit `0f38d52`)
- [x] 아트 방향 확정 → **Semi-realistic 인물·환경 + 포토리얼 의료 도구** (`docs/03-technical-spec.md §1.1`)
- [x] **KABONE 그라운딩 — `docs/09-references.md` 작성** (KABONE 핵심기본간호술 제4.1판 #3 근육주사 21단계 원문 캡처)
- [x] **KABONE 그라운딩 — `docs/02-functional-spec.md` 재작성** (13개 SO 단계 시뮬레이터 ↔ KABONE 21단계 매핑, 핵심항목(*) 6개 critical gate 정의)

### DoD
- `File > Build` 성공 (빈 씬 포함)
- `Simulation_IMInjection` 씬에 처치실·환자·트레이 배치
- 기획 문서 전부 커밋됨
- **`docs/09-references.md` 작성 완료, `docs/02 §6` 자체 점검 체크리스트 통과**

## Phase 1 — 프로토타입 (Week 2~4)

### 작업
- [x] `IInteractable` 인터페이스, `InteractionManager` (Raycast 기반)
- [x] 하이라이트 (MaterialPropertyBlock 기반 간이 틴트 — URP Outline/QuickOutline은 Phase 3에서 셰이더 교체 예정)
- [x] `FeedbackBus` (SO 이벤트 채널 6개: StepStarted/Progress/Completed, InstantFeedback, ScoreChanged, ScenarioCompleted)
- [x] `ScenarioRunner` 상태 머신 (SO 기반, Idle→StepActive→StepEvaluating→Completed)
- [x] `ChecklistStep` + `ToolInteractionStep` 컨트롤러 구현
- [x] 손위생 단계 E2E: 펌프 클릭 2회 → 비비기 15초 타이머 → 합격
- [x] 물품 준비 단계 E2E: 캐비닛 클릭 → 아이템 팝업 선택 → 트레이 배치

### Phase 1 실행 방법 (Unity 열기 후)

1. `Tools > Nursing Sim > Phase 1 > 1. Create Channels + Bus` — 이벤트 채널 + FeedbackBus .asset 생성
2. `Tools > Nursing Sim > Phase 1 > 2. Create Mini Scenario` — 축소 시나리오 SO + 2개 스텝 서브에셋 생성
3. `Tools > Nursing Sim > Phase 1 > 3. Wire Simulation Scene` — `Simulation_IMInjection` 씬에 러너/인터랙션/HUD 자동 배치 및 저장
4. `Simulation_IMInjection` 씬 열고 ▶ Play
   - 손소독제 펌프(작은 실린더) 2회 클릭 후 15초 경과 → 자동으로 2단계 전이
   - 캐비닛 클릭 → 팝업에서 필수 10개 선택(distractor 3개 선택 시 감점) → "트레이에 담기" → 완료 배너

### DoD
- 1~2단계만 포함한 축소 시나리오로 플레이 가능
- 체크리스트 UI, 감점 토스트 동작
- 친구 1명에게 10분 플레이 테스트 수행

## Phase 2 — 시나리오 풀 구현 (Week 5~8)

### 작업
- [x] 나머지 Step 타입 컨트롤러: Dialogue, Selection, LandmarkPick, Sequence, ToggleGroup (`Assets/_Project/Scripts/Gameplay/`)
- [x] `NursingScenario` SO 생성 + **13개 SO 단계** 에셋 작성 (KABONE 21단계 매핑, [docs/02 §3.0](./02-functional-spec.md)) — Phase 2 와이어링 도구로 자동 생성: `Tools/Nursing Sim/Phase 2/1. Create Full KABONE Scenario`
- [x] 약물 준비 미니UI (앰플 흡입/공기 제거 4개 항목 ChecklistItem) — Diclofenac 4mg 앰플 시나리오, 재구성 없음
- [x] 환자 부위 선정 (Selection) + 랜드마크 촉진 (LandmarkPick: 대전자/ASIS/장골능 3개 포인트, 순서 강제)
- [x] 주사 Sequence (각도 90°±10° + 흡인 + 혈액 분기 + 주입 속도) — `SequenceStepController`
- [x] 채점: `StepResult` 집계 → `DebriefingReport` 빌더 (`ScenarioRunner.Finish()`)
- [x] 로컬 저장 (JSON) — `SaveService` → `Application.persistentDataPath/playhistory.json`, 최근 10회 보관

### DoD
- 13개 SO 단계 전부 진행 가능 (그래픽 완성도는 낮아도 됨)
- 핵심항목(*) 6개가 critical gate로 동작
- 총점 0~100이 정확히 계산됨
- playhistory.json에 1회분 기록됨

### Phase 2 실행 방법 (Unity 열기 후)

1. `Tools > Nursing Sim > Phase 2 > 1. Create Full KABONE Scenario` — 13단계 KABONE 시나리오 .asset 생성
2. `Tools > Nursing Sim > Phase 2 > 2. Wire Simulation Scene (Full)` — 씬에 모든 컨트롤러+UI 자동 배치
3. `Simulation_IMInjection` 씬 열고 ▶ Play로 13단계 시나리오 진행

## Phase 3 — UX·오디오·디브리핑 (Week 9~10)

### 작업
- [ ] MainMenu / Briefing / Debriefing 씬 완성
- [ ] Cinemachine 카메라 블렌드 (상호작용 줌)
- [ ] 포스트 프로세싱 (컬러 그레이딩 · Bloom · DoF)
- [ ] 한글 자막 표시/숨김 토글, 폰트 크기 옵션
- [ ] SFX 세트, 병실 앰비언트 루프
- [ ] 디브리핑 리포트 UI (표, 학습 포인트 카드)
- [ ] 설정 창, 일시정지 메뉴 (ESC)

### DoD
- 전체 플로우 MainMenu → Debriefing 30분 내 완주
- 자막/사운드 토글 동작
- 디브리핑이 단계별 결과와 학습 포인트 모두 출력

## Phase 4 — QA·빌드 (Week 11~12)

### 작업
- [ ] **`docs/02 §6` 자체 점검 체크리스트 통과 확인** (KABONE 출처 대조 9개 항목)
- [ ] 시뮬레이터 실제 플레이로 KABONE 21단계 모두 평가 가능한지 회귀 테스트
- [ ] 버그 리스트 소진
- [ ] Windows x64 IL2CPP 빌드, macOS Universal 빌드
- [ ] 공증(optional) / 배포 압축
- [ ] 학생 3~5명 베타 테스트
- [ ] v0.1.0 태그

### DoD
- `docs/02 §6` 자체 점검 9개 항목 모두 통과
- Win/Mac 빌드가 타깃 사양에서 60fps 유지
- 베타 테스터 평균 완주율 ≥ 80%

> **참고**: 감수자(간호학 교수/임상 간호사) 검증 단계는 본 프로젝트에서 운영하지 않는다 (사용자 결정, 2026-05-06). 의학적 그라운딩은 KABONE 공식 PDF 직접 인용으로 마감 ([docs/09-references.md](./09-references.md)).

## 마일스톤 요약

```
W1   ████ 셋업
W2   ██████ 프로토타입 시작
W3   ██████
W4   ██████
W5   ████████ 시나리오 풀 구현
W6   ████████
W7   ████████
W8   ████████
W9   ██████ UX·오디오·디브리핑
W10  ██████
W11  ████ 감수·QA·빌드
W12  ████
```

## 이후 로드맵 (MVP 검증 후)

- 시나리오 확장: 활력징후 측정, 수혈, 유치도뇨, 경관영양 등
- 시나리오 에디터 (감수자 직접 편집)
- 학생 성적 집계 (학교 LMS 연동 — 요건 확인 필요)
- VR(Meta Quest) 모드 — 핵심 상호작용 재사용
- 다국어(영어) — Localization Package 활성화
