# 개별 도트 샘플 대응표

본편 배치는 원본을 사용하며 자동 전체 치환하지 않는다. 원래 맵은 문자 격자를 개별 SpriteRenderer/BoxCollider2D로 빌드한다. 복구 기준 시점에 실제 Tilemap/Tile Palette/프리팹은 없었으므로 복구했다고 주장하지 않는다. 새 샘플에 실제 Unity Tile/Tilemap과 조립 프리팹을 별도로 만들었다.

| 기존 역할/원본 | 새 샘플 | 원본/샘플 규격 | 연결 기준 |
|---|---|---|---|
| Tile_Platform, L_Floor / L_FloorFill 상단 | platform_left / middle / right.png | 32×8 및 32×32 / 32×16 | 32 PPU, 새 발판 상단 중앙 피벗; 상단 높이는 유지하고 하단 장식만 8px 연장 |
| L_ForePipe, 기존 장식 배관 | pipe_horizontal / vertical / elbow / t / t_right / cap.png | 기존 장식 여러 크기 / 각각 16×16 | 32 PPU, 중앙 피벗, 접합 포트 폭 8px, 중심 8px; 0.5u Grid |
| L_TankBack / L_TankGlass (2프레임 시트) | specimen_tank.png | 프레임 80×112 / 80×112 | 32 PPU, 아래 중앙 피벗; 개별 독립 소품 |
| Lu_Idle 원본 | 변경 없음 | 프레임 48×64 | 같은 32 PPU, 확대/축소 없음 |

최종 PNG는 Assets/Game/Art/PixelSamples에 있다. 25색 팔레트, 알파 0/255, Point, 무압축, 밉맵 없음. 파이프와 발판의 접합 픽셀은 바이트 단위 검사했다. 모든 네이티브 픽셀 좌표는 Tools/author_native_pixel_samples.py에 있어 수정·재생성 가능하다. 고해상도 생성 그림의 축소/모자이크가 아니다. 실제 호출한 생성 결과는 치수·격자가 맞지 않아 Docs/Media/pixel_restore/generated_design_reference/not_a_tileset.png에 참고용으로만 보관했다.

실험통은 정적 샘플이다. 유리·액체·기포의 독립 애니메이션과 파괴 상태, 전체 벽/천장 안팎 모서리 타일, 배관 밸브/파손 끝은 이번 세트에 포함하지 않았다. 원본 탱크 애니메이션과 본편 연출은 유지한다.

테스트 씬: Assets/Game/Art/PixelSamples/Tests/PixelConnectionTest.unity. 플랫폼 Tilemap(1u), 배관 Tilemap(0.5u), 독립 실험통과 기존 캐릭터, 원경0.7/중경0.3/전경-0.06으로 분리한다. 그림 Tile의 ColliderType은 None; 걷는 상단만 별도 BoxCollider2D로 처리한다. Production 맵/진행/빌드 목록에는 이 씬을 끼워 넣지 않는다.
