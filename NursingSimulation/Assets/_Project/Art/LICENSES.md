# Third-Party Asset Licenses

이 프로젝트에 포함된 모든 외부 에셋의 출처·라이선스·증빙을 기록합니다. 새 에셋 임포트 시 반드시 이 표에 추가하세요. 템플릿은 `docs/06-asset-plan.md §5` 참조.

## 에셋 목록

| 자산명 | 제공처 | 라이선스 | 상용 가능 | 재배포 조건 | 다운로드일 | 증빙 |
|---|---|---|:---:|---|---|---|
| Microsoft Rocketbox (avatars) | [github.com/microsoft/Microsoft-Rocketbox](https://github.com/microsoft/Microsoft-Rocketbox) | **MIT** | ✓ | 저작권 고지 유지 | 2026-04-22 | commit `0943055db6ec570bcef9f2c8b41c9e5467c808f9` (2022-10-02) |
| Pretendard (Regular + Bold) | [github.com/orioncactus/pretendard](https://github.com/orioncactus/pretendard) | **SIL OFL 1.1** | ✓ | 저작권 고지 유지 + 폰트 자체 상품화 금지 | 2026-04-22 | v1.3.9 배포본. `Pretendard-LICENSE.txt` 동봉 |
| armTutorial hand/arms package | 사용자 제공 `armTutorial.unitypackage` | 사용자 제공 / 재배포 전 확인 필요 | 확인 필요 | 재배포 전 원 라이선스 확인 | 2026-05-21 | `Assets/ThirdParty/ArmTutorial/` |

## 프로젝트에 복사된 아바타 (부분 임포트)

전체 115 avatars + 417 animations 중 Phase 0 테스트 목적으로 아래만 `Assets/ThirdParty/Rocketbox/` 에 복사:

- `Professions/Medical_Male_01` — 의료진 후보
- `Adults/Male_Adult_01` — 환자 후보
- `Editor/FixRocketboxMaxImport.cs` — 3DSMax Z-up 축 자동 보정 (AssetPostprocessor)

Phase 1 진입 전 필요에 따라 Medical_Male_02~05 / Medical_Female_01~03 및 Adults 추가 복사.

## MIT 라이선스 고지 (Rocketbox)

```
MIT License

Copyright (c) 2020 Microsoft

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
```

배포 빌드 크레딧/라이선스 화면에 상기 고지 포함 필수.
