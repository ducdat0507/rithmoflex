using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JudgeHolder : MonoBehaviour
{
    public Judge judge;
    public LineRenderer line;

    // Start is called before the first frame update
    void Start()
    {
        Update();
    }

    // Update is called once per frame
    void Update()
    {
        judge.Advance(ChartPlayer.main.TrackTime * 1000);
        transform.position = new Vector3(judge.Position.x - 6, -judge.Position.y + 3.375f, -judge.Position.z);
        transform.eulerAngles = Vector3.back * judge.Rotation;
        line.startColor = line.endColor = new Color(1, 1, 1, judge.Opacity);
        UpdateLine();
        
        while (true)
        {
            if (judge.Objects.Count > 0 && ChartPlayer.main.TrackTime * 1000 > judge.Objects[0].Offset - judge.Objects[0].AppearTime) 
            {
                HitObject ho = judge.Objects[0];
                HitHolder hh = Instantiate(
                    ho.Type == HitObject.HitType.Catch ? ChartPlayer.main.CatchHit : 
                    ho.Type == HitObject.HitType.Flick ? ChartPlayer.main.FlickHit : 
                    ChartPlayer.main.NormalHit, 
                    transform);
                hh.hit = ho;
                hh.parent = this;
                judge.Objects.RemoveAt(0);
            }
            else break;
        }
    }

    void UpdateLine() 
    {
        if (judge.Type == Judge.JudgeType.Line) 
        {
            line.positionCount = 2;
            line.SetPosition(0, Vector3.left * judge.Length);
            line.SetPosition(1, Vector3.right * judge.Length);
        }
        else if (judge.Type == Judge.JudgeType.Arc) 
        {
            float ang = Mathf.Max(judge.ArcAngle, 360);
            line.positionCount = Mathf.RoundToInt(ang) + 1;
            for (int a = 0; a <= ang; a++) 
            {
                float an = ang * a / Mathf.RoundToInt(ang) * Mathf.Deg2Rad;
                line.SetPosition(a, new Vector3(-Mathf.Cos(an), -Mathf.Sin(an)) * judge.Length);
            }
        }
    }

    public Vector3 GetPosition(float pos) 
    {
        if (judge.Type == Judge.JudgeType.Line) 
        {
            return Vector3.right * judge.Length * (pos * 2 - 1);
        }
        else if (judge.Type == Judge.JudgeType.Arc) {
            float ang = pos * judge.ArcAngle * Mathf.Deg2Rad;
            return new Vector3(Mathf.Cos(ang), -Mathf.Sin(ang)) * judge.Length;
        }
        else return Vector3.zero;
    }
}
