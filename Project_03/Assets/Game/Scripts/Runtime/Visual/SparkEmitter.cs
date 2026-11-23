using UnityEngine;

namespace EW
{
    /// <summary>잔류 스파크: 플래그 조건에서 가끔 작은 불꽃과 소리를 낸다 (강한 점멸 없이 드문드문).</summary>
    public class SparkEmitter : MonoBehaviour
    {
        public string requireFlag;
        public string hideFlag;
        public Vector2 interval = new Vector2(1.2f, 3.5f);
        public float radius = 0.3f;
        public float scale = 0.6f;
        public bool sound = true;
        float next;

        void Update()
        {
            var g = Game.I;
            if (g == null || g.State == null) return;
            if (!string.IsNullOrEmpty(requireFlag) && !g.State.Has(requireFlag)) return;
            if (!string.IsNullOrEmpty(hideFlag) && g.State.Has(hideFlag)) return;
            if (Time.time < next) return;
            next = Time.time + Random.Range(interval.x, interval.y);
            Fx.Spawn("Spark", transform.position + (Vector3)(Random.insideUnitCircle * radius), Random.value < 0.5f, 31, scale);
            var cam = Camera.main;
            if (sound && cam != null && Mathf.Abs(cam.transform.position.x - transform.position.x) < 11f) Sfx.Play("sfx_spark_small", 0.25f, Random.Range(0.9f, 1.2f));
        }
    }
}
