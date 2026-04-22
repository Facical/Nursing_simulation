# 07. 로드맵

> 1인 중급 Unity 개발자 기준, 주당 약 15~20시간 투입 가정. 총 9~12주 MVP.
> 날짜는 예시 플레이스홀더(2026-04-22 기준)이며, 실제 착수 시 조정.

## 타임라인 개요

| Phase | 기간 | 목표 산출물 |
|---|---|---|
| 0. 셋업 | 1주 | Unity 프로젝트, VCS, 에셋 1차 조달, 감수자 접촉 |
| 1. 프로토타입 | 2~3주 | Simulation 씬 1단계 플레이 가능 (손위생 + 물품준비) |
| 2. 시나리오 풀 구현 | 3~4주 | IM injection 11단계 전부 플레이 가능, 채점 동작 |
| 3. UX·오디오·디브리핑 | 2주 | Briefing/Debriefing 씬, SFX, 폴리싱 |
| 4. 감수·QA·빌드 | 1~2주 | 감수 반영, Win/Mac 빌드, 스모크 테스트 |

## Phase 0 — 프로젝트 셋업 (Week 1)

### 작업
- [ ] `Nursing_simulation/` 아래 `git init`, 기획 문서 초기 커밋
- [ ] Unity 6 LTS 프로젝트 생성 (URP, Linear 컬러), 폴더 구조 적용 ([03-technical-spec.md §4](./03-technical-spec.md))
- [ ] 패키지 설치: Input System, TextMeshPro, Cinemachine
- [ ] `.gitattributes` 작성 (Unity YAML merge, LFS)
- [ ] 한글 SDF 폰트 Atlas 생성 (`나눔고딕` or `Pretendard`)
- [ ] 에셋 1차 조달: 처치실 팩, 의료 도구 팩
- [ ] **Microsoft Rocketbox clone** (`git clone https://github.com/microsoft/Microsoft-Rocketbox`) → `Medical_*` 직군 8종 + 환자 후보(`Adults/...`) Unity로 임포트 테스트 (`FixRocketboxMaxImport.cs` 동작 확인, Humanoid rig 변환, URP 재질 일괄 변환)
- [x] 아트 방향 확정 → **Semi-realistic 인물·환경 + 포토리얼 의료 도구** (`docs/03-technical-spec.md §1.1`)
- [ ] 감수자(간호학 교수/임상 간호사 1명) 컨택 & 일정 잡기

### DoD
- `File > Build` 성공 (빈 씬 포함)
- `Simulation_IMInjection` 씬에 처치실·환자·트레이 배치
- 기획 문서 전부 커밋됨

## Phase 1 — 프로토타입 (Week 2~4)

### 작업
- [ ] `IInteractable` 인터페이스, `InteractionManager` (Raycast 기반)
- [ ] URP Outline Renderer Feature 또는 QuickOutline 적용
- [ ] `FeedbackBus` (SO 이벤트 채널 5~6개)
- [ ] `ScenarioRunner` 상태 머신 (단계 1~2개 하드코딩)
- [ ] `ChecklistStep` + `ToolInteractionStep` 컨트롤러 구현
- [ ] 손위생 단계 E2E: 펌프 클릭 → 손 비비기 타이머 → 합격
- [ ] 물품 준비 단계 E2E: 캐비닛 클릭 → 아이템 선택 → 트레이 배치

### DoD
- 1~2단계만 포함한 축소 시나리오로 플레이 가능
- 체크리스트 UI, 감점 토스트 동작
- 친구 1명에게 10분 플레이 테스트 수행

## Phase 2 — 시나리오 풀 구현 (Week 5~8)

### 작업
- [ ] 나머지 Step 타입 컨트롤러: Dialogue, Selection, LandmarkPick, Sequence, ToggleGroup
- [ ] `NursingScenario` SO 생성 + 11단계 에셋 작성
- [ ] 약물 재구성 단계 미니UI (용량 슬라이더/스피너, 공기제거)
- [ ] 환자 부위 선정 + 랜드마크 촉진 (환자 모델에 앵커 3~5개)
- [ ] 주사 Sequence (각도·속도 체크) — 가장 난이도 높은 단계
- [ ] 채점: `StepResult` 집계 → `DebriefingReport` 빌더
- [ ] 로컬 저장 (JSON)

### DoD
- 11단계 전부 진행 가능 (그래픽 완성도는 낮아도 됨)
- 총점 0~100이 정확히 계산됨
- playhistory.json에 1회분 기록됨

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

## Phase 4 — 감수·QA·빌드 (Week 11~12)

### 작업
- [ ] 감수자와 `02-functional-spec.md` 1차 리뷰 → 결과 반영
- [ ] 감수자와 실제 플레이 리뷰 → 가중치/지문 조정
- [ ] 버그 리스트 소진
- [ ] Windows x64 IL2CPP 빌드, macOS Universal 빌드
- [ ] 공증(optional) / 배포 압축
- [ ] 학생 3~5명 베타 테스트
- [ ] v0.1.0 태그

### DoD
- 감수자 승인 레터(비공식이라도)
- Win/Mac 빌드가 타깃 사양에서 60fps 유지
- 베타 테스터 평균 완주율 ≥ 80%

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
