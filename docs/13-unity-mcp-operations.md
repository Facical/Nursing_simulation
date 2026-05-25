# 13. Unity MCP 운영 메모

> 기준일: 2026-05-25. 다음 Codex 세션에서 Unity MCP 계열 도구를 언제, 어떻게 쓸지 판단하기 위한 운영 기준이다.

## 1. 현재 결론

이 프로젝트에는 Unity를 다루는 MCP 계열이 두 개 설치되어 있다.

1. `uLoop / Unity CLI Loop`
   - 패키지: `io.github.hatayama.uloopmcp`
   - CLI: `/opt/homebrew/bin/uloop`
   - 역할: 컴파일, 테스트, 콘솔 로그, PlayMode 제어, 입력 시뮬레이션, Game/Scene/Hierarchy 스크린샷, hierarchy inspection.
   - 현재 확인: `uloop list` 성공. 실제 노출 도구는 15개다.

2. `AI Game Developer / Unity MCP`
   - 패키지: `com.ivanmurzak.unity.mcp` + animation, particlesystem, probuilder 확장
   - CLI: `/opt/homebrew/bin/unity-mcp-cli`
   - 설정: `NursingSimulation/UserSettings/AI-Game-Developer-Config.json`
   - 역할: scene, GameObject, component, prefab, asset, material, animation, animator, particle, ProBuilder 조작.
   - 현재 확인: MCP 서버 `http://localhost:26097` 연결 성공. `scene-list-opened` 호출 성공, `Simulation_IMInjection` loaded/clean 상태 확인.

두 도구는 경쟁 관계가 아니다. `uLoop`는 검증 루프와 PlayMode 조작에 강하고, `AI Game Developer MCP`는 Unity Editor 내부 오브젝트와 자산을 구조적으로 읽고 고치는 데 강하다.

## 2. 다음 세션 시작 루틴

Unity 관련 작업을 시작하면 먼저 아래 순서로 연결 상태를 확인한다.

```bash
unity-mcp-cli status /Users/macstudio_kang/Developer/Nursing_simulation/NursingSimulation
```

- `MCP server is reachable`이면 AI Game Developer MCP 서버는 살아 있다.
- `Unity Editor Process`가 warning이어도 곧바로 실패로 판단하지 않는다. 아래 scene tool이 성공하면 실제 도구 호출은 가능한 상태다.

```bash
unity-mcp-cli run-tool scene-list-opened /Users/macstudio_kang/Developer/Nursing_simulation/NursingSimulation --raw
```

```bash
uloop list --project-path /Users/macstudio_kang/Developer/Nursing_simulation/NursingSimulation
```

- Codex 샌드박스에서 `connect EPERM 127.0.0.1:8700`가 나오면 로컬 MCP 포트 접근 권한 문제다. 사용자에게 승인 요청 후 다시 실행한다.
- 현재 `uloop --help`에는 `get-project-info`가 보이지만 이 프로젝트의 Unity 쪽 실제 tool list에는 없다. 프로젝트 정보 확인은 `uloop get-version`을 사용한다.

```bash
uloop get-version --project-path /Users/macstudio_kang/Developer/Nursing_simulation/NursingSimulation
```

연결이 안 될 때 사용자에게 확인을 부탁할 메뉴:

- `Window > AI Game Developer`: host가 `http://localhost:26097`인지, MCP server가 실행 중인지 확인.
- `Window > Unity CLI Loop > Settings`: CLI 설치 상태와 Unity CLI Loop 서버 상태 확인.

## 3. 도구 선택 기준

코드 수정은 기본적으로 파일을 직접 읽고 패치한다. Unity MCP는 코드 수정 후 검증하거나 Unity Editor 상태를 바꿔야 할 때 적극 사용한다.

`uLoop`를 우선 쓰는 경우:

- C# 컴파일 확인
- EditMode/PlayMode 테스트 실행
- Unity Console error/warning 확인
- PlayMode 시작, 정지, 일시정지
- Game View, Scene View, Hierarchy 스크린샷
- UI 클릭, 키보드, 마우스 입력 시뮬레이션
- hand simulation 조작 재현, record/replay
- hierarchy를 JSON으로 덤프해서 넓게 훑기

`AI Game Developer MCP`를 우선 쓰는 경우:

- 열린 scene 목록과 scene root 상태 확인
- 특정 GameObject 검색, 생성, 삭제, 이동, parent 변경
- Component 추가, 제거, 값 읽기, 값 수정
- Prefab open/save/create/instantiate
- AssetDatabase 검색, asset data 읽기, material 생성/수정
- Animation/Animator 데이터 생성 또는 수정
- ParticleSystem 값 확인/수정
- ProBuilder shape 생성, face material 지정, mesh 정보 확인

직접 파일 편집을 우선하는 경우:

- `Assets/_Project/Scripts/**/*.cs` 코드 변경
- `docs/**/*.md` 문서 변경
- `.asset`, `.unity`, `.prefab`의 대량 텍스트 패치가 불안전한 경우
- KABONE 절차, 채점, 감점 사유 등 의료 정확성 관련 변경

동적 C# 실행은 마지막 수단이다. 조회성 코드나 Unity API로만 가능한 작은 Editor 작업에 한정하고, 시나리오 절차나 채점 규칙을 임의 생성하지 않는다.

## 4. 권장 검증 루프

코드 또는 Unity 자산을 바꾼 뒤 기본 루프:

```bash
uloop compile --project-path /Users/macstudio_kang/Developer/Nursing_simulation/NursingSimulation --force-recompile true --wait-for-domain-reload true
```

```bash
uloop get-logs --project-path /Users/macstudio_kang/Developer/Nursing_simulation/NursingSimulation --log-type Error --max-count 20
```

```bash
uloop run-tests --project-path /Users/macstudio_kang/Developer/Nursing_simulation/NursingSimulation --test-mode EditMode
```

씬 상태 확인:

```bash
unity-mcp-cli run-tool scene-list-opened /Users/macstudio_kang/Developer/Nursing_simulation/NursingSimulation --raw
```

PlayMode와 화면 확인:

```bash
uloop control-play-mode --project-path /Users/macstudio_kang/Developer/Nursing_simulation/NursingSimulation --action Play
```

```bash
uloop screenshot --project-path /Users/macstudio_kang/Developer/Nursing_simulation/NursingSimulation --window-name Game --capture-mode rendering
```

작업이 UI 레이아웃, HUD, 손 조작, 물품 배치에 닿으면 screenshot까지 확인한다. Scene 배치나 hand rig를 바꾸면 [11-treatment-room-environment-map.md](./11-treatment-room-environment-map.md)도 확인한다.

## 5. 현재 활성 도구 요약

`uLoop` 실제 노출 도구:

- `clear-console`
- `compile`
- `control-play-mode`
- `execute-dynamic-code`
- `find-game-objects`
- `focus-window`
- `get-hierarchy`
- `get-logs`
- `record-input`
- `replay-input`
- `run-tests`
- `screenshot`
- `simulate-keyboard`
- `simulate-mouse-input`
- `simulate-mouse-ui`

AI Game Developer MCP에서 활성화된 주요 도구:

- Scene: `scene-create`, `scene-get-data`, `scene-list-opened`, `scene-open`, `scene-save`, `scene-set-active`, `scene-unload`
- GameObject: `gameobject-create`, `gameobject-find`, `gameobject-modify`, `gameobject-destroy`, `gameobject-duplicate`, `gameobject-set-parent`
- Component: `gameobject-component-add`, `gameobject-component-get`, `gameobject-component-modify`, `gameobject-component-destroy`, `gameobject-component-list-all`
- Object/Asset: `object-get-data`, `object-modify`, `assets-find`, `assets-find-built-in`, `assets-get-data`, `assets-modify`, `assets-refresh`
- Prefab/Material/Shader: `assets-prefab-open`, `assets-prefab-save`, `assets-prefab-close`, `assets-prefab-create`, `assets-prefab-instantiate`, `assets-material-create`, `assets-shader-get-data`, `assets-shader-list-all`
- Animation/Animator: `animation-create`, `animation-get-data`, `animation-modify`, `animator-create`, `animator-get-data`, `animator-modify`
- Tests/Console/Script: `tests-run`, `console-get-logs`, `script-execute`, `unity-tool-list`
- Rendering/FX: `screenshot-isolated`, `particle-system-get`, `particle-system-modify`
- ProBuilder: `probuilder-create-shape`, `probuilder-delete-faces`, `probuilder-extrude`, `probuilder-get-mesh-info`, `probuilder-set-face-material`

AI Game Developer MCP에서 현재 꺼진 주요 도구:

- 패키지 설치/삭제/검색: `package-add`, `package-remove`, `package-search`, `package-list`
- 스크립트 파일 쓰기/삭제/읽기: `script-update-or-create`, `script-delete`, `script-read`
- 에디터 상태/선택 제어: `editor-application-*`, `editor-selection-*`
- destructive asset 조작: `assets-delete`, `assets-move`, `assets-copy`, `assets-create-folder`
- profiler 계열, reflection 계열, 일부 screenshot 계열

꺼진 도구를 켜거나 CLI/패키지를 업데이트하지 않는다. 사용자가 명시적으로 요청할 때만 변경한다.

## 6. 프로젝트별 주의점

- 의료 절차와 채점은 항상 [09-references.md](./09-references.md), [02-functional-spec.md](./02-functional-spec.md)를 먼저 확인한다.
- KABONE에 없는 best-practice를 MCP나 동적 코드로 추가하지 않는다.
- 시나리오 로직은 MonoBehaviour에 하드코딩하지 않고 ScriptableObject step과 controller 구조를 유지한다.
- `.uloop/settings.permissions.json`의 `allowThirdPartyTools=false`, `dynamicCodeSecurityLevel=1`을 유지한다.
- `unity-mcp-cli`는 현재 `0.73.0`이고 `0.75.3` 업데이트 알림이 있다. 자동 업데이트하지 않는다.
- `com.ivanmurzak.unity.mcp` 패키지는 `0.75.3`, Unity는 `6000.0.73f1`, uLoop server version은 `2.1.3`으로 확인했다.
