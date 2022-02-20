using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitHolder : MonoBehaviour
{
    public HitObject hit;
    public JudgeHolder parent;

    public SpriteRenderer sprite;
    public LineRenderer rail;

    public Vector3 HitPosition;

    // Start is called before the first frame update
    void Start()
    {
        
        Update();
    }

    // Update is called once per frame
    void Update()
    {
        hit.Advance(ChartPlayer.main.TrackTime * 1000);

        float tt = hit.Offset - ChartPlayer.main.TrackTime * 1000;
        float gt = hit.Rail.Count > 0 ? hit.Rail[hit.Rail.Count - 1].Offset - ChartPlayer.main.TrackTime * 1000 : tt;
        if (ChartPlayer.main.Autoplay && gt < 0) {
            ChartPlayer.main.Score(hit.Type == HitObject.HitType.Catch ? 1 : 3, 1);
            StartCoroutine(Fade());
        } else if (gt < -200) StartCoroutine(Fade());

        transform.eulerAngles = ChartPlayer.main.MainCamera.eulerAngles;
        float rPos = hit.Position;
                                    
        if (hit.Rail.Count > 0) {
            List<Vector3> railPoints = new List<Vector3>();
            hit.Rail[hit.Rail.Count - 1].Advance(ChartPlayer.main.TrackTime * 1000);
            rPos = hit.Rail[hit.Rail.Count - 1].Position;
            float st = 0;
            float et = 0;
            for (int a = 0; a < hit.Rail.Count; a++) {
                if (ChartPlayer.main.TrackTime > hit.Rail[a].Offset / 1000f) continue;
                hit.Rail[a].Advance(ChartPlayer.main.TrackTime * 1000);
                if (railPoints.Count == 0) {
                    RailTimestamp startRail = a < 1 ? new RailTimestamp { Offset = hit.Offset, Position = hit.Position, Velocity = hit.Velocity } : hit.Rail[a - 1];
                    st = Mathf.Max(startRail.Offset / 1000f - Mathf.Max(hit.Offset / 1000f, ChartPlayer.main.TrackTime), 0);
                    float sPos = Mathf.Lerp(startRail.Position, hit.Rail[a].Position, (ChartPlayer.main.TrackTime * 1000f - startRail.Offset) / (hit.Rail[a].Offset - (float)startRail.Offset));
                    rPos = sPos;
                    railPoints.Add(Vector3.zero);
                }
                et = Mathf.Max(hit.Rail[a].Offset / 1000f - Mathf.Max(hit.Offset / 1000f, ChartPlayer.main.TrackTime), 0);
                float ePos = hit.Rail[a].Position;
                Vector3 eVec = parent.GetPosition(ePos - (rPos - .5f));
                eVec += Quaternion.Euler(-transform.eulerAngles) * (new Vector3(hit.Rail[a].Velocity.x, -hit.Rail[a].Velocity.y, hit.Rail[a].Velocity.z) * et);
                railPoints.Add(eVec);
            }
            rail.positionCount = railPoints.Count;
            rail.SetPositions(railPoints.ToArray());
        }

        sprite.color = new Color(0, 0, 0, 1 + gt / 200);
        if (tt < 0) tt = 0;
        transform.localPosition = parent.GetPosition(rPos);
        if (hit.CoordinateMode == CoordinateMode.Local)
            transform.localPosition += new Vector3(hit.Velocity.x, -hit.Velocity.y, hit.Velocity.z) * tt / 1000;
        else
            transform.position += new Vector3(hit.Velocity.x, -hit.Velocity.y, hit.Velocity.z) * tt / 1000;
    }

    public IEnumerator Fade() {
        Destroy(gameObject);
        yield return null;
    }
}
