# Nursing Simulation (가칭)

간호대 학부생을 위한 PC(Win/Mac) 3D **기본간호학 술기 시뮬레이터**. 아트 방향은 **Semi-realistic(인물·환경) + 포토리얼(의료 도구 클로즈업)** 혼합.
1차 MVP는 **근육주사(IM Injection)** 단일 시나리오를 타깃으로, 기본 모드에서는 UI 선택과 3D 손 애니메이션으로 절차를 진행하며 임상적 추론을 반복 훈련하는 것을 목표로 한다. 기존의 세밀한 키보드·마우스 손 조작은 설정에서 켜는 **Hard Hand Simulator** 모드로 분리한다. Practigame(https://www.practigame.com)을 벤치마크하되 국내 기본간호학 교과과정·평가 체크리스트에 맞춘다.

- **플랫폼**: Windows / macOS 스탠드얼론 빌드
- **언어**: 한국어
- **주 대상**: 간호대 학부생 (기본간호학 실습 예습/복습)
- **1차 시나리오**: IM Injection (근육주사)
- **기본 플레이 방식**: UI 기반 절차 선택 + 손 모델 애니메이션 진행
- **고급 플레이 방식**: Settings에서 켜는 Hard Hand Simulator (ASDF/Space, Z/X/C/V/B 등 세밀 손 조작)
- **참고 영상**: https://www.youtube.com/watch?v=9f8rzK8ClE8 (현문사 기본간호학 핵심술기, 근육주사)

## 문서 인덱스

기획·설계 문서는 모두 [`docs/`](./docs) 아래에 있다. 읽는 권장 순서:

1. [01. 제품 비전](./docs/01-product-vision.md) — 왜 만드는가, 누구를 위한가
2. [02. 기능 명세 (IM Injection)](./docs/02-functional-spec.md) — **핵심 문서**: 단계별 절차·평가 기준
3. [03. 기술 명세](./docs/03-technical-spec.md) — Unity 버전, 패키지, 아키텍처
4. [04. 씬 & UX 플로우](./docs/04-scene-and-ux-flow.md) — 씬 구성, 와이어프레임
5. [05. 데이터 모델](./docs/05-data-model.md) — 시나리오 SO 스키마
6. [06. 에셋 계획](./docs/06-asset-plan.md) — 3D/오디오 조달
7. [07. 로드맵](./docs/07-roadmap.md) — Phase별 마일스톤
8. [08. 리스크](./docs/08-risks-and-mitigation.md) — 리스크와 대응
9. [10. Hard Hand Simulator 통합 계획](./docs/10-hand-simulation-plan.md) — 선택형 Hard Hand Simulator 통합 계획
10. [11. 처치실 Environment Map](./docs/11-treatment-room-environment-map.md) — `Simulation_IMInjection` 처치실 오브젝트/좌표 기준표
11. [12. 중간 진행 보고서](./docs/12-interim-progress-report.md) — 현재 구현 상태 요약
12. [13. Unity MCP 운용](./docs/13-unity-mcp-operations.md) — Unity 자동화/검증 도구 운용 메모

## 개발 환경

| 항목 | 값 |
|---|---|
| Unity | **6000.0.73f1** (`ProjectSettings/ProjectVersion.txt` 기준) |
| Render Pipeline | URP |
| Target | Windows 64-bit, macOS (Apple Silicon + Intel) |
| 주요 패키지 | Input System, TextMeshPro, Cinemachine, glTFast, Unity Test Framework |
| 추가 패키지 레지스트리 | Unity Registry, OpenUPM (`package.openupm.com`), Git URL package |
| IDE | Rider 또는 VS Code + Unity extension |
| VCS | Git + Git LFS |

## 처음 클론해서 열기

이 저장소는 repo 루트와 Unity 프로젝트 루트가 다르다. Unity Hub에서 열어야 하는 폴더는 repo 루트가 아니라 **`NursingSimulation/`** 하위 폴더다.

```bash
git lfs install
git clone https://github.com/Facical/Nursing_simulation.git
cd Nursing_simulation
git lfs pull
```

1. Unity Hub에서 **Unity 6000.0.73f1** 설치 여부를 확인한다.
2. Unity Hub의 `Add project from disk`로 `Nursing_simulation/NursingSimulation` 폴더를 선택한다.
3. 첫 실행 시 Unity가 `Library/`와 패키지 캐시를 재생성한다. 에셋과 패키지가 많아서 첫 import는 시간이 걸릴 수 있다.
4. 패키지는 `Packages/manifest.json` 기준으로 복원된다. Unity Registry, OpenUPM, GitHub 접근이 가능한 네트워크 환경이 필요하다.
5. 에디터에서 실행할 때는 `Assets/_Project/Scenes/MainMenu.unity`를 열고 Play를 누른다. 기본값은 UI 기반 시뮬레이션 모드이며, 세밀한 손 조작은 Settings에서 Hard Hand Simulator를 켰을 때만 사용한다.

GitHub의 `Download ZIP`은 Git LFS 에셋을 제대로 받지 못할 수 있으므로 사용하지 않는다. 반드시 `git clone`과 `git lfs pull`을 사용한다.

## 씬과 실행

`ProjectSettings/EditorBuildSettings.asset`에는 다음 씬이 등록되어 있다.

- `Assets/_Project/Scenes/MainMenu.unity`
- `Assets/_Project/Scenes/Briefing.unity`
- `Assets/_Project/Scenes/Simulation_IMInjection.unity`
- `Assets/_Project/Scenes/Debriefing.unity`

시나리오나 씬 와이어링을 다시 생성해야 할 때는 Unity 메뉴에서 아래 순서로 실행한다.

1. `Tools > Nursing Sim > Phase 2 > 1. Create Full KABONE Scenario`
2. `Tools > Nursing Sim > Phase 2 > 2. Wire Simulation Scene (Full)`
3. 선택 사항: `Tools > Nursing Sim > Phase 3 > 1. Spike: Wire Step 2 손위생 (3D)`

## 시뮬레이션 모드

- **Basic Simulation Mode (기본값)**: MainMenu에서 바로 시작하는 표준 학습 모드. 현재 단계의 카드/버튼을 누르면 손 모델 애니메이션과 3D 환경 반응이 실행되고, 평가는 KABONE 절차 선택·순서·확인 여부를 기준으로 한다.
- **Hard Hand Simulator Mode (설정에서 ON)**: 기존 세밀 손 조작 모드. `A/S/D/F/Space`, `Z/X/C/V/B`, 마우스 회전·그랩 등으로 손을 직접 움직인다. 간호생 기본 학습 경로가 아니라 고급 연습, 디버그, 시연용으로 둔다.

우선 구현 방향은 Step 1 처방 확인을 카드형 정보 패널로 정리하고, 이후 나머지 시뮬레이션 단계도 Basic Simulation Mode에서 끝까지 진행 가능하게 만드는 것이다.

## Public Clone 유의사항

대용량 `.glb`, `.fbx`, 이미지, 폰트, `.pptx`는 Git LFS로 관리한다. LFS 파일이 포인터 텍스트로 보이면 `git lfs pull`을 다시 실행한다.

라이선스가 확인되지 않은 `Assets/ThirdParty/ArmTutorial/` 원본 에셋은 public repo에 포함하지 않는다. 대신 public clone에서도 missing mesh 없이 열리도록 `PlayerHand_Left`/`PlayerHand_Right`는 자체 placeholder 손 prefab으로 저장되어 있다. 정식 라이선스가 있는 `armTutorial.unitypackage`를 가진 경우에만 `Assets/ThirdParty/ArmTutorial/`로 import한 뒤 `Tools > Nursing Sim > Phase 3 > 0. Build Player Hand Prefabs`를 실행해 실제 손 mesh prefab을 재생성한다.

`Library/`, `Temp/`, `Builds/`, `.codex/`, `.uloop/`, `.agents/`는 로컬 생성물이며 commit 대상이 아니다.

## 빌드 방법

1. `File > Build Profiles` 또는 `File > Build Settings`에서 Windows x64 또는 macOS 대상 플랫폼을 선택한다.
2. Scenes in Build에 위 4개 씬이 등록되어 있는지 확인한다.
3. 필요하면 `Development Build`를 끄고 `Build`를 실행한다.

## 기여

현재는 MVP 범위를 KABONE #3 근육주사 단일 시나리오로 제한한다. 절차·평가 로직을 바꿀 때는 `docs/09-references.md`의 KABONE 4.1 원문과 `docs/02-functional-spec.md`의 13단계 SO 매핑을 함께 확인한다.

## 라이선스

미정. Public repo 공개는 소스/에셋의 재사용 라이선스 부여를 의미하지 않는다. 외부 에셋별 라이선스는 [Assets/_Project/Art/LICENSES.md](./NursingSimulation/Assets/_Project/Art/LICENSES.md)와 [06-asset-plan.md](./docs/06-asset-plan.md)를 참조한다.
