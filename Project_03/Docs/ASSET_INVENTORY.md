# 에셋 목록 (실제 보유 기준)

조사일: 2026-11-14. `Assets/ASSET/` 의 ZIP 8개를 임시 폴더에 풀어 실제 파일·픽셀 크기·라이선스 파일을 확인했다.
압축 내부 경로 중 대상 폴더 밖으로 나가는 항목은 **없었다** (8개 모두 검사). ZIP 원본은 그대로 두었고,
게임은 `Assets/ThirdParty/<팩>/` 로 선별 복사한 파일만 참조한다.

## 1. 보유 ZIP과 사용 여부

| 팩 (원본 ZIP) | 라이선스 (동봉 파일로 확인) | 이번 시연판 사용 |
|---|---|---|
| Foozle Sci-fi Lab Tileset/Decor/Traps (`Foozle_2DT0001_Science_Fiction_Lab_TilesetDecorTraps.zip`) | CC0 — `Readme.txt` | 사용 |
| Foozle Sci-fi Lab Droids (`Foozle_2DC0006_Science_Fiction_Labs_Droids_Pack.zip`) | CC0 — `Readme.txt` | 사용 (일반 적 3종) |
| Foozle Mecha Boss + Exploding Drone (`Foozle_2DC0008_Sci_Fi_Lab_Mecha_Boss_Plus_Drone.zip`) | CC0 — `Readme.txt` | **미사용** (가져오지 않음) |
| Foozle Sci-fi Lab Pickups (`Foozle_2DS0011_Science_Fiction_Labs_Pickups.zip`) | CC0 — `Readme.txt` | 사용 (회복 붕대, 약품병 장식) |
| Free Industrial Zone Tileset (`Free Industrial Zone Tileset.zip`) | CraftPix 파일 라이선스 — `license.txt`(URL만 기재: craftpix.net/file-licenses). 원본 재배포 불가 | **미사용** (가져오지 않음) |
| ansimuz Synth Cities / cyberpunk street (`cyberpunk-street-files.zip`) | CC0 — `public-license.pdf` (이미지형 PDF, 내용 확인함) | 사용 (타이틀·옥상·골목 배경, 타이틀 음악) |
| ansimuz Synth Cities Godot 데모 (`SynthCitiesGodot.zip`) | 동봉 라이선스 파일 없음 → **미확인** | **미사용** (Godot 프로젝트·`.godot` 캐시·셰이더. 같은 그림이 위 cyberpunk-street ZIP 에 있음) |
| Kenney Pixel UI Pack (`kenney_pixel-ui-pack.zip`) | CC0 — `License.txt` | 사용 (대화창·메뉴 9-slice 패널) |

추가로 받은 무료 에셋 (GDD 13.3 A13, 결제 없음):

| 에셋 | 출처 | 라이선스 | 사용 |
|---|---|---|---|
| Galmuri v2.40.4 (`Galmuri11.ttf`, `Galmuri11-Bold.ttf`) | github.com/quiple/galmuri 공식 Releases | SIL OFL 1.1 — `LICENSE_OFL.txt` 동봉. 배포 시 고지 포함 | 모든 UI 한글 |

## 2. 실제 파일 구성과 분할 규격

### Foozle Sci-fi Lab Droids → `Assets/ThirdParty/Foozle_SciFiLab_Droids/`
시트는 가로 한 줄, **프레임 = 시트 높이(정사각형)**. Droid01 은 32px 과 48px 시트가 섞여 있어 중앙 피벗으로 맞췄다.

| 시트 | 크기 | 프레임 | 사용 상태 |
|---|---|---|---|
| Droid01Idle / Move | 128×32 | 32×32 ×4 | 포획 드론 Idle/Move |
| Droid01Shoot | 576×48 | 48×48 ×12 | Attack |
| Droid01Hurt | 96×32 | 32×32 ×3 | Hurt |
| Droid01Death | 288×32 | 32×32 ×9 | Death |
| Droid02Idle / Move / Hurt | 192×48 | 48×48 ×4 | 운반 로봇(대용) |
| Droid02Attack | 336×48 | 48×48 ×7 | Attack |
| Droid02Death | 624×48 | 48×48 ×13 | Death |
| Droid03Idle / Hurt | 192×48 | 48×48 ×4 | 순찰 경비(대용) |
| Droid3Move | 384×48 | 48×48 ×8 | Move |
| Droid03Attack | 432×48 | 48×48 ×9 | Attack |
| Droid03Death | 528×48 | 48×48 ×11 | Death |
| 각 Projectile/Impact | 48~64×16 | 16×16 | (가져왔으나 이번엔 미사용) |
| SelfDestruct/CountDown | — | — | 가져왔으나 미사용 |

같은 ZIP 의 `.aseprite`, GIF 미리보기는 가져오지 않았다.

### Foozle Sci-fi Lab Tileset → `Assets/ThirdParty/Foozle_SciFiLab_Tileset/`
| 파일 | 크기 | 사용 |
|---|---|---|
| level_tileset.png | 300×300 (32px 격자 9×9 = 288 사용) | `lab_block` (0,172,128,128, 9-slice 테두리 L16 B16 R22 T22): 벽·바닥 / `lab_panel` (192,108,96,96): 배경 반복 / `lab_bricks` |
| full_decor_tiles.png | 96×96 = 32×32 ×9 | `decor_0..8` 통·단말·배양 통·상자 장식 |
| control_panel_idle / deactivate | 640×32 = 32×32 ×20 | 제어 단말(켜짐 애니메이션 20프레임 / 꺼짐), 실험 통 센서 |
| sci fi mockup.png → `sci_fi_mockup.png` | 884×512 | 1장 친구 단말 속 '연구실 배경화면' |
| Traps·Barrier·Moving platform | — | 가져오지 않음 (이번 범위에 함정 없음) |

### Foozle Pickups → `Assets/ThirdParty/Foozle_SciFiLab_Pickups/`
medkit / chemical_pot / access_card: 각 384×32 = 32×32 ×12. medkit = 회복 붕대(바닥 아이템 애니메이션), chemical_pot = 혼합실 장식. ammo_box 미사용.
과자·배터리의 정확한 그림은 이 팩에 없다 → 임시 그림 사용.

### ansimuz → `Assets/ThirdParty/ansimuz_SynthCities/`
Version 2 레이어 `back`(112×272) / `middle`(256×272) / `foreground`(688×272): 가로 반복 배경. `cyberpunk-street.ogg`: 타이틀 음악. PSD·WAV·MP3·Version 1 은 가져오지 않음.

### Kenney Pixel UI → `Assets/ThirdParty/Kenney_PixelUI/`
`9-Slice/space.png`, `space_inlay.png` (48×48, 테두리 4), `Outline/yellow.png`. 스프라이트시트(16×16, 간격 2)는 가져오지 않음.

## 3. 개발용 임시 그림 (직접 생성, 최종 디자인 아님)

`Tools/placeholder_gen.py` 로 만든 84개 PNG → `Assets/Game/Art/Placeholder/`.
캐릭터 몸통에 `LU` / `VN` / `RS` 글자를 찍어 임시 그림임을 표시했다.

| 종류 | 파일 | 프레임 |
|---|---|---|
| 루 | Lu_Idle·Walk·Jump·Fall·Climb·Hurt·Down·Sit·Flick·Point·Swing·Kick·Charge·Discharge·Carry | 48×64 |
| 베넬 | Venelle_Idle·Walk·Jump·Fall·Climb·Hurt·Down·Sit·Attack·Collapsed·Supported·Dizzy | 48×64 |
| 연구원 | Researcher_Idle·Walk·Jump·Fall·Climb·Hurt·Down·Sit·Scared | 48×64 |
| 효과 (유료 전기 효과 대신) | FX_Spark(32×32×6)·Bolt(16×8×2)·Explosion(32×32×6)·Arc(64×32×3)·Heal·Acid(32×32×4)·LightDot(16×16×4) | |
| 장치 | Prop_PowerSocket(입구/빛/끊김)·WireCoil·ExposedWire(×4)·Battery·Switch·RestoreButton·Lever·Computer(켜짐/꺼짐/부팅/메시지)·Door(닫힘/열림/정전)·Tank(정상/깨짐)·CeilingLight·EmergencyLight·Vending(켜짐/꺼짐/고장) | |
| 소품 (유료 가구 대신) | Mirror·Cupboard·Cups·Clothes·Desk·Bed·Bench·Stone·Snack·Doc·Water·GlassWindow·LaundryLine(·Empty)·Messenger·AddressPanel·Puddle·Rubble·Ladder | |
| 로고 자리 | Prop_PosterEmblem — **'LOGO ?' 표시만** 둔 빈 자리. 사용자 원안 로고를 받기 전까지 로고를 임의로 그리지 않았다 | 48×48 |
| 타일 | Tile_HomeWall·HomeFloor·Digital·DigitalBlock·Street·Roof·Platform·Pixel | 32×32 등 |

## 4. 미구매·누락

| 항목 | 상태 | 대체 |
|---|---|---|
| kiddolink — VFX ELECTRICITY (A09) | **미구매, 사용·참조 없음** | 직접 만든 FX_* 임시 효과 |
| GandalfHardcore — Medieval Fantasy Tiles / House (A10) | **미구매, 사용·참조 없음** | 집은 임시 타일·소품 |
| ansimuz Warped Caves (A07) | 보유 ZIP 없음 | 4장 지하 구간은 이번 범위 밖 |
| JDWasabi 효과음 (A11) | 보유 없음 | 효과음 없음 (`Sfx.Play` 호출 자리만 있음) |
| Tallbeard Music Loop Bundle (A12) | 보유 없음 | 타이틀만 ansimuz 곡, 맵 BGM 없음 |
| 주요 인물 원화 (루·베넬·연구원) | 사용자 제작 예정 | 임시 그림 |
| 정비 거미 등 고유 적 외형 | 없음 | 운반 로봇·순찰 경비는 Foozle 드로이드로 **임시 대용** (정비 거미는 미구현) |
| 사용자 로고 원안 | 이미지 파일 없음 (GDD 의 링크는 접근 불가) | 'LOGO ?' 빈 자리 |

## 4-1. 2026-11-14 개편에서 직접 만든 임시 자산 (최종 아님)

| 종류 | 위치 | 만든 방법 | 비고 |
|---|---|---|---|
| 효과음 46·환경음 5·음악 2 | `Assets/Game/Audio/Generated/*.wav` | `Tools/audio_gen.py` (numpy 합성) | 보유 ZIP 에 효과음이 없고(ansimuz 는 음악 1곡), 유료 팩은 사지 않아 직접 합성. 불·바람(`amb_fire`), 집(`amb_house`), 바탕화면(`amb_digital`), 실험실(`amb_lab`), 정전 후(`amb_lab_dark`), 발소리·끌기, 접속·흡수·강제 전송(위이잉—쑥), 유리 두드림, 문, 제압봉, 충전·대방전·유리 깨짐·정전, 메뉴·대사, 전투 타격(전기/물리 구분)·기물 아크 등. **이 PC 에 오디오 장치가 없어 실제로 들어 보지 못했다** |
| 장면 블록아웃 59장 | `Assets/Game/Art/Placeholder/Scene/` | `Tools/scene_art_gen.py` (Pillow) | 판타지 마을·무너진 집·바탕화면·실험실 레이어와 소품, 불·연기·불티·재·빛·연무. 연구소 타일을 색만 바꿔 마을로 쓰지 않았다 |
| 인물 임시 동작 추가 | `Assets/Game/Art/Placeholder/Lu_*`, `Researcher2_*` 등 | `Tools/placeholder_gen.py` | 오프닝 동작 16종, 연구원 2, 제압봉 동작 |
| 임시 스탠딩 20장 | `Assets/Game/Art/Portraits/` | `Tools/portrait_gen.py` (Pillow, 맑은 고딕 글꼴로 '임시 스탠딩' 표시) | 루 9·베넬 5·연구원 1·2 각 3 표정 |

타이틀 음악은 이전처럼 ansimuz `cyberpunk-street.ogg`(CC0) 를 `bgm_title` 이름으로 쓴다.
참고 이미지 `Docs/References/*.png` 는 구도·비율 참고용이며 게임에 넣지 않았다.

## 5. 배포 시 주의

- CC0 에셋은 크레딧 의무가 없지만 크레딧에 Foozle(제작: Baldur, aimen23b), Kenney, Luis Zuno(ansimuz) 를 남기는 것을 권장한다.
- Galmuri 는 OFL: 빌드 배포 시 저작권·라이선스 고지를 함께 넣는다.
- Free Industrial Zone 은 사용하지 않았으므로 빌드에 포함되지 않는다. 쓰게 되면 원본 파일을 공개 저장소에 그대로 올리지 않는다.
- `SynthCitiesGodot.zip` 은 라이선스 파일이 없어 미확인으로 둔다 (같은 그림은 CC0 PDF 가 있는 cyberpunk-street ZIP 에서 가져옴).
