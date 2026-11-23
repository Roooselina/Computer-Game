using System.Collections.Generic;
using UnityEngine;

namespace EW
{
    // Small, bounded pool for repeated droplets, sparks and residual vapor.
    // Each particle has independent ballistic motion; the effect is never a scaled still image.
    public class ReactionParticles : MonoBehaviour
    {
        static readonly List<ReactionParticles> pool = new List<ReactionParticles>();
        static Material material;
        ParticleSystem particles;
        float age, life;
        bool active;
        static Material Material()
        {
            if(material!=null)return material;
            var tex=new Texture2D(32,32,TextureFormat.RGBA32,false);
            for(int y=0;y<32;y++)for(int x=0;x<32;x++)
            {
                float d=Vector2.Distance(new Vector2(x,y),new Vector2(15.5f,15.5f))/15.5f;
                tex.SetPixel(x,y,new Color(1,1,1,Mathf.Clamp01((1-d)*5)));
            }
            tex.Apply();
            material=new Material(Shader.Find("Particles/Standard Unlit"));
            material.mainTexture=tex;material.SetFloat("_Mode",2);material.SetFloat("_SrcBlend",5);material.SetFloat("_DstBlend",10);material.SetFloat("_ZWrite",0);material.renderQueue=3000;
            return material;
        }
        public static void Emit(Vector2 at,int direction,bool chemical,int count=24,float spread=1f)
        {
            pool.RemoveAll(x=>x==null);
            ReactionParticles fx=pool.Find(x=>!x.active);
            if(fx==null)
            {
                if(pool.Count>=16)return;
                var go=new GameObject("ReactionParticlePool"); fx=go.AddComponent<ReactionParticles>();
                fx.particles=go.AddComponent<ParticleSystem>();fx.particles.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
                var renderer=go.GetComponent<ParticleSystemRenderer>();renderer.material=Material();renderer.sortingOrder=32;
                pool.Add(fx);
            }
            fx.transform.position=at;fx.age=0;fx.life=chemical?1.6f:0.9f;fx.active=true;
            var main=fx.particles.main;main.loop=false;main.playOnAwake=false;main.maxParticles=90;main.simulationSpace=ParticleSystemSimulationSpace.World;main.startLifetime=fx.life;main.startSpeed=0;main.startSize=0.06f;
            var emission=fx.particles.emission;emission.enabled=false;
            var shape=fx.particles.shape;shape.enabled=false;
            var size=fx.particles.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,AnimationCurve.Linear(0,1,1,0));
            main.gravityModifier=chemical?0.75f:0;
            fx.particles.Play();
            for(int i=0;i<count;i++)
            {
                var e=new ParticleSystem.EmitParams();
                e.position=Random.insideUnitCircle*0.12f;
                e.velocity=chemical?new Vector3(direction*Random.Range(1f,5.8f)*spread,Random.Range(0.2f,2.5f),0):(Vector3)(Random.insideUnitCircle*Random.Range(1f,5f)*spread);
                e.startColor=chemical?Color.Lerp(new Color(0.43f,0.72f,0.28f,0.9f),new Color(0.88f,0.8f,0.37f,0.85f),Random.value):new Color(0.7f,0.9f,1,0.9f);
                e.startSize=chemical?Random.Range(0.04f,0.13f):Random.Range(0.025f,0.07f);
                e.startLifetime=Random.Range(fx.life*0.45f,fx.life);
                fx.particles.Emit(e,1);
            }
        }
        void Update()
        {
            if(!active)return;
            if(Hazard.Paused && !Cutscene.Running){particles.Pause();return;}
            if(particles.isPaused)particles.Play();
            age+=Time.deltaTime;
            if(age>life){active=false;particles.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);}
        }
        void OnDestroy(){pool.Remove(this);}
    }
}
