using UnityEngine;

namespace EW
{
    /// <summary>
    /// 소리를 내는 물건 (매달린 공구 쟁반·느슨한 금속판 등). 약한 간섭으로 덜컹거려 근처 경비가 그쪽을 보게 한다.
    /// 경비가 고개를 돌리고 소리 쪽으로 걸어가는 모습으로 결과가 보인다.
    /// </summary>
    public class NoiseMaker : ZapTarget
    {
        public float radius = 7f;
        public float reuse = 4f;
        public string sfx = "sfx_clatter";
        float cd, shake;
        Vector3 basePos;

        void Start() { basePos = transform.localPosition; }
        public override bool CanJam => cd <= 0;
        public override bool CanPull => false;
        public override string JamResult => "덜컹 — 근처 경비가 소리 쪽을 본다";

        public override void Jam(float seconds)
        {
            cd = reuse; shake = 0.6f;
            Sfx.Play(sfx, 1f);
            Noise.Emit(AimPoint, radius, "noisemaker");
        }

        void Update()
        {
            if (cd > 0) cd -= Time.deltaTime;
            if (shake > 0)
            {
                shake -= Time.deltaTime;
                transform.localPosition = basePos + new Vector3(Mathf.Round(Mathf.Sin(Time.time * 60f) * 2f) / 32f, 0, 0);
                if (shake <= 0) transform.localPosition = basePos;
            }
        }
    }
}
