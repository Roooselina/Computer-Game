# ASSET IMPLEMENTATION MAPPING — 그림을 넣을 위치

> 분석일 2026-11-17 · 기준: 현재 프로젝트 파일 (코드·씬·데이터·이미지 메타). **현재 구현 기준**이다. 재기획(Docs/Redesign)으로 바뀔 수 있는 요구는 따로 표시했다.
> 상태: EXISTING 정상 사용 · PLACEHOLDER 임시 · MISSING 필요하지만 없음 · REPLACE 있으나 교체 필요 · OPTIONAL 선택 / 우선순위 P0~P3. **추정**은 따로 표시. 이미지를 만들거나 프로젝트 파일을 바꾸지 않았다.

## 교체 원칙

1. **같은 파일 이름, 같은 프레임 크기**로 덮어쓰면 코드 수정 없이 연결된다. 프레임 수는 달라도 된다 (가로 한 줄, 프레임 폭은 아래 표).
2. 납품은 프로젝트 루트 `ArtDrop/<날짜_내용>/` 에 PNG + `manifest.txt`(`<파일명> -> <대상 경로>`) + 완료 표시 `READY.txt` (Docs/ART_REQUESTS.md §0). 개발 쪽이 규격 검사 후 복사한다.
3. 복사 뒤 Unity 메뉴 `EW > 1. 전체 구성` — 가져오기 설정·시트 분할·클립 재생성(`EWAssetSetup.BuildCharacters`)·GameConfig 세트·스탠딩 등록·씬 재생성을 한다. **씬은 방 정의 코드에서 다시 만들어지므로 씬에 직접 손댄 수정은 사라진다** (`EWSceneBuilder`).
4. Prefab 은 쓰지 않는다 (본편 씬에 Prefab 인스턴스 없음 — 확인). 예외: `Assets/Game/Art/PixelSamples/` 의 샘플 Tilemap·`PipeConnectionGrid.prefab` 은 시험용이며 본편에 연결돼 있지 않다.
5. 무료 팩 파일(`Assets/ThirdParty/`)은 덮어쓰지 말고 새 이름으로 넣은 뒤 `EWArtSpecs.Characters()` 의 경로를 바꾸는 것을 권장 (코드 변경 1곳 — 이번 작업에서는 하지 않음).

## 캐릭터 시트

| 파일 (교체 대상) | 프레임 | 피벗 | Animator 컨트롤러 | 클립 | CharacterArt | 씬·연결 |
|---|---|---|---|---|---|---|
| `Art/Placeholder/Lu_Idle.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lu/lu.controller` (상태 `Idle`) | `lu_Idle.anim` | `Data/Art/Art_lu.asset` | 모든 플레이 씬 (P01~M47). P03·M40·M41 은 컴퓨터 안 (무중력) |
| `Art/Placeholder/Lu_Walk.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lu/lu.controller` (상태 `Walk`) | `lu_Walk.anim` | `Data/Art/Art_lu.asset` | 모든 플레이 씬 (P01~M47). P03·M40·M41 은 컴퓨터 안 (무중력) |
| `Art/Placeholder/Lu_Jump.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lu/lu.controller` (상태 `Jump`) | `lu_Jump.anim` | `Data/Art/Art_lu.asset` | 모든 플레이 씬 (P01~M47). P03·M40·M41 은 컴퓨터 안 (무중력) |
| `Art/Placeholder/Lu_Fall.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lu/lu.controller` (상태 `Fall`) | `lu_Fall.anim` | `Data/Art/Art_lu.asset` | 모든 플레이 씬 (P01~M47). P03·M40·M41 은 컴퓨터 안 (무중력) |
| `Art/Placeholder/Lu_Climb.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lu/lu.controller` (상태 `Climb`) | `lu_Climb.anim` | `Data/Art/Art_lu.asset` | 모든 플레이 씬 (P01~M47). P03·M40·M41 은 컴퓨터 안 (무중력) |
| `Art/Placeholder/Lu_Hurt.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lu/lu.controller` (상태 `Hurt`) | `lu_Hurt.anim` | `Data/Art/Art_lu.asset` | 모든 플레이 씬 (P01~M47). P03·M40·M41 은 컴퓨터 안 (무중력) |
| `Art/Placeholder/Lu_Down.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lu/lu.controller` (상태 `Down`) | `lu_Down.anim` | `Data/Art/Art_lu.asset` | 모든 플레이 씬 (P01~M47). P03·M40·M41 은 컴퓨터 안 (무중력) |
| `Art/Placeholder/Lu_Sit.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lu/lu.controller` (상태 `Sit`) | `lu_Sit.anim` | `Data/Art/Art_lu.asset` | 모든 플레이 씬 (P01~M47). P03·M40·M41 은 컴퓨터 안 (무중력) |
| `Art/Placeholder/Lu_Flick.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lu/lu.controller` (상태 `Flick`) | `lu_Flick.anim` | `Data/Art/Art_lu.asset` | 모든 플레이 씬 (P01~M47). P03·M40·M41 은 컴퓨터 안 (무중력) |
| `Art/Placeholder/Lu_Point.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lu/lu.controller` (상태 `Point`) | `lu_Point.anim` | `Data/Art/Art_lu.asset` | 모든 플레이 씬 (P01~M47). P03·M40·M41 은 컴퓨터 안 (무중력) |
| `Art/Placeholder/Lu_Swing.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lu/lu.controller` (상태 `Swing`) | `lu_Swing.anim` | `Data/Art/Art_lu.asset` | 모든 플레이 씬 (P01~M47). P03·M40·M41 은 컴퓨터 안 (무중력) |
| `Art/Placeholder/Lu_Kick.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lu/lu.controller` (상태 `Kick`) | `lu_Kick.anim` | `Data/Art/Art_lu.asset` | 모든 플레이 씬 (P01~M47). P03·M40·M41 은 컴퓨터 안 (무중력) |
| `Art/Placeholder/Lu_Charge.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lu/lu.controller` (상태 `Charge`) | `lu_Charge.anim` | `Data/Art/Art_lu.asset` | 모든 플레이 씬 (P01~M47). P03·M40·M41 은 컴퓨터 안 (무중력) |
| `Art/Placeholder/Lu_Discharge.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lu/lu.controller` (상태 `Discharge`) | `lu_Discharge.anim` | `Data/Art/Art_lu.asset` | 모든 플레이 씬 (P01~M47). P03·M40·M41 은 컴퓨터 안 (무중력) |
| `Art/Placeholder/Lu_Carry.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lu/lu.controller` (상태 `Carry`) | `lu_Carry.anim` | `Data/Art/Art_lu.asset` | 모든 플레이 씬 (P01~M47). P03·M40·M41 은 컴퓨터 안 (무중력) |
| `Art/Placeholder/Lu_BackIdle.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lu/lu.controller` (상태 `BackIdle`) | `lu_BackIdle.anim` | `Data/Art/Art_lu.asset` | 모든 플레이 씬 (P01~M47). P03·M40·M41 은 컴퓨터 안 (무중력) |
| `Art/Placeholder/Lu_BackLook.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lu/lu.controller` (상태 `BackLook`) | `lu_BackLook.anim` | `Data/Art/Art_lu.asset` | 모든 플레이 씬 (P01~M47). P03·M40·M41 은 컴퓨터 안 (무중력) |
| `Art/Placeholder/Lu_Limp.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lu/lu.controller` (상태 `Limp`) | `lu_Limp.anim` | `Data/Art/Art_lu.asset` | 모든 플레이 씬 (P01~M47). P03·M40·M41 은 컴퓨터 안 (무중력) |
| `Art/Placeholder/Lu_LimpIdle.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lu/lu.controller` (상태 `LimpIdle`) | `lu_LimpIdle.anim` | `Data/Art/Art_lu.asset` | 모든 플레이 씬 (P01~M47). P03·M40·M41 은 컴퓨터 안 (무중력) |
| `Art/Placeholder/Lu_Sad.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lu/lu.controller` (상태 `Sad`) | `lu_Sad.anim` | `Data/Art/Art_lu.asset` | 모든 플레이 씬 (P01~M47). P03·M40·M41 은 컴퓨터 안 (무중력) |
| `Art/Placeholder/Lu_ReachPC.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lu/lu.controller` (상태 `ReachPC`) | `lu_ReachPC.anim` | `Data/Art/Art_lu.asset` | 모든 플레이 씬 (P01~M47). P03·M40·M41 은 컴퓨터 안 (무중력) |
| `Art/Placeholder/Lu_Connect.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lu/lu.controller` (상태 `Connect`) | `lu_Connect.anim` | `Data/Art/Art_lu.asset` | 모든 플레이 씬 (P01~M47). P03·M40·M41 은 컴퓨터 안 (무중력) |
| `Art/Placeholder/Lu_Land.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lu/lu.controller` (상태 `Land`) | `lu_Land.anim` | `Data/Art/Art_lu.asset` | 모든 플레이 씬 (P01~M47). P03·M40·M41 은 컴퓨터 안 (무중력) |
| `Art/Placeholder/Lu_GetUp.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lu/lu.controller` (상태 `GetUp`) | `lu_GetUp.anim` | `Data/Art/Art_lu.asset` | 모든 플레이 씬 (P01~M47). P03·M40·M41 은 컴퓨터 안 (무중력) |
| `Art/Placeholder/Lu_LookAround.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lu/lu.controller` (상태 `LookAround`) | `lu_LookAround.anim` | `Data/Art/Art_lu.asset` | 모든 플레이 씬 (P01~M47). P03·M40·M41 은 컴퓨터 안 (무중력) |
| `Art/Placeholder/Lu_Knock.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lu/lu.controller` (상태 `Knock`) | `lu_Knock.anim` | `Data/Art/Art_lu.asset` | 모든 플레이 씬 (P01~M47). P03·M40·M41 은 컴퓨터 안 (무중력) |
| `Art/Placeholder/Lu_BackAway.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lu/lu.controller` (상태 `BackAway`) | `lu_BackAway.anim` | `Data/Art/Art_lu.asset` | 모든 플레이 씬 (P01~M47). P03·M40·M41 은 컴퓨터 안 (무중력) |
| `Art/Placeholder/Lu_Fear.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lu/lu.controller` (상태 `Fear`) | `lu_Fear.anim` | `Data/Art/Art_lu.asset` | 모든 플레이 씬 (P01~M47). P03·M40·M41 은 컴퓨터 안 (무중력) |
| `Art/Placeholder/Lu_ChargeEyes.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lu/lu.controller` (상태 `ChargeEyes`) | `lu_ChargeEyes.anim` | `Data/Art/Art_lu.asset` | 모든 플레이 씬 (P01~M47). P03·M40·M41 은 컴퓨터 안 (무중력) |
| `Art/Placeholder/Lu_Recover.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lu/lu.controller` (상태 `Recover`) | `lu_Recover.anim` | `Data/Art/Art_lu.asset` | 모든 플레이 씬 (P01~M47). P03·M40·M41 은 컴퓨터 안 (무중력) |
| `Art/Placeholder/Lu_Squeeze.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lu/lu.controller` (상태 `Squeeze`) | `lu_Squeeze.anim` | `Data/Art/Art_lu.asset` | 모든 플레이 씬 (P01~M47). P03·M40·M41 은 컴퓨터 안 (무중력) |
| `Art/Placeholder/Lu_Crouch.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lu/lu.controller` (상태 `Crouch`) | `lu_Crouch.anim` | `Data/Art/Art_lu.asset` | 모든 플레이 씬 (P01~M47). P03·M40·M41 은 컴퓨터 안 (무중력) |
| `Art/Placeholder/Lu_CrouchWalk.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lu/lu.controller` (상태 `CrouchWalk`) | `lu_CrouchWalk.anim` | `Data/Art/Art_lu.asset` | 모든 플레이 씬 (P01~M47). P03·M40·M41 은 컴퓨터 안 (무중력) |
| `Art/Placeholder/Lu_Dodge.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lu/lu.controller` (상태 `Dodge`) | `lu_Dodge.anim` | `Data/Art/Art_lu.asset` | 모든 플레이 씬 (P01~M47). P03·M40·M41 은 컴퓨터 안 (무중력) |
| `Art/Placeholder/Lu_WallLean.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lu/lu.controller` (상태 `WallLean`) | `lu_WallLean.anim` | `Data/Art/Art_lu.asset` | 모든 플레이 씬 (P01~M47). P03·M40·M41 은 컴퓨터 안 (무중력) |
| `Art/Placeholder/Lu_TankCrouch.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lu/lu.controller` (상태 `TankCrouch`) | `lu_TankCrouch.anim` | `Data/Art/Art_lu.asset` | 모든 플레이 씬 (P01~M47). P03·M40·M41 은 컴퓨터 안 (무중력) |
| `Art/Placeholder/Lu_TankStruggle.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lu/lu.controller` (상태 `TankStruggle`) | `lu_TankStruggle.anim` | `Data/Art/Art_lu.asset` | 모든 플레이 씬 (P01~M47). P03·M40·M41 은 컴퓨터 안 (무중력) |
| `Art/Placeholder/Lu_TankCower.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lu/lu.controller` (상태 `TankCower`) | `lu_TankCower.anim` | `Data/Art/Art_lu.asset` | 모든 플레이 씬 (P01~M47). P03·M40·M41 은 컴퓨터 안 (무중력) |
| `Art/Placeholder/Lu_EyesClosed.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lu/lu.controller` (상태 `EyesClosed`) | `lu_EyesClosed.anim` | `Data/Art/Art_lu.asset` | 모든 플레이 씬 (P01~M47). P03·M40·M41 은 컴퓨터 안 (무중력) |
| `Art/Placeholder/Lu_Electrify.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lu/lu.controller` (상태 `Electrify`) | `lu_Electrify.anim` | `Data/Art/Art_lu.asset` | 모든 플레이 씬 (P01~M47). P03·M40·M41 은 컴퓨터 안 (무중력) |
| `Art/Placeholder/Lu_Disgust.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lu/lu.controller` (상태 `Disgust`) | `lu_Disgust.anim` | `Data/Art/Art_lu.asset` | 모든 플레이 씬 (P01~M47). P03·M40·M41 은 컴퓨터 안 (무중력) |
| `Art/Placeholder/Lu_HeadHold.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lu/lu.controller` (상태 `HeadHold`) | `lu_HeadHold.anim` | `Data/Art/Art_lu.asset` | 모든 플레이 씬 (P01~M47). P03·M40·M41 은 컴퓨터 안 (무중력) |
| `Art/Placeholder/Lu_Lifted.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lu/lu.controller` (상태 `Lifted`) | `lu_Lifted.anim` | `Data/Art/Art_lu.asset` | 모든 플레이 씬 (P01~M47). P03·M40·M41 은 컴퓨터 안 (무중력) |
| `Art/Placeholder/Lu_Thrown.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lu/lu.controller` (상태 `Thrown`) | `lu_Thrown.anim` | `Data/Art/Art_lu.asset` | 모든 플레이 씬 (P01~M47). P03·M40·M41 은 컴퓨터 안 (무중력) |
| `Art/Placeholder/Lu_HoldHand.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lu/lu.controller` (상태 `HoldHand`) | `lu_HoldHand.anim` | `Data/Art/Art_lu.asset` | 모든 플레이 씬 (P01~M47). P03·M40·M41 은 컴퓨터 안 (무중력) |
| `Art/Placeholder/Lu_HairUp.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lu/lu.controller` (상태 `HairUp`) | `lu_HairUp.anim` | `Data/Art/Art_lu.asset` | 모든 플레이 씬 (P01~M47). P03·M40·M41 은 컴퓨터 안 (무중력) |
| `Art/Placeholder/Lu_Share.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lu/lu.controller` (상태 `Share`) | `lu_Share.anim` | `Data/Art/Art_lu.asset` | 모든 플레이 씬 (P01~M47). P03·M40·M41 은 컴퓨터 안 (무중력) |
| `Art/Placeholder/Lu_HandsUp.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lu/lu.controller` (상태 `HandsUp`) | `lu_HandsUp.anim` | `Data/Art/Art_lu.asset` | 모든 플레이 씬 (P01~M47). P03·M40·M41 은 컴퓨터 안 (무중력) |
| `Art/Placeholder/Lu_Wind.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lu/lu.controller` (상태 `Wind`) | `lu_Wind.anim` | `Data/Art/Art_lu.asset` | 모든 플레이 씬 (P01~M47). P03·M40·M41 은 컴퓨터 안 (무중력) |
| `Art/Placeholder/Lu_Cold.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lu/lu.controller` (상태 `Cold`) | `lu_Cold.anim` | `Data/Art/Art_lu.asset` | 모든 플레이 씬 (P01~M47). P03·M40·M41 은 컴퓨터 안 (무중력) |
| `Art/Placeholder/Lu_Limit.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lu/lu.controller` (상태 `Limit`) | `lu_Limit.anim` | `Data/Art/Art_lu.asset` | 모든 플레이 씬 (P01~M47). P03·M40·M41 은 컴퓨터 안 (무중력) |
| `Art/Placeholder/Lu_Lie.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lu/lu.controller` (상태 `Lie`) | `lu_Lie.anim` | `Data/Art/Art_lu.asset` | 모든 플레이 씬 (P01~M47). P03·M40·M41 은 컴퓨터 안 (무중력) |
| `Art/Placeholder/Lu_Kneel.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lu/lu.controller` (상태 `Kneel`) | `lu_Kneel.anim` | `Data/Art/Art_lu.asset` | 모든 플레이 씬 (P01~M47). P03·M40·M41 은 컴퓨터 안 (무중력) |
| `Art/Placeholder/Lu_HandHold.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lu/lu.controller` (상태 `HandHold`) | `lu_HandHold.anim` | `Data/Art/Art_lu.asset` | 모든 플레이 씬 (P01~M47). P03·M40·M41 은 컴퓨터 안 (무중력) |
| `Art/Placeholder/Lu_Release.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lu/lu.controller` (상태 `Release`) | `lu_Release.anim` | `Data/Art/Art_lu.asset` | 모든 플레이 씬 (P01~M47). P03·M40·M41 은 컴퓨터 안 (무중력) |
| `Art/Placeholder/Venelle_Idle.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/venelle/venelle.controller` (상태 `Idle`) | `venelle_Idle.anim` | `Data/Art/Art_venelle.asset` | C06, M15, M30, M39 |
| `Art/Placeholder/Venelle_Walk.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/venelle/venelle.controller` (상태 `Walk`) | `venelle_Walk.anim` | `Data/Art/Art_venelle.asset` | C06, M15, M30, M39 |
| `Art/Placeholder/Venelle_Jump.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/venelle/venelle.controller` (상태 `Jump`) | `venelle_Jump.anim` | `Data/Art/Art_venelle.asset` | C06, M15, M30, M39 |
| `Art/Placeholder/Venelle_Fall.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/venelle/venelle.controller` (상태 `Fall`) | `venelle_Fall.anim` | `Data/Art/Art_venelle.asset` | C06, M15, M30, M39 |
| `Art/Placeholder/Venelle_Climb.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/venelle/venelle.controller` (상태 `Climb`) | `venelle_Climb.anim` | `Data/Art/Art_venelle.asset` | C06, M15, M30, M39 |
| `Art/Placeholder/Venelle_Hurt.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/venelle/venelle.controller` (상태 `Hurt`) | `venelle_Hurt.anim` | `Data/Art/Art_venelle.asset` | C06, M15, M30, M39 |
| `Art/Placeholder/Venelle_Down.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/venelle/venelle.controller` (상태 `Down`) | `venelle_Down.anim` | `Data/Art/Art_venelle.asset` | C06, M15, M30, M39 |
| `Art/Placeholder/Venelle_Sit.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/venelle/venelle.controller` (상태 `Sit`) | `venelle_Sit.anim` | `Data/Art/Art_venelle.asset` | C06, M15, M30, M39 |
| `Art/Placeholder/Venelle_Attack.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/venelle/venelle.controller` (상태 `Attack`) | `venelle_Attack.anim` | `Data/Art/Art_venelle.asset` | C06, M15, M30, M39 |
| `Art/Placeholder/Venelle_Collapsed.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/venelle/venelle.controller` (상태 `Collapsed`) | `venelle_Collapsed.anim` | `Data/Art/Art_venelle.asset` | C06, M15, M30, M39 |
| `Art/Placeholder/Venelle_Supported.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/venelle/venelle.controller` (상태 `Supported`) | `venelle_Supported.anim` | `Data/Art/Art_venelle.asset` | C06, M15, M30, M39 |
| `Art/Placeholder/Venelle_Dizzy.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/venelle/venelle.controller` (상태 `Dizzy`) | `venelle_Dizzy.anim` | `Data/Art/Art_venelle.asset` | C06, M15, M30, M39 |
| `Art/Placeholder/Venelle_Stalk.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/venelle/venelle.controller` (상태 `Stalk`) | `venelle_Stalk.anim` | `Data/Art/Art_venelle.asset` | C06, M15, M30, M39 |
| `Art/Placeholder/Venelle_Windup.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/venelle/venelle.controller` (상태 `Windup`) | `venelle_Windup.anim` | `Data/Art/Art_venelle.asset` | C06, M15, M30, M39 |
| `Art/Placeholder/Venelle_Dash.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/venelle/venelle.controller` (상태 `Dash`) | `venelle_Dash.anim` | `Data/Art/Art_venelle.asset` | C06, M15, M30, M39 |
| `Art/Placeholder/Venelle_Pant.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/venelle/venelle.controller` (상태 `Pant`) | `venelle_Pant.anim` | `Data/Art/Art_venelle.asset` | C06, M15, M30, M39 |
| `Art/Placeholder/Venelle_Laugh.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/venelle/venelle.controller` (상태 `Laugh`) | `venelle_Laugh.anim` | `Data/Art/Art_venelle.asset` | C06, M15, M30, M39 |
| `Art/Placeholder/Venelle_Grab.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/venelle/venelle.controller` (상태 `Grab`) | `venelle_Grab.anim` | `Data/Art/Art_venelle.asset` | C06, M15, M30, M39 |
| `Art/Placeholder/Venelle_Throw.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/venelle/venelle.controller` (상태 `Throw`) | `venelle_Throw.anim` | `Data/Art/Art_venelle.asset` | C06, M15, M30, M39 |
| `Art/Placeholder/Venelle_Spray.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/venelle/venelle.controller` (상태 `Spray`) | `venelle_Spray.anim` | `Data/Art/Art_venelle.asset` | C06, M15, M30, M39 |
| `Art/Placeholder/Venelle_Mix.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/venelle/venelle.controller` (상태 `Mix`) | `venelle_Mix.anim` | `Data/Art/Art_venelle.asset` | C06, M15, M30, M39 |
| `Art/Placeholder/Venelle_Toss.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/venelle/venelle.controller` (상태 `Toss`) | `venelle_Toss.anim` | `Data/Art/Art_venelle.asset` | C06, M15, M30, M39 |
| `Art/Placeholder/Venelle_Downed.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/venelle/venelle.controller` (상태 `Downed`) | `venelle_Downed.anim` | `Data/Art/Art_venelle.asset` | C06, M15, M30, M39 |
| `Art/Placeholder/Venelle_TalkWalk.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/venelle/venelle.controller` (상태 `TalkWalk`) | `venelle_TalkWalk.anim` | `Data/Art/Art_venelle.asset` | C06, M15, M30, M39 |
| `Art/Placeholder/Venelle_Comb.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/venelle/venelle.controller` (상태 `Comb`) | `venelle_Comb.anim` | `Data/Art/Art_venelle.asset` | C06, M15, M30, M39 |
| `Art/Placeholder/Venelle_Wind.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/venelle/venelle.controller` (상태 `Wind`) | `venelle_Wind.anim` | `Data/Art/Art_venelle.asset` | C06, M15, M30, M39 |
| `Art/Placeholder/Venelle_SitQuiet.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/venelle/venelle.controller` (상태 `SitQuiet`) | `venelle_SitQuiet.anim` | `Data/Art/Art_venelle.asset` | C06, M15, M30, M39 |
| `Art/Placeholder/Venelle_Share.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/venelle/venelle.controller` (상태 `Share`) | `venelle_Share.anim` | `Data/Art/Art_venelle.asset` | C06, M15, M30, M39 |
| `Art/Placeholder/Ivy_Idle.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/ivy/ivy.controller` (상태 `Idle`) | `ivy_Idle.anim` | `Data/Art/Art_ivy.asset` | M25 회상 컷신 (StoryEventsUnderground.Still) |
| `Art/Placeholder/Ivy_Walk.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/ivy/ivy.controller` (상태 `Walk`) | `ivy_Walk.anim` | `Data/Art/Art_ivy.asset` | M25 회상 컷신 (StoryEventsUnderground.Still) |
| `Art/Placeholder/Ivy_Hurt.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/ivy/ivy.controller` (상태 `Hurt`) | `ivy_Hurt.anim` | `Data/Art/Art_ivy.asset` | M25 회상 컷신 (StoryEventsUnderground.Still) |
| `Art/Placeholder/Ivy_Sit.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/ivy/ivy.controller` (상태 `Sit`) | `ivy_Sit.anim` | `Data/Art/Art_ivy.asset` | M25 회상 컷신 (StoryEventsUnderground.Still) |
| `Art/Placeholder/Ivy_Carry.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/ivy/ivy.controller` (상태 `Carry`) | `ivy_Carry.anim` | `Data/Art/Art_ivy.asset` | M25 회상 컷신 (StoryEventsUnderground.Still) |
| `Art/Placeholder/Ivy_Kneel.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/ivy/ivy.controller` (상태 `Kneel`) | `ivy_Kneel.anim` | `Data/Art/Art_ivy.asset` | M25 회상 컷신 (StoryEventsUnderground.Still) |
| `Art/Placeholder/Ivy_Give.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/ivy/ivy.controller` (상태 `Give`) | `ivy_Give.anim` | `Data/Art/Art_ivy.asset` | M25 회상 컷신 (StoryEventsUnderground.Still) |
| `Art/Placeholder/Ivy_Wave.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/ivy/ivy.controller` (상태 `Wave`) | `ivy_Wave.anim` | `Data/Art/Art_ivy.asset` | M25 회상 컷신 (StoryEventsUnderground.Still) |
| `Art/Placeholder/Raviel_Idle.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/raviel/raviel.controller` (상태 `Idle`) | `raviel_Idle.anim` | `Data/Art/Art_raviel.asset` | M32, M33, M44, M46 |
| `Art/Placeholder/Raviel_Walk.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/raviel/raviel.controller` (상태 `Walk`) | `raviel_Walk.anim` | `Data/Art/Art_raviel.asset` | M32, M33, M44, M46 |
| `Art/Placeholder/Raviel_Hurt.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/raviel/raviel.controller` (상태 `Hurt`) | `raviel_Hurt.anim` | `Data/Art/Art_raviel.asset` | M32, M33, M44, M46 |
| `Art/Placeholder/Raviel_Sit.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/raviel/raviel.controller` (상태 `Sit`) | `raviel_Sit.anim` | `Data/Art/Art_raviel.asset` | M32, M33, M44, M46 |
| `Art/Placeholder/Raviel_Down.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/raviel/raviel.controller` (상태 `Down`) | `raviel_Down.anim` | `Data/Art/Art_raviel.asset` | M32, M33, M44, M46 |
| `Art/Placeholder/Raviel_Talk.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/raviel/raviel.controller` (상태 `Talk`) | `raviel_Talk.anim` | `Data/Art/Art_raviel.asset` | M32, M33, M44, M46 |
| `Art/Placeholder/Raviel_Point.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/raviel/raviel.controller` (상태 `Point`) | `raviel_Point.anim` | `Data/Art/Art_raviel.asset` | M32, M33, M44, M46 |
| `Art/Placeholder/Raviel_Type.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/raviel/raviel.controller` (상태 `Type`) | `raviel_Type.anim` | `Data/Art/Art_raviel.asset` | M32, M33, M44, M46 |
| `Art/Placeholder/Raviel_HoldHand.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/raviel/raviel.controller` (상태 `HoldHand`) | `raviel_HoldHand.anim` | `Data/Art/Art_raviel.asset` | M32, M33, M44, M46 |
| `Art/Placeholder/Raviel_Catch.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/raviel/raviel.controller` (상태 `Catch`) | `raviel_Catch.anim` | `Data/Art/Art_raviel.asset` | M32, M33, M44, M46 |
| `Art/Placeholder/Raviel_Injured.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/raviel/raviel.controller` (상태 `Injured`) | `raviel_Injured.anim` | `Data/Art/Art_raviel.asset` | M32, M33, M44, M46 |
| `Art/Placeholder/Raviel_Cup.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/raviel/raviel.controller` (상태 `Cup`) | `raviel_Cup.anim` | `Data/Art/Art_raviel.asset` | M32, M33, M44, M46 |
| `Art/Placeholder/Raviel_Sad.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/raviel/raviel.controller` (상태 `Sad`) | `raviel_Sad.anim` | `Data/Art/Art_raviel.asset` | M32, M33, M44, M46 |
| `Art/Placeholder/RavielBeast_Idle.png` | 80×96 | 아래 가운데 | `Art/Generated/Anim/ravielBeast/ravielBeast.controller` (상태 `Idle`) | `ravielBeast_Idle.anim` | `Data/Art/Art_ravielBeast.asset` | M32, M45 |
| `Art/Placeholder/RavielBeast_Walk.png` | 80×96 | 아래 가운데 | `Art/Generated/Anim/ravielBeast/ravielBeast.controller` (상태 `Walk`) | `ravielBeast_Walk.anim` | `Data/Art/Art_ravielBeast.asset` | M32, M45 |
| `Art/Placeholder/RavielBeast_Windup.png` | 80×96 | 아래 가운데 | `Art/Generated/Anim/ravielBeast/ravielBeast.controller` (상태 `Windup`) | `ravielBeast_Windup.anim` | `Data/Art/Art_ravielBeast.asset` | M32, M45 |
| `Art/Placeholder/RavielBeast_Leap.png` | 80×96 | 아래 가운데 | `Art/Generated/Anim/ravielBeast/ravielBeast.controller` (상태 `Leap`) | `ravielBeast_Leap.anim` | `Data/Art/Art_ravielBeast.asset` | M32, M45 |
| `Art/Placeholder/RavielBeast_Slam.png` | 80×96 | 아래 가운데 | `Art/Generated/Anim/ravielBeast/ravielBeast.controller` (상태 `Slam`) | `ravielBeast_Slam.anim` | `Data/Art/Art_ravielBeast.asset` | M32, M45 |
| `Art/Placeholder/RavielBeast_Swipe.png` | 80×96 | 아래 가운데 | `Art/Generated/Anim/ravielBeast/ravielBeast.controller` (상태 `Swipe`) | `ravielBeast_Swipe.anim` | `Data/Art/Art_ravielBeast.asset` | M32, M45 |
| `Art/Placeholder/RavielBeast_Stun.png` | 80×96 | 아래 가운데 | `Art/Generated/Anim/ravielBeast/ravielBeast.controller` (상태 `Stun`) | `ravielBeast_Stun.anim` | `Data/Art/Art_ravielBeast.asset` | M32, M45 |
| `Art/Placeholder/RavielBeast_Hurt.png` | 80×96 | 아래 가운데 | `Art/Generated/Anim/ravielBeast/ravielBeast.controller` (상태 `Hurt`) | `ravielBeast_Hurt.anim` | `Data/Art/Art_ravielBeast.asset` | M32, M45 |
| `Art/Placeholder/RavielBeast_Hold.png` | 80×96 | 아래 가운데 | `Art/Generated/Anim/ravielBeast/ravielBeast.controller` (상태 `Hold`) | `ravielBeast_Hold.anim` | `Data/Art/Art_ravielBeast.asset` | M32, M45 |
| `Art/Placeholder/RavielBeast_Down.png` | 80×96 | 아래 가운데 | `Art/Generated/Anim/ravielBeast/ravielBeast.controller` (상태 `Down`) | `ravielBeast_Down.anim` | `Data/Art/Art_ravielBeast.asset` | M32, M45 |
| `Art/Placeholder/Meilin_Idle.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/meilin/meilin.controller` (상태 `Idle`) | `meilin_Idle.anim` | `Data/Art/Art_meilin.asset` | M28, M43, M44 |
| `Art/Placeholder/Meilin_Walk.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/meilin/meilin.controller` (상태 `Walk`) | `meilin_Walk.anim` | `Data/Art/Art_meilin.asset` | M28, M43, M44 |
| `Art/Placeholder/Meilin_Roll.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/meilin/meilin.controller` (상태 `Roll`) | `meilin_Roll.anim` | `Data/Art/Art_meilin.asset` | M28, M43, M44 |
| `Art/Placeholder/Meilin_Windup.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/meilin/meilin.controller` (상태 `Windup`) | `meilin_Windup.anim` | `Data/Art/Art_meilin.asset` | M28, M43, M44 |
| `Art/Placeholder/Meilin_Snip.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/meilin/meilin.controller` (상태 `Snip`) | `meilin_Snip.anim` | `Data/Art/Art_meilin.asset` | M28, M43, M44 |
| `Art/Placeholder/Meilin_Dash.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/meilin/meilin.controller` (상태 `Dash`) | `meilin_Dash.anim` | `Data/Art/Art_meilin.asset` | M28, M43, M44 |
| `Art/Placeholder/Meilin_Stun.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/meilin/meilin.controller` (상태 `Stun`) | `meilin_Stun.anim` | `Data/Art/Art_meilin.asset` | M28, M43, M44 |
| `Art/Placeholder/Meilin_Hurt.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/meilin/meilin.controller` (상태 `Hurt`) | `meilin_Hurt.anim` | `Data/Art/Art_meilin.asset` | M28, M43, M44 |
| `Art/Placeholder/Meilin_Talk.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/meilin/meilin.controller` (상태 `Talk`) | `meilin_Talk.anim` | `Data/Art/Art_meilin.asset` | M28, M43, M44 |
| `Art/Placeholder/Meilin_Angry.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/meilin/meilin.controller` (상태 `Angry`) | `meilin_Angry.anim` | `Data/Art/Art_meilin.asset` | M28, M43, M44 |
| `Art/Placeholder/Meilin_Pull.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/meilin/meilin.controller` (상태 `Pull`) | `meilin_Pull.anim` | `Data/Art/Art_meilin.asset` | M28, M43, M44 |
| `Art/Placeholder/Meilin_Down.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/meilin/meilin.controller` (상태 `Down`) | `meilin_Down.anim` | `Data/Art/Art_meilin.asset` | M28, M43, M44 |
| `Art/Placeholder/Lacrimosa_Idle.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lacrimosa/lacrimosa.controller` (상태 `Idle`) | `lacrimosa_Idle.anim` | `Data/Art/Art_lacrimosa.asset` | M37, M39 |
| `Art/Placeholder/Lacrimosa_Walk.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lacrimosa/lacrimosa.controller` (상태 `Walk`) | `lacrimosa_Walk.anim` | `Data/Art/Art_lacrimosa.asset` | M37, M39 |
| `Art/Placeholder/Lacrimosa_Sing.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lacrimosa/lacrimosa.controller` (상태 `Sing`) | `lacrimosa_Sing.anim` | `Data/Art/Art_lacrimosa.asset` | M37, M39 |
| `Art/Placeholder/Lacrimosa_Windup.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lacrimosa/lacrimosa.controller` (상태 `Windup`) | `lacrimosa_Windup.anim` | `Data/Art/Art_lacrimosa.asset` | M37, M39 |
| `Art/Placeholder/Lacrimosa_Stagger.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lacrimosa/lacrimosa.controller` (상태 `Stagger`) | `lacrimosa_Stagger.anim` | `Data/Art/Art_lacrimosa.asset` | M37, M39 |
| `Art/Placeholder/Lacrimosa_Listen.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lacrimosa/lacrimosa.controller` (상태 `Listen`) | `lacrimosa_Listen.anim` | `Data/Art/Art_lacrimosa.asset` | M37, M39 |
| `Art/Placeholder/Lacrimosa_Hurt.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lacrimosa/lacrimosa.controller` (상태 `Hurt`) | `lacrimosa_Hurt.anim` | `Data/Art/Art_lacrimosa.asset` | M37, M39 |
| `Art/Placeholder/Lacrimosa_Bow.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lacrimosa/lacrimosa.controller` (상태 `Bow`) | `lacrimosa_Bow.anim` | `Data/Art/Art_lacrimosa.asset` | M37, M39 |
| `Art/Placeholder/Lacrimosa_Down.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/lacrimosa/lacrimosa.controller` (상태 `Down`) | `lacrimosa_Down.anim` | `Data/Art/Art_lacrimosa.asset` | M37, M39 |
| `Art/Placeholder/Edward_Idle.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/edward/edward.controller` (상태 `Idle`) | `edward_Idle.anim` | `Data/Art/Art_edward.asset` | M37 |
| `Art/Placeholder/Edward_Write.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/edward/edward.controller` (상태 `Write`) | `edward_Write.anim` | `Data/Art/Art_edward.asset` | M37 |
| `Art/Placeholder/EdwardKid_Idle.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/edwardKid/edwardKid.controller` (상태 `Idle`) | `edwardKid_Idle.anim` | `Data/Art/Art_edwardKid.asset` | M37 |
| `Art/Placeholder/EdwardKid_Hide.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/edwardKid/edwardKid.controller` (상태 `Hide`) | `edwardKid_Hide.anim` | `Data/Art/Art_edwardKid.asset` | M37 |
| `Art/Placeholder/EdwardKid_Listen.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/edwardKid/edwardKid.controller` (상태 `Listen`) | `edwardKid_Listen.anim` | `Data/Art/Art_edwardKid.asset` | M37 |
| `Art/Placeholder/Aurelius_Idle.png` | 64×96 | 아래 가운데 | `Art/Generated/Anim/aurelius/aurelius.controller` (상태 `Idle`) | `aurelius_Idle.anim` | `Data/Art/Art_aurelius.asset` | M44 |
| `Art/Placeholder/Aurelius_Talk.png` | 64×96 | 아래 가운데 | `Art/Generated/Anim/aurelius/aurelius.controller` (상태 `Talk`) | `aurelius_Talk.anim` | `Data/Art/Art_aurelius.asset` | M44 |
| `Art/Placeholder/Aurelius_Windup.png` | 64×96 | 아래 가운데 | `Art/Generated/Anim/aurelius/aurelius.controller` (상태 `Windup`) | `aurelius_Windup.anim` | `Data/Art/Art_aurelius.asset` | M44 |
| `Art/Placeholder/Aurelius_Cast.png` | 64×96 | 아래 가운데 | `Art/Generated/Anim/aurelius/aurelius.controller` (상태 `Cast`) | `aurelius_Cast.anim` | `Data/Art/Art_aurelius.asset` | M44 |
| `Art/Placeholder/Aurelius_Hurt.png` | 64×96 | 아래 가운데 | `Art/Generated/Anim/aurelius/aurelius.controller` (상태 `Hurt`) | `aurelius_Hurt.anim` | `Data/Art/Art_aurelius.asset` | M44 |
| `Art/Placeholder/Aurelius_Broken.png` | 64×96 | 아래 가운데 | `Art/Generated/Anim/aurelius/aurelius.controller` (상태 `Broken`) | `aurelius_Broken.anim` | `Data/Art/Art_aurelius.asset` | M44 |
| `Art/Placeholder/Aurelius_Ruined.png` | 64×96 | 아래 가운데 | `Art/Generated/Anim/aurelius/aurelius.controller` (상태 `Ruined`) | `aurelius_Ruined.anim` | `Data/Art/Art_aurelius.asset` | M44 |
| `Art/Placeholder/Aurelius_Down.png` | 64×96 | 아래 가운데 | `Art/Generated/Anim/aurelius/aurelius.controller` (상태 `Down`) | `aurelius_Down.anim` | `Data/Art/Art_aurelius.asset` | M44 |
| `Assets/ThirdParty/Foozle_SciFiLab_Droids/Droid03/Droid03Idle.png` | 시트 높이 정사각 | 아래 가운데 | `Art/Generated/Anim/droid03/droid03.controller` (상태 `Idle`) | `droid03_Idle.anim` | `Data/Art/Art_droid03.asset` | C02~M35 감시 구역 (Guard 컴포넌트, artId 고정) |
| `Assets/ThirdParty/Foozle_SciFiLab_Droids/Droid03/Droid3Move.png` | 시트 높이 정사각 | 아래 가운데 | `Art/Generated/Anim/droid03/droid03.controller` (상태 `Move`) | `droid03_Move.anim` | `Data/Art/Art_droid03.asset` | C02~M35 감시 구역 (Guard 컴포넌트, artId 고정) |
| `Assets/ThirdParty/Foozle_SciFiLab_Droids/Droid03/Droid03Attack.png` | 시트 높이 정사각 | 아래 가운데 | `Art/Generated/Anim/droid03/droid03.controller` (상태 `Attack`) | `droid03_Attack.anim` | `Data/Art/Art_droid03.asset` | C02~M35 감시 구역 (Guard 컴포넌트, artId 고정) |
| `Assets/ThirdParty/Foozle_SciFiLab_Droids/Droid03/Droid03Hurt.png` | 시트 높이 정사각 | 아래 가운데 | `Art/Generated/Anim/droid03/droid03.controller` (상태 `Hurt`) | `droid03_Hurt.anim` | `Data/Art/Art_droid03.asset` | C02~M35 감시 구역 (Guard 컴포넌트, artId 고정) |
| `Assets/ThirdParty/Foozle_SciFiLab_Droids/Droid03/Droid03Death.png` | 시트 높이 정사각 | 아래 가운데 | `Art/Generated/Anim/droid03/droid03.controller` (상태 `Death`) | `droid03_Death.anim` | `Data/Art/Art_droid03.asset` | C02~M35 감시 구역 (Guard 컴포넌트, artId 고정) |
| `Assets/ThirdParty/Foozle_SciFiLab_Droids/Droid01/Droid01Idle.png` | 시트 높이 정사각 | 가운데 | `Art/Generated/Anim/droid01/droid01.controller` (상태 `Idle`) | `droid01_Idle.anim` | `Data/Art/Art_droid01.asset` | 감시 구역 드론·지원 드론 (Reinforcements) |
| `Assets/ThirdParty/Foozle_SciFiLab_Droids/Droid01/Droid01Move.png` | 시트 높이 정사각 | 가운데 | `Art/Generated/Anim/droid01/droid01.controller` (상태 `Move`) | `droid01_Move.anim` | `Data/Art/Art_droid01.asset` | 감시 구역 드론·지원 드론 (Reinforcements) |
| `Assets/ThirdParty/Foozle_SciFiLab_Droids/Droid01/Droid01Shoot.png` | 시트 높이 정사각 | 가운데 | `Art/Generated/Anim/droid01/droid01.controller` (상태 `Attack`) | `droid01_Attack.anim` | `Data/Art/Art_droid01.asset` | 감시 구역 드론·지원 드론 (Reinforcements) |
| `Assets/ThirdParty/Foozle_SciFiLab_Droids/Droid01/Droid01Hurt.png` | 시트 높이 정사각 | 가운데 | `Art/Generated/Anim/droid01/droid01.controller` (상태 `Hurt`) | `droid01_Hurt.anim` | `Data/Art/Art_droid01.asset` | 감시 구역 드론·지원 드론 (Reinforcements) |
| `Assets/ThirdParty/Foozle_SciFiLab_Droids/Droid01/Droid01Death.png` | 시트 높이 정사각 | 가운데 | `Art/Generated/Anim/droid01/droid01.controller` (상태 `Death`) | `droid01_Death.anim` | `Data/Art/Art_droid01.asset` | 감시 구역 드론·지원 드론 (Reinforcements) |
| `Assets/ThirdParty/Foozle_SciFiLab_Droids/Droid02/Droid02Idle.png` | 시트 높이 정사각 | 가운데 | `Art/Generated/Anim/droid02/droid02.controller` (상태 `Idle`) | `droid02_Idle.anim` | `Data/Art/Art_droid02.asset` | M47 |
| `Assets/ThirdParty/Foozle_SciFiLab_Droids/Droid02/Droid02Move.png` | 시트 높이 정사각 | 가운데 | `Art/Generated/Anim/droid02/droid02.controller` (상태 `Move`) | `droid02_Move.anim` | `Data/Art/Art_droid02.asset` | M47 |
| `Assets/ThirdParty/Foozle_SciFiLab_Droids/Droid02/Droid02Attack.png` | 시트 높이 정사각 | 가운데 | `Art/Generated/Anim/droid02/droid02.controller` (상태 `Attack`) | `droid02_Attack.anim` | `Data/Art/Art_droid02.asset` | M47 |
| `Assets/ThirdParty/Foozle_SciFiLab_Droids/Droid02/Droid02Hurt.png` | 시트 높이 정사각 | 가운데 | `Art/Generated/Anim/droid02/droid02.controller` (상태 `Hurt`) | `droid02_Hurt.anim` | `Data/Art/Art_droid02.asset` | M47 |
| `Assets/ThirdParty/Foozle_SciFiLab_Droids/Droid02/Droid02Death.png` | 시트 높이 정사각 | 가운데 | `Art/Generated/Anim/droid02/droid02.controller` (상태 `Death`) | `droid02_Death.anim` | `Data/Art/Art_droid02.asset` | M47 |
| `Art/Placeholder/Researcher_Idle.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/researcher/researcher.controller` (상태 `Idle`) | `researcher_Idle.anim` | `Data/Art/Art_researcher.asset` | C01 |
| `Art/Placeholder/Researcher_Walk.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/researcher/researcher.controller` (상태 `Walk`) | `researcher_Walk.anim` | `Data/Art/Art_researcher.asset` | C01 |
| `Art/Placeholder/Researcher_Hurt.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/researcher/researcher.controller` (상태 `Hurt`) | `researcher_Hurt.anim` | `Data/Art/Art_researcher.asset` | C01 |
| `Art/Placeholder/Researcher_Down.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/researcher/researcher.controller` (상태 `Down`) | `researcher_Down.anim` | `Data/Art/Art_researcher.asset` | C01 |
| `Art/Placeholder/Researcher_Scared.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/researcher/researcher.controller` (상태 `Scared`) | `researcher_Scared.anim` | `Data/Art/Art_researcher.asset` | C01 |
| `Art/Placeholder/Researcher_Talk.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/researcher/researcher.controller` (상태 `Talk`) | `researcher_Talk.anim` | `Data/Art/Art_researcher.asset` | C01 |
| `Art/Placeholder/Researcher_BatonOut.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/researcher/researcher.controller` (상태 `BatonOut`) | `researcher_BatonOut.anim` | `Data/Art/Art_researcher.asset` | C01 |
| `Art/Placeholder/Researcher_BatonIdle.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/researcher/researcher.controller` (상태 `BatonIdle`) | `researcher_BatonIdle.anim` | `Data/Art/Art_researcher.asset` | C01 |
| `Art/Placeholder/Researcher_BatonWalk.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/researcher/researcher.controller` (상태 `BatonWalk`) | `researcher_BatonWalk.anim` | `Data/Art/Art_researcher.asset` | C01 |
| `Art/Placeholder/Researcher2_Idle.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/researcher2/researcher2.controller` (상태 `Idle`) | `researcher2_Idle.anim` | `Data/Art/Art_researcher2.asset` | C01 |
| `Art/Placeholder/Researcher2_Walk.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/researcher2/researcher2.controller` (상태 `Walk`) | `researcher2_Walk.anim` | `Data/Art/Art_researcher2.asset` | C01 |
| `Art/Placeholder/Researcher2_Hurt.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/researcher2/researcher2.controller` (상태 `Hurt`) | `researcher2_Hurt.anim` | `Data/Art/Art_researcher2.asset` | C01 |
| `Art/Placeholder/Researcher2_Down.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/researcher2/researcher2.controller` (상태 `Down`) | `researcher2_Down.anim` | `Data/Art/Art_researcher2.asset` | C01 |
| `Art/Placeholder/Researcher2_Scared.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/researcher2/researcher2.controller` (상태 `Scared`) | `researcher2_Scared.anim` | `Data/Art/Art_researcher2.asset` | C01 |
| `Art/Placeholder/Researcher2_Talk.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/researcher2/researcher2.controller` (상태 `Talk`) | `researcher2_Talk.anim` | `Data/Art/Art_researcher2.asset` | C01 |
| `Art/Placeholder/Researcher2_BatonOut.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/researcher2/researcher2.controller` (상태 `BatonOut`) | `researcher2_BatonOut.anim` | `Data/Art/Art_researcher2.asset` | C01 |
| `Art/Placeholder/Researcher2_BatonIdle.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/researcher2/researcher2.controller` (상태 `BatonIdle`) | `researcher2_BatonIdle.anim` | `Data/Art/Art_researcher2.asset` | C01 |
| `Art/Placeholder/Researcher2_BatonWalk.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/researcher2/researcher2.controller` (상태 `BatonWalk`) | `researcher2_BatonWalk.anim` | `Data/Art/Art_researcher2.asset` | C01 |
| `Art/Placeholder/F_Idle.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/f/f.controller` (상태 `Idle`) | `f_Idle.anim` | `Data/Art/Art_f.asset` | M17, M39 |
| `Art/Placeholder/F_Walk.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/f/f.controller` (상태 `Walk`) | `f_Walk.anim` | `Data/Art/Art_f.asset` | M17, M39 |
| `Art/Placeholder/F_Hurt.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/f/f.controller` (상태 `Hurt`) | `f_Hurt.anim` | `Data/Art/Art_f.asset` | M17, M39 |
| `Art/Placeholder/F_Sit.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/f/f.controller` (상태 `Sit`) | `f_Sit.anim` | `Data/Art/Art_f.asset` | M17, M39 |
| `Art/Placeholder/F_Talk.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/f/f.controller` (상태 `Talk`) | `f_Talk.anim` | `Data/Art/Art_f.asset` | M17, M39 |
| `Art/Placeholder/F_Point.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/f/f.controller` (상태 `Point`) | `f_Point.anim` | `Data/Art/Art_f.asset` | M17, M39 |
| `Art/Placeholder/F_Toss.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/f/f.controller` (상태 `Toss`) | `f_Toss.anim` | `Data/Art/Art_f.asset` | M17, M39 |
| `Art/Placeholder/F_Guard.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/f/f.controller` (상태 `Guard`) | `f_Guard.anim` | `Data/Art/Art_f.asset` | M17, M39 |
| `Art/Placeholder/F_Tag.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/f/f.controller` (상태 `Tag`) | `f_Tag.anim` | `Data/Art/Art_f.asset` | M17, M39 |
| `Art/Placeholder/Kid008_Idle.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/kid008/kid008.controller` (상태 `Idle`) | `kid008_Idle.anim` | `Data/Art/Art_kid008.asset` | M17, M19, M39 |
| `Art/Placeholder/Kid008_Walk.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/kid008/kid008.controller` (상태 `Walk`) | `kid008_Walk.anim` | `Data/Art/Art_kid008.asset` | M17, M19, M39 |
| `Art/Placeholder/Kid008_Hurt.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/kid008/kid008.controller` (상태 `Hurt`) | `kid008_Hurt.anim` | `Data/Art/Art_kid008.asset` | M17, M19, M39 |
| `Art/Placeholder/Kid008_Sit.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/kid008/kid008.controller` (상태 `Sit`) | `kid008_Sit.anim` | `Data/Art/Art_kid008.asset` | M17, M19, M39 |
| `Art/Placeholder/Kid008_Talk.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/kid008/kid008.controller` (상태 `Talk`) | `kid008_Talk.anim` | `Data/Art/Art_kid008.asset` | M17, M19, M39 |
| `Art/Placeholder/Kid008_Hide.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/kid008/kid008.controller` (상태 `Hide`) | `kid008_Hide.anim` | `Data/Art/Art_kid008.asset` | M17, M19, M39 |
| `Art/Placeholder/Kid008_Eat.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/kid008/kid008.controller` (상태 `Eat`) | `kid008_Eat.anim` | `Data/Art/Art_kid008.asset` | M17, M19, M39 |
| `Art/Placeholder/Kid008_Hold.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/kid008/kid008.controller` (상태 `Hold`) | `kid008_Hold.anim` | `Data/Art/Art_kid008.asset` | M17, M19, M39 |
| `Art/Placeholder/Kid008_Draw.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/kid008/kid008.controller` (상태 `Draw`) | `kid008_Draw.anim` | `Data/Art/Art_kid008.asset` | M17, M19, M39 |
| `Art/Placeholder/SurvivorA_Idle.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/survivorA/survivorA.controller` (상태 `Idle`) | `survivorA_Idle.anim` | `Data/Art/Art_survivorA.asset` | M19, M27, M30, M39 |
| `Art/Placeholder/SurvivorA_Walk.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/survivorA/survivorA.controller` (상태 `Walk`) | `survivorA_Walk.anim` | `Data/Art/Art_survivorA.asset` | M19, M27, M30, M39 |
| `Art/Placeholder/SurvivorA_Hurt.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/survivorA/survivorA.controller` (상태 `Hurt`) | `survivorA_Hurt.anim` | `Data/Art/Art_survivorA.asset` | M19, M27, M30, M39 |
| `Art/Placeholder/SurvivorA_Sit.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/survivorA/survivorA.controller` (상태 `Sit`) | `survivorA_Sit.anim` | `Data/Art/Art_survivorA.asset` | M19, M27, M30, M39 |
| `Art/Placeholder/SurvivorA_Scared.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/survivorA/survivorA.controller` (상태 `Scared`) | `survivorA_Scared.anim` | `Data/Art/Art_survivorA.asset` | M19, M27, M30, M39 |
| `Art/Placeholder/SurvivorA_Guard.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/survivorA/survivorA.controller` (상태 `Guard`) | `survivorA_Guard.anim` | `Data/Art/Art_survivorA.asset` | M19, M27, M30, M39 |
| `Art/Placeholder/SurvivorA_Windup.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/survivorA/survivorA.controller` (상태 `Windup`) | `survivorA_Windup.anim` | `Data/Art/Art_survivorA.asset` | M19, M27, M30, M39 |
| `Art/Placeholder/SurvivorA_Strike.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/survivorA/survivorA.controller` (상태 `Strike`) | `survivorA_Strike.anim` | `Data/Art/Art_survivorA.asset` | M19, M27, M30, M39 |
| `Art/Placeholder/SurvivorA_Toss.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/survivorA/survivorA.controller` (상태 `Toss`) | `survivorA_Toss.anim` | `Data/Art/Art_survivorA.asset` | M19, M27, M30, M39 |
| `Art/Placeholder/SurvivorA_Calm.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/survivorA/survivorA.controller` (상태 `Calm`) | `survivorA_Calm.anim` | `Data/Art/Art_survivorA.asset` | M19, M27, M30, M39 |
| `Art/Placeholder/SurvivorA_Down.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/survivorA/survivorA.controller` (상태 `Down`) | `survivorA_Down.anim` | `Data/Art/Art_survivorA.asset` | M19, M27, M30, M39 |
| `Art/Placeholder/SurvivorB_Idle.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/survivorB/survivorB.controller` (상태 `Idle`) | `survivorB_Idle.anim` | `Data/Art/Art_survivorB.asset` | M19, M27, M30, M39 |
| `Art/Placeholder/SurvivorB_Walk.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/survivorB/survivorB.controller` (상태 `Walk`) | `survivorB_Walk.anim` | `Data/Art/Art_survivorB.asset` | M19, M27, M30, M39 |
| `Art/Placeholder/SurvivorB_Hurt.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/survivorB/survivorB.controller` (상태 `Hurt`) | `survivorB_Hurt.anim` | `Data/Art/Art_survivorB.asset` | M19, M27, M30, M39 |
| `Art/Placeholder/SurvivorB_Sit.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/survivorB/survivorB.controller` (상태 `Sit`) | `survivorB_Sit.anim` | `Data/Art/Art_survivorB.asset` | M19, M27, M30, M39 |
| `Art/Placeholder/SurvivorB_Scared.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/survivorB/survivorB.controller` (상태 `Scared`) | `survivorB_Scared.anim` | `Data/Art/Art_survivorB.asset` | M19, M27, M30, M39 |
| `Art/Placeholder/SurvivorB_Guard.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/survivorB/survivorB.controller` (상태 `Guard`) | `survivorB_Guard.anim` | `Data/Art/Art_survivorB.asset` | M19, M27, M30, M39 |
| `Art/Placeholder/SurvivorB_Windup.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/survivorB/survivorB.controller` (상태 `Windup`) | `survivorB_Windup.anim` | `Data/Art/Art_survivorB.asset` | M19, M27, M30, M39 |
| `Art/Placeholder/SurvivorB_Strike.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/survivorB/survivorB.controller` (상태 `Strike`) | `survivorB_Strike.anim` | `Data/Art/Art_survivorB.asset` | M19, M27, M30, M39 |
| `Art/Placeholder/SurvivorB_Toss.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/survivorB/survivorB.controller` (상태 `Toss`) | `survivorB_Toss.anim` | `Data/Art/Art_survivorB.asset` | M19, M27, M30, M39 |
| `Art/Placeholder/SurvivorB_Calm.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/survivorB/survivorB.controller` (상태 `Calm`) | `survivorB_Calm.anim` | `Data/Art/Art_survivorB.asset` | M19, M27, M30, M39 |
| `Art/Placeholder/SurvivorB_Down.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/survivorB/survivorB.controller` (상태 `Down`) | `survivorB_Down.anim` | `Data/Art/Art_survivorB.asset` | M19, M27, M30, M39 |
| `Art/Placeholder/MemberA_Idle.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/memberA/memberA.controller` (상태 `Idle`) | `memberA_Idle.anim` | `Data/Art/Art_memberA.asset` | M36, M39, M42 |
| `Art/Placeholder/MemberA_Walk.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/memberA/memberA.controller` (상태 `Walk`) | `memberA_Walk.anim` | `Data/Art/Art_memberA.asset` | M36, M39, M42 |
| `Art/Placeholder/MemberA_Hurt.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/memberA/memberA.controller` (상태 `Hurt`) | `memberA_Hurt.anim` | `Data/Art/Art_memberA.asset` | M36, M39, M42 |
| `Art/Placeholder/MemberA_Sit.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/memberA/memberA.controller` (상태 `Sit`) | `memberA_Sit.anim` | `Data/Art/Art_memberA.asset` | M36, M39, M42 |
| `Art/Placeholder/MemberA_Down.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/memberA/memberA.controller` (상태 `Down`) | `memberA_Down.anim` | `Data/Art/Art_memberA.asset` | M36, M39, M42 |
| `Art/Placeholder/MemberA_Wait.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/memberA/memberA.controller` (상태 `Wait`) | `memberA_Wait.anim` | `Data/Art/Art_memberA.asset` | M36, M39, M42 |
| `Art/Placeholder/MemberA_Scared.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/memberA/memberA.controller` (상태 `Scared`) | `memberA_Scared.anim` | `Data/Art/Art_memberA.asset` | M36, M39, M42 |
| `Art/Placeholder/MemberA_Cheer.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/memberA/memberA.controller` (상태 `Cheer`) | `memberA_Cheer.anim` | `Data/Art/Art_memberA.asset` | M36, M39, M42 |
| `Art/Placeholder/MemberB_Idle.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/memberB/memberB.controller` (상태 `Idle`) | `memberB_Idle.anim` | `Data/Art/Art_memberB.asset` | M36, M39, M42, M43 |
| `Art/Placeholder/MemberB_Walk.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/memberB/memberB.controller` (상태 `Walk`) | `memberB_Walk.anim` | `Data/Art/Art_memberB.asset` | M36, M39, M42, M43 |
| `Art/Placeholder/MemberB_Hurt.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/memberB/memberB.controller` (상태 `Hurt`) | `memberB_Hurt.anim` | `Data/Art/Art_memberB.asset` | M36, M39, M42, M43 |
| `Art/Placeholder/MemberB_Sit.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/memberB/memberB.controller` (상태 `Sit`) | `memberB_Sit.anim` | `Data/Art/Art_memberB.asset` | M36, M39, M42, M43 |
| `Art/Placeholder/MemberB_Down.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/memberB/memberB.controller` (상태 `Down`) | `memberB_Down.anim` | `Data/Art/Art_memberB.asset` | M36, M39, M42, M43 |
| `Art/Placeholder/MemberB_Wait.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/memberB/memberB.controller` (상태 `Wait`) | `memberB_Wait.anim` | `Data/Art/Art_memberB.asset` | M36, M39, M42, M43 |
| `Art/Placeholder/MemberB_Scared.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/memberB/memberB.controller` (상태 `Scared`) | `memberB_Scared.anim` | `Data/Art/Art_memberB.asset` | M36, M39, M42, M43 |
| `Art/Placeholder/MemberB_Cheer.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/memberB/memberB.controller` (상태 `Cheer`) | `memberB_Cheer.anim` | `Data/Art/Art_memberB.asset` | M36, M39, M42, M43 |
| `Art/Placeholder/Warden_Idle.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/warden/warden.controller` (상태 `Idle`) | `warden_Idle.anim` | `Data/Art/Art_warden.asset` | M30 |
| `Art/Placeholder/Warden_Walk.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/warden/warden.controller` (상태 `Walk`) | `warden_Walk.anim` | `Data/Art/Art_warden.asset` | M30 |
| `Art/Placeholder/Warden_Hurt.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/warden/warden.controller` (상태 `Hurt`) | `warden_Hurt.anim` | `Data/Art/Art_warden.asset` | M30 |
| `Art/Placeholder/Warden_Sit.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/warden/warden.controller` (상태 `Sit`) | `warden_Sit.anim` | `Data/Art/Art_warden.asset` | M30 |
| `Art/Placeholder/Warden_Down.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/warden/warden.controller` (상태 `Down`) | `warden_Down.anim` | `Data/Art/Art_warden.asset` | M30 |
| `Art/Placeholder/Warden_Guard.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/warden/warden.controller` (상태 `Guard`) | `warden_Guard.anim` | `Data/Art/Art_warden.asset` | M30 |
| `Art/Placeholder/Warden_Strike.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/warden/warden.controller` (상태 `Strike`) | `warden_Strike.anim` | `Data/Art/Art_warden.asset` | M30 |
| `Art/Placeholder/Linden_Idle.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/linden/linden.controller` (상태 `Idle`) | `linden_Idle.anim` | `Data/Art/Art_linden.asset` | M25 회상 컷신 |
| `Art/Placeholder/Linden_Walk.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/linden/linden.controller` (상태 `Walk`) | `linden_Walk.anim` | `Data/Art/Art_linden.asset` | M25 회상 컷신 |
| `Art/Placeholder/Linden_Hurt.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/linden/linden.controller` (상태 `Hurt`) | `linden_Hurt.anim` | `Data/Art/Art_linden.asset` | M25 회상 컷신 |
| `Art/Placeholder/Linden_Sit.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/linden/linden.controller` (상태 `Sit`) | `linden_Sit.anim` | `Data/Art/Art_linden.asset` | M25 회상 컷신 |
| `Art/Placeholder/Linden_Hold.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/linden/linden.controller` (상태 `Hold`) | `linden_Hold.anim` | `Data/Art/Art_linden.asset` | M25 회상 컷신 |
| `Art/Placeholder/Linden_Wave.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/linden/linden.controller` (상태 `Wave`) | `linden_Wave.anim` | `Data/Art/Art_linden.asset` | M25 회상 컷신 |
| `Art/Placeholder/Adult_Idle.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/adult/adult.controller` (상태 `Idle`) | `adult_Idle.anim` | `Data/Art/Art_adult.asset` | M25 회상 컷신 (추정: 회상 장면 다섯 중) |
| `Art/Placeholder/Adult_Walk.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/adult/adult.controller` (상태 `Walk`) | `adult_Walk.anim` | `Data/Art/Art_adult.asset` | M25 회상 컷신 (추정: 회상 장면 다섯 중) |
| `Art/Placeholder/Adult_Crutch.png` | 48×64 | 아래 가운데 | `Art/Generated/Anim/adult/adult.controller` (상태 `Crutch`) | `adult_Crutch.anim` | `Data/Art/Art_adult.asset` | M25 회상 컷신 (추정: 회상 장면 다섯 중) |

SpriteRenderer: 인물 오브젝트는 실행 중 `Field/Actor`·`Field/PlayerController` 가 `CharacterArt` 를 읽어 만든다 (씬의 `ActorSpawn` 은 artId 만 가진다). 정렬 순서: 루 10~12, 적 9 (`Art_*.asset` sortingOrder).

## 스탠딩

| 파일 | 규격 | 연결 |
|---|---|---|
| `Assets/Game/Art/Portraits/aurelius_determined.png` | 600×900 권장 (2:3), 투명, 아래 가운데 | `GameConfig.portraits` id `aurelius_determined` → 대사 줄 `"s"`/`"p"` + `"e"` |
| `Assets/Game/Art/Portraits/aurelius_gentle.png` | 600×900 권장 (2:3), 투명, 아래 가운데 | `GameConfig.portraits` id `aurelius_gentle` → 대사 줄 `"s"`/`"p"` + `"e"` |
| `Assets/Game/Art/Portraits/aurelius_pained.png` | 600×900 권장 (2:3), 투명, 아래 가운데 | `GameConfig.portraits` id `aurelius_pained` → 대사 줄 `"s"`/`"p"` + `"e"` |
| `Assets/Game/Art/Portraits/edward_quiet.png` | 600×900 권장 (2:3), 투명, 아래 가운데 | `GameConfig.portraits` id `edward_quiet` → 대사 줄 `"s"`/`"p"` + `"e"` |
| `Assets/Game/Art/Portraits/f_determined.png` | 600×900 권장 (2:3), 투명, 아래 가운데 | `GameConfig.portraits` id `f_determined` → 대사 줄 `"s"`/`"p"` + `"e"` |
| `Assets/Game/Art/Portraits/f_gentle.png` | 600×900 권장 (2:3), 투명, 아래 가운데 | `GameConfig.portraits` id `f_gentle` → 대사 줄 `"s"`/`"p"` + `"e"` |
| `Assets/Game/Art/Portraits/f_neutral.png` | 600×900 권장 (2:3), 투명, 아래 가운데 | `GameConfig.portraits` id `f_neutral` → 대사 줄 `"s"`/`"p"` + `"e"` |
| `Assets/Game/Art/Portraits/kid008_neutral.png` | 600×900 권장 (2:3), 투명, 아래 가운데 | `GameConfig.portraits` id `kid008_neutral` → 대사 줄 `"s"`/`"p"` + `"e"` |
| `Assets/Game/Art/Portraits/kid008_sad.png` | 600×900 권장 (2:3), 투명, 아래 가운데 | `GameConfig.portraits` id `kid008_sad` → 대사 줄 `"s"`/`"p"` + `"e"` |
| `Assets/Game/Art/Portraits/kid008_smile.png` | 600×900 권장 (2:3), 투명, 아래 가운데 | `GameConfig.portraits` id `kid008_smile` → 대사 줄 `"s"`/`"p"` + `"e"` |
| `Assets/Game/Art/Portraits/kid008_surprised.png` | 600×900 권장 (2:3), 투명, 아래 가운데 | `GameConfig.portraits` id `kid008_surprised` → 대사 줄 `"s"`/`"p"` + `"e"` |
| `Assets/Game/Art/Portraits/lacrimosa_gentle.png` | 600×900 권장 (2:3), 투명, 아래 가운데 | `GameConfig.portraits` id `lacrimosa_gentle` → 대사 줄 `"s"`/`"p"` + `"e"` |
| `Assets/Game/Art/Portraits/lacrimosa_pained.png` | 600×900 권장 (2:3), 투명, 아래 가운데 | `GameConfig.portraits` id `lacrimosa_pained` → 대사 줄 `"s"`/`"p"` + `"e"` |
| `Assets/Game/Art/Portraits/lacrimosa_quiet.png` | 600×900 권장 (2:3), 투명, 아래 가운데 | `GameConfig.portraits` id `lacrimosa_quiet` → 대사 줄 `"s"`/`"p"` + `"e"` |
| `Assets/Game/Art/Portraits/lacrimosa_sad.png` | 600×900 권장 (2:3), 투명, 아래 가운데 | `GameConfig.portraits` id `lacrimosa_sad` → 대사 줄 `"s"`/`"p"` + `"e"` |
| `Assets/Game/Art/Portraits/lu_angry_cry.png` | 600×900 권장 (2:3), 투명, 아래 가운데 | `GameConfig.portraits` id `lu_angry_cry` → 대사 줄 `"s"`/`"p"` + `"e"` |
| `Assets/Game/Art/Portraits/lu_despair.png` | 600×900 권장 (2:3), 투명, 아래 가운데 | `GameConfig.portraits` id `lu_despair` → 대사 줄 `"s"`/`"p"` + `"e"` |
| `Assets/Game/Art/Portraits/lu_determined.png` | 600×900 권장 (2:3), 투명, 아래 가운데 | `GameConfig.portraits` id `lu_determined` → 대사 줄 `"s"`/`"p"` + `"e"` |
| `Assets/Game/Art/Portraits/lu_neutral.png` | 600×900 권장 (2:3), 투명, 아래 가운데 | `GameConfig.portraits` id `lu_neutral` → 대사 줄 `"s"`/`"p"` + `"e"` |
| `Assets/Game/Art/Portraits/lu_pained.png` | 600×900 권장 (2:3), 투명, 아래 가운데 | `GameConfig.portraits` id `lu_pained` → 대사 줄 `"s"`/`"p"` + `"e"` |
| `Assets/Game/Art/Portraits/lu_sad.png` | 600×900 권장 (2:3), 투명, 아래 가운데 | `GameConfig.portraits` id `lu_sad` → 대사 줄 `"s"`/`"p"` + `"e"` |
| `Assets/Game/Art/Portraits/lu_scared.png` | 600×900 권장 (2:3), 투명, 아래 가운데 | `GameConfig.portraits` id `lu_scared` → 대사 줄 `"s"`/`"p"` + `"e"` |
| `Assets/Game/Art/Portraits/lu_smile.png` | 600×900 권장 (2:3), 투명, 아래 가운데 | `GameConfig.portraits` id `lu_smile` → 대사 줄 `"s"`/`"p"` + `"e"` |
| `Assets/Game/Art/Portraits/lu_surprised.png` | 600×900 권장 (2:3), 투명, 아래 가운데 | `GameConfig.portraits` id `lu_surprised` → 대사 줄 `"s"`/`"p"` + `"e"` |
| `Assets/Game/Art/Portraits/meilin_determined.png` | 600×900 권장 (2:3), 투명, 아래 가운데 | `GameConfig.portraits` id `meilin_determined` → 대사 줄 `"s"`/`"p"` + `"e"` |
| `Assets/Game/Art/Portraits/meilin_sad.png` | 600×900 권장 (2:3), 투명, 아래 가운데 | `GameConfig.portraits` id `meilin_sad` → 대사 줄 `"s"`/`"p"` + `"e"` |
| `Assets/Game/Art/Portraits/meilin_scared.png` | 600×900 권장 (2:3), 투명, 아래 가운데 | `GameConfig.portraits` id `meilin_scared` → 대사 줄 `"s"`/`"p"` + `"e"` |
| `Assets/Game/Art/Portraits/meilin_smile.png` | 600×900 권장 (2:3), 투명, 아래 가운데 | `GameConfig.portraits` id `meilin_smile` → 대사 줄 `"s"`/`"p"` + `"e"` |
| `Assets/Game/Art/Portraits/meilin_surprised.png` | 600×900 권장 (2:3), 투명, 아래 가운데 | `GameConfig.portraits` id `meilin_surprised` → 대사 줄 `"s"`/`"p"` + `"e"` |
| `Assets/Game/Art/Portraits/member_neutral.png` | 600×900 권장 (2:3), 투명, 아래 가운데 | `GameConfig.portraits` id `member_neutral` → 대사 줄 `"s"`/`"p"` + `"e"` |
| `Assets/Game/Art/Portraits/raviel_neutral.png` | 600×900 권장 (2:3), 투명, 아래 가운데 | `GameConfig.portraits` id `raviel_neutral` → 대사 줄 `"s"`/`"p"` + `"e"` |
| `Assets/Game/Art/Portraits/raviel_pained.png` | 600×900 권장 (2:3), 투명, 아래 가운데 | `GameConfig.portraits` id `raviel_pained` → 대사 줄 `"s"`/`"p"` + `"e"` |
| `Assets/Game/Art/Portraits/raviel_sad.png` | 600×900 권장 (2:3), 투명, 아래 가운데 | `GameConfig.portraits` id `raviel_sad` → 대사 줄 `"s"`/`"p"` + `"e"` |
| `Assets/Game/Art/Portraits/raviel_smile.png` | 600×900 권장 (2:3), 투명, 아래 가운데 | `GameConfig.portraits` id `raviel_smile` → 대사 줄 `"s"`/`"p"` + `"e"` |
| `Assets/Game/Art/Portraits/raviel_smirk.png` | 600×900 권장 (2:3), 투명, 아래 가운데 | `GameConfig.portraits` id `raviel_smirk` → 대사 줄 `"s"`/`"p"` + `"e"` |
| `Assets/Game/Art/Portraits/raviel_surprised.png` | 600×900 권장 (2:3), 투명, 아래 가운데 | `GameConfig.portraits` id `raviel_surprised` → 대사 줄 `"s"`/`"p"` + `"e"` |
| `Assets/Game/Art/Portraits/researcher2_neutral.png` | 600×900 권장 (2:3), 투명, 아래 가운데 | `GameConfig.portraits` id `researcher2_neutral` → 대사 줄 `"s"`/`"p"` + `"e"` |
| `Assets/Game/Art/Portraits/researcher2_smirk.png` | 600×900 권장 (2:3), 투명, 아래 가운데 | `GameConfig.portraits` id `researcher2_smirk` → 대사 줄 `"s"`/`"p"` + `"e"` |
| `Assets/Game/Art/Portraits/researcher2_surprised.png` | 600×900 권장 (2:3), 투명, 아래 가운데 | `GameConfig.portraits` id `researcher2_surprised` → 대사 줄 `"s"`/`"p"` + `"e"` |
| `Assets/Game/Art/Portraits/researcher_neutral.png` | 600×900 권장 (2:3), 투명, 아래 가운데 | `GameConfig.portraits` id `researcher_neutral` → 대사 줄 `"s"`/`"p"` + `"e"` |
| `Assets/Game/Art/Portraits/researcher_smile.png` | 600×900 권장 (2:3), 투명, 아래 가운데 | `GameConfig.portraits` id `researcher_smile` → 대사 줄 `"s"`/`"p"` + `"e"` |
| `Assets/Game/Art/Portraits/researcher_smirk.png` | 600×900 권장 (2:3), 투명, 아래 가운데 | `GameConfig.portraits` id `researcher_smirk` → 대사 줄 `"s"`/`"p"` + `"e"` |
| `Assets/Game/Art/Portraits/survivor_neutral.png` | 600×900 권장 (2:3), 투명, 아래 가운데 | `GameConfig.portraits` id `survivor_neutral` → 대사 줄 `"s"`/`"p"` + `"e"` |
| `Assets/Game/Art/Portraits/survivor_scared.png` | 600×900 권장 (2:3), 투명, 아래 가운데 | `GameConfig.portraits` id `survivor_scared` → 대사 줄 `"s"`/`"p"` + `"e"` |
| `Assets/Game/Art/Portraits/venelle_blank.png` | 600×900 권장 (2:3), 투명, 아래 가운데 | `GameConfig.portraits` id `venelle_blank` → 대사 줄 `"s"`/`"p"` + `"e"` |
| `Assets/Game/Art/Portraits/venelle_confused.png` | 600×900 권장 (2:3), 투명, 아래 가운데 | `GameConfig.portraits` id `venelle_confused` → 대사 줄 `"s"`/`"p"` + `"e"` |
| `Assets/Game/Art/Portraits/venelle_excited.png` | 600×900 권장 (2:3), 투명, 아래 가운데 | `GameConfig.portraits` id `venelle_excited` → 대사 줄 `"s"`/`"p"` + `"e"` |
| `Assets/Game/Art/Portraits/venelle_gentle.png` | 600×900 권장 (2:3), 투명, 아래 가운데 | `GameConfig.portraits` id `venelle_gentle` → 대사 줄 `"s"`/`"p"` + `"e"` |
| `Assets/Game/Art/Portraits/venelle_quiet.png` | 600×900 권장 (2:3), 투명, 아래 가운데 | `GameConfig.portraits` id `venelle_quiet` → 대사 줄 `"s"`/`"p"` + `"e"` |
| `Assets/Game/Art/Portraits/lu_<새 표정>.png` (MISSING) | 같음 | 새 표정 이름을 대사 데이터 `"e"` 에 지정해야 화면에 나온다 (데이터 변경 필요 — 이번 작업 범위 밖) |

## 씬 배경·지형·소품

| 파일 | 크기 | 연결 씬 | 연결 방식 |
|---|---|---|---|
| `Game/Art/Placeholder/FX_LightDot.png` | 64×16 | M40, P03 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Prop_Battery.png` | 32×32 | M22, M34 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Prop_Bed.png` | 64×32 | M25, M33 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Prop_Bench.png` | 48×24 | C05, M15, M16, M21, M24, M26, M28, M29, M31, M33, M35, M36, M38, M42, M43 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Prop_CeilingLight.png` | 64×8 | C01, M21 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Prop_Clothes.png` | 32×32 | C01, M25 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Prop_Computer.png` | 128×32 | M22, M27, M34 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Prop_Cupboard.png` | 32×48 | M24, M25, M33, M38 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Prop_Cups.png` | 16×16 | M25 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Prop_Desk.png` | 48×32 | C03, M09, M22, M25, M27, M29, M30, M33, M34, M37, M38 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Prop_Doc.png` | 16×16 | C03, M21, M29, M34 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Prop_Door.png` | 96×64 | C03, C05, M09, M14, M15, M17, M18, M19, M22, M23, M24, M25, M26, M27, M28, M29, M30, M32, M36, M38 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Prop_EmergencyLight.png` | 32×16 | C01 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Prop_ExposedWire.png` | 128×32 | C05, M20, M22, M27 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Prop_Ladder.png` | 32×32 | C03, C04, C05, M15, M22, M23 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Prop_Lever.png` | 32×32 | M09 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Prop_Mirror.png` | 32×48 | M25 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Prop_PosterEmblem.png` | 48×48 | C03, M16, M19, M28 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Prop_PowerSocket.png` | 96×32 | C04, M15, M23 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Prop_Puddle.png` | 64×8 | M13, M14, M20, M23 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Prop_RestoreButton.png` | 32×32 | M21, M22 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Prop_Snack.png` | 16×16 | C03, C04, C05, M13, M14, M15, M16, M20, M22, M23, M27, M29, M32, M34, M36 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Prop_Vending.png` | 96×64 | M09 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Prop_Water.png` | 16×16 | M15 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/D_Desktop.png` | 640×352 | P03 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/D_Icon_guide.png` | 24×24 | P03 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/D_Icon_status.png` | 24×24 | P03 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/D_Taskbar.png` | 640×32 | P03 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/FX_Ash.png` | 2×2 | P01 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/FX_Dust.png` | 2×2 | C01, C02, C03, C04, C05, C06, M09, M13, M14, M15, M16, M17, M18, M19, M20, M21, M22, M23, M24, M25, M26, M27, M28, M29, M30, M31, M32, M34, M35, M36, M37, M38, M39, M42, M43, M44, M45, M46, M47, P02 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/FX_Ember.png` | 2×2 | P01 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/FX_FadeDown.png` | 4×64 | P01 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/FX_Fire.png` | 96×24 | P01 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/FX_FireBig.png` | 192×48 | P01 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/FX_Glow.png` | 64×64 | M16, M24, P01 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/FX_GlowSoft.png` | 64×64 | C01, C02, C03, C05, C06, M16, M17, M20, M21, M22, M23, M25, M27, M28, M30, M32, M33, M34, M35, M36, M42, M44, M45, M46 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/FX_HazeBand.png` | 4×64 | M26, P01 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/FX_Smoke.png` | 128×32 | C01, P01 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/H_Bed.png` | 64×32 | P02 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/H_Desk.png` | 56×32 | P02 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/H_Floor.png` | 32×32 | P02 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/H_MaterialsA.png` | 64×20 | P02 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/H_PC.png` | 160×32 | P02 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/H_RuinBack.png` | 448×288 | P02 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/H_Shelf.png` | 40×48 | P02 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/L_Cables.png` | 96×24 | C01, M23 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/L_Chamber.png` | 128×96 | C03, C05, M13 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/L_Console.png` | 144×40 | C01, M21, M31, M34, M39 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/L_Crane.png` | 160×120 | C04, M16, M22 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/L_DebrisS.png` | 48×20 | C01 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/L_Door.png` | 144×80 | C01, C02, C03, C06, M23, M24, M28, M32 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/L_FallenPanel.png` | 72×28 | C01 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/L_Far.png` | 640×260 | C01, C02, C03, C04, C05, C06, M09, M13, M14, M15, M16, M17, M18, M19, M20, M21, M22, M23, M24, M27, M28, M29, M30, M31, M32, M34, M35, M36, M37, M38, M39, M42, M43, M44, M45, M46, M47 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/L_Floor.png` | 32×32 | C03, C04, C05, C06, M13, M15, M16, M17, M18, M20, M21, M22, M23, M24, M27, M28, M32, M34, M35, M36, M37, M39, M42, M43 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/L_FloorFill.png` | 32×32 | C01, C02, C03, C04, C05, C06, M09, M13, M14, M15, M16, M17, M18, M19, M20, M21, M22, M23, M24, M27, M28, M29, M30, M31, M32, M34, M35, M36, M37, M38, M39, M42, M43, M44, M45, M46, M47 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/L_ForePillar.png` | 40×240 | M09, M13, M14, M18, M23, M29, M38 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/L_ForePipe.png` | 240×36 | C01, C02, C03, C04, C05, C06, M09, M13, M14, M15, M16, M17, M18, M19, M20, M21, M22, M23, M24, M27, M28, M29, M30, M31, M32, M34, M35, M36, M37, M38, M39, M42, M43, M44, M45, M46, M47 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/L_MidA.png` | 320×200 | C01, C03, C05, C06, M09, M14, M15, M17, M19, M21, M23, M24, M28, M29, M31, M34, M36, M38, M42, M46 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/L_MidB.png` | 320×200 | C02, C04, M13, M16, M18, M20, M22, M27, M30, M32, M35, M37, M39, M43, M44, M45, M47 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/L_Scorch.png` | 64×16 | C01 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/L_TankBack.png` | 160×112 | C01 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/L_TankGlass.png` | 160×112 | C01 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/L_WallPanel.png` | 64×64 | C01, C02, C03, C04, C05, C06, M09, M13, M14, M15, M16, M17, M18, M19, M20, M21, M22, M23, M24, M27, M28, M29, M30, M31, M32, M34, M35, M36, M37, M38, M39, M42, M43, M44, M45, M46, M47 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/S_AlarmBeam.png` | 96×24 | C01, C02, C06, M30, M44, M45 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/S_AlarmLamp.png` | 14×12 | C01, C02, C06, M30, M44, M45 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/S_BgBox.png` | 40×24 | M16 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/S_BodyBoard.png` | 48×40 | C01, C02, C03, C04, C05, C06, M15, M16, M17, M20, M21, M22, M24, M27, M28, M30, M32, M34, M35, M36, M37, M39, M42, M43, M44, M45, M46, M47 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/S_CctvHead.png` | 26×12 | C02, C03, C04, C05, M20, M22, M27 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/S_CctvMount.png` | 12×14 | C02, C03, C04, C05, M20, M22, M27 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/S_CrateS.png` | 32×32 | C02, C03, C04, M20 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/S_CrateSolid.png` | 48×32 | C02, C03, C04, C05, M16 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/S_CrateTall.png` | 48×48 | C05 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/S_HideRack.png` | 56×44 | C02, C03, C04, C05, M16, M20, M22, M23, M27, M34 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/S_HideSacks.png` | 64×36 | C02, C03, C05, M20, M22, M27, M35 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/S_IconPower.png` | 11×13 | C03, C05, M18, M20, M22, M27, M28, M44 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/S_IconPowerBusy.png` | 11×13 | C03, C05, M20, M22, M27, M28, M44 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/S_IconPowerEmpty.png` | 11×13 | C03, C05, M20, M22, M27, M28, M44 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/S_Jar0.png` | 18×34 | C01, C02, C03, C04, C05, C06, M13, M15, M16, M17, M18, M19, M20, M21, M22, M24, M27, M28, M30, M31, M32, M34, M35, M36, M37, M39, M42, M43, M44, M45, M46, M47 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/S_Jar1.png` | 18×42 | C01, C02, C03, C04, C05, C06, M09, M13, M14, M15, M16, M17, M18, M19, M20, M21, M22, M23, M24, M27, M28, M29, M30, M31, M32, M34, M35, M36, M37, M38, M39, M42, M43, M44, M45, M46, M47 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/S_Junction.png` | 22×28 | C06, M22, M23, M28 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/S_Lamp.png` | 4×4 | C03 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/S_Lens.png` | 4×4 | C02, C03, C04, C05, M20, M22, M27 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/S_LensOff.png` | 4×4 | C02, C03, C04, C05, M20, M22, M27 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/S_Lockers.png` | 64×48 | C01, C02, C03, C04, C05, C06, M09, M14, M15, M16, M17, M19, M20, M21, M22, M23, M24, M27, M28, M29, M30, M31, M32, M34, M35, M36, M37, M38, M39, M42, M43, M44, M45, M46 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/S_ObsWindow.png` | 96×56 | C01, C02, C03, C04, C05, C06, M13, M14, M15, M16, M17, M18, M19, M20, M21, M22, M23, M24, M27, M28, M29, M30, M31, M32, M34, M35, M36, M37, M38, M39, M42, M43, M44, M45, M46, M47 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/S_PartsBin.png` | 32×26 | C01, M17, M21, M29 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/S_PowerBox.png` | 26×34 | C03, C05, M20, M22, M27, M28, M44 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/S_PowerBoxDead.png` | 26×34 | M22 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/S_RestraintBed.png` | 64×30 | C01 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/S_ShutterPanel.png` | 16×24 | C03, M23, M28, M32 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/S_SignIsolation.png` | 64×16 | C01 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/S_SurgLamp.png` | 40×26 | C01 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/S_TankLabel.png` | 30×14 | C01 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/S_ToolTray.png` | 28×30 | C01, M21 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/S_Tray.png` | 30×30 | C03, C04, M20 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/S_WorkLamp.png` | 30×12 | C01, C06, M16, M20, M23, M27, M28, M32, M34, M35, M36, M42 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_Barricade.png` | 72×44 | M17 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_Belt.png` | 128×10 | M22, M23 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_BeltCrate.png` | 20×16 | M22, M28 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_BeltCtrl.png` | 72×32 | M22 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_BoardStatus.png` | 64×28 | M42 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_BoothLock.png` | 16×20 | M29 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_Brake.png` | 40×30 | M28 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_Breaker.png` | 48×36 | M18 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_BrokenTag.png` | 12×8 | M39 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_Bulkhead.png` | 72×96 | M30, M31 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_CallBoard.png` | 160×20 | M28 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_Capacitor.png` | 56×52 | M44 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_Capsule.png` | 80×76 | M43, M44 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_CaptureTerm.png` | 144×72 | M39 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_CellBars.png` | 80×72 | M27 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_CellPanel.png` | 40×36 | M27 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_ChalkLines.png` | 96×10 | M19 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_ChildPhoto.png` | 28×22 | M33 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_ChocoBox.png` | 20×10 | M25 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_Claw.png` | 128×48 | M28 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_ClawRail.png` | 32×6 | M28 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_CommandChart.png` | 56×40 | M36 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_ConductFloor.png` | 32×6 | M28 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_CoolPipe.png` | 16×64 | M44 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_Coupling.png` | 216×72 | M44 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_CrateBed.png` | 48×20 | M17, M27 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_Cups.png` | 48×12 | M33 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_Doll.png` | 24×44 | M28 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_IconBin.png` | 16×16 | M41 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_IconDoc.png` | 16×16 | M40, M41 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_Jug.png` | 32×24 | M17 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_Lantern.png` | 24×20 | M19 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_LaundryLine.png` | 80×34 | M17 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_Letter.png` | 14×10 | M25 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_LiftDeck.png` | 96×14 | M16, M30 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_LiftPanel.png` | 32×32 | M16 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_LiftShaft.png` | 112×120 | M16 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_Magnet.png` | 32×24 | M44 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_MaintTerm.png` | 56×36 | M28 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_Mic.png` | 40×40 | M37 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_Mixer.png` | 96×24 | M37 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_Monitor.png` | 144×36 | M30, M31 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_OtherOutfits.png` | 64×40 | M24 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_Part.png` | 14×12 | M17, M18 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_PartShelf.png` | 40×36 | M18 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_Photo.png` | 12×14 | M25 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_PipeH.png` | 16×16 | M18 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_PlanNotes.png` | 64×28 | M36 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_Pump.png` | 256×48 | M18 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_Rail.png` | 32×14 | M26 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_Register.png` | 14×10 | M43, M44 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_Roller.png` | 12×8 | M44 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_Rubble.png` | 80×32 | M39, M44, M45, M46, M47 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_SealShutter.png` | 48×96 | M16, M30 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_ServerRack.png` | 56×64 | M34, M39 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_Speaker.png` | 64×48 | M37 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_StallA.png` | 64×48 | M19 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_StallB.png` | 64×48 | M19 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_StillHand.png` | 16×8 | M39 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_Stove.png` | 48×30 | M17, M39 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_Tally.png` | 40×24 | M17 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_Tape.png` | 20×14 | M38 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_Tarp.png` | 96×20 | M17, M39 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_TestRig.png` | 56×22 | M21 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_TicketMachine.png` | 24×44 | M28 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_Truss.png` | 96×16 | M37, M39 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_TwinOutfits.png` | 64×40 | M24 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_Valve.png` | 72×32 | M18, M44 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_Washer.png` | 56×32 | M24 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_WaterFill.png` | 8×8 | M18 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_WaterTop.png` | 64×6 | M18 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_WaterTower.png` | 48×64 | M26 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/U_Wrappers.png` | 40×8 | M25 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/V_Cart.png` | 64×40 | P01 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/V_Far.png` | 640×220 | P01 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/V_Fence.png` | 48×24 | P01 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/V_ForeBeam.png` | 80×200 | P01 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/V_ForeRubble.png` | 200×56 | P01 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/V_Ground.png` | 32×32 | P01 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/V_GroundFill.png` | 32×32 | P01 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/V_HouseA.png` | 80×100 | P01 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/V_HouseB.png` | 64×80 | P01 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/V_HouseC.png` | 96×112 | P01 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/V_LuHouse.png` | 112×96 | P01 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/V_RubbleL.png` | 120×40 | P01 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/V_RubbleM.png` | 80×32 | P01 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/V_RubbleS.png` | 48×24 | P01 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/V_RuinWall.png` | 64×48 | P01 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Scene/V_Sky.png` | 4×360 | P01 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Tile_HomeFloor.png` | 32×32 | M25, M33 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Tile_HomeWall.png` | 32×32 | M25, M33 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Tile_Platform.png` | 32×8 | C02, C03, C04, C05, M13, M15, M22 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `Game/Art/Placeholder/Tile_Roof.png` | 32×32 | M26 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `ThirdParty/Foozle_SciFiLab_Pickups/chemical_pot_item.png` | 384×32 | M13 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `ThirdParty/Foozle_SciFiLab_Tileset/full_decor_tiles.png` | 96×96 | C04, C05, M13, M14 | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `ThirdParty/ansimuz_SynthCities/back.png` | 112×272 | M26, M35, Title | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |
| `ThirdParty/ansimuz_SynthCities/middle.png` | 256×272 | Title | 방 정의 코드가 이름으로 SpriteRenderer 생성 (`Editor/EWRooms*.cs` → 씬) |

## 이펙트·UI 스프라이트 세트 (GameConfig)

| 세트 이름 | 원본 파일 | 프레임 | FPS | 코드 사용 |
|---|---|---|---|---|
| `FX_Acid` | `Game/Art/Placeholder/FX_Acid.png` | 4 | 16 | 세트 이름으로 동적 사용 (추정) |
| `FX_AcidFan` | `Game/Art/Placeholder/FX_AcidFan.png` | 4 | 14 | Action/VenelleBoss.cs:197 |
| `FX_AcidSplash` | `Game/Art/Placeholder/FX_AcidSplash.png` | 5 | 14 | Action/VenelleBoss.cs:177, Action/VenelleBoss.cs:388 |
| `FX_Arc` | `Game/Art/Placeholder/FX_Arc.png` | 3 | 16 | Action/AureliusBoss.cs:122, Action/AureliusBoss.cs:143 |
| `FX_Ash` | `Game/Art/Placeholder/Scene/FX_Ash.png` | 1 | 12 | 세트 이름으로 동적 사용 (추정) |
| `FX_Bolt` | `Game/Art/Placeholder/FX_Bolt.png` | 2 | 16 | 세트 이름으로 동적 사용 (추정) |
| `FX_Bubbles` | `Game/Art/Placeholder/FX_Bubbles.png` | 4 | 8 | Action/LacrimosaBoss.cs:132, Action/VenelleBoss.cs:239 |
| `FX_Dust` | `Game/Art/Placeholder/Scene/FX_Dust.png` | 1 | 12 | 세트 이름으로 동적 사용 (추정) |
| `FX_Ember` | `Game/Art/Placeholder/Scene/FX_Ember.png` | 1 | 12 | 세트 이름으로 동적 사용 (추정) |
| `FX_Explosion` | `Game/Art/Placeholder/FX_Explosion.png` | 6 | 16 | Action/AureliusBoss.cs:291, Action/AureliusBoss.cs:300 |
| `FX_Fire` | `Game/Art/Placeholder/Scene/FX_Fire.png` | 6 | 12 | 세트 이름으로 동적 사용 (추정) |
| `FX_FireBig` | `Game/Art/Placeholder/Scene/FX_FireBig.png` | 6 | 12 | 세트 이름으로 동적 사용 (추정) |
| `FX_Flask` | `Game/Art/Placeholder/FX_Flask.png` | 4 | 14 | Action/VenelleBoss.cs:370, Story/StoryEventsUnderground.cs:515 |
| `FX_Glow` | `Game/Art/Placeholder/Scene/FX_Glow.png` | 1 | 12 | Story/Cutscene.cs:284 |
| `FX_GlowSoft` | `Game/Art/Placeholder/Scene/FX_GlowSoft.png` | 1 | 12 | 세트 이름으로 동적 사용 (추정) |
| `FX_Heal` | `Game/Art/Placeholder/FX_Heal.png` | 4 | 16 | Field/Pickup.cs:24 |
| `FX_LightDot` | `Game/Art/Placeholder/FX_LightDot.png` | 4 | 12 | Action/BossBase.cs:164, Action/Projectile.cs:19 |
| `FX_PuddleAcid` | `Game/Art/Placeholder/FX_PuddleAcid.png` | 3 | 14 | Action/VenelleBoss.cs:396 |
| `FX_ReactBurst` | `Game/Art/Placeholder/FX_ReactBurst.png` | 5 | 14 | Action/LacrimosaBoss.cs:146, Action/VenelleBoss.cs:240 |
| `FX_Smoke` | `Game/Art/Placeholder/Scene/FX_Smoke.png` | 4 | 6 | Action/AureliusBoss.cs:220, Action/LacrimosaBoss.cs:194 |
| `FX_Spark` | `Game/Art/Placeholder/FX_Spark.png` | 6 | 16 | Action/AureliusBoss.cs:156, Action/BossBase.cs:177 |
| `FX_ToxicMist` | `Game/Art/Placeholder/FX_ToxicMist.png` | 4 | 5 | Action/VenelleBoss.cs:157 |
| `UI_Prev_Cctv` | `Game/Art/Placeholder/UI_Prev_Cctv.png` | 1 | 1 | UI/GuideBook.cs:201, UI/GuideBook.cs:254 |
| `UI_Prev_CctvOff` | `Game/Art/Placeholder/UI_Prev_CctvOff.png` | 1 | 1 | UI/GuideBook.cs:254 |
| `UI_Prev_Dummy` | `Game/Art/Placeholder/UI_Prev_Dummy.png` | 1 | 1 | UI/GuideBook.cs:199 |
| `UI_Prev_Panel` | `Game/Art/Placeholder/UI_Prev_Panel.png` | 1 | 1 | UI/GuideBook.cs:204 |
| `UI_Prev_Socket` | `Game/Art/Placeholder/UI_Prev_Socket.png` | 1 | 1 | UI/GuideBook.cs:206, UI/GuideBook.cs:207 |
| `UI_Prev_Source` | `Game/Art/Placeholder/UI_Prev_Source.png` | 1 | 1 | UI/GuideBook.cs:203 |
| `UI_Skill_jam` | `Game/Art/Placeholder/UI_Skill_jam.png` | 1 | 1 | 세트 이름으로 동적 사용 (추정) |
| `UI_Skill_pull` | `Game/Art/Placeholder/UI_Skill_pull.png` | 1 | 1 | 세트 이름으로 동적 사용 (추정) |
| `UI_Skill_spark_shot` | `Game/Art/Placeholder/UI_Skill_spark_shot.png` | 1 | 1 | 세트 이름으로 동적 사용 (추정) |
| `UI_Skill_tok` | `Game/Art/Placeholder/UI_Skill_tok.png` | 1 | 1 | 세트 이름으로 동적 사용 (추정) |
| `UI_Skill_wire` | `Game/Art/Placeholder/UI_Skill_wire.png` | 1 | 1 | 세트 이름으로 동적 사용 (추정) |
| `U_IconBin` | `Game/Art/Placeholder/Scene/U_IconBin.png` | 1 | 4 | UI/EndingScreens.cs:58 |
| `U_IconDoc` | `Game/Art/Placeholder/Scene/U_IconDoc.png` | 1 | 4 | UI/EndingScreens.cs:58 |
| `U_IconFolder` | `Game/Art/Placeholder/Scene/U_IconFolder.png` | 1 | 4 | UI/EndingScreens.cs:58 |
| `U_IconGame` | `Game/Art/Placeholder/Scene/U_IconGame.png` | 1 | 4 | UI/EndingScreens.cs:58 |

`U_*` 소품 세트 77개는 같은 파일이 씬에도 배치된다 (위 표). 세트는 연출 코드가 `StoryEvents.PropSprite` 로 이름을 불러 쓴다.
