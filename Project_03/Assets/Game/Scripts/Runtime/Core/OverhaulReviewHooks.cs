#if UNITY_EDITOR
using System.Collections;
using System.IO;
using UnityEngine;

namespace EW
{
    public static class OverhaulReviewHooks
    {
        class Runner : MonoBehaviour {}
        public static void FullRun()
        {
            DevHooks.DevSave();
            var go=new GameObject("OverhaulReviewRecorder");Object.DontDestroyOnLoad(go);
            go.AddComponent<Runner>().StartCoroutine(Record());
            StealthDemo.StartFull();
        }
        static IEnumerator Record()
        {
            string dir=Path.GetFullPath(Path.Combine(Application.dataPath,"../Docs/Media/art_play_overhaul/run_"+System.DateTime.Now.ToString("HHmmss")));
            Directory.CreateDirectory(dir);
            var rt=new RenderTexture(1280,720,0);
            var small=new Texture2D(1280,720,TextureFormat.RGB24,false);
            float next=Time.realtimeSinceStartup;int n=0;
            using(var log=new StreamWriter(Path.Combine(dir,"frames.tsv")))
            {
                log.WriteLine("frame\trealtime\tmap\tcutscene\tstate");
                while(n<2400)
                {
                    yield return new WaitForEndOfFrame();
                    if(Time.realtimeSinceStartup<next)continue;
                    next=Time.realtimeSinceStartup+0.35f;
                    var full=ScreenCapture.CaptureScreenshotAsTexture();Graphics.Blit(full,rt);
                    var prev=RenderTexture.active;RenderTexture.active=rt;small.ReadPixels(new Rect(0,0,1280,720),0,0);small.Apply();RenderTexture.active=prev;Object.Destroy(full);
                    File.WriteAllBytes(Path.Combine(dir,"f_"+n.ToString("0000")+".jpg"),small.EncodeToJPG(88));
                    var g=Game.I;
                    log.WriteLine(n+"\t"+Time.realtimeSinceStartup.ToString("0.00")+"\t"+g?.CurrentMapId+"\t"+Cutscene.CurrentId+"\t"+DevHooks.Status());log.Flush();n++;
                    if(g!=null&&g.CurrentMapId=="M15"&&g.State.Has("c3_rest_done")&&!g.InEvent)break;
                }
            }
            Object.Destroy(rt);Object.Destroy(small);
            Debug.Log("[EW] Review recording finished: "+dir);
        }
        public static void CaptureOnly()
        {
            DevHooks.DevSave();GameInput.AutoReadDelay=0.8f;
            var go=new GameObject("CaptureReview");go.AddComponent<Runner>().StartCoroutine(CaptureReview());
        }
        static IEnumerator CaptureReview()
        {
            while(Game.I==null||Game.I.Player==null||Cutscene.Running||Game.I.InEvent)yield return null;
            var g=Game.I;g.State.Set("pc_guide_read");g.State.Set("pc_internet_checked");
            g.Player.Teleport(new Vector2(6,4.7f));yield return new WaitForSeconds(0.4f);
            g.RunEvent(StoryEvents.Run(g,"pc_log"));
        }
        public static void DesktopShot(){DevHooks.DevSave();GameInput.AutoReadDelay=1f;}
    }
}
#endif
