# 10. Hard Hand Simulator 통합 계획

> 기준일: 2026-05-25. 본 문서는 근육주사(IM Injection) MVP에서 세밀한 데스크톱 손 조작을 **선택형 Hard Hand Simulator 모드**로 유지하기 위한 실행 기준이다.

## 1. 기준과 범위

- 기준 술기는 **KABONE #3 근육주사**로 유지한다.
- corrected reference video: [현문사 기본간호학 핵심술기, 근육주사](https://www.youtube.com/watch?v=9f8rzK8ClE8)
- 영상은 손 조작, 카메라 연출, 물품 동선 참고로만 사용한다. 절차와 채점은 항상 [02-functional-spec.md](./02-functional-spec.md), [09-references.md](./09-references.md)가 우선이다.
- MVP의 기본 경로는 **Basic Simulation Mode**다. UI 카드/버튼을 누르면 손 모델 애니메이션과 3D 환경 반응이 실행된다.
- **Hard Hand Simulator Mode**는 Settings에서 ON으로 켜는 고급 모드다. 기존 키보드 이동 + 마우스 손목/그랩 기반 데스크톱 손/팔 조작을 이 모드에 한정한다.
- VR, full finger IK, 모든 도구의 완전 물리 시뮬레이션은 MVP 후속 범위다.
- KABONE에 없는 Z-track, 마사지 회피, 부피별 부위 거부, 둔부 배면 회피 정책은 추가하지 않는다.

## 2. 현재 상태

- Unity: `6000.0.73f1`
- MCP: `io.github.hatayama.uloopmcp`, `com.ivanmurzak.unity.mcp` 계열 패키지가 설치되어 있다.
- `ScenarioRunner`는 `physicalStepIds`에 들어간 step만 3D physical controller로 라우팅할 수 있다.
- `Hand3DController`는 active hand의 키보드 이동, 좌클릭 토글 그랩, 우클릭 손목 회전, 빠른 마우스 이동 기반 손 비비기 신호를 제공한다. 양손 모두 입력 가능한 controller를 갖고, `Left Shift`로 active hand를 전환한다. `F1`은 조작법 overlay를 표시한다.
- `ToolInteraction3DStepController`는 현재 `InteractionKind.Pour`를 처리하며, 1차로 손위생 step에 사용한다.
- `Phase3WiringTool`은 `PlayerHand_Right`, `PlayerHand_Left`, 손소독제 펌프 3D 컴포넌트, close-up camera, physical step routing을 배치한다.
- public clone에서는 라이선스 미확정 `ArmTutorial` 원본 에셋 없이도 placeholder 손 prefab으로 열린다.

## 3. 구현 순서

1. 모드 설정 고정
   - MainMenu/Settings에 `Hard Hand Simulator` 토글을 둔다.
   - 기본값은 OFF이며, PlayerPrefs에는 `BasicSimulation`을 기본 모드로 저장한다.
   - 토글 OFF 상태에서도 근육주사 13개 단계가 끝까지 진행 가능해야 한다.

2. Basic Simulation Mode 액션 계층
   - 현재 단계의 카드/버튼은 의미 단위 이벤트를 발행한다.
   - 예: `ConfirmPrescription`, `PressPump`, `RubHands`, `SelectTool`, `SelectLandmark`, `Aspirate`, `Inject`.
   - 손 모델은 이벤트에 맞는 애니메이션을 재생한다.
   - step controller는 손 조작 숙련도가 아니라 KABONE 절차 선택·순서·확인 여부를 평가한다.

3. Hard Hand Simulator 입력 계층
   - `Hand3DController`는 입력과 손 위치만 담당한다. 오른손이 기본 active hand이며, `Left Shift`로 왼손/오른손을 전환한다.
   - 현재 키 매핑은 `A/S/D/F/Space` 손 이동, `Z/X/C/V/B` 엄지/검지/중지/약지/소지 curl, `Ctrl/R` 앉기/일어서기, `RMB` 손목 회전, `LMB` 집기/놓기, `Mouse Wheel` 손 높이 조절이다.
   - `HandActionEvents`는 `Grabbed`, `Released`, `PumpPressed`, `Rubbed`, `NeedleInserted`, `Aspirated`, `Injected` 신호를 제공한다.
   - `HandPoseController`는 optional `Animator`를 받아 idle, grab, pump press, rub, syringe hold 같은 pose 상태를 갱신한다.
   - step controller는 직접 입력을 읽지 않고 의미 단위 event를 구독한다.

4. Basic 모드 우선 구현 순서
   - Step 1 처방 확인: 처방 카드, 환자 카드, 투약원칙 카드, 확인 완료 버튼.
   - Step 2/5/9/12 손위생: 손위생 액션 버튼 + 손 애니메이션 + rub timer.
   - Step 3 약물 준비: 앰플 확인, 흡입, 공기 제거를 카드/미니 UI로 진행.
   - Step 8b 랜드마크 촉진: 3개 landmark를 순서대로 선택하고 손가락 애니메이션을 재생.
   - Step 10 주사 시퀀스: 90도 각도, 흡인, 혈액 없음 확인, 천천히 주입을 단계 버튼과 인디케이터로 진행.
   - Step 12 폐기: 폐기 대상과 용기 선택 UI로 진행.

5. Hard Hand 확장 후보
   - Step 2 손위생 #1: pump press + faucet water contact + rub timer + HUD 통과.
   - Step 5, Step 9 손소독제 손위생: 같은 `InteractionKind.Pour` controller를 재사용한다.
   - Step 3 약물 준비: 앰플/주사기 그랩, 라벨 확인, 용량 인출, 공기 제거.
   - Step 8b 랜드마크 촉진: `IndexTip` collider로 대전자, ASIS, 장골능 marker를 순서 touch.
   - Step 9 피부 소독: 알콜솜 grabbable + 안쪽에서 바깥쪽 원형 coverage.
   - Step 10 주사 시퀀스: 주사기 attach, 90도 각도, 삽입 trigger, 흡인, slow injection hold.
   - Step 12 폐기: 사용 도구 drop target과 재캡 여부 추적.

## 4. MCP 운영

- uLoop compile:
  `uloop compile --project-path /Users/macstudio_kang/Developer/Nursing_simulation/NursingSimulation --force-recompile true --wait-for-domain-reload true`
- uLoop EditMode tests:
  `uloop run-tests --project-path /Users/macstudio_kang/Developer/Nursing_simulation/NursingSimulation --test-mode EditMode`
- uLoop logs:
  `uloop get-logs --project-path /Users/macstudio_kang/Developer/Nursing_simulation/NursingSimulation --log-type Error --max-count 20`
- uLoop screenshot:
  `uloop screenshot --project-path /Users/macstudio_kang/Developer/Nursing_simulation/NursingSimulation --window-name Game --capture-mode rendering`
- AI Game Developer MCP status:
  `unity-mcp-cli status /Users/macstudio_kang/Developer/Nursing_simulation/NursingSimulation`
- AI Game Developer MCP scene check:
  `unity-mcp-cli run-tool scene-list-opened /Users/macstudio_kang/Developer/Nursing_simulation/NursingSimulation --raw`

운영 규칙:

- `NursingSimulation/.uloop/settings.permissions.json`의 `allowThirdPartyTools=false`, `dynamicCodeSecurityLevel=1`을 유지한다.
- 동적 C# 실행은 마지막 수단으로만 사용한다.
- 각 구현 단위는 `compile -> logs -> scene check -> Play/screenshot` 순서로 확인한다.

## 5. Acceptance Criteria

- Settings의 `Hard Hand Simulator` 기본값은 OFF다.
- OFF 상태에서 Step 1 처방 확인 카드와 Basic Simulation Mode 액션으로 전체 13단계 완주가 가능해야 한다.
- OFF 상태에서 사용자는 `ASDF/Space`, `Z/X/C/V/B` 같은 세밀 손 조작을 몰라도 시나리오를 진행할 수 있어야 한다.
- ON 상태에서는 기존 손/팔 조작 입력과 `F1` 조작법 overlay가 활성화된다.
- Hard Hand 입력과 Basic UI 액션은 가능한 한 같은 의미 단위 event로 step controller에 전달된다.
- 두 모드는 같은 KABONE 절차·채점 기준을 사용한다.
- 손 모델 prefab이 없으면 기존 placeholder로 안전하게 fallback된다.
- 3D hand event는 EditMode 테스트에서 입력 없이 직접 raise하여 검증할 수 있다.
