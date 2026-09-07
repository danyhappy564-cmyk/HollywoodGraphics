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

---

<26/09/03 상세 변경점>

- (08/30 수정 정정) 나이트비전 AO 억제 가드가 "밝아졌다"고 확인했던 건 반쪽짜리
  확인이었음 — `NightVision.enabled`로 온/오프를 판단했는데, 이 필드는 켜는
  순간(toggle-ON)엔 정상 반영되지만 끄는 순간(toggle-OFF)엔 전혀 안 바뀜(N키로
  여러 번 껐다 켜도 로그상 "꺼짐" 전환이 단 한 번도 안 뜸). 즉 나이트비전을 라이드
  중 한 번이라도 쓰면, 그 이후로는 나이트비전을 꺼도 AO가 라이드 끝날 때까지 계속
  억제된 채로 남아있던 버그 — 필드 리포트로 "나이트비전 안 써도 AO가 꺼진 것 같다"
  로 발견됨

- 정확한 신호를 몰라서(디컴파일 없이는 필드명 확인 불가) `NightVision` 컴포넌트의
  모든 bool 필드를 리플렉션으로 훑어서 실제로 토글 시점에 값이 바뀌는 걸 로그로
  잡는 진단을 임시로 넣음 — 실제 라이드에서 N키 여러 번 눌러본 결과 private 필드
  `_on`이 정확히 토글에 맞춰 True/False 전환됨을 확인 (`.enabled`는 계속 True로
  고정)

- `_on`을 리플렉션으로 직접 읽도록 가드 로직 교체, 진단용으로 넣었던 전체 필드
  덤프/폴링 코드는 제거 (더 이상 필요 없음 — 알아낸 필드 하나만 매 프레임 읽음).
  `_on`을 못 찾는 미래 빌드에서는 조용히 오작동하는 대신 경고 로그 남기고 AO
  상시 켜짐(수정 전 안전한 기본값)으로 폴백

- 결과: 나이트비전 온/오프 양방향 전환 모두 정상 추적 확인 (`_on` 필드 기준). AO가
  나이트비전 끈 뒤에도 계속 억제된 채로 남던 문제 해소

---

<26/09/07 상세 변경점 — SPT 4.1.5 대응>

**주의: 이 변경분은 컴파일 검증이 안 된 상태입니다.** 이 플러그인은 게임 어셈블리
(`Assembly-CSharp.dll` 등)를 참조해 빌드되는데 그건 실제 설치본에만 있습니다.

- **빌드 경로를 `E:\SPT 4.1`로 변경.** 다른 경로면 `-p:SptRoot=<설치 루트>`

- **게임/BepInEx 위치를 가정하지 않고 탐색.** 4.1이 서버를 `SPT_Runtime\` 밑으로
  옮겼습니다. 게임과 BepInEx는 보통 루트에 남지만 "보통"은 "항상"이 아니라서,
  루트 → `SPT_Runtime\` 순으로 찾고 게임과 BepInEx를 각각 따로 찾습니다.
  세 가지 배치로 확인. `-p:GameRoot=` `-p:BepInExRoot=`로 직접 지정 가능

- 참조가 안 잡히면 어느 폴더에 뭐가 없는지 한 줄로 말하고 멈춥니다

- 빌드 후 설치 복사를 `copy /Y` 대신 MSBuild `Copy`로 교체. 게임이 켜진 채면 DLL이
  잠기는데 그걸로 빌드를 실패시키지 않고 경고로 끝냅니다 (`-p:AutoInstall=false`)

- **`TarkovApplication.method_41`을 로그로 드러냄 — 이 레포에서 제일 주의할 부분.**
  번호가 붙은 이름은 난독화기가 세어서 붙인 거라 BSG가 메서드를 넣고 빼면 밀립니다.
  그리고 이 패치는 하필 **조용히 깨지는 쪽**입니다: prefix가 읽는 `_raidSettings`는
  메서드가 아니라 `TarkovApplication` 타입에서 찾으므로, 번호가 밀려 엉뚱한 메서드에
  붙어도 바인딩은 멀쩡히 되고 그냥 **엉뚱한 시점에 실행됩니다.**

  어셈블리 없이는 막을 수 없어서, 대신 실제로 잡힌 메서드 시그니처를 로그에 찍습니다.
  라이드 들어갈 때 `Running raid initialization` 줄이 안 뜨거나 맵별 그래픽 오버라이드가
  안 먹으면 이걸 의심하시면 됩니다

- `NightVision._on` 리플렉션(08/30·09/03 수정분)은 이미 폴백이 있어서 그대로 뒀습니다 —
  4.1.5에서 이름이 바뀌었으면 경고 로그 남기고 AO 상시 켜짐으로 떨어집니다

- 나머지 패치 대상(`GameWorld.OnGameStarted`, `LampController.Awake`,
  `GPUInstancerDetailManager.Awake`)은 실제 이름이라 없어지면 컴파일 에러로 잡힙니다

**(같은 날 추가 — 공식 위키 확인 후)**

SPT 공식 위키의 [Client Mod Migration 4.0 to 4.1] 문서 확인 결과:

> **4.1은 클라이언트를 역난독화했습니다.** 타입들이 진짜 이름과 네임스페이스를 갖게 됐고,
> **4.0 클라이언트 모드는 전부 4.1로 재빌드해야 합니다.**

위키의 5,957줄짜리 이름 매핑 표에 이 모드가 참조하는 식별자를 전부 대조했습니다.
**바뀐 것은 하나뿐입니다** — 이 모드가 EFT 타입을 적게 건드리는 덕분입니다:

| 4.0 | 4.1 |
| --- | --- |
| `CameraClass` | `EFT.CameraControl.CameraManager` |

3곳(`AmbientOcclusion.cs`, `Bloom.cs`, `MotionBlur.cs`) 치환 + `using EFT.CameraControl;` 추가.

나머지(`GameWorld`, `TarkovApplication`, `LampController`, `GPUInstancerDetailManager`,
`FlareLight`, `MaterialEmission`, `RaidSettings`, `BSG.CameraEffects.NightVision`,
`HBAO`, `UltimateBloom`)는 표에 없으므로 이름이 그대로입니다.

4.1.4에서 되돌린 직렬화 필드 이름 표(`AmbianceAffectedComponent.String` 등)도 확인했는데
이 모드는 해당 없습니다.

**여전히 남은 위험**: `TarkovApplication.method_41`. 위키의 매핑 표는 **타입 이름만**
다루고 메서드 이름은 다루지 않습니다. 위에 적은 대로 이건 조용히 깨지는 쪽이라
`PatchTarget` 로그로 확인해야 합니다.

**(같은 날 재차 추가 — SPT `assembly-tool` 소스 확인 후)**

역난독화 도구(`SP-Tushonka/assembly-tool`) 소스와 그 매핑 데이터를 확인했습니다.
**메서드에는 자동 개명기가 없습니다** — 명시적 `MethodRenames` 목록으로만 바뀝니다.

`Assets/Json/Mappings/Named-Class-Mappings.json5`의 `EFT.TarkovApplication` 항목:

```
"MethodRenames": {
    "method_9":  "ShowProfileLoadingScreen",   "method_10": "ShowCriticalBackendErrorDialog",
    "method_13": "ShowLegacyLoginScreen",      "method_16": "InitNotificationManager",
    "method_17": "DestroyNotificationManager", "method_32": "RunProfile",
    "method_35": "ShowTimeHasComeScreen",      "method_36": "MainMenu",
    "method_38": "OnApplicationLoaded",        "method_49": "LocalGameCreate",
    "method_51": "TryUnloadGame",
}
```

**`method_41`은 목록에 없습니다.** 즉 SPT는 이 메서드 이름을 건드리지 않고, 4.1에서도
`method_41`로 남습니다. 빌드는 통과합니다.

다만 이게 "같은 메서드"라는 보장은 아닙니다. 번호는 BSG의 난독화기가 매긴 것이라
BSG가 클래스에 메서드를 넣고 빼면 밀립니다(목록에 `method_51`까지 있는 걸 보면
현재 어셈블리에도 그 근처 번호대가 존재합니다). 그래서 `PatchTarget` 로그가
여전히 확인 수단입니다.

참고로 같은 목록의 `method_49` = `LocalGameCreate`는 이름만 보면 라이드 시작에
해당합니다. 만약 `method_41` 프리픽스가 엉뚱한 시점에 도는 것으로 확인되면
그쪽이 유력한 후보입니다 — 다만 이건 어셈블리를 봐야 확정할 수 있습니다.

`_raidSettings`는 난독화 접두사로 시작하지 않으므로 필드 개명 대상이 아닙니다. 그대로입니다.

**(같은 날 정정 — 빌드 에러의 진짜 원인)**

`CameraManager` 없음 / `LampController.Awake` 없음 / `TarkovApplication.method_41` 없음 —
전부 참조하던 `Assembly-CSharp.dll`이 **아직 역난독화되지 않았기** 때문입니다.
개명 자체는 맞았습니다.

덤프해 보니 타입 15,137개 중 **10,172개(67%)**가 `AICorePointHolder+\ue000` 같은
유니코드 PUA 이름이었고, SPT 이름은 4.0 것도 4.1 것도 **0개**였습니다.

SPT 공식 위키 [Client Modding Quick Guide] Step 1-5: 역난독화는 설치가 아니라
**런처로 게임을 한 번 메인 메뉴까지 띄웠을 때** 일어납니다.

(탐색 순서 정정) 게임과 BepInEx는 설치 루트에 있습니다. `SPT_Runtime\`은 SPT 자체
런처·서버·user 폴더용입니다. 한때 잘못 읽고 `SPT_Runtime`을 먼저 보게 했던 것을
되돌렸습니다.

**(같은 날 — `method_41` 확정: `LocalGameMatching`)**

덤프 두 개(난독화 상태 / 역난독화 상태)가 **같은 파일의 전·후**라 메서드 순서가
같다는 점을 이용해 번호를 역산했습니다. 난독화된 메서드만 순서대로 0,1,2… 세면
de4dot의 번호가 재구성됩니다.

`assembly-tool`이 이름을 알려준 메서드 11개를 앵커로 검증한 결과 **오프셋 +2에서
11/11 일치**했습니다 (`method_38 = OnApplicationLoaded`, `method_49 = LocalGameCreate` …).

그 정렬에서:

```
method_40 = OnAbortFinished
method_41 = LocalGameMatching      ← Task LocalGameMatching(TimeAndWeatherSettings, bool)
method_42 = NetworkGameMatching    ← 네트워크 게임용 짝
```

**`LocalGameMatching`** — 싱글플레이 라이드로 매칭해 들어가는 지점입니다. `_raidSettings`가
채워져 있고 맵이 로드되기 전이라, 맵별 그래픽 오버라이드를 정하는 이 패치의 자리로
의미까지 정확합니다. 바로 옆 42번이 `NetworkGameMatching`인 것도 이 근방이 라이드
시작 구간임을 뒷받침합니다.

이제 진짜 이름이라 다음에 없어지면 **컴파일 에러**로 잡힙니다. 조용히 엉뚱한 메서드에
붙을 일이 없어져서 `PatchTarget`(로그로 감시하던 장치)은 삭제했습니다.

`_raidSettings`는 덤프에서도 그대로 있습니다 (`field RaidSettings _raidSettings`).
