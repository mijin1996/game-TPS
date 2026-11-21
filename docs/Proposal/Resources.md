# 리소스 (아트 · 사운드) 명세 — game-TPS

> 문서 세트: [목차](Proposal.md) · 기획: [컨셉](Concept.md) · [시스템-전투](System_Weapon.md) · [시스템-게임 흐름](System_GameFlow.md) · [전투·개체](Combat_Entity.md) · [레벨 디자인](LevelDesign.md) · [밸런스](Balance.md) · 사업·운영: [사업 검토](Business.md) · [요구사항·로드맵](Requirements.md) · [Business Model](Monetization.md) · [서버·데이터](Server_Data.md) · **리소스** · [로그·지표](Analytics.md) · [QA·출시](QA_Release.md) · [라이브 운영](LiveOps.md) · [사업화·법무](Business_Legal.md) · [needFix](needFix.md)

> **문서 정보** · 문서 번호 `D09-RES` · 버전 v1.0 · 상태 `Draft` · 담당 아티스트 · 사운드 디자이너 · 최종 수정 2026-11-21 · 관련 문서 [전투·개체](Combat_Entity.md) · [사업화·법무](Business_Legal.md)

**변경 이력**

| 버전 | 날짜 | 변경 내용 | 사유 | 결정자 | 영향 범위 |
|---|---|---|---|---|---|
| v1.0 | 2026-11-21 | 최초 작성 | — | 아티스트 | — |

---

## 1. 이 문서의 역할
게임에 들어가는 **모든 그림 · 모델 · 소리 · 폰트의 목록 · 규격 · 라이선스**를 관리해요. 외주 발주서와 출시 전 라이선스 점검표로 써요.
> **규칙:** 라이선스 칸이 `확인됨`이 아닌 리소스가 하나라도 있으면 출시하지 않아요 ([QA · 출시](QA_Release.md)).

## 2. 지금 쓰는 리소스 (`Assets` 폴더 기준)

**모델 · 애니메이션**
| 리소스 ID | 파일 · 폴더 | 쓰는 곳 | 출처 (추정) | 라이선스 |
|---|---|---|---|---|
| `MDL_PLAYER` | `Models/Woman.fbx` · `tps에 필요 재료/Characters/Character-Player` | 플레이어 | 수업 · 교재 예제 에셋 | ⚠️ 확인 필요 — 교육용 에셋은 상업 출시가 안 될 수 있음 |
| `MDL_GOBLIN` | `tps에 필요 재료/Characters/Character-Goblin` | 고블린 | 미확인 | ⚠️ 확인 필요 |
| `MDL_ZOMBIE` | `Models/Zombie.fbx` | (예전 좀비 · 지금 쓰는지 확인) | 수업 · 교재 예제 에셋 | ⚠️ 확인 필요 |
| `MDL_WEAPON` | `Models/Uzi.fbx` · `BattleRoyaleDuoPAPBR/Mesh/Weapon` | 소총 | 예제 에셋 · Unity Asset Store 팩 | ⚠️ 확인 필요 (Asset Store 팩이면 Asset Store 약관상 게임에 포함 가능) |
| `ANI_PLAYER_*` | `BattleRoyaleDuoPAPBR/Animation` (소총 · 권총 자세 92개) | 플레이어 이동 · 사격 | Unity Asset Store 팩 (이름 기준) | ⚠️ 구매 · 다운로드 기록 확인 |
| `MDL_ITEM_*` | `Models/Ammo.fbx` · `Coin.fbx` · `Heart.obj` · `ShellCasing.FBX` | 아이템 · 탄피 | 교재 예제 에셋 | ⚠️ 확인 필요 |
| `MDL_LEVEL` | `Models/Level Art` · `tps에 필요 재료/Buttons, UI/map` | 맵 | 미확인 | ⚠️ 확인 필요 |

**이펙트 · UI**
| 리소스 ID | 파일 · 폴더 | 쓰는 곳 | 출처 | 라이선스 |
|---|---|---|---|---|
| `FX_CFXR_*` | `JMO Assets/Cartoon FX Remaster` | 폭발 · 피격 이펙트 | Unity Asset Store (JMO) | ⚠️ 무료판 · 유료판 어느 쪽인지 기록 |
| `UI_BLOODLINES_*` | `Buttons, UI/Button-Bloodlines UI` | 버튼 · 슬라이더 · 아이콘 | 에셋 팩 (README 있음) | ⚠️ 라이선스 원문 없음 → 출처 페이지 확인 |
| `UI_JOYSTICK` | `Buttons, UI/Joystick` | 이동 조이스틱 | Joystick Pack (Asset Store) | ⚠️ 확인 필요 |
| `UI_FLOATTEXT` | `BlackMassSoftware/FloatingTextEngineLite` | 데미지 숫자 | 에셋 (고지 파일 있음) | ✅ 폰트는 SIL OFL 1.1, 코드 라이선스는 확인 필요 |
| `UI_HUD_*` · `SPR_PLAY_*` | `Sprites/Play` (발사 · 수류탄 · 점프 · 재장전 버튼, 조준점, 배경) | HUD · 인트로 | 직접 제작 추정 | ⚠️ 직접 만든 것인지 기록 |

**사운드**
| 리소스 ID | 파일 | 쓰는 곳 | 출처 (파일 이름 기준) | 라이선스 |
|---|---|---|---|---|
| `BGM_INTRO` | `INTRO_tatamusic-ecology-...-579421.mp3` | 인트로 | 무료 음원 사이트 (Pixabay 이름 형식) | ⚠️ 다운로드 페이지 · 라이선스 저장 |
| `BGM_MAIN` | `MAIN_desifreemusic-...-180641.mp3` | 플레이 | 위와 같음 | ⚠️ 위와 같음 |
| `BGM_GAMEOVER` | `GAMEOVER_melodigne-western-330170.mp3` | 게임오버 | 위와 같음 | ⚠️ 위와 같음 |
| `SFX_GUN_SHOOT` · `SFX_GUN_RELOAD` | `Gun Shoot.wav` · `Gun Reload.wav` | 사격 · 재장전 | 교재 예제 에셋 | ⚠️ 확인 필요 |
| `SFX_PLAYER_HIT` · `SFX_PLAYER_DIE` | `Woman Damage.ogg` · `Woman Die.ogg` | 플레이어 피격 · 사망 | 교재 예제 에셋 | ⚠️ 확인 필요 |
| `SFX_GOB_HIT` · `SFX_GOB_DIE` · `SFX_GOB_CRY_*` | `Zombie Damage.wav` · `Zombie Die.wav` · `zombieCry_*` | 고블린 | 미확인 | ⚠️ 확인 필요 |
| `SFX_UI_CLICK` · `SFX_UI_HOVER` | `Click Button SFX.wav` · `Hover Button SFX.wav` | 버튼 | Bloodlines UI 팩 | ⚠️ 팩 라이선스 따라감 |
| `SFX_PICKUP` · `SFX_EXPLODE` | `Pick Up.ogg` · `exproe.wav` | 아이템 · 수류탄 | 미확인 | ⚠️ 확인 필요 |

**폰트**
| 리소스 ID | 파일 | 라이선스 |
|---|---|---|
| `FONT_MAIN` | `Fonts/Kenney Future Narrow.ttf` | ✅ Kenney 무료 폰트 (`Fonts/License.txt` 보관) |
| `FONT_UI` | `Bloodlines UI/Fonts/ManufacturingConsent-Regular.ttf` | ⚠️ 확인 필요 (Google Fonts 배포 폰트면 OFL) |
| `FONT_DMG` | `Oswald-VariableFont_wght.ttf` | ✅ SIL OFL 1.1 |
| (예제) | `TextMesh Pro/Examples & Extras/Fonts/*` | 빌드에 안 쓰면 삭제 권장 |

> 같은 파일이 `Audios/`와 `tps에 필요 재료/Clip*`에 **두 벌** 있어요. 쓰는 쪽만 남기면 빌드 용량 · 관리가 줄어요 → [F-07](needFix.md#8-기획서-기준-적용-후-정할-것)

## 3. 새로 필요한 리소스 (needFix 제안 기준)
| 리소스 ID | 종류 | 규격 | 프레임 · 길이 | 관련 | 담당 | 상태 |
|---|---|---|---|---|---|---|
| `MDL_GOB_RUNNER` · `MDL_GOB_BRUTE` | 고블린 변형 (색 · 크기 차이로도 가능) | 기존 고블린 리그 재사용 | — | S-16 | 미정 | 대기 |
| `ANI_GOB_ATK_WINDUP` | 공격 예고 동작 | 기존 리그 | 0.3초 | S-17 | 미정 | 대기 |
| `FX_GOB_HITFLASH` | 피격 흰색 번쩍임 | 머티리얼 · 셰이더 | 0.1초 | S-17 | ME | 대기 |
| `UI_CLEAR` | 클리어 창 | 1080 × 1920 기준 | — | S-01 | 미정 | 대기 |
| `UI_SHOP_*` | 업그레이드 상점 아이콘 3종 | 256 × 256 PNG | — | [Business Model](Monetization.md) | 미정 | 대기 |
| `SFX_MAG_EMPTY` | 빈 탄창 소리 | WAV · 0.2초 이내 | — | S-04 | 미정 | 대기 |
| `STORE_*` | 스토어 아이콘 · 스크린샷 · 피처 그래픽 | 스토어 규격 | — | [QA · 출시](QA_Release.md) | 미정 | 대기 |

## 4. 외주 · 무료 에셋 규칙
- 외주 리소스는 계약서에 **저작권 양도 또는 이용 범위**(상업 · 기간 · 지역)를 적어요.
- 무료 에셋 · 폰트 · 음원은 **다운로드한 페이지 주소 · 날짜 · 라이선스 원문**을 `docs/licenses/`에 저장해요.
- Asset Store 에셋은 구매 · 다운로드한 계정의 기록이 근거예요.
- 출처를 모르는 리소스는 **같은 역할의 확인된 리소스로 바꿔요.**
