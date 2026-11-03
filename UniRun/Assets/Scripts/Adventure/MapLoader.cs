using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// adventure_map.json 을 읽어서 타일맵, 액체, 적, 아이템, 장식을 배치하는 스크립트
[DefaultExecutionOrder(-100)]
public class MapLoader : MonoBehaviour {
    public static MapLoader instance;

    public TextAsset mapJson;
    public Tilemap terrain;        // 보이는 지형
    public Tilemap collision;      // 충돌 전용 (외곽 타일만)
    public Tilemap fluid;          // 물/용암 (플레이어 위에 반투명으로 그림)
    public Sprite[] terrainSprites; // zone * 27 + (타일번호 - 1)
    public Sprite[] fluidSprites;   // fluid * 2 + (0 몸통, 1 표면)
    public Sprite[] props;          // 이름으로 찾는 장식/적/아이템 스프라이트
    public Sprite logSprite;
    public Sprite glowSprite;
    public Color[] fluidColors = {
        new Color(1f, 1f, 1f, 0.55f), new Color(1f, 1f, 1f, 0.78f),
        new Color(1f, 1f, 1f, 0.85f), new Color(1f, 1f, 1f, 1f)
    };

    public MapData Data { get; private set; }
    public Vector2 PlayerStart { get; private set; }
    public float GoalX { get; private set; }

    private readonly Dictionary<string, Sprite> spriteByName = new Dictionary<string, Sprite>();
    private Transform objectRoot;

    private void Awake() {
        instance = this;
        Data = JsonUtility.FromJson<MapData>(mapJson.text);
        foreach (Sprite s in props) {
            if (s != null) spriteByName[s.name] = s;
        }
        BuildTiles();
        BuildObjects();
        PlayerStart = new Vector2(4f, ColumnTop(4) + 1f);
    }

    public Sprite GetSprite(string spriteName) {
        Sprite s;
        return spriteByName.TryGetValue(spriteName, out s) ? s : null;
    }

    // ---------------------------------------------------------------- 맵 질의
    public int Column(float x) {
        return Mathf.Clamp(Mathf.FloorToInt(x), 0, Data.width - 1);
    }

    public int ColumnTop(float x) {
        return Data.columnTop[Column(x)];
    }

    public char ColumnKind(float x) {
        return Data.columnKind[Column(x)];
    }

    public ZoneData ZoneAt(float x) {
        return Data.zones[Data.columnZone[Column(x)]];
    }

    public int ZoneIndexAt(float x) {
        return Data.columnZone[Column(x)];
    }

    public ZoneData FluidAt(float x) {
        return Data.zones[Data.fluidZone[Column(x)]];
    }

    public float FluidSurface(float x) {
        return FluidAt(x).water;
    }

    public float Progress(float x) {
        return Mathf.Clamp01(x / GoalX);
    }

    // 착지해도 안전한 평지인가 (좌우 1칸까지 같은 높이 평지, 가시/위험 액체 제외 → 좁은 기둥도 후보)
    public bool IsSafeGround(float x) {
        int c = Column(x);
        if (c < 1 || c > Data.width - 2) return false;
        for (int i = c - 1; i <= c + 1; i++) {
            if (Data.columnKind[i] != 'f' || Data.columnTop[i] != Data.columnTop[c]) return false;
        }
        if (Data.columnTop[c] <= FluidAt(x).water) return false;
        return !spikeColumns.Contains(c);
    }

    private readonly HashSet<int> spikeColumns = new HashSet<int>();

    // x 부터 앞으로 틈이나 가시 없이 달릴 수 있는 칸 수
    public int RunwayAhead(float x) {
        int c = Column(x), n = 0;
        while (c + n < Data.width && Data.columnKind[c + n] != 'g' && !spikeColumns.Contains(c + n)
               && Data.columnTop[c + n] > FluidAt(c + n).water) n++;
        return n;
    }

    // ---------------------------------------------------------------- 타일
    private void BuildTiles() {
        int w = Data.width, h = Data.height;
        var visualTiles = new Dictionary<int, Tile>();
        var colliderTiles = new Dictionary<int, Tile>();
        var visual = new TileBase[w * h];
        var solid = new TileBase[w * h];

        for (int r = 0; r < h; r++) {
            for (int x = 0; x < w; x++) {
                int id = Data.tiles[r * w + x];
                if (id == 0) continue;
                int zone = id / 32, n = id % 32;
                Tile t;
                if (!visualTiles.TryGetValue(id, out t)) {
                    t = ScriptableObject.CreateInstance<Tile>();
                    t.sprite = terrainSprites[zone * 27 + n - 1];
                    t.colliderType = Tile.ColliderType.None;
                    visualTiles[id] = t;
                }
                visual[r * w + x] = t;

                // 안쪽 흙 타일은 충돌이 필요 없으니 외곽만 충돌 타일맵에 넣는다
                if (IsEdge(x, r)) {
                    if (!colliderTiles.TryGetValue(id, out t)) {
                        t = ScriptableObject.CreateInstance<Tile>();
                        t.sprite = terrainSprites[zone * 27 + n - 1];
                        t.colliderType = n == 3 || n == 8 || n == 17 ? Tile.ColliderType.Grid : Tile.ColliderType.Sprite;
                        colliderTiles[id] = t;
                    }
                    solid[r * w + x] = t;
                }
            }
        }
        var bounds = new BoundsInt(0, 0, 0, w, h, 1);
        terrain.SetTilesBlock(bounds, visual);
        collision.SetTilesBlock(bounds, solid);

        // 액체: 틈과 물속 지형 위에만 깐다
        var fluidTiles = new Tile[fluidSprites.Length];
        for (int i = 0; i < fluidSprites.Length; i++) {
            fluidTiles[i] = ScriptableObject.CreateInstance<Tile>();
            fluidTiles[i].sprite = fluidSprites[i];
            fluidTiles[i].colliderType = Tile.ColliderType.None;
            fluidTiles[i].flags = TileFlags.None;
        }
        for (int x = 0; x < w; x++) {
            ZoneData z = Data.zones[Data.fluidZone[x]];
            for (int r = 0; r <= z.water && r < h; r++) {
                var pos = new Vector3Int(x, r, 0);
                fluid.SetTile(pos, fluidTiles[z.fluid * 2 + (r == z.water ? 1 : 0)]);
                fluid.SetColor(pos, fluidColors[z.fluid]);
            }
        }
        GoalX = w - 7;
    }

    private bool IsEdge(int x, int r) {
        int w = Data.width, h = Data.height;
        if (x == 0 || x == w - 1 || r == h - 1) return true;
        return Data.tiles[r * w + x - 1] == 0 || Data.tiles[r * w + x + 1] == 0
            || Data.tiles[(r + 1) * w + x] == 0 || (r > 0 && Data.tiles[(r - 1) * w + x] == 0);
    }

    // ---------------------------------------------------------------- 오브젝트
    private void BuildObjects() {
        objectRoot = new GameObject("Map Objects").transform;
        foreach (MapObject o in Data.objects) {
            if (o.kind == "spike") spikeColumns.Add(Mathf.FloorToInt(o.x));
        }
        foreach (MapObject o in Data.objects) {
            switch (o.kind) {
                case "decor_back": Decor(o, -5); break;
                case "decor_front": Decor(o, 6); break;
                case "sign": Decor(o, -5); break;
                case "gem": SpawnPickup(o, Pickup.Kind.Gem); break;
                case "heart": SpawnPickup(o, Pickup.Kind.Heart); break;
                case "shield": SpawnPickup(o, Pickup.Kind.Shield); break;
                case "magnet": SpawnPickup(o, Pickup.Kind.Magnet); break;
                case "spike": SpawnSpike(o); break;
                case "log": SpawnLog(o); break;
                case "checkpoint": SpawnFlag(o, false); break;
                case "goal": SpawnFlag(o, true); break;
                case "enemy_walk": SpawnEnemy(o, PatrolEnemy.Mode.Walk); break;
                case "enemy_spiky": SpawnEnemy(o, PatrolEnemy.Mode.Spiky); break;
                case "enemy_fly": SpawnEnemy(o, PatrolEnemy.Mode.Fly); break;
                case "enemy_float": SpawnEnemy(o, PatrolEnemy.Mode.Float); break;
                case "enemy_swim": SpawnEnemy(o, PatrolEnemy.Mode.Swim); break;
            }
        }
    }

    private SpriteRenderer MakeSprite(string objName, Sprite sprite, Vector2 pos, int order, float scale = 1f) {
        var go = new GameObject(objName);
        go.transform.SetParent(objectRoot, false);
        go.transform.position = pos;
        go.transform.localScale = Vector3.one * (scale <= 0f ? 1f : scale);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = order;
        return sr;
    }

    // 스프라이트 바닥이 y 에 오도록 위치 보정
    private static Vector2 BottomAligned(Sprite s, float x, float y, float scale) {
        float pivotToBottom = s.bounds.min.y * scale;
        return new Vector2(x, y - pivotToBottom);
    }

    private void Decor(MapObject o, int order) {
        Sprite s = GetSprite(o.sprite);
        if (s == null) return;
        float sc = o.scale <= 0 ? 1f : o.scale;
        SpriteRenderer sr = MakeSprite(o.sprite, s, BottomAligned(s, o.x, o.y, sc), order, sc);
        if (order > 0 && Random.value < 0.5f) sr.flipX = true;
        if (o.sprite.StartsWith("plant")) sr.gameObject.AddComponent<Sway>();
    }

    private void SpawnPickup(MapObject o, Pickup.Kind kind) {
        string spriteName = kind == Pickup.Kind.Gem ? o.sprite : kind.ToString().ToLower();
        Sprite s = GetSprite(spriteName);
        SpriteRenderer sr = MakeSprite(kind.ToString(), s, new Vector2(o.x, o.y), 8, kind == Pickup.Kind.Gem ? 0.8f : 1f);
        var col = sr.gameObject.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = kind == Pickup.Kind.Gem ? 0.35f : 0.5f;
        var p = sr.gameObject.AddComponent<Pickup>();
        p.kind = kind;
        p.glow = glowSprite;
    }

    private void SpawnSpike(MapObject o) {
        Sprite s = GetSprite(o.sprite);
        SpriteRenderer sr = MakeSprite("Spike", s, BottomAligned(s, o.x, o.y - 0.05f, 1f), 7);
        var col = sr.gameObject.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        // 보이는 것보다 판정을 작게 해서 억울한 피격을 줄인다
        Bounds b = s.bounds;
        col.size = new Vector2(b.size.x * 0.7f, b.size.y * 0.45f);
        col.offset = new Vector2(b.center.x, b.min.y + col.size.y * 0.5f);
        var spike = sr.gameObject.AddComponent<Spike>();
        spike.glow = glowSprite;
    }

    private void SpawnLog(MapObject o) {
        float widthTiles = o.scale;
        SpriteRenderer sr = MakeSprite("Log", logSprite, new Vector2(o.x, o.y), 4);
        sr.drawMode = SpriteDrawMode.Sliced;
        sr.size = new Vector2(widthTiles, logSprite.bounds.size.y);
        var body = sr.gameObject.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        var col = sr.gameObject.AddComponent<BoxCollider2D>();
        col.size = new Vector2(widthTiles - 0.1f, logSprite.bounds.size.y * 0.8f);
        col.usedByEffector = true;
        var effector = sr.gameObject.AddComponent<PlatformEffector2D>();
        effector.useOneWay = true;
        effector.surfaceArc = 160f;
        sr.gameObject.AddComponent<FloatingLog>();
    }

    private void SpawnFlag(MapObject o, bool goal) {
        Sprite s = GetSprite(o.sprite);
        float sc = o.scale <= 0 ? 1f : o.scale;
        SpriteRenderer sr = MakeSprite(goal ? "Goal" : "Checkpoint", s, BottomAligned(s, o.x, o.y, sc), 5, sc);
        var col = sr.gameObject.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(1f, 30f);
        var cp = sr.gameObject.AddComponent<Checkpoint>();
        cp.isGoal = goal;
        cp.raisedSprite = GetSprite(goal ? "flagRed_up" : "flagGreen_up");
    }

    private void SpawnEnemy(MapObject o, PatrolEnemy.Mode mode) {
        string baseName = o.sprite.Substring(0, o.sprite.Length - 2);
        Sprite s = GetSprite(o.sprite);
        float sc = o.scale <= 0 ? 1f : o.scale;
        Vector2 pos = mode == PatrolEnemy.Mode.Walk || mode == PatrolEnemy.Mode.Spiky
            ? BottomAligned(s, o.x, o.y - 0.04f, sc) : new Vector2(o.x, o.y);
        SpriteRenderer sr = MakeSprite(baseName, s, pos, 9, sc);
        var body = sr.gameObject.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        var col = sr.gameObject.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = s.bounds.size * 0.75f;
        col.offset = s.bounds.center;
        var e = sr.gameObject.AddComponent<PatrolEnemy>();
        e.mode = mode;
        e.minX = o.minX;
        e.maxX = o.maxX;
        e.frames = new[] { GetSprite(baseName + "_1"), GetSprite(baseName + "_2"), GetSprite(baseName + "_3") };
        e.deadSprite = GetSprite(baseName + "_4");
    }
}
