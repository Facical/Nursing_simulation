# 10. 핸드 시뮬레이션 통합 계획

> 기준일: 2026-05-21. 본 문서는 근육주사(IM Injection) MVP에 데스크톱 핸드 조작을 통합하기 위한 실행 기준이다.

## 1. 기준과 범위

- 기준 술기는 **KABONE #3 근육주사**로 유지한다.
- corrected reference video: [현문사 기본간호학 핵심술기, 근육주사](https://www.youtube.com/watch?v=9f8rzK8ClE8)
- 영상은 손 조작, 카메라 연출, 물품 동선 참고로만 사용한다. 절차와 채점은 항상 [02-functional-spec.md](./02-functional-spec.md), [09-references.md](./09-references.md)가 우선이다.
- 1차 목표는 **키보드 이동 + 마우스 손목/그랩 기반 데스크톱 손/팔 조작**이다. VR, full finger IK, 모든 도구의 완전 물리 시뮬레이션은 MVP 후속 범위다.
- KABONE에 없는 Z-track, 마사지 회피, 부피별 부위 거부, 둔부 배면 회피 정책은 추가하지 않는다.

## 2. 현재 상태

- Unity: `6000.0.73f1`
- MCP: `io.github.hatayama.uloopmcp`, `com.ivanmurzak.unity.mcp` 계열 패키지가 설치되어 있다.
- `ScenarioRunner`는 `physicalStepIds`에 들어간 step만 3D physical controller로 라우팅할 수 있다.
- `Hand3DController`는 active hand의 키보드 이동, 좌클릭 토글 그랩, 우클릭 손목 회전, 빠른 마우스 이동 기반 손 비비기 신호를 제공한다. 양손 모두 입력 가능한 controller를 갖고, `Left Shift`로 active hand를 전환한다. `F1`은 조작법 overlay를 표시한다.
- `ToolInteraction3DStepController`는 현재 `InteractionKind.Pour`를 처리하며, 1차로 손위생 step에 사용한다.
- `Phase3WiringTool`은 `PlayerHand_Right`, `PlayerHand_Left`, 손소독제 펌프 3D 컴포넌트, close-up camera, physical step routing을 배치한다.

## 3. 구현 순서

1. 문서 기준 고정
   - README의 참고 영상을 근육주사 영상으로 정정한다.
   - 본 문서를 다음 세션의 기준 문서로 유지한다.

2. 핸드 모델 반입
   - `armTutorial.unitypackage`를 Unity로 import한다.
   - 모델과 텍스처는 `Assets/ThirdParty/ArmTutorial/`에 둔다.
   - `Assets/_Project/Art/LICENSES.md`에 출처, 라이선스, 반입일을 기록한다.
   - `doctorArms.fbx` 기반 `PlayerHand_Right`, `PlayerHand_Left` prefab을 만들고 각각 `Palm`, `GripSocket`, `IndexTip`, `ThumbTip` anchor를 둔다.

3. 핸드 조작 계층
   - `Hand3DController`는 입력과 손 위치만 담당한다. 오른손이 기본 active hand이며, `Left Shift`로 왼손/오른손을 전환한다.
   - 현재 키 매핑은 `A/S/D/F/Space` 손 이동, `Z/X/C/V/B` 엄지/검지/중지/약지/소지 curl, `Ctrl/R` 앉기/일어서기, `RMB` 손목 회전, `LMB` 집기/놓기, `Mouse Wheel` 손 높이 조절이다.
   - `HandActionEvents`는 `Grabbed`, `Released`, `PumpPressed`, `Rubbed`, `NeedleInserted`, `Aspirated`, `Injected` 신호를 제공한다.
   - `HandPoseController`는 optional `Animator`를 받아 idle, grab, pump press, rub, syringe hold 같은 pose 상태를 갱신한다.
   - step controller는 직접 입력을 읽지 않고 hand event를 구독한다.

4. 3D 대체 우선순위
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

- Step 2/5/9의 `InteractionKind.Pour` 손위생 step이 3D hand controller로 라우팅된다.
- 손이 펌프 근처에서 pump action을 발생시키면 pump count가 증가한다.
- Step 2는 pump count가 threshold 이상이고 `FaucetWaterZone` 접촉 후에만 rub time이 누적된다.
- Step 5/9는 pump count가 threshold 이상일 때 rub time이 누적된다.
- 손 모델 prefab이 없으면 기존 placeholder로 안전하게 fallback된다.
- 오른손과 왼손이 모두 씬에 배치되고, `Left Shift`로 active hand 전환이 가능하다.
- `F1` 조작법 overlay가 PlayMode에서 열리고 닫힌다.
- 3D hand event는 EditMode 테스트에서 입력 없이 직접 raise하여 검증할 수 있다.
- 기존 UI controller fallback은 유지된다.
