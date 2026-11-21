# 시스템 — 전투 (사격 · 재장전 · 수류탄 · 스코프 · 아이템)

> 문서 세트: [목차](../Proposal.md) · 기획: [컨셉](Concept.md) · **시스템-전투** · [시스템-게임 흐름](System_GameFlow.md) · [전투·개체](Combat_Entity.md) · [레벨 디자인](LevelDesign.md) · [밸런스](Balance.md) · 사업·운영: [사업 검토](../대표/Business.md) · [요구사항·로드맵](Requirements.md) · [Business Model](Monetization.md) · [서버·데이터](../개발/Server_Data.md) · [리소스](../디자인/Resources.md) · [로그·지표](Analytics.md) · [QA·출시](../QA/QA_Release.md) · [라이브 운영](../운영/LiveOps.md) · [사업화·법무](../대표/Business_Legal.md) · [needFix](../needFix.md)
> 관련 코드: `Gun.cs` · `PlayerShooter.cs` · `PlayerInput.cs` · `PlayerGrenade.cs` · `Grenade.cs` · `Scope.cs` · `ItemSpawn.cs` · `PlayerHealth.cs` · `Item/*.cs`

> **문서 정보** · 문서 번호 `D03-WEAPON` · 버전 v1.0 · 상태 `Review` · 담당 기획자 · 클라이언트 개발자 · 최종 수정 2026-11-21 · 관련 문서 [전투·개체](Combat_Entity.md) · [밸런스](Balance.md)

**변경 이력**

| 버전 | 날짜 | 변경 내용 | 사유 | 결정자 | 영향 범위 |
|---|---|---|---|---|---|
| v0.9 | 2026-11-20 | 기존 문서 (자세한 기록은 git 커밋) | — | 기획자 | — |
| v1.0 | 2026-11-21 | 문서 정보 · 변경 이력 · 문서 번호 추가, 사업 · 운영 문서와 연결 | — | 기획자 | 문서만 |

---

## 0. 한눈에 보는 시스템 구조

**[구성도] 스크립트 연결 — 전투**
```mermaid
flowchart LR
  subgraph L1["① 입력 · 씬"]
    IN["🎬 PlayerInput<br/>버튼 · 키보드"]:::scene
  end
  subgraph L2["② 게임 로직 (스크립트)"]
    SH["📜 PlayerShooter"]:::script
    G["📜 Gun"]:::script
    PG["📜 PlayerGrenade"]:::script
    SC["📜 Scope"]:::script
    IS["📜 ItemSpawn"]:::script
    PH["📜 PlayerHealth"]:::script
    GM["📜 GM"]:::script
  end
  subgraph L3["③ 게임 오브젝트"]
    GB["📦 고블린"]:::obj
    GR["📦 수류탄"]:::obj
    IT["📦 아이템 4종"]:::obj
  end
  subgraph L4["④ 화면 · 소리"]
    UI["🖥️ UIM · ItemCntUI"]:::ui
  end
  IN -->|"① fire · reload"| SH
  SH -->|"② Fire() · Reload()"| G
  G -->|"③ OnDamage(25)"| GB
  IN -->|"① grenade"| PG
  PG -->|"② 투척"| GR
  GR -->|"③ 반경 6m 100 피해"| GB
  IN -->|"① scope"| SC
  IS -->|"2~7초마다"| IT
  IT -->|"탄약 +30"| G
  IT -->|"체력 +50"| PH
  IT -->|"수류탄 +5"| PG
  IT -->|"코인 +200"| GM
  G -->|"onAmmoChanged"| UI
  PG -->|"onGrenadeCntChanged"| UI
  classDef scene fill:#2F8F6B,color:#fff,stroke:#2F8F6B
  classDef script fill:#3AA6C8,color:#fff,stroke:#3AA6C8
  classDef obj fill:#6B7280,color:#fff,stroke:#6B7280
  classDef ui fill:#5A4A9E,color:#fff,stroke:#5A4A9E
  classDef data fill:#A33A33,color:#fff,stroke:#A33A33
```

## 1. 개요
| 기능 | 한 줄 설명 | 수치 출처 |
|---|---|---|
| 사격 | 화면 중앙 조준점이 가리키는 곳으로 레이캐스트 발사 | `ScriptableData/Play/Gun Data.asset` |
| 재장전 | 1.8초 동안 사격 불가, 예비 탄약에서 탄창 채움 | `Gun Data` |
| 수류탄 | 앞으로 포물선 투척, 부딪히거나 2.5초 뒤 폭발 | `Grenade` 프리팹, `Play` 씬 `PlayerGrenade` |
| 스코프 | 스코프 카메라 켜기 + 스코프 UI, 조준점 UI 숨김 | `Scope` |
| 아이템 | 플레이어 주변에 2 ~ 7초마다 생성, 5초 뒤 사라짐 | `Play` 씬 `ItemSpawn` |

## 2. 입력 → 행동
| 입력 (모바일 / PC) | 판정 방식 | 행동 |
|---|---|---|
| Fire 버튼 누르고 있기 / `F` 누른 순간 | 버튼은 누르는 동안 매 프레임, F는 누른 프레임만 | `Gun.Fire()` (연사 간격 0.12초로 제한) |
| Reload 버튼 / `R` | 누른 순간 한 프레임 | `Gun.Reload()` |
| Grenade 버튼 / `G` | 누른 순간 한 프레임 | `PlayerGrenade.ThrowGrenade()` |
| Scope 버튼(토글) / 마우스 오른쪽 누르기 | 상태 유지 | `Scope.Apply(on)` |
| 게임오버 · 일시정지 중 | 모든 입력 0 / false | 행동 없음 |

## 3. 총 상태 머신

**[상태도] 총 상태 머신**
```mermaid
stateDiagram-v2
  [*] --> Ready: 활성화 (탄창 25 · 예비 100)
  Ready --> Ready: Fire() 성공, 탄창 > 0
  Ready --> Empty: Fire() 후 탄창 = 0
  Ready --> Reloading: Reload() 성공
  Empty --> Reloading: Reload() 성공
  Reloading --> Ready: 1.8초 뒤 탄창 채움
  Empty --> Empty: Fire() 무시 / 예비 탄약 0이면 Reload() 실패
  classDef play fill:#2F8F6B,color:#fff,stroke:#2F8F6B
  classDef wait fill:#3AA6C8,color:#fff,stroke:#3AA6C8
  classDef fail fill:#A33A33,color:#fff,stroke:#A33A33
  classDef stop fill:#6B7280,color:#fff,stroke:#6B7280
  classDef done fill:#5A4A9E,color:#fff,stroke:#5A4A9E
  class Ready wait
  class Empty fail
  class Reloading play
```

```
mag    [=========================] 25   Ready
mag    [=======                  ]  7   Ready
mag    [                         ]  0   Empty  -> 발사 눌러도 반응 없음 (재장전 필요)
mag    [....reloading 1.8s.......]      Reloading
mag    [=========================] 25   Ready  (예비 탄약에서 채운 만큼 이동)
```

## 4. 작동 로직
### 4-1. 사격 `Gun.Fire()`

**[흐름도] 한 발 쏘기**
```mermaid
flowchart TD
  n1(["▶ 발사 입력"]):::start
  n2{"총 상태 = Ready?"}:::dec
  n1 --> n2
  n3(["⏹ 무시 (Empty · Reloading)"]):::endc
  n2 -->|N| n3
  n4{"지난 발사 후<br/>0.12초 지남?"}:::dec
  n2 -->|Y| n4
  n5(["⏹ 무시"]):::endc
  n4 -->|N| n5
  n6["⚙️ 카메라 선택<br/>(스코프 켜짐 → 스코프 카메라)"]:::proc
  n4 -->|Y| n6
  n7["⚙️ 화면 중앙 레이 → 조준점 X<br/>(없으면 50m 앞)"]:::proc
  n6 --> n7
  n8["⚙️ 총구 → X 방향 50m 레이"]:::proc
  n7 --> n8
  n9{"맞은 것이<br/>IDamageable?"}:::dec
  n8 --> n9
  n10["✏️ OnDamage(25)"]:::data
  n9 -->|Y| n10
  n11[/"📢 총구 불꽃 · 탄피 · 발사음 · 궤적선"/]:::io
  n9 -->|N/Y 모두| n11
  n12["✏️ 탄창 -1"]:::data
  n11 --> n12
  n13{"탄창 = 0 ?"}:::dec
  n12 --> n13
  n14["✏️ 상태 Empty"]:::data
  n13 -->|Y| n14
  n15[/"📢 화면 #quot;탄창/예비#quot; 갱신"/]:::io
  n13 -->|N/Y 모두| n15
  classDef start fill:#2F8F6B,color:#fff,stroke:#2F8F6B
  classDef proc fill:#3AA6C8,color:#fff,stroke:#3AA6C8
  classDef data fill:#A33A33,color:#fff,stroke:#A33A33
  classDef dec fill:#EE7E1C,color:#fff,stroke:#EE7E1C
  classDef io fill:#5A4A9E,color:#fff,stroke:#5A4A9E
  classDef endc fill:#6B7280,color:#fff,stroke:#6B7280
```

**조준 레이 두 번 쏘기 (위에서 본 모습)**
```
   C -------------------------------------> X
                                          ^
   G -------------------------------------+
   P

C = 카메라: 화면 중앙(0.5, 0.5)으로 레이, Player 레이어 제외
X = 조준점: 맞은 곳 (없으면 50m 앞)
G = 총구: 조준점 방향으로 실제 발사 레이 (최대 50m)
스코프 켜짐 -> C 대신 스코프 카메라 기준
```

> 세부 조건(코드 기준)은 [부록 A-1](#a-1-사격-gunfire)에 있어요.

### 4-2. 재장전 `Gun.Reload()`

**[흐름도] 재장전**
```mermaid
flowchart TD
  n1(["▶ 재장전 입력 (R · 버튼)"]):::start
  n2{"같은 프레임에<br/>발사 입력도 있음?"}:::dec
  n1 --> n2
  n3(["⏹ 무시"]):::endc
  n2 -->|Y| n3
  n4{"이미 Reloading?"}:::dec
  n2 -->|N| n4
  n5(["⏹ 실패"]):::endc
  n4 -->|Y| n5
  n6{"예비 탄약 0?"}:::dec
  n4 -->|N| n6
  n7(["⏹ 실패"]):::endc
  n6 -->|Y| n7
  n8{"탄창 가득 (25)?"}:::dec
  n6 -->|N| n8
  n9(["⏹ 실패"]):::endc
  n8 -->|Y| n9
  n10["✏️ 상태 Reloading"]:::data
  n8 -->|N| n10
  n11[/"📢 재장전 소리 · 애니메이션"/]:::io
  n10 --> n11
  n12["⚙️ 1.8초 기다림"]:::proc
  n11 --> n12
  n13["✏️ 채울 양 = min(25 - 탄창, 예비)<br/>탄창 + · 예비 -"]:::data
  n12 --> n13
  n14(["⏹ 상태 Ready · 화면 갱신"]):::endc
  n13 --> n14
  classDef start fill:#2F8F6B,color:#fff,stroke:#2F8F6B
  classDef proc fill:#3AA6C8,color:#fff,stroke:#3AA6C8
  classDef data fill:#A33A33,color:#fff,stroke:#A33A33
  classDef dec fill:#EE7E1C,color:#fff,stroke:#EE7E1C
  classDef io fill:#5A4A9E,color:#fff,stroke:#5A4A9E
  classDef endc fill:#6B7280,color:#fff,stroke:#6B7280
```

> 세부 조건(코드 기준)은 [부록 A-2](#a-2-재장전-gunreload)에 있어요.

### 4-3. 수류탄

**[흐름도] 수류탄 던지기부터 폭발까지**
```mermaid
flowchart TD
  n1(["▶ 수류탄 입력 (G · 버튼)"]):::start
  n2{"보유 #gt; 0 ?"}:::dec
  n1 --> n2
  n3(["⏹ 무시"]):::endc
  n2 -->|N| n3
  n4["✏️ 보유 -1 · 개수 UI 톡 튀기"]:::data
  n2 -->|Y| n4
  n5["⚙️ 생성 → 앞 × 12 + 위 × 4 로 던짐"]:::proc
  n4 --> n5
  n6{"먼저 일어나는 것"}:::dec
  n5 --> n6
  n7["⚙️ 무언가에 충돌 (플레이어 제외)"]:::proc
  n6 --> n7
  n8["⚙️ 던진 뒤 2.5초"]:::proc
  n6 --> n8
  n9{"이미 폭발했음?"}:::dec
  n6 --> n9
  n10(["⏹ 무시"]):::endc
  n9 -->|Y| n10
  n11["✏️ 반경 6m Goblin에 100 피해"]:::data
  n9 -->|N| n11
  n12[/"📢 폭발 이펙트 · 폭발음"/]:::io
  n11 --> n12
  n13(["⏹ 수류탄 제거"]):::endc
  n12 --> n13
  classDef start fill:#2F8F6B,color:#fff,stroke:#2F8F6B
  classDef proc fill:#3AA6C8,color:#fff,stroke:#3AA6C8
  classDef data fill:#A33A33,color:#fff,stroke:#A33A33
  classDef dec fill:#EE7E1C,color:#fff,stroke:#EE7E1C
  classDef io fill:#5A4A9E,color:#fff,stroke:#5A4A9E
  classDef endc fill:#6B7280,color:#fff,stroke:#6B7280
```

**투척 궤적 (옆에서 본 모습)**
```
              .  -  -  .
          .               .        힘 = 앞 x 12 + 위 x 4 (Impulse)
       .                     .
   P                           *   바닥 · 고블린에 닿는 즉시 폭발 (늦어도 2.5초)
 ------------------------------------
```

> 세부 조건(코드 기준)은 [부록 A-3](#a-3-수류탄)에 있어요.

### 4-4. 아이템 생성 · 획득

**[흐름도] 아이템 생성과 줍기**
```mermaid
flowchart TD
  subgraph s0["① 생성 (매 프레임)"]
    direction TB
    n1(["▶ 매 프레임"]):::start
    n2{"지난 생성 후<br/>2 ~ 7초 지남?"}:::dec
    n1 --> n2
    n3["⚙️ 기다림"]:::proc
    n2 -->|N| n3
    n4["⚙️ 플레이어 반경 5m 랜덤 점<br/>→ NavMesh 위 + 0.5m"]:::proc
    n2 -->|Y| n4
    n5(["⏹ 4종 중 하나 생성 → 5초 뒤 제거"]):::endc
    n4 --> n5
  end
  subgraph s1["② 줍기"]
    direction TB
    n6(["▶ 플레이어가 아이템에 닿음"]):::start
    n7{"죽었음 또는<br/>일시정지?"}:::dec
    n6 --> n7
    n8(["⏹ 무시"]):::endc
    n7 -->|Y| n8
    n9["⚙️ IItem.Use()"]:::proc
    n7 -->|N| n9
    n10["✏️ 탄약 팩 → 예비 +30"]:::data
    n9 --> n10
    n11["✏️ 체력 팩 → 체력 +50 (상한 없음)"]:::data
    n9 --> n11
    n12["✏️ 수류탄 팩 → +5 (최대 100)"]:::data
    n9 --> n12
    n13["✏️ 코인 → 점수 +200"]:::data
    n9 --> n13
    n14[/"📢 획득음 · 아이템 제거"/]:::io
    n9 --> n14
  end
  s0 ~~~ s1
  classDef start fill:#2F8F6B,color:#fff,stroke:#2F8F6B
  classDef proc fill:#3AA6C8,color:#fff,stroke:#3AA6C8
  classDef data fill:#A33A33,color:#fff,stroke:#A33A33
  classDef dec fill:#EE7E1C,color:#fff,stroke:#EE7E1C
  classDef io fill:#5A4A9E,color:#fff,stroke:#5A4A9E
  classDef endc fill:#6B7280,color:#fff,stroke:#6B7280
```

**생성 범위 (위에서 본 모습)**
```
          . - - - - .
        /     A       \        반경 5m 안 랜덤 점 -> NavMesh 위 + 0.5m
       |   H     P     |       2 ~ 7초마다 1개, 5초 뒤 사라짐
        \         G   /
          ' - - - - '

A = 탄약 +30 · H = 체력 +50 · G = 수류탄 +5 · C = 코인 +200 (4종 균등 랜덤)
```

> 세부 조건(코드 기준)은 [부록 A-4](#a-4-아이템-생성--획득)에 있어요.

## 5. 예외 처리 (Edge Case) — 현재 동작
**[구성도] 예외 처리 지도 — 전투**
```mermaid
flowchart TB
  subgraph G0["🔫 사격"]
    direction LR
    S2["#2 재장전 중 발사"]:::sit --> R2["✅ 무시 (Ready가 아님)"]:::ok
    S4["#4 탄창 0에서 발사"]:::sit --> R4["⚠️ 반응 없음 · 자동 재장전 없음 (S-04)"]:::bad
    S6["#6 발사와 재장전이 같은 프레임"]:::sit --> R6["⚠️ 재장전 입력 무시 (S-06)"]:::bad
    S12["#12 조준 레이가 내 몸에 맞음"]:::sit --> R12["✅ Player 레이어 제외"]:::ok
  end
  subgraph G1["🔄 재장전"]
    direction LR
    S1["#1 재장전 중 재장전 연타"]:::sit --> R1["✅ 거부 (Reloading)"]:::ok
    S3["#3 탄창 가득일 때 재장전"]:::sit --> R3["✅ 거부"]:::ok
    S5["#5 예비 탄약 0에서 재장전"]:::sit --> R5["⚠️ 조용히 실패 (S-05)"]:::bad
  end
  subgraph G2["💣 수류탄"]
    direction LR
    S7["#7 수류탄 연타"]:::sit --> R7["⚠️ 쿨다운 없음 (S-07)"]:::bad
    S8["#8 보유 0에서 투척"]:::sit --> R8["✅ 무시"]:::ok
    S9["#9 충돌 폭발 + 시간 폭발 겹침"]:::sit --> R9["✅ hasExploded로 1번만"]:::ok
    S10["#10 콜라이더 여러 개인 고블린"]:::sit --> R10["✅ dead 검사로 1번만 피해"]:::ok
    S11["#11 수류탄이 플레이어에 닿음"]:::sit --> R11["✅ 무시 · 플레이어 피해 없음"]:::ok
  end
  subgraph G3["🎁 아이템 · 체력"]
    direction LR
    S13["#13 체력 팩으로 최대 체력 초과"]:::sit --> R13["⚠️ 상한 없음 → 100 넘음 (S-02)"]:::bad
    S14["#14 예비 탄약 · 점수"]:::sit --> R14["💡 상한 없음 (S-08)"]:::todo
    S15["#15 일시정지 · 사망 중 아이템 접촉"]:::sit --> R15["✅ 획득 무시"]:::ok
    S16["#16 생성 위치에 NavMesh 없음"]:::sit --> R16["⚠️ 결과 검사 안 함 (S-03)"]:::bad
  end
  subgraph G4["🎮 입력 · 기타"]
    direction LR
    S17["#17 게임오버 · 일시정지 중 입력"]:::sit --> R17["✅ 모든 입력 0"]:::ok
    S18["#18 Scope.zoomFov (씬 값 20)"]:::sit --> R18["💡 코드에서 안 씀 (S-09)"]:::todo
  end
  G0 ~~~ G1
  G1 ~~~ G2
  G2 ~~~ G3
  G3 ~~~ G4
  classDef sit fill:#EE7E1C,color:#fff,stroke:#EE7E1C
  classDef ok fill:#2F8F6B,color:#fff,stroke:#2F8F6B
  classDef bad fill:#A33A33,color:#fff,stroke:#A33A33
  classDef todo fill:#6B7280,color:#fff,stroke:#6B7280
```

| # | 상황 | 현재 동작 | 상태 |
|---|---|---|---|
| 1 | 재장전 중 재장전 연타 | `Reloading`이면 거부 | ✅ |
| 2 | 재장전 중 발사 | `Ready`가 아니면 무시 | ✅ |
| 3 | 탄창 가득일 때 재장전 | 거부 | ✅ |
| 4 | 탄창 0에서 발사 | 반응 없음 (소리 · 안내 없음, 자동 재장전 없음) | ⚠️ → [S-04](../needFix.md) |
| 5 | 예비 탄약 0에서 재장전 | 조용히 실패 | ⚠️ → [S-05](../needFix.md) |
| 6 | 발사 입력과 재장전이 같은 프레임 | 재장전 입력 무시 | ⚠️ → [S-06](../needFix.md) |
| 7 | 수류탄 연타 | 쿨다운 없음 | ⚠️ → [S-07](../needFix.md) |
| 8 | 보유 수류탄 0에서 투척 | 무시 | ✅ |
| 9 | 충돌 폭발과 시간 폭발이 겹침 | `hasExploded`로 1번만 | ✅ |
| 10 | 콜라이더가 여러 개인 고블린 | `dead` 검사로 1번만 피해 | ✅ |
| 11 | 수류탄이 플레이어에 닿음 | 충돌 무시, 플레이어 피해 없음 | ✅ |
| 12 | 조준 레이가 플레이어 몸에 맞음 | Player 레이어 제외 | ✅ |
| 13 | 체력 팩으로 최대 체력 초과 | `RestoreHealth`에 상한 없음 | ⚠️ → [S-02](../needFix.md) |
| 14 | 예비 탄약 · 점수 | 상한 없음 | → [S-08](../needFix.md) |
| 15 | 일시정지 · 사망 중 아이템 접촉 | 획득 무시 | ✅ |
| 16 | 아이템 생성 위치에 NavMesh 없음 | `SamplePosition` 결과를 검사하지 않음 | ⚠️ → [S-03](../needFix.md) |
| 17 | 게임오버 · 일시정지 중 입력 | `PlayerInput`이 모든 값 0 | ✅ |
| 18 | `Scope.zoomFov` (씬 값 20) | 코드에서 사용하지 않음 (스코프 카메라로 대체) | → [S-09](../needFix.md) |

## 6. UI 피드백
| 이벤트 | 화면 | 소리 |
|---|---|---|
| 발사 | 총구 불꽃 · 탄피 · 궤적선 | 발사음 |
| 탄 수 변경 | `mag/remain` 글자 | – |
| 재장전 | 상체 재장전 애니메이션 | 재장전음 |
| 수류탄 개수 변경 | 개수 글자 톡 튀기 (`IPop`) | – |
| 체력 감소 | 체력바 깜빡임 (`IBlink`) | 피격음 |
| 체력 회복 | 체력바 톡 튀기 | 획득음 |

---

## 부록 A. 세부 조건 (코드 기준)
본문 흐름도의 조건을 코드 그대로 적은 개발 참고용이에요. 기획 내용은 본문에 있어요.

### A-1. 사격 `Gun.Fire()`
```
발사 입력 발생
├─ IF 총 상태 ≠ Ready            → 무시 (Empty · Reloading)
├─ IF 마지막 발사 후 0.12초 미만  → 무시 (연사 간격)
└─ ELSE
    ├─ 조준 카메라 선택: 스코프 켜짐 → 스코프 카메라 / 꺼짐 → 메인 카메라
    ├─ 화면 중앙(0.5, 0.5)으로 레이 → Player 레이어 제외하고 맞은 지점 = 조준점 (없으면 50m 앞)
    ├─ 총구 → 조준점 방향으로 50m 레이캐스트
    │   ├─ IF 맞은 대상에 IDamageable 있음 → OnDamage(25)
    │   └─ 궤적 끝 = 맞은 지점 (없으면 50m 앞)
    ├─ 연출: 총구 불꽃 · 탄피 · 발사음 · 궤적선 0.03초
    ├─ 탄창 -1 → IF 탄창 = 0 → 상태 Empty
    └─ onAmmoChanged(탄창, 예비) 알림 → UI 갱신
```

### A-2. 재장전 `Gun.Reload()`
```
재장전 입력 발생
├─ IF 발사 입력이 같은 프레임에 들어옴 → 재장전 입력 무시 (PlayerShooter: if fire else if reload)
├─ IF 상태 = Reloading            → 실패 (false)
├─ IF 예비 탄약 ≤ 0               → 실패
├─ IF 탄창이 이미 가득(25)        → 실패
└─ ELSE → 성공 (true)
    ├─ 상태 Reloading, 재장전 소리, 재장전 애니메이션 트리거
    ├─ (옵션) 상체 애니메이션 레이어 Weight 0 → 1 로 부드럽게
    ├─ 1.8초 대기
    ├─ 채울 양 = min(25 - 탄창, 예비 탄약) → 탄창 += 채울 양, 예비 -= 채울 양
    └─ 상태 Ready, onAmmoChanged 알림
```

### A-3. 수류탄
```
수류탄 입력 발생
├─ IF 보유 수류탄 ≤ 0 → 무시
└─ ELSE
    ├─ 보유 -1 → onGrenadeCntChanged 알림 (UI 숫자 톡 튀기)
    ├─ 투척 지점에서 생성 → 힘 = 앞 × 12 + 위 × 4 (Impulse, 포물선)
    └─ 폭발 조건 (먼저 오는 것 1번만 — hasExploded)
        ├─ 무언가에 충돌 (단, 플레이어 자신은 무시) → 즉시 폭발
        └─ 던진 뒤 2.5초 경과 → 폭발
            ├─ 반경 6 안의 Goblin 레이어 전부 → 살아 있으면 100 데미지
            ├─ 폭발 이펙트 생성(2초 뒤 제거) · 폭발음
            └─ 수류탄 제거
```

### A-4. 아이템 생성 · 획득
```
매 프레임
└─ IF 지난 생성 후 (2 ~ 7초 랜덤) 경과 AND 플레이어 있음
    ├─ 플레이어 주변 반경 5 안 랜덤 점 → NavMesh 위 가장 가까운 점 + 0.5m 위
    ├─ 4종 중 균등 랜덤 1개 생성 → 5초 뒤 제거
    └─ 다음 생성 시간 다시 랜덤

플레이어가 아이템에 닿음 (OnTriggerEnter)
├─ IF 플레이어 사망 OR 일시정지 → 무시
└─ ELSE → IItem.Use() + 획득음
    탄약 +30 / 체력 +50 / 수류탄 +5 (최대 100) / 코인 → 점수 +200
```
