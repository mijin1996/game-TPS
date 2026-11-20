# 🔫 game-TPS

> 한 줄 소개: (직접 작성)
> 예) 웨이브마다 몰려오는 고블린을 총과 수류탄으로 막아내는 모바일 3인칭 슈팅 게임

---

## 📜 개발 과정 히스토리

### Phase 1 — 기초 뼈대 (교수님 수업 · `TPS-초기스크립트/`)
수업에서 짠 초기 스크립트로 **TPS의 기본 루프(이동·사격·적 AI·웨이브·아이템)**를 만들었다.

| 사용한 개념 | 적용된 곳 |
|---|---|
| 상속 (`virtual` / `override`) | `LivingEntity` → `PlayerHealth`, `Goblin` 이 체력·피격·사망을 공통으로 씀 |
| 인터페이스 | `IDamageable`(맞을 수 있는 것), `IItem`(먹을 수 있는 것) |
| ScriptableObject 데이터 | `GunData`, `GoblinData` 로 수치를 코드 밖에서 관리 |
| NavMeshAgent | 고블린이 플레이어를 길 찾아 추적 |
| Raycast + LineRenderer | 총알 판정과 총알 궤적 |
| 코루틴 | 재장전, 사격 이펙트, 경로 갱신 |
| 컴포넌트 분리 | `PlayerInput` / `PlayerMovement` / `PlayerShooter` |
| 싱글톤 매니저 | `GM`(점수·게임오버), `UIM`(UI 갱신을 직접 호출) |

### Phase 2 — 기능 확장 · 리팩토링 (ME)

**① 이벤트(`static event Action`) 기반 구조로 변경**
- 초기 코드는 `GM`·`Gun`·`GoblinSpawn`이 `UIM.ins.UpdateXXX()`를 **직접 호출** → 서로 묶여 있었음
- 지금은 각자 "알림"만 보내고(`onScoreChanged`, `onGameOver`, `onAmmoChanged`, `onWaveStarted`, `onGoblinKilled`, `Coin.onCollected` …) 필요한 쪽이 구독 → `UIM` 싱글톤 제거
- 매니저도 역할별로 추가: `SM`(소리만), `SceneChange`(씬 이동만), `SettingsMenu`(설정 창)

**② 모바일 조작**
- `DynamicJoystick` 이동 + 화면 버튼(발사·재장전·수류탄·점프·스코프)
- `CamLook`: 터치로 시점 회전 (조이스틱·버튼 위에서 시작한 손가락은 제외)

**③ 플레이어 기능 추가**
- 점프(최대 3단, Raycast 바닥 판정), 앞뒤·좌우 블렌드 애니메이션
- 수류탄: `PlayerGrenade`(보유 개수·투척) + `Grenade`(`OverlapSphere` 범위 폭발) + `GrenadePack` 아이템
- 스코프: 화면 중앙으로 조준 레이를 쏴서 맞는 지점으로 발사 (Player 레이어 제외)
- 재장전할 때만 상체 애니메이션 레이어를 부드럽게 켜기

**④ 게임 규칙 · 적**
- 웨이브 최대 10, 웨이브마다 고블린 수 2배
- 게임오버 시 고블린 정지 + 대기 동작, 고블린 바닥 오라 이펙트·공격음

**⑤ 설정 · UI · 연출**
- 설정 창: 음량 조절(%), 소리 끄기, 일시정지(`Time.timeScale`), `PlayerPrefs`로 저장(`SettingsData`)
- 씬 이동: 클릭음 대기 후 이동, 멈춘 상태에서도 동작(Realtime), 중복 클릭 방지
- `IPop` / `IBlink` 효과: 체력바 깜빡임, 아이템 개수(`ItemCntUI`) 톡 튀기
- `FindObjectOfType` → `FindAnyObjectByType` (Unity 6 API)

#### 💭 고민한 점 · 배운 점
- (직접 작성)
- (직접 작성)

---

## 🛠 기술 스택
- **Language:** C#
- **Engine:** Unity 6 (6000.6.3f1)
- **그 외:** NavMesh, Joystick Pack, TextMesh Pro

---

## 📁 문서
- [`Docs/Proposal/Proposal.md`](Docs/Proposal/Proposal.md) — 기획서 세트(컨셉 · 시스템 · 전투 · 레벨 · 밸런스) · 시뮬레이션
- [`Docs/MeMemo.md`](Docs/MeMemo.md) — 코루틴→UniTask, 델타타임·랜덤 정리 메모
