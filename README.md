<26/08/30 상세 변경점>

- 빌드 경로가 원작자 로컬 폴더 구조(`..\..\..\Client_Dev\...` 상대경로)로 하드코딩
  되어 있어서 다른 환경에서 어셈블리를 못 찾던 문제 — SptRoot 속성으로 오버라이드
  가능하게 수정, 실제 로컬 SPT 설치 경로를 기본값으로 지정

- `Bloom.cs` 생성자가 `AddComponent<UltimateBloom>()` 직후 아직 `Start()`가 안 돈
  상태의 필드(`m_BloomIntensities` 등)를 바로 읽다가 죽는 문제 — 매 프레임 반복되던
  NullReferenceException 도배의 원인이었음. null 체크 3곳 추가:
  - `ResetIntensities`에 배열 null 체크
  - `Bloom.Update()`에 `_ultimateBloom` null 체크
  - `GraphicsController.Update()`에 `_bloom` null 체크 (결정적 수정 — 생성자가 어떤
    이유로 실패하든 상관없이 매 프레임 도배를 막아주는 최종 안전망)
  - 덤으로 `UpdateMapSettings`/`UpdateBloomSettings`/`UpdateLensDust` 등 외부에서도
    호출되는 public 메서드들도 같은 이유로 null 안전 처리
  - 생성자 실패 자체는 라이드당 한 번 정도 여전히 뜨지만(무해함, 비주얼 차이 없음),
    매 프레임 반복되던 도배는 완전히 사라짐 — 실전 로그로 확인 완료

- `AmbientOcclusion.cs`도 같은 종류의 버그: `camera.GetComponent<HBAO>()`에 null
  체크 없이 바로 다음 줄에서 필드를 읽다가 죽던 문제 — null 체크 추가

- 나이트비전 착용 시 화면이 비정상적으로 어둡게 보이던 문제 — 이미지 이펙트가
  컴포넌트 순서대로 실행되는데 HBAO(AO)가 NightVision보다 먼저 실행되고 있어서,
  AO가 화면을 먼저 어둡게 만든 다음 나이트비전이 그 위에 증폭을 거는 구조였음.
  컴포넌트 순서를 바꾸는 안전한 런타임 API가 없어서, 대신 나이트비전이 켜져 있는
  동안만 AO를 꺼주고 꺼지면 바로 복구하는 방식으로 수정 (모든 맵에서 재현되는
  문제였음, 필드 테스트로 개선 확인 완료)

- 결과: 매 프레임 예외 스팸 제거, 나이트비전 밝기 정상화. 둘 다 필드 리포트로 확인 완료
