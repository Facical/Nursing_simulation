# 06. 에셋 계획 (3D / 오디오 / UI)

## 1. 전략 요약

- 아트 방향은 **Semi-realistic 인물·환경 + 포토리얼 의료 도구**로 결정 (`docs/03-technical-spec.md §1.1`).
- **인물은 Microsoft Rocketbox(MIT) 무료 에셋을 1차 채택** — 캐릭터 조달 비용·시간 리스크(R1) 해소.
- **환경**은 에셋스토어 병실/처치실 팩에서 필요 부분만. **의료 도구**는 클로즈업 대상이므로 품질·정확성 투자 집중 — 개별 구매 또는 외주.
- 모든 에셋의 **라이선스와 재배포 조건을 최초 확보 시 기록** (아래 §5 라이선스 트래커).

## 2. 필수 3D 에셋 목록

### 2.1 환경
| 자산 | 용도 | 후보 경로 |
|---|---|---|
| 처치실 / 병실 (1칸) | 메인 씬 배경 | Unity Asset Store "Hospital Room", Sketchfab CC-BY, Quixel Megascans(조합) |
| 커튼 (프라이버시) | 환자 주변 차폐 애니메이션 | 간단 제작 가능 (Cloth or Blend Shape) |
| 처치용 트레이 | 물품 배치 | 에셋스토어 medical pack |
| 이동식 카트 | 물품 보관 | 동상 |

### 2.2 환자·의료진 캐릭터 — **Microsoft Rocketbox를 1차 후보로 채택**

#### Microsoft Rocketbox (최우선)
- **소스**: https://github.com/microsoft/Microsoft-Rocketbox
- **라이선스**: **MIT** (LICENSE.md 확인) — 상용 포함 자유 사용, 저작권 고지만 유지. 블로그 제목의 "research and academic use"는 마케팅 문구이며 실제 조항은 MIT.
- **구성**: 115개 리깅 아바타(Adults / Children / Professions), 공통 스켈레톤, 417개 공용 애니메이션. FBX 포맷.
- **이 프로젝트에 유용한 카테고리**
  - `Assets/Avatars/Professions/Medical_Female_01~03`, `Medical_Male_01~05` — **간호사/의료진 NPC**로 즉시 사용
  - `Assets/Avatars/Adults/...` — **환자**(병원 가운 입힐 평균 체형 성인) 후보
- **스타일**: Semi-realistic(중폴리). 아트 방향 결정과 일치(`docs/03-technical-spec.md §1.1`).
- **Unity 임포트 절차**
  1. 레포 clone 후 `Assets/Avatars/**/*.fbx`를 프로젝트로 복사
  2. `Source/Unity/FixRocketboxMaxImport.cs`를 `Assets/Editor/`에 배치 (3ds Max 재질 → Unity 재질 자동 변환)
  3. 아바타의 Rig Import Settings에서 `Humanoid` 설정 — Mixamo 애니메이션과도 재사용 가능
  4. URP 프로젝트이므로 재질을 URP/Lit으로 일괄 변환 (수작업 또는 마이그레이션 유틸)
- **감점**: 얼굴 디테일(피부 SSS, 눈 리얼리티)이 현대 포토리얼 캐릭터 대비 약함. 클로즈업 얼굴 샷은 피하도록 연출.

#### 대체/보완 옵션
| 자산 | 용도 | 경로 |
|---|---|---|
| 병원 가운 | 환자 의상 | 에셋스토어 medical apparel, Rocketbox `Casual` 위에 커스텀 가운 메시 |
| Mixamo 애니메이션 | 앉기/눕기/반응 | https://mixamo.com — Rocketbox 스켈레톤과 Humanoid rig로 호환 |
| Character Creator 4 / MetaHuman | **MVP 범위 외** (향후 v0.2 업그레이드 경로로만 보유) | 라이선스 재확인 필수 |

### 2.3 의료 도구 (정확성 중요)
| 자산 | 비고 |
|---|---|
| 주사기 1cc / 3cc / 5cc | 눈금과 라벨 가독성 |
| 바늘 (18G, 21G, 23G, 25G, 26G) | 게이지별 색상 허브 구분 — 21G 녹색, 23G 파란색 등 국제 표준 |
| 바이알 / 앰플 | 라벨 교체 가능한 머티리얼 (약물명 바꿔 사용) |
| 알콜솜 개별 포장 | 포장 벗기기 애니메이션 |
| 멸균 장갑 박스 | 착용 애니메이션(간단화) |
| 샤프스 컨테이너 | 뚜껑 개폐 가능 |
| 손소독제 펌프 | 펌프 애니메이션 |
| 커다란 알콜 솜 통 | 선택 |
| 전자 처방전 태블릿(UI only) | 2D UI로 대체 가능 |

### 2.4 UI 아이콘·일러스트
- 에셋: Google Material Symbols (Apache 2.0), Lucide(ISC), 한글 직접 디자인은 최소화.
- 디브리핑 그래프: Unity UI + TextMeshPro만으로 구현(차트 라이브러리 도입 보류).

## 3. 오디오 에셋

| 분류 | 예시 | 조달 |
|---|---|---|
| UI | 클릭, 포커스, 정답/오답 | Freesound CC0, Zapsplat(계정 필요) |
| 앰비언트 | 병실 룸톤, 먼 심전도 알람(약하게) | Freesound, Epidemic Sound(유료) |
| 도구 | 주사기 캡, 앰플 커팅, 펌프, 장갑 | Freesound CC0 조합 |
| 내레이션 | 브리핑 한국어 TTS 또는 성우 | 네이버 CLOVA Voice, 전문 성우 외주 |

## 4. 룩(Look) 품질 체크 항목

- **의료 도구(포토리얼 지향)**: PBR 텍스처 완비(Albedo/Normal/Roughness/Metallic/AO), 2K 해상도, 실물 치수 기반(주사기 전장 약 10cm 등). 라벨·눈금·게이지 색상은 가독성 최우선.
- **인물·환경(semi-realistic)**: Rocketbox 기본 머티리얼 → URP/Lit 일괄 변환, 1K 텍스처 유지. 환자 얼굴은 **클로즈업 회피 연출**로 보완(4K 업스케일 불필요).
- **조명**: URP 간접광 + Reflection Probe 1~2개, 처치실은 Baked GI.
- **포스트**: Color Grading(LUT), Bloom 약하게, Depth of Field는 상호작용 줌 시에만.

## 5. 라이선스 트래커 (템플릿)

프로젝트에 `Assets/_Project/Art/LICENSES.md`로 추가해 운영.

| 자산명 | 제공처 | 라이선스 | 상용 가능 | 재배포 조건 | 구매/다운로드일 | 증빙 경로 |
|---|---|---|:---:|---|---|---|
| Microsoft Rocketbox (115 avatars + 417 anims) | GitHub microsoft/Microsoft-Rocketbox | **MIT** | ✓ | 저작권 고지 유지 | (예정) | github commit SHA |
| (예) Hospital Room Pack | Unity Asset Store | Unity AS EULA | ✓ | 빌드에 포함만 가능 | 2026-04-22 | invoice-xxx.pdf |
| (예) Mixamo "Idle" | Adobe Mixamo | Royalty-Free | ✓ | 재배포 에셋 자체 금지 | 2026-04-22 | - |

## 6. MVP 에셋 예산 초안 (참고)

| 항목 | 예상 비용 (USD) |
|---|---:|
| 처치실/병실 에셋 팩 | 20 ~ 60 |
| 캐릭터 (Rocketbox, MIT) | **0** |
| 의료 도구 팩 (주사기·바이알 등, 포토리얼) | 20 ~ 80 |
| 텍스처 (선택) Quixel | 필요시 구독 |
| SFX 팩 | 0 ~ 30 |
| 한국어 폰트 | 0 (OFL 나눔고딕/프리텐다드) |
| **MVP 합계 (러프)** | **40 ~ 170** |
| *v0.2 옵션: 포토리얼 환자로 업그레이드* | *+30 ~ +150* |

## 7. 대체 전략 (에셋 수급 실패 시)

- Rocketbox 임포트에 재질/rig 이슈 발생 → Mixamo 캐릭터 + 별도 의료진 가운 메시로 대체.
- 병실 에셋이 과하면 → **minimal 처치실** (벽 + 침대 + 트레이 + 조명)로 범위 축소. MVP 평가에 지장 없음.
- 포토리얼 의료 도구 팩 품질 부족 → 핵심 6개(주사기·바늘·바이알·앰플·알콜솜·샤프스)만 외주 또는 자체 Blender 제작.
