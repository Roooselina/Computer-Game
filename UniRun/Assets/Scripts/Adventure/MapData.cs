using System;

// adventure_map.json 구조 (Tools/MapGen/gamemap.py 가 생성)
[Serializable]
public class MapData {
    public int width;
    public int height;
    public int seed;
    public ZoneData[] zones;
    public int[] tiles;        // r * width + x, 0 = 빈칸, zone * 32 + 타일번호(1~27)
    public int[] columnZone;   // 열마다 지형 색 구역
    public int[] fluidZone;    // 열마다 액체 구역
    public int[] columnTop;    // 열의 지면 높이 (-1 = 틈)
    public string columnKind;  // f 평지 / u 오르막 / d 내리막 / g 틈
    public MapObject[] objects;
}

[Serializable]
public class ZoneData {
    public string name;
    public int fluid;          // 0 물, 1 흙탕물, 2 독물, 3 용암
    public int water;          // 액체 표면 높이
    public bool lethal;
    public int startX;
    public int endX;
    public int[] skyTop;
    public int[] skyBottom;
    public int[] hill;
    public string hills;
    public string gem;
}

[Serializable]
public class MapObject {
    public string kind;
    public string sprite;
    public float x;
    public float y;
    public float minX;
    public float maxX;
    public float scale;
}
