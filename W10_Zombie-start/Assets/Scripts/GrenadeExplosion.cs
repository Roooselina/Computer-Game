using UnityEngine;

[RequireComponent(typeof(ParticleSystem), typeof(AudioSource))]
public class GrenadeExplosion : MonoBehaviour {
    public float radius = 5f;
    public Material ringMaterial;
    LineRenderer ring;
    Light flash;
    float age;
    void Start() {
        var particles = GetComponent<ParticleSystem>();
        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = particles.main;
        main.loop = false; main.duration = .25f; main.startLifetime = .65f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 8f);
        main.startSize = new ParticleSystem.MinMaxCurve(.3f, 1.2f);
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, .25f, .02f), new Color(.2f, .15f, .1f));
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        var shape = particles.shape; shape.enabled = true; shape.shapeType = ParticleSystemShapeType.Sphere; shape.radius = .3f;
        var emission = particles.emission; emission.rateOverTime = 0; emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 65) });
        particles.Play();
        var audio = GetComponent<AudioSource>();
        audio.spatialBlend = 1; audio.minDistance = 5; audio.maxDistance = 35; audio.dopplerLevel = 0;
        if (audio.clip) audio.Play();
        flash = gameObject.AddComponent<Light>(); flash.color = new Color(1, .4f, .1f); flash.range = radius * 2;
        if (ringMaterial) {
            ring = gameObject.AddComponent<LineRenderer>(); ring.sharedMaterial = ringMaterial;
            ring.positionCount = 64; ring.loop = true; ring.widthMultiplier = .06f;
        }
    }
    void Update() {
        age += Time.deltaTime;
        if (flash) flash.intensity = Mathf.Max(0, 8f * (1f - age / .25f));
        if (!ring) return;
        float t = Mathf.Clamp01(age / .45f);
        for (int i = 0; i < 64; i++) {
            float angle = i * Mathf.PI * 2 / 64;
            ring.SetPosition(i, transform.position + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius * t + Vector3.up * .05f);
        }
        ring.enabled = age < .55f;
    }
}
