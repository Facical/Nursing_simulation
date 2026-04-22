# 05. 데이터 모델 (Scenario SO 스키마)

> 시나리오는 **코드가 아닌 데이터**로 기술한다. 간호학 감수자(코드 모름)가 인스펙터에서 수치·지문을 조정할 수 있어야 한다.

## 1. 최상위: `NursingScenario` (ScriptableObject)

```csharp
// namespace NursingSim.Data
[CreateAssetMenu(menuName = "NursingSim/Scenario", fileName = "SO_Scenario_")]
public class NursingScenario : ScriptableObject {
    public string scenarioId;              // e.g., "SCN_IM_INJECTION_001"
    [TextArea] public string title;        // "근육주사 투약"
    [TextArea] public string briefingText; // 브리핑 본문
    public ScenarioLevel level;            // Easy/Normal/Hard
    public int maxScore = 100;             // 항상 100 권장
    public string sceneKey;                // "Simulation_IMInjection"
    public PatientProfile patient;
    public Prescription prescription;
    public List<ScenarioStep> steps;       // 아래 추상 타입의 자식 에셋들
    public DebriefingTemplate debriefing;
}
```

## 2. 단계: `ScenarioStep` (추상 SO)

```csharp
public abstract class ScenarioStep : ScriptableObject {
    public string stepId;                 // e.g., "STEP_FIVE_RIGHTS"
    [TextArea] public string title;       // "처방 확인"
    [TextArea] public string instruction; // 학생에게 보일 지시문
    public int weight = 10;               // 이 단계의 만점
    public FeedbackTiming feedbackTiming; // Instant / Deferred
    public bool isCriticalGate;           // true면 실패 시 치명 실수
    [TextArea] public string failHint;    // 실패 시 힌트
    [TextArea] public string theoryRef;   // 디브리핑 학습 포인트 근거
    public abstract StepCategory Category { get; }
}
```

### 하위 타입

| 타입 | 용도 | 주요 필드 |
|---|---|---|
| `ChecklistStep` | 5 Rights 같은 체크박스 검증 | `List<ChecklistItem> items` (label, required, distractor) |
| `ToolInteractionStep` | 손위생·물품준비·소독 | `InteractionKind kind`(Click, DragPath, Pour), `target` 참조, `thresholds`(시간·커버리지) |
| `DialogueStep` | 환자 확인·설명 | `List<DialogueChoice> choices`(text, isCorrect, score) |
| `SelectionStep` | 주사 부위 선정 | `List<SelectionOption>`(id, label, score, reasonIfWrong) |
| `LandmarkPickStep` | 랜드마크 촉진 | `List<LandmarkPoint>`(id, label, worldAnchor), `requiredOrder` |
| `SequenceStep` | 주사·흡인·주입·발침 복합 | `List<SequenceAction>`(ToolUse, AngleCheck, SpeedCheck) |
| `ToggleGroupStep` | 동의/프라이버시 등 | `List<ToggleItem>`(label, required) |

각 타입은 런타임에 대응되는 **StepController** (MonoBehaviour)가 있고, `ScenarioRunner`가 팩토리로 인스턴스화한다.

## 3. 평가 결과: `StepResult`

```csharp
public class StepResult {
    public string stepId;
    public int earned;          // 0..weight
    public int weight;
    public TimeSpan duration;
    public List<string> deductionReasons;  // enum 매핑된 문자열
    public bool criticalFail;
}
```

`DebriefingReport = List<StepResult> + totalScore + meta`.

## 4. 감점 사유 (Enum)

```csharp
public enum DeductionReason {
    HandHygieneSkipped,
    HandHygieneTooShort,
    WrongBottleAlcoholSkipped,
    DoseOutOfTolerance,
    DistractorItemSelected,
    RequiredItemMissing,
    ClosedQuestionOnly,
    OneIdentifierOnly,
    PrivacyNotSecured,
    SiteVolumeExceeded,
    SiteUsesDorsogluteal,        // 권장 안 함 정책 감점
    LandmarkOrderWrong,
    AsepticBreach,
    DisinfectionPathWrong,
    DisinfectionDryTimeShort,
    AngleOutOfRange,
    InjectionTooFast,
    InjectionTooSlow,
    WithdrawalAngleDiff,
    SiteRubbedAfter,
    NeedleRecapped,
    SharpsBinMissed,
    RecordFieldMissing,
    PostObservationSkipped,
}
```

문자열 현지화는 `Localization/Deductions_ko.csv`에서 매핑.

## 5. 예시 JSON 표현 (내보내기 용도)

SO 에셋 자체는 Unity가 관리하지만, 외부 도구(감수자용 Excel 등)와의 교환을 위해 JSON export를 제공한다.

```json
{
  "scenarioId": "SCN_IM_INJECTION_001",
  "title": "근육주사 투약",
  "maxScore": 100,
  "steps": [
    {
      "type": "ChecklistStep",
      "stepId": "STEP_FIVE_RIGHTS",
      "title": "처방 확인",
      "weight": 10,
      "feedbackTiming": "Deferred",
      "items": [
        { "label": "환자(Right Patient)", "required": true },
        { "label": "약물(Right Drug)",    "required": true },
        { "label": "용량(Right Dose)",    "required": true },
        { "label": "경로(Right Route)",   "required": true },
        { "label": "시간(Right Time)",    "required": true }
      ]
    },
    {
      "type": "ToolInteractionStep",
      "stepId": "STEP_HAND_HYGIENE",
      "title": "손위생",
      "weight": 5,
      "feedbackTiming": "Instant",
      "isCriticalGate": true,
      "kind": "Pour",
      "thresholds": { "minPumps": 2, "minDurationSec": 15 }
    }
  ]
}
```

## 6. 저장 데이터 (playhistory.json)

```json
{
  "version": 1,
  "plays": [
    {
      "scenarioId": "SCN_IM_INJECTION_001",
      "startedAt": "2026-04-22T10:00:00+09:00",
      "endedAt":   "2026-04-22T10:12:34+09:00",
      "totalScore": 82,
      "criticalFails": 0,
      "stepResults": [
        { "stepId": "STEP_FIVE_RIGHTS", "earned": 10, "weight": 10, "reasons": [] },
        { "stepId": "STEP_HAND_HYGIENE", "earned": 4, "weight": 5,
          "reasons": ["HandHygieneTooShort"] }
      ]
    }
  ]
}
```

## 7. 감수자용 편집 UX

- **Odin Inspector** 또는 기본 인스펙터로 `NursingScenario.asset`을 열면 단계 순서, 지문, 가중치, 감점 사유를 바로 편집 가능.
- `List<ScenarioStep>`은 각 요소가 **서브에셋**(`AssetDatabase.AddObjectToAsset`)이어야 드래그 없이 리오더/추가 가능 → 별도 `ScenarioEditor` 툴 작성 고려(Phase 3).
- 모든 텍스트는 Localization Table에 키로 참조(현재는 `ko`만 채움). Step 필드는 **Localization Key**를 담고, 실제 문자열은 테이블에서.
