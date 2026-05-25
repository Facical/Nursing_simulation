# Nursing Simulation (가칭)

간호대 학부생을 위한 PC(Win/Mac) 3D **기본간호학 술기 시뮬레이터**. 아트 방향은 **Semi-realistic(인물·환경) + 포토리얼(의료 도구 클로즈업)** 혼합.
1차 MVP는 **근육주사(IM Injection)** 단일 시나리오를 타깃으로, 3D 도구를 직접 조작(drag/click)하며 임상적 추론을 반복 훈련하는 것을 목표로 한다. Practigame(https://www.practigame.com)을 벤치마크하되 국내 기본간호학 교과과정·평가 체크리스트에 맞춘다.

- **플랫폼**: Windows / macOS 스탠드얼론 빌드
- **언어**: 한국어
- **주 대상**: 간호대 학부생 (기본간호학 실습 예습/복습)
- **1차 시나리오**: IM Injection (근육주사)
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
9. [10. 핸드 시뮬레이션 통합 계획](./docs/10-hand-simulation-plan.md) — 데스크톱 손/팔 조작 통합 계획
10. [11. 처치실 Environment Map](./docs/11-treatment-room-environment-map.md) — `Simulation_IMInjection` 처치실 오브젝트/좌표 기준표

## 개발 환경 (예정)

| 항목 | 값 |
|---|---|
| Unity | 6 LTS (대안 2022.3 LTS) |
| Render Pipeline | URP |
| Target | Windows 64-bit, macOS (Apple Silicon + Intel) |
| 주요 패키지 | Input System, TextMeshPro, Cinemachine |
| IDE | Rider 또는 VS Code + Unity extension |
| VCS | Git + Git LFS (바이너리 에셋용) |

## 빌드 방법 (초안)

> Unity 프로젝트 생성 후 갱신 예정. 현재는 기획 단계.

1. Unity Hub에서 Unity 6 LTS로 프로젝트 열기
2. Package Manager에서 누락 패키지 설치 확인
3. `File → Build Settings`에서 대상 플랫폼 선택
4. `Scenes in Build`에 `MainMenu`, `Briefing`, `Simulation_IMInjection`, `Debriefing` 추가
5. `Build` 실행

## 기여

현재는 단독 개발. 감수자(간호학 교수/임상 간호사) 섭외 후 `docs/02-functional-spec.md`의 절차 검증을 1차 품질 게이트로 삼는다.

## 라이선스

미정 (내부 교육용 우선, 에셋 라이선스는 [06-asset-plan.md](./docs/06-asset-plan.md) 참조).
