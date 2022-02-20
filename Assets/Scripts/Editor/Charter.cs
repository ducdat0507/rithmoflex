using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
using System.Collections.Generic;

public class Charter : EditorWindow
{
    public PlayableSong TargetSong;
    public Chart TargetChart;
    public Judge TargetJudge;
    public HitObject TargetHit;

    public AudioSource TargetPlayer;

    public object TargetThing;
    public object InitingItem;
    public object DeletingItem;
    public string CurrentTool = "select";

    public int SeparateFactor = 2;

    float depthConst = Mathf.Tan(Mathf.PI * 1 / 3) * 3.375f;

    bool ToolbarExpanded = false;

    [MenuItem("Rithmoflex/Open Charter")]
    public static void Open()
    {
        Charter wnd = GetWindow<Charter>();
        wnd.titleContent = new GUIContent("Charter");
        wnd.minSize = new Vector2(960, 600);
    }

    public static void Open(PlayableSong target)
    {
        Charter wnd = GetWindow<Charter>();
        wnd.titleContent = new GUIContent("Charter");
        wnd.minSize = new Vector2(960, 600);
        wnd.TargetSong = target;
    }

    public void OnGUI()
    {
        if (TargetSong) 
        {
            if (!TargetSong.Charts.Contains(TargetChart)) 
            {
                TargetChart = null;
                TargetJudge = null;
                TargetHit = null;
            }
            else if (!TargetChart.Judges.Contains(TargetJudge))
            {
                TargetJudge = null;
                TargetHit = null;
            }

            Rect sa = new Rect(40, 37, position.width - 292, position.height - 163);
            if (sa.width / sa.height < 16 / 9f)
            {
                float height = (sa.width / 16 * 9);
                sa.y = sa.y + (sa.height - height) / 2;
                sa.height = height;
            } else {
                float width = (sa.height * 16 / 9);
                sa.x = sa.x + (sa.width - width) / 2;
                sa.width = width;
            }
            float scale = sa.width / 12;

            Handles.color = new Color(.5f, .5f, .5f, .5f);
            Handles.DrawPolyLine(new Vector2(sa.x, sa.y), new Vector2(sa.x + sa.width, sa.y), 
                new Vector2(sa.x + sa.width, sa.y + sa.height), new Vector2(sa.x, sa.y + sa.height),
                new Vector2(sa.x, sa.y)
            );

            if (TargetChart != null)
            {
                Dictionary<string, JudgeGroup> groups = new Dictionary<string, JudgeGroup>();
                foreach (JudgeGroup group in TargetChart.Groups) {
                    groups[group.Name] = (JudgeGroup)group.Get(TargetPlayer ? TargetPlayer.time * 1000 : 0);
                }

                Vector2 GetPosition (Vector3 pos) {
                    return (pos - new Vector3(6, 3.375f)) / (pos.z + depthConst) * depthConst + new Vector3(6, 3.375f);
                }

                void RenderJudge(Judge judge, bool chosen = false) 
                {
                    judge = (Judge)judge.Get(TargetPlayer ? TargetPlayer.time * 1000 : 0);

                    if (judge.Group != "" && groups.ContainsKey(judge.Group)) {
                        judge.Rotation += groups[judge.Group].Rotation;
                        judge.Position = Quaternion.Euler(0, 0, groups[judge.Group].Rotation) * judge.Position + groups[judge.Group].Position;
                    }

                    if (judge.Offset == 0 || (TargetPlayer && TargetPlayer.time * 1000 >= judge.Offset)) 
                    {
                        if (judge.Type == Judge.JudgeType.Line)
                        {
                            Handles.color = new Color(1, 1, 1, judge.Opacity);
                            float depth = judge.Position.z;
                            float size = judge.Length / (depth + depthConst) * depthConst;
                            Vector2 pos = GetPosition(judge.Position);
                            Vector2 delta = new Vector2(size * Mathf.Cos(judge.Rotation * Mathf.Deg2Rad), judge.Length * Mathf.Sin(judge.Rotation * Mathf.Deg2Rad));
                            Vector3 start = pos - delta;
                            Vector3 end = pos + delta;
                            Handles.DrawLine((Vector3)sa.position + start * scale + Vector3.back, (Vector3)sa.position + end * scale + Vector3.back, (chosen ? 3 : 2) / (depth + depthConst) * depthConst);
                            foreach (HitObject obj in judge.Objects) {
                                if (TargetPlayer && (TargetPlayer.time - .25f < (obj.Rail.Count > 0 ? obj.Rail[obj.Rail.Count - 1].Offset : obj.Offset) / 1000f) && TargetPlayer.time > (obj.Offset - obj.AppearTime) / 1000f) {
                                    Vector3 rStart = judge.Position - (Vector3)delta;
                                    Vector3 rEnd = judge.Position + (Vector3)delta;
                                    HitObject objg = (HitObject)obj.Get(TargetPlayer ? TargetPlayer.time * 1000 : 0);
                                    List<Vector3> railPoints = new List<Vector3>();
                                    float rPos = objg.Position;
                                    float rSize = 0;
                                    if (objg.Rail.Count > 0) {
                                        rPos = ((RailTimestamp)objg.Rail[objg.Rail.Count - 1].Get(TargetPlayer ? TargetPlayer.time * 1000 : 0)).Position;
                                        for (int a = 0; a < objg.Rail.Count; a++) {
                                            if (TargetPlayer.time > objg.Rail[a].Offset / 1000f) continue;
                                            RailTimestamp endRail = (RailTimestamp)objg.Rail[a].Get(TargetPlayer ? TargetPlayer.time * 1000 : 0);
                                            if (railPoints.Count == 0) {
                                                RailTimestamp startRail = a < 1 ? new RailTimestamp { Offset = objg.Offset, Position = objg.Position, Velocity = objg.Velocity } :
                                                    (RailTimestamp)objg.Rail[a - 1].Get(TargetPlayer ? TargetPlayer.time * 1000 : 0);
                                                float sPos = Mathf.Lerp(startRail.Position, endRail.Position, (TargetPlayer.time * 1000 - startRail.Offset) / (endRail.Offset - (float)startRail.Offset));
                                                rPos = sPos;
                                                Vector3 sVel = startRail.Velocity;
                                                if (obj.CoordinateMode == CoordinateMode.Local) sVel = Quaternion.Euler(0, 0, judge.Rotation) * startRail.Velocity;
                                                Vector3 sVec = Vector3.Lerp(rStart, rEnd, sPos) + sVel * Mathf.Max(startRail.Offset / 1000f - TargetPlayer.time, 0);
                                                rSize = 5 / (sVec.z + depthConst) * depthConst;
                                                railPoints.Add((Vector3)sa.position + (Vector3)GetPosition(sVec) * scale + Vector3.back);
                                            }
                                            float ePos = endRail.Position;
                                            Vector3 eVel = endRail.Velocity;
                                            if (obj.CoordinateMode == CoordinateMode.Local) eVel = Quaternion.Euler(0, 0, judge.Rotation) * endRail.Velocity;
                                            Vector3 eVec = Vector3.Lerp(rStart, rEnd, ePos) + eVel * (endRail.Offset / 1000f - TargetPlayer.time);
                                            railPoints.Add((Vector3)sa.position + (Vector3)GetPosition(eVec) * scale + Vector3.back);
                                        }
                                        Handles.DrawAAPolyLine(EditorGUIUtility.whiteTexture, rSize, railPoints.ToArray());
                                    }
                                    RenderObject(objg, Vector3.Lerp(rStart, rEnd, rPos), judge, TargetThing == obj);
                                }
                            }
                        }
                        else if (judge.Type == Judge.JudgeType.Arc)
                        {
                            Handles.color = new Color(1, 1, 1, judge.Opacity);
                            float depth = judge.Position.z;
                            float size = judge.Length / (depth + depthConst) * depthConst;
                            Vector2 center = (judge.Position - new Vector3(6, 3.375f)) / (depth + depthConst) * depthConst + new Vector3(6, 3.375f);
                            Handles.DrawWireArc((Vector3)sa.position + (Vector3)center * scale + Vector3.back, Vector3.back,
                            new Vector3(Mathf.Cos(judge.Rotation * Mathf.Deg2Rad), Mathf.Sin(judge.Rotation * Mathf.Deg2Rad)), 
                            judge.ArcAngle, size * scale, (chosen ? 3 : 2) / (depth + depthConst) * depthConst);
                            foreach (HitObject obj in judge.Objects) {
                                if (TargetPlayer && TargetPlayer.time - .25f < obj.Offset / 1000f && TargetPlayer.time > (obj.Offset - obj.AppearTime) / 1000f) {
                                    HitObject objg = (HitObject)obj.Get(TargetPlayer ? TargetPlayer.time * 1000 : 0);
                                    float angle = (judge.Rotation + judge.ArcAngle * objg.Position) * Mathf.Deg2Rad;
                                    RenderObject(objg, judge.Position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * judge.Length, judge, TargetThing == obj);
                                }
                            }
                        }
                    }
                }
                void RenderObject(HitObject obj, Vector3 pos, Judge judge, bool chosen = false)
                {

                    float rTime = obj.Offset / 1000f - (TargetPlayer ? TargetPlayer.time : 0);
                    float aTime = (obj.Rail.Count > 0 ? obj.Rail[obj.Rail.Count - 1].Offset : obj.Offset) / 1000f;
                    float arTime = aTime - (TargetPlayer ? TargetPlayer.time : 0);
                    if (obj.CoordinateMode == CoordinateMode.Local)
                        pos += (Quaternion.Euler(0, 0, judge.Rotation) * obj.Velocity) * Mathf.Max(rTime, 0);
                    else 
                        pos += obj.Velocity * Mathf.Max(rTime, 0);
                    float depth = pos.z;
                    pos = (pos - new Vector3(6, 3.375f)) / (depth + depthConst) * depthConst + new Vector3(6, 3.375f);

                    float pow = 1 - Mathf.Pow(1 - Mathf.Max(-arTime / .25f, 0), 5);
                    float pow2 = 1 - Mathf.Pow(Mathf.Max(-arTime / .25f, 0), 5);

                    float angle = 360;
                    float size = .375f;
                    float thick = 8;
                    float thickMul = 1;
                    if (rTime < 0)
                    {
                        thick = 5;
                    }
                    else if (obj.Type == HitObject.HitType.Catch) 
                    {
                        angle = 360 * 59 / 4;
                        size = .3f;
                    }
                    else if (obj.Type == HitObject.HitType.Flick) 
                    {
                        angle = 360 * 59 / 3;
                        size = .45f;
                        thickMul = 1.5f;
                    }

                    if (chosen) thick *= 1.5f;

                    size *= Mathf.Lerp(1, 2, pow) / (depth + depthConst) * depthConst;
                    thick /= (depth + depthConst) / depthConst;
                    Handles.color = new Color(1, 1, 1, obj.Opacity * pow2);
                    Handles.DrawWireArc((Vector3)(sa.position + (Vector2)pos * scale) + Vector3.back * thick, Vector3.back, Vector2.down, 360, .05f * size * scale, pow2 * thick);
                    Handles.DrawWireArc((Vector3)(sa.position + (Vector2)pos * scale) + Vector3.back * thick, Vector3.back, Vector2.down, angle, size * scale, pow2 * thick * thickMul);
                }

                foreach (Judge judge in TargetChart.Judges) RenderJudge(judge, TargetJudge == judge);
                if (InitingItem is Judge) RenderJudge((Judge)InitingItem);

                if (Event.current.type == EventType.MouseDown && Event.current.button == 0) 
                {
                    Vector2 mPos = Event.current.mousePosition;
                    if (sa.Contains(mPos))
                    {
                        if (CurrentTool == "j_line")
                        {
                            Judge line = new Judge
                            {
                                Offset = TargetPlayer ? Mathf.RoundToInt(TargetPlayer.time * 1000) : 0,
                                Type = Judge.JudgeType.Line,
                                Position = (mPos - sa.position) / scale,
                                Length = 0,
                                Rotation = 0,
                            };
                            InitingItem = line;
                            Repaint();
                            RenderJudge(line);
                        }
                        else if (CurrentTool == "j_arc")
                        {
                            Judge line = new Judge
                            {
                                Offset = TargetPlayer ? Mathf.RoundToInt(TargetPlayer.time * 1000) : 0,
                                Type = Judge.JudgeType.Arc,
                                Position = (mPos - sa.position) / scale,
                                Length = 0,
                                Rotation = 0,
                                ArcAngle = 360,
                            };
                            InitingItem = line;
                            Repaint();
                            RenderJudge(line);
                        }
                        else if (CurrentTool.StartsWith("h_") && TargetJudge != null)
                        {
                            HitObject.HitType type = HitObject.HitType.Normal;
                            if (CurrentTool == "h_catch") type = HitObject.HitType.Catch;
                            if (CurrentTool == "h_flick") type = HitObject.HitType.Flick;
                            
                            float pos = 0;
                            if (TargetJudge.Type == Judge.JudgeType.Line) 
                            {
                                Judge judge = TargetJudge;
                                Vector3 delta = new Vector2(judge.Length * Mathf.Cos(judge.Rotation * Mathf.Deg2Rad), judge.Length * Mathf.Sin(judge.Rotation * Mathf.Deg2Rad));
                                Vector3 start = judge.Position - delta;
                                Vector3 end = judge.Position + delta;
                                Vector3 point = (mPos - sa.position) / scale;
                                pos = Mathf.Clamp01(1 - Vector2.Dot(point - end, start - end) / Vector2.Dot(start - end, start - end));
                            }

                            HitObject hit = new HitObject
                            {
                                Offset = TargetPlayer ? Mathf.RoundToInt(TargetPlayer.time * 1000) : 0,
                                Type = type,
                                Position = pos,
                                Velocity = TargetThing is HitObject ? ((HitObject)TargetThing).Velocity : Vector3.zero,
                            };

                            TargetJudge.Objects.Add(hit);
                            TargetJudge.Objects.Sort((x, y) => x.Offset.CompareTo(y.Offset));
                            Repaint();
                        }
                        else if (CurrentTool == "rail" && TargetHit != null)
                        {

                            float pos = 0;
                            if (TargetJudge.Type == Judge.JudgeType.Line) 
                            {
                                Judge judge = TargetJudge;
                                Vector3 delta = new Vector2(judge.Length * Mathf.Cos(judge.Rotation * Mathf.Deg2Rad), judge.Length * Mathf.Sin(judge.Rotation * Mathf.Deg2Rad));
                                Vector3 start = judge.Position - delta;
                                Vector3 end = judge.Position + delta;
                                Vector3 point = (mPos - sa.position) / scale;
                                pos = Mathf.Clamp01(1 - Vector2.Dot(point - end, start - end) / Vector2.Dot(start - end, start - end));
                            }

                            RailTimestamp rail = new RailTimestamp
                            {
                                Offset = TargetPlayer ? Mathf.RoundToInt(TargetPlayer.time * 1000) : 0,
                                Position = pos,
                                Velocity = TargetHit.Velocity,
                            };

                            TargetHit.Rail.Add(rail);
                            TargetHit.Rail.Sort((x, y) => x.Offset.CompareTo(y.Offset));
                            Repaint();
                            
                        }
                    }
                }
                else if (Event.current.type == EventType.MouseDrag && Event.current.button == 0) 
                {
                    Vector2 mPos = Event.current.mousePosition;
                    if (InitingItem != null)
                    {
                        if (InitingItem is Judge)
                        {
                            Judge judge = (Judge)InitingItem;
                            Vector3 aPos = (mPos - sa.position) / scale;
                            judge.Length = Vector2.Distance(judge.Position, aPos);
                            judge.Rotation = Vector3.SignedAngle(Vector2.right, aPos - judge.Position, Vector3.forward);
                            Repaint();
                        }
                    }
                }
                else if (Event.current.type == EventType.MouseUp && Event.current.button == 0) 
                {
                    if (InitingItem != null)
                    {
                        if (InitingItem is Judge && ((Judge)InitingItem).Length > 0)
                        {
                            TargetChart.Judges.Add((Judge)InitingItem);
                            TargetChart.Judges.Sort((x, y) => x.Offset.CompareTo(y.Offset));
                            EditorUtility.SetDirty(TargetSong);
                            Repaint();
                        }
                        InitingItem = null;
                    }

                }
            }
        }

        BeginWindows();
        GUI.Window(0, new Rect(-2, -2, position.width + 4, 31), Topbar, "");
        if (TargetSong) 
        {
            GUI.Window(1, new Rect(-2, position.height - 119, position.width + 4, 122), Timeline, "");
            GUI.Window(2, new Rect(position.width - 245, 35, 240, 24), ConfigBar, "");
            GUI.Window(3, new Rect(position.width - 245, 64, 240, position.height - 189), Attributes, "");
            GUI.Window(4, new Rect(5, 35, ToolbarExpanded ? 120 : 28, position.height - 160), Toolbar, "");
        } 
        else 
        {
            GUI.Window(2, new Rect(position.width / 2 - 160, position.height / 2 - 150, 360, 240), Attributes, "");
        }
        EndWindows();

        if (TargetPlayer && TargetPlayer.isPlaying) Repaint();
    }

    public void Topbar(int id)
    {
        if (!TargetSong)
        {
            TargetSong = (PlayableSong)EditorGUI.ObjectField(new Rect(5, 6, 145, 20), TargetSong, typeof(PlayableSong), false);
        }
        else
        {
            TargetSong = (PlayableSong)EditorGUI.ObjectField(new Rect(130, 6, 20, 20), TargetSong, typeof(PlayableSong), false);
            if (GUI.Button(new Rect(5, 6, 126, 20), TargetSong.SongName, "buttonLeft")) TargetThing = TargetSong;
            
            if (TargetChart != null)
            {
                if (GUI.Button(new Rect(155, 6, 126, 20), TargetChart.DifficultyName + " " + TargetChart.DifficultyLevel, "buttonLeft")) TargetThing = TargetChart;
                
                List<string> sels = new List<string>();
                foreach (Chart chart in TargetSong.Charts) sels.Add(chart.DifficultyName + " " + chart.DifficultyLevel);
                int sel = EditorGUI.Popup(new Rect(280, 6, 18, 20), -1, sels.ToArray(), "buttonRight");
                if (sel >= 0) TargetChart = TargetSong.Charts[sel];
            }
            else 
            {
                List<string> sels = new List<string>();
                foreach (Chart chart in TargetSong.Charts) sels.Add(chart.DifficultyName + " " + chart.DifficultyLevel);
                int sel = EditorGUI.Popup(new Rect(155, 6, 143, 20), -1, sels.ToArray(), "button");
                if (sel >= 0) TargetChart = TargetSong.Charts[sel];

                GUIStyle center = new GUIStyle("label");
                center.alignment = TextAnchor.MiddleCenter;
                GUI.Label(new Rect(155, 6, 145, 20), "Select Chart...", center);
            }

            if (TargetSong.Clip) 
            {
                if (TargetPlayer) 
                {
                    if (GUI.Button(new Rect(position.width / 2 - 15, 6, 32, 20), EditorGUIUtility.IconContent(TargetPlayer.isPlaying ? "PauseButton" : "PlayButton"))) 
                    {
                        TargetPlayer.clip = TargetSong.Clip;
                        if (TargetPlayer.isPlaying) TargetPlayer.Pause();
                        else TargetPlayer.Play();
                    }
                    TargetPlayer = (AudioSource)EditorGUI.ObjectField(new Rect(position.width - 22, 6, 20, 20), TargetPlayer, typeof(AudioSource), true);
                }
                else 
                {
                    TargetPlayer = (AudioSource)EditorGUI.ObjectField(new Rect(position.width - 147, 6, 145, 20), TargetPlayer, typeof(AudioSource), true);
                }
            }
        }
    }

    float seekStart, seekEnd = 2000;

    float getPos(float ms)
    { 
        return (ms - seekStart) / (seekEnd - seekStart) * position.width; 
    }
    float fromPos(float px)
    { 
        return px / position.width * (seekEnd - seekStart) + seekStart; 
    }

    bool seekDrag;
    int visualMode = 0, editMode = 0, configMode = 1;

    public void Timeline(int id)
    {
        GUIStyle tLabel = new GUIStyle(EditorStyles.miniLabel);
        tLabel.alignment = TextAnchor.MiddleCenter;

        EditorGUI.DrawRect(new Rect(2, 0, position.width, 90), EditorGUIUtility.isProSkin ? new Color(.2f, .2f, .2f, .5f) : new Color(.9f, .9f, .9f, .5f));
        EditorGUI.DrawRect(new Rect(2, 90, position.width, 1), new Color(.5f, .5f, .5f, .75f));

        EditorGUI.DrawRect(new Rect(2, 4, position.width, 1), new Color(.5f, .5f, .5f, .45f));
        EditorGUI.DrawRect(new Rect(2, 75, position.width, 1), new Color(.5f, .5f, .5f, .45f));

        float length = TargetSong.Clip.length * 1000;

        // Time Indicators

        for (float a = Mathf.Ceil(seekStart / 1000) * 1000; a <= seekEnd; a += 1000)
        {
            EditorGUI.DrawRect(new Rect(getPos(a) + 1.5f, 80, 1, 10), new Color(.5f, .5f, .5f, .25f));
        }

        float zoom = (seekEnd - seekStart) / position.width;
        float tPos = 100;
        if (zoom > 200)       tPos = 20000;
        else if (zoom > 100)  tPos = 10000;
        else if (zoom > 50)   tPos = 5000;
        else if (zoom > 20)   tPos = 2000;
        else if (zoom > 10)   tPos = 1000;
        else if (zoom > 5)    tPos = 500;
        else if (zoom > 2)    tPos = 200;
        else                  tPos = 100;

        for (float a = Mathf.Ceil(seekStart / tPos) * tPos; a <= seekEnd; a += tPos)
        {
            string str = Mathf.Floor(a / 60000).ToString("0") + ":" + Mathf.Floor(a / 1000 % 60).ToString("00") + "." + Mathf.Floor(a / 100 % 10).ToString("0");
            
            GUI.Label(new Rect(getPos(a) - 38, 75, 80, 15), str, tLabel);
        }

        // Beat Indicators

        int msClick = 0;

        if (TargetSong.Timing.Stops.Count > 0) 
        {
            float getSlope(BPMStop stop)
            {
                float delta = stop.BPM / zoom;
                float s = 1 / Mathf.Pow(SeparateFactor, 6);
                while (delta * s < 5) s *= s > 1 ? stop.Signature : SeparateFactor;
                return s;
            }

            float beat = Mathf.Floor(TargetSong.Timing.ToBeat(Mathf.RoundToInt(seekStart)));
            int ms = TargetSong.Timing.ToMilliseconds(beat);
            BPMStop c = TargetSong.Timing.GetStop(ms);
            float cSlope = getSlope(c);
            BPMStop cBPM = c;

            beat = seekStart < 0 ? Mathf.Ceil(TargetSong.Timing.ToBeat(Mathf.RoundToInt(seekStart)) / cSlope) * cSlope : 
                Mathf.Floor(TargetSong.Timing.ToBeat(Mathf.RoundToInt(seekStart)) / cSlope) * cSlope;

            int t = 0;

            bool isClick = Event.current.type == EventType.MouseDown;
            float mpx = Event.current.mousePosition.x;

            while (t < 1000) 
            {
                ms = TargetSong.Timing.ToMilliseconds(beat);
                c = TargetSong.Timing.GetStop(ms);
                if (c != cBPM)
                {
                    cSlope = getSlope(c);
                    cBPM = c;
                }

                if (isClick && Mathf.Abs(getPos(msClick) - mpx) > Mathf.Abs(getPos(ms) - mpx))
                {
                    msClick = ms;
                }

                float size = 20;
                if (beat < 0) size = 20;
                else if (beat % (cSlope * c.Signature * SeparateFactor) < cSlope) size = 70;
                else if (beat % (cSlope * SeparateFactor * SeparateFactor) < cSlope) size = 50;
                else if (beat % (cSlope * SeparateFactor) < cSlope) size = 35;

                EditorGUI.DrawRect(new Rect(getPos(ms) + 1.5f, 5, 1, size), new Color(.5f, .5f, .5f, .25f));
                if (size == 70) GUI.Label(new Rect(getPos(ms) - 38, 59, 80, 15), beat.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture), tLabel);
                t++;

                beat += cSlope;
                if (getPos(ms) > seekEnd) break;
            }
        }
        
        // Song Start/End

        if (seekStart <= 0)
        {
            EditorGUI.DrawRect(new Rect(2, 5, getPos(0), 85), EditorGUIUtility.isProSkin ? new Color(0, .1f, 0, .45f) : new Color(.65f, 1, .65f, .45f));
        }
        if (seekEnd >= length)
        {
            float pos = getPos(length);
            EditorGUI.DrawRect(new Rect(pos + 2, 5, position.width - pos, 85), EditorGUIUtility.isProSkin ? new Color(.2f, 0, 0, .45f) : new Color(1, .5f, .5f, .45f));
        }

        // Seek Line

        if (TargetPlayer && TargetPlayer.time * 1000 <= seekEnd && TargetPlayer.time * 1000 >= seekStart)
        {
            EditorGUI.DrawRect(new Rect(getPos(TargetPlayer.time * 1000) + 1, 5, 2, 85), EditorGUIUtility.isProSkin ? new Color(.9f, .9f, .9f, .5f) : new Color(.2f, .2f, .2f, .5f));
        }

        // Seek Slider

        float minSize = position.width;

        EditorGUI.MinMaxSlider(new Rect(1, -10, position.width - 2, 16), ref seekStart, ref seekEnd, -zoom * 50, length + zoom * 50);
        seekStart = Mathf.Max(Mathf.Min(seekStart, length - minSize + zoom * 50), -zoom * 50);
        seekEnd = Mathf.Max(Mathf.Min(seekEnd, length + zoom * 50), seekStart + minSize);

        if (TargetPlayer) EditorGUI.DrawRect(new Rect((TargetPlayer.time * 1000 + zoom * 50) / (length + zoom * 100) * (position.width - 12) + 6, 0, 1, 4), 
            (EditorGUIUtility.isProSkin ^ (TargetPlayer.time * 1000 <= seekEnd && TargetPlayer.time * 1000 >= seekStart)) ? new Color(.9f, .9f, .9f, .75f) : new Color(.2f, .2f, .2f, .75f));
        
        // Toolbar

        string[] editModes = new[] { "Timing", "S.board", "Judges" };
        if (TargetJudge != null) editModes = new[] { "Timing", "S.board", "Judges", "Hits" };
        if (TargetHit != null) editModes = new[] { "Timing", "S.board", "Judges", "Hits", "Line" };
        editMode = GUI.Toolbar(new Rect(5, 95, 320, 20), editMode, editModes);

        GUI.Label(new Rect(330, 96, 120, 18), TargetPlayer ? (TargetPlayer.time * 1000).ToString("0") : "");

        SeparateFactor = Mathf.RoundToInt(GUI.HorizontalSlider(new Rect(position.width - 70, 96, 20, 18), SeparateFactor, 2, 3));

        if (TargetPlayer)
        {
            EditorGUI.DrawRect(new Rect(position.width - 45, 96, 42, 18), Color.black);
            if (visualMode == 0)
            {
                float beat = TargetSong.Timing.ToBeat(Mathf.RoundToInt(TargetPlayer.time * 1000));
                int sig = TargetSong.Timing.GetStop((Mathf.RoundToInt(TargetPlayer.time * 1000))).Signature;
                EditorGUI.DrawRect(new Rect(position.width - 44 + 40 * (((Mathf.Floor(beat) / sig % 1) + 1) % 1), 97, 40 / sig, 16), Color.white * (1 - (((beat % 1) + 1) % 1)));
            }
            else if (visualMode == 1)
            {
                float[] sum = new float[8];
                float[] data = new float[256];
                TargetPlayer.GetSpectrumData(data, 0, FFTWindow.Rectangular);
                for (int a = 0; a < 8; a++)
                {
                    for (int b = Mathf.RoundToInt(Mathf.Pow(2, a)); b < Mathf.Pow(2, a + 1); b++)
                    {
                        sum[a] += data[b];
                    }
                    float height = Math.Min(Mathf.Floor(16 * sum[a]) + 1, 16);
                    EditorGUI.DrawRect(new Rect(position.width - 44 + 5 * a, 113 - height, 5, height), Color.white);
                }
            }
        }

        //if (TargetPlayer) TargetPlayer.time = EditorGUI.FloatField(new Rect(330, 95, 100, 20), TargetPlayer.time);

        // Objects

        GUIStyle bStyle = new GUIStyle(GUI.skin.button);
        bStyle.fontSize = 11;

        List<float> Times = new List<float>();
        int AddTime(float time, float width) {
            for (int a = 0; a < Times.Count; a++)
            {
                if (time - Times[a] > width) 
                {
                    Times[a] = time;
                    return a;
                }
            }
            Times.Add(time);
            return Times.Count - 1;
        }

        if (editMode == 0)
        {
            foreach (BPMStop stop in TargetSong.Timing.Stops)
            {
                if (stop.Offset <= seekEnd && stop.Offset >= seekStart)
                {
                    float pos = getPos(stop.Offset);
                    int y = AddTime(pos, 62);
                    EditorGUI.DrawRect(new Rect(pos + 1, 5, 2, 70), EditorGUIUtility.isProSkin ? new Color(1, .4f, .3f, .75f) : new Color(.5f, .2f, .1f, .75f));
                    if (GUI.Button(new Rect(pos - 28, 6 + 20 * y, 60, 18), stop.BPM.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture), bStyle)) 
                    {
                        TargetThing = stop;
                    }
                }
            }
        } 
        else if (editMode == 2)
        {
            if (TargetChart != null)
            {
                foreach (Judge judge in TargetChart.Judges)
                {
                    float pos = getPos(judge.Offset);
                    int y = AddTime(pos, 20);
                    if (GUI.Button(new Rect(pos - 7, 6 + 20 * y, 18, 18), DeletingItem == judge ? "?" : judge.Type.ToString().Remove(1), bStyle)) 
                    {
                        if (CurrentTool == "delete") 
                        {
                            if (DeletingItem == judge) 
                            {
                                TargetChart.Judges.Remove(judge);
                                if (TargetThing == judge) TargetThing = null;
                                if (TargetJudge == judge) TargetJudge = null;
                                if (judge.Objects.Contains(TargetHit)) TargetHit = null;
                                break;
                            }
                            else DeletingItem = judge;
                        }
                        else 
                        {
                            TargetThing = TargetJudge = judge;
                            TargetHit = null;
                        } 
                    }
                }
            }
        }
        else if (editMode == 3)
        {
            if (TargetJudge != null)
            {
                foreach (HitObject hit in TargetJudge.Objects)
                {
                    float pos = getPos(hit.Offset);
                    int y = AddTime(pos, 20);
                    if (GUI.Button(new Rect(pos - 7, 6 + 20 * y, 18, 18), DeletingItem == hit ? "?" : hit.Type.ToString().Remove(1), bStyle)) 
                    {
                        if (CurrentTool == "delete") 
                        {
                            if (DeletingItem == hit) 
                            {
                                TargetJudge.Objects.Remove(hit);
                                if (TargetThing == hit) TargetThing = null;
                                break;
                            }
                            else DeletingItem = hit;
                        }
                        else 
                        {
                            TargetThing = TargetHit = hit;
                        } 
                    }
                }
            }
        }
        else if (editMode == 4)
        {
            if (TargetJudge != null)
            {
                foreach (RailTimestamp rail in TargetHit.Rail)
                {
                    float pos = getPos(rail.Offset);
                    int y = AddTime(pos, 20);
                    if (GUI.Button(new Rect(pos - 7, 6 + 20 * y, 18, 18), DeletingItem == rail ? "?" : "", bStyle)) 
                    {
                        if (CurrentTool == "delete") 
                        {
                            if (DeletingItem == rail) 
                            {
                                TargetHit.Rail.Remove(rail);
                                if (TargetThing == rail) TargetThing = null;
                                break;
                            }
                            else DeletingItem = rail;
                        }
                        else 
                        {
                            TargetThing = rail;
                        } 
                    }
                }
            }
        }

        // Click events

        if (Event.current.type == EventType.MouseDown && Event.current.button == 0) 
        {
            Vector2 mPos = Event.current.mousePosition;
            if (TargetPlayer && mPos.y > 5 && mPos.y < 75) 
            {
                TargetPlayer.time = Mathf.Clamp(msClick / 1000f, 0, TargetPlayer.clip.length - .0001f);
                Repaint();
            }
            if (TargetPlayer && mPos.y > 75 && mPos.y < 90) 
            {
                TargetPlayer.time = Mathf.Clamp(fromPos(mPos.x) / 1000, 0, TargetPlayer.clip.length - .0001f);
                seekDrag = true;
                Repaint();
            }
            if (new Rect(position.width - 43, 96, 41, 18).Contains(mPos)) 
            {
                visualMode = (visualMode + 1) % 2;
                Repaint();
            }
            DeletingItem = null;
        }
        else if (Event.current.type == EventType.MouseDrag && Event.current.button == 0) 
        {
            Vector2 mPos = Event.current.mousePosition;
            if (TargetPlayer && seekDrag) 
            {
                TargetPlayer.time = Mathf.Clamp(fromPos(mPos.x) / 1000, 0, TargetPlayer.clip.length - .0001f);
                Repaint();
            }
        }
        else if (Event.current.type == EventType.MouseUp && Event.current.button == 0) 
        {
            seekDrag = false;
        }
    }

    string initName, initArtist;
    AudioClip initClip;
    Vector2 AttributeScroll;

    public void Attributes(int id)
    {
        EditorGUIUtility.labelWidth = 100;
        if (!TargetSong)
        {
            GUIStyle title = new GUIStyle(EditorStyles.largeLabel);
            title.fontSize = 24;
            title.fontStyle = FontStyle.Bold;
            title.wordWrap = true;
            GUILayout.Label("Welcome to Project: Rithmoflex Charter Engine", title);

            title = new GUIStyle(EditorStyles.label);
            title.wordWrap = true;
            GUILayout.Label("Please select a song into the box above to start editing, or "
                + "press the button to start making a playable song from scratch.", title);

            GUILayout.Space(10);
            initName = EditorGUILayout.TextField("Name", initName);
            initArtist = EditorGUILayout.TextField("Artist", initArtist);
            initClip = (AudioClip)EditorGUILayout.ObjectField("Clip", initClip, typeof(AudioClip), false);
            
            if (GUILayout.Button("Create Playable Song"))
            
            {
                PlayableSong song = ScriptableObject.CreateInstance<PlayableSong>();
                song.SongName = initName;
                song.SongArtist = initArtist;
                song.Clip = initClip;

                string path = AssetDatabase.GetAssetPath(Selection.activeInstanceID);
                if (!System.IO.Directory.Exists(path)) path = System.IO.Path.GetDirectoryName(path);

                AssetDatabase.CreateAsset(song, AssetDatabase.GenerateUniqueAssetPath(path + "/" + initName + " - " + initArtist + ".asset"));
                AssetDatabase.SaveAssets();

                TargetSong = song;
            }
        }
        else 
        {
            EditorGUIUtility.labelWidth = 80;
            GUI.Label(new Rect(0, 0, 240, 26), "", "Button");
            if (configMode == 0) 
            {
                GUI.Label(new Rect(6, 4, 140, 18), "Configurations", EditorStyles.boldLabel);
                
                GUILayout.Space(8);
                GUILayout.BeginScrollView(AttributeScroll);
                GUILayout.EndScrollView();
            }
            else if (configMode == 3) 
            {
                GUI.Label(new Rect(6, 4, 140, 18), "Groups", EditorStyles.boldLabel);
                
                if (TargetChart != null) {
                    GUILayout.Space(8);
                    AttributeScroll = GUILayout.BeginScrollView(AttributeScroll);
                    float y = 0;

                    float total = TargetChart.Groups.Count * 22 + 20;
                    bool ovf = total > position.height - 229;

                    foreach (JudgeGroup group in TargetChart.Groups) 
                    {
                        if (GUI.Button(new Rect(3, y, ovf ? 195 : 207, 20), group.Name, "buttonLeft")) {
                            TargetThing = group;
                            configMode = 1;
                        }

                        if (GUI.Button(new Rect(ovf ? 197 : 209, y, 20, 20), DeletingItem == group ? "?" : "x", "buttonRight")) {
                            if (DeletingItem == group) TargetChart.Groups.Remove(group);
                            else DeletingItem = group;
                        }
                        
                        y += 22;
                    }

                    if (GUI.Button(new Rect(3, y, ovf ? 214 : 226, 20), "Add")) {
                        TargetChart.Groups.Add(new JudgeGroup() {
                            Name = "Group " + (TargetChart.Groups.Count + 1),
                        });
                    }

                    GUILayout.Space(y + 20);

                    GUILayout.EndScrollView();
                }
            }
            else if (configMode == 2) 
            {
                if (TargetThing == (object)TargetSong) 
                {
                    GUI.Label(new Rect(6, 4, 140, 18), "Song Storyboard", EditorStyles.boldLabel);
                }
                else if (TargetThing == (object)TargetChart && TargetChart != null) 
                {
                    GUI.Label(new Rect(6, 4, 140, 18), "Chart Storyboard", EditorStyles.boldLabel);
                }
                else if (TargetThing is BPMStop) 
                {
                    GUI.Label(new Rect(6, 4, 140, 18), "BPM Stop", EditorStyles.boldLabel);
                }
                else if (TargetThing is JudgeGroup) 
                {
                    GUI.Label(new Rect(6, 4, 140, 18), "Judge Group", EditorStyles.boldLabel);
                }
                else if (TargetThing is Judge) 
                {
                    GUI.Label(new Rect(6, 4, 140, 18), "Judge Line", EditorStyles.boldLabel);
                }
                else if (TargetThing is HitObject) 
                {
                    GUI.Label(new Rect(6, 4, 140, 18), "Hit Object", EditorStyles.boldLabel);
                }
                else if (TargetThing is RailTimestamp)
                {
                    GUI.Label(new Rect(6, 4, 140, 18), "Rail Timestamp", EditorStyles.boldLabel);
                }
                else {
                    GUI.Label(new Rect(6, 4, 140, 18), "No Object", EditorStyles.boldLabel);
                    GUILayout.Space(8);
                    GUILayout.BeginScrollView(AttributeScroll);
                    GUIStyle style = new GUIStyle(EditorStyles.label);
                    style.wordWrap = true;
                    GUILayout.Label("Please select an object in Timeline to begin editing.", style);
                    GUILayout.EndScrollView();
                    return;
                }
                if (TargetThing is IStoryboardable) {
                    IStoryboardable isb = (IStoryboardable)TargetThing;
                    Storyboard sb = isb.Storyboard;
                    
                    GUIStyle center = new GUIStyle("label");
                    center.alignment = TextAnchor.MiddleCenter;
                    GUIStyle msStyle = new GUIStyle("textField");
                    msStyle.alignment = TextAnchor.MiddleRight;

                    List<string> tst = new List<string>();
                    List<string> tso = new List<string>();
                    foreach (TimestampType type in (TimestampType[])isb.GetType().GetField("TimestampTypes").GetValue(null)) {
                        tso.Add(type.ID);
                        tst.Add(type.Name);
                    }

                    List<string> est = new List<string>();
                    List<string> eso = new List<string>();
                    foreach (Ease ease in Ease.Eases) {
                        eso.Add(ease.ID);
                        est.Add(ease.Name);
                    }

                    float total = sb.Timestamps.Count * 46 + 20;
                    bool ovf = total > position.height - 229;
                    
                    GUILayout.Space(8);
                    AttributeScroll = GUILayout.BeginScrollView(AttributeScroll);

                    float y = 0;
                    foreach (Timestamp ts in sb.Timestamps) 
                    {
                        GUI.Label(new Rect(3, y, 207, 44), "", "buttonLeft");

                        int type = tso.IndexOf(ts.ID);

                        int newType = EditorGUI.Popup(new Rect(6, y + 3, ovf ? 115 : 127, 18), type, tst.ToArray());
                        if (newType != type) ts.ID = tso[newType];
                        ts.Time = EditorGUI.IntField(new Rect(ovf ? 123 : 135, y + 3, 50, 18), ts.Time, msStyle);
                        if (GUI.Button(new Rect(ovf ? 174 : 186, y + 3, 20, 18), "ms", "label")) {
                            ts.Time = Mathf.RoundToInt(TargetPlayer.time * 1000);
                        };
                        
                        int ease = eso.IndexOf(ts.Easing);

                        ts.Target = EditorGUI.FloatField(new Rect(6, y + 23, 50, 18), ts.Target);
                        int newEase = EditorGUI.Popup(new Rect(58, y + 23, ovf ? 63 : 75, 18), ease, est.ToArray());
                        if (newEase != ease) ts.Easing = eso[newEase];
                        ts.Duration = EditorGUI.IntField(new Rect(ovf ? 123 : 135, y + 23, 50, 18), ts.Duration, msStyle);
                        if (GUI.Button(new Rect(ovf ? 174 : 186, y + 23, 20, 18), "ms", "label")) {
                            ts.Duration = Mathf.RoundToInt(TargetPlayer.time * 1000) - ts.Time;
                        }

                        if (GUI.Button(new Rect(ovf ? 197 : 209, y, 20, 44), DeletingItem == ts ? "?" : "x", "buttonRight")) {
                            if (DeletingItem == ts) sb.Timestamps.Remove(ts);
                            else DeletingItem = ts;
                        }
                        
                        y += 46;
                    }

                    int add = EditorGUI.Popup(new Rect(3, y, ovf ? 214 : 226, 20), -1, tst.ToArray(), "button");
                    if (add != -1) {
                        sb.Timestamps.Add(new Timestamp {
                            ID = tso[add],
                            Time = TargetPlayer ? Mathf.RoundToInt(TargetPlayer.time * 1000) : 0,
                        });
                    }
                    GUI.Label(new Rect(3, y, 226, 20), "Add...", center);

                    GUILayout.Space(y + 20);

                    GUILayout.EndScrollView();

                    tst.Insert(0, "All");
                    EditorGUI.Popup(new Rect(124, 4, 75, 18), 0, tst.ToArray());
                    GUI.Button(new Rect(201, 4, 35, 18), "Sort");
                } else {
                    GUIStyle style = new GUIStyle(EditorStyles.label);
                    style.wordWrap = true;

                    GUILayout.Space(8);
                    AttributeScroll = GUILayout.BeginScrollView(AttributeScroll);
                    GUILayout.Label("This object does not have a storyboard.", style);
                    GUILayout.EndScrollView();
                }
            }
            else if (configMode == 1) 
            {
                if (TargetThing == (object)TargetSong) 
                {
                    GUI.Label(new Rect(6, 4, 140, 18), "Song Details", EditorStyles.boldLabel);

                    GUIStyle bStyle = new GUIStyle("textField");
                    bStyle.fontStyle = FontStyle.Bold;

                    GUILayout.Space(8);
                    GUILayout.BeginScrollView(AttributeScroll);
                    if (GUILayout.Button("Set Dirty")) EditorUtility.SetDirty(TargetSong);
                    GUILayout.Space(8);
                    GUILayout.Label("Metadata", "BoldLabel");
                    TargetSong.SongName = EditorGUILayout.TextField("Title", TargetSong.SongName, bStyle);
                    TargetSong.SongArtist = EditorGUILayout.TextField("Artist", TargetSong.SongArtist);
                    GUILayout.Space(8);
                    GUILayout.Label("Charts", "BoldLabel");
                    foreach (Chart chart in TargetSong.Charts)
                    {
                        GUILayout.BeginHorizontal();
                        if (GUILayout.Toggle(TargetChart == chart, chart.DifficultyName + " " + chart.DifficultyLevel, "ButtonLeft"))
                        {
                            TargetChart = chart;
                        }
                        if (GUILayout.Button("x", "ButtonRight", GUILayout.MaxWidth(18)) && TargetChart != chart)
                        {
                            TargetSong.Charts.Remove(chart);
                            EditorUtility.SetDirty(TargetSong);
                            break;
                        }
                        GUILayout.EndHorizontal();
                    }
                    if (GUILayout.Button("Create New Chart"))
                    {
                        Chart chart = new Chart();
                        TargetSong.Charts.Add(chart);
                        TargetChart = chart;
                        EditorUtility.SetDirty(TargetSong);
                    }
                    GUILayout.EndScrollView();
                }
                else if (TargetThing == (object)TargetChart && TargetChart != null) 
                {
                    GUI.Label(new Rect(6, 4, 140, 18), "Chart Details", EditorStyles.boldLabel);

                    GUIStyle bStyle = new GUIStyle("textField");
                    bStyle.fontStyle = FontStyle.Bold;

                    GUILayout.Space(8);
                    GUILayout.BeginScrollView(AttributeScroll);
                    GUILayout.Label("Difficulty", "BoldLabel");
                    TargetChart.DifficultyName = EditorGUILayout.TextField("Name", TargetChart.DifficultyName, bStyle);
                    TargetChart.DifficultyLevel = EditorGUILayout.TextField("Level", TargetChart.DifficultyLevel);
                    GUILayout.EndScrollView();
                }
                else if (TargetThing is BPMStop) 
                {
                    BPMStop obj = (BPMStop)TargetThing;
                    GUI.Label(new Rect(6, 4, 140, 18), "BPM Stop", EditorStyles.boldLabel);

                    GUIStyle msStyle = new GUIStyle("textField");
                    msStyle.alignment = TextAnchor.MiddleRight;
                    msStyle.fontStyle = FontStyle.Bold;
                    obj.Offset = EditorGUI.IntField(new Rect(154, 4, 60, 18), obj.Offset, msStyle);
                    GUI.Label(new Rect(214, 4, 20, 18), "ms");

                    GUILayout.Space(8);
                    GUILayout.BeginScrollView(AttributeScroll);
                    GUILayout.Label("Timing", "BoldLabel");
                    obj.BPM = EditorGUILayout.FloatField("BPM", obj.BPM);
                    obj.Signature = EditorGUILayout.IntField("Signature", obj.Signature);
                    GUILayout.EndScrollView();
                }
                else if (TargetThing is JudgeGroup) 
                {
                    JudgeGroup obj = (JudgeGroup)TargetThing;
                    GUI.Label(new Rect(6, 4, 140, 18), "Judge Group", EditorStyles.boldLabel);

                    GUIStyle msStyle = new GUIStyle("textField");
                    msStyle.alignment = TextAnchor.MiddleRight;
                    msStyle.fontStyle = FontStyle.Bold;

                    GUILayout.Space(8);
                    GUILayout.BeginScrollView(AttributeScroll);
                    GUILayout.Label("Refer Name", "BoldLabel");
                    obj.Name = GUILayout.TextField(obj.Name);
                    GUILayout.Space(8);
                    GUILayout.Label("Transform", "BoldLabel");
                    obj.Position = EditorGUILayout.Vector3Field("Position", obj.Position);
                    obj.Rotation = EditorGUILayout.FloatField("Rotation", obj.Rotation);
                    GUILayout.Space(8);
                    GUILayout.EndScrollView();
                }
                else if (TargetThing is Judge) 
                {
                    Judge obj = (Judge)TargetThing;
                    GUI.Label(new Rect(6, 4, 140, 18), "Judge Line", EditorStyles.boldLabel);

                    GUIStyle msStyle = new GUIStyle("textField");
                    msStyle.alignment = TextAnchor.MiddleRight;
                    msStyle.fontStyle = FontStyle.Bold;
                    obj.Offset = EditorGUI.IntField(new Rect(154, 4, 60, 18), obj.Offset, msStyle);
                    GUI.Label(new Rect(214, 4, 20, 18), "ms");

                    GUILayout.Space(8);
                    GUILayout.BeginScrollView(AttributeScroll);
                    GUILayout.Label("Type", "BoldLabel");
                    obj.Type = (Judge.JudgeType)EditorGUILayout.EnumPopup((System.Enum)obj.Type);
                    GUILayout.Space(8);
                    GUILayout.Label("Transform", "BoldLabel");
                    obj.Group = EditorGUILayout.TextField("Group", obj.Group);
                    if (obj.Type == Judge.JudgeType.Line)
                    {
                        obj.Position = EditorGUILayout.Vector3Field("Position", obj.Position);
                        obj.Rotation = EditorGUILayout.FloatField("Rotation", obj.Rotation);
                        obj.Length = EditorGUILayout.FloatField("Length", obj.Length);
                    }
                    if (obj.Type == Judge.JudgeType.Arc)
                    {
                        obj.Position = EditorGUILayout.Vector3Field("Position", obj.Position);
                        obj.Rotation = EditorGUILayout.FloatField("Rotation", obj.Rotation);
                        obj.Length = EditorGUILayout.FloatField("Radius", obj.Length);
                        obj.ArcAngle = EditorGUILayout.FloatField("Angle", obj.ArcAngle);
                    }
                    GUILayout.Space(8);
                    GUILayout.Label("Appearance", "BoldLabel");
                    obj.Opacity = EditorGUILayout.FloatField("Opacity", obj.Opacity);
                    GUILayout.EndScrollView();
                }
                else if (TargetThing is HitObject) 
                {
                    HitObject obj = (HitObject)TargetThing;
                    GUI.Label(new Rect(6, 4, 140, 18), "Hit Object", EditorStyles.boldLabel);

                    GUIStyle msStyle = new GUIStyle("textField");
                    msStyle.alignment = TextAnchor.MiddleRight;
                    msStyle.fontStyle = FontStyle.Bold;
                    obj.Offset = EditorGUI.IntField(new Rect(154, 4, 60, 18), obj.Offset, msStyle);
                    GUI.Label(new Rect(214, 4, 20, 18), "ms");

                    GUILayout.Space(8);
                    GUILayout.BeginScrollView(AttributeScroll);
                    GUILayout.Label("Type", "BoldLabel");
                    obj.Type = (HitObject.HitType)EditorGUILayout.EnumPopup(obj.Type);
                    GUILayout.Space(8);
                    GUILayout.Label("Transform", "BoldLabel");
                    obj.Position = EditorGUILayout.Slider("Position", obj.Position, 0, 1);
                    obj.Velocity = EditorGUILayout.Vector3Field("Velocity", obj.Velocity);
                    GUILayout.Label("Coordinate Mode");
                    obj.CoordinateMode = (CoordinateMode)EditorGUILayout.EnumPopup(obj.CoordinateMode);
                    GUILayout.Space(8);
                    GUILayout.Label("Appearance", "BoldLabel");
                    obj.Opacity = EditorGUILayout.FloatField("Opacity", obj.Opacity);
                    GUILayout.EndScrollView();
                }
                else if (TargetThing is RailTimestamp) 
                {
                    RailTimestamp obj = (RailTimestamp)TargetThing;
                    GUI.Label(new Rect(6, 4, 140, 18), "Rail Timestamp", EditorStyles.boldLabel);

                    GUIStyle msStyle = new GUIStyle("textField");
                    msStyle.alignment = TextAnchor.MiddleRight;
                    msStyle.fontStyle = FontStyle.Bold;
                    obj.Offset = EditorGUI.IntField(new Rect(154, 4, 60, 18), obj.Offset, msStyle);
                    GUI.Label(new Rect(214, 4, 20, 18), "ms");

                    GUILayout.Space(8);
                    GUILayout.BeginScrollView(AttributeScroll);
                    GUILayout.Label("Transform", "BoldLabel");
                    obj.Position = EditorGUILayout.Slider("Position", obj.Position, 0, 1);
                    obj.Velocity = EditorGUILayout.Vector3Field("Velocity", obj.Velocity);
                    GUILayout.EndScrollView();
                }
                else if (editMode == 1 && TargetChart != null) 
                {
                    GUI.Label(new Rect(6, 4, 140, 18), "Judgements", EditorStyles.boldLabel);

                    GUIStyle msStyle = new GUIStyle("textField");
                    msStyle.alignment = TextAnchor.MiddleRight;
                    msStyle.fontStyle = FontStyle.Bold;

                    GUILayout.Space(8);
                    GUILayout.BeginScrollView(AttributeScroll);
                    foreach (Judge judge in TargetChart.Judges)
                    {
                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button(judge.Type.ToString(), "ButtonLeft"))
                        {
                            TargetThing = judge;
                        }
                        if (GUILayout.Button("x", "ButtonRight", GUILayout.MaxWidth(18)))
                        {
                            TargetChart.Judges.Remove(judge);
                            EditorUtility.SetDirty(TargetSong);
                            break;
                        }
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.EndScrollView();
                }
                else 
                {
                    GUI.Label(new Rect(6, 4, 140, 18), "No Object", EditorStyles.boldLabel);
                    GUILayout.Space(8);
                    GUILayout.BeginScrollView(AttributeScroll);
                    GUIStyle style = new GUIStyle(EditorStyles.label);
                    style.wordWrap = true;
                    GUILayout.Label("Please select an object in Timeline to begin editing.", style);
                    GUILayout.EndScrollView();
                }
            }
        }
    }
    
    public void ConfigBar(int id)
    {
        configMode = GUI.Toolbar(new Rect(0, 0, 240, 24), configMode, new[]{ "Config.", "Attrib.", "S.board", "Groups" });
    }
    
    public void Toolbar(int id)
    {
        GUIStyle itemStyle = new GUIStyle("Button");
        itemStyle.alignment = ToolbarExpanded ? TextAnchor.MiddleLeft : TextAnchor.MiddleCenter;

        float width = ToolbarExpanded ? 120 : 28;

        GUIContent GetContent(string name, string icon)
        {
            if(ToolbarExpanded) return new GUIContent(name, EditorGUIUtility.FindTexture(icon));
            else return EditorGUIUtility.IconContent(icon, name);
        } 
        GUI.Label(new Rect(0, 0, width, position.height - 150), "", itemStyle);

        if (GUI.Toggle(new Rect(0, 0, width, 28), CurrentTool == "select", GetContent("Select", "Grid.Default"), itemStyle)) CurrentTool = "select"; 
        if (GUI.Toggle(new Rect(0, 27, width, 28), CurrentTool == "delete", GetContent("Delete", "winbtn_win_close_a"), itemStyle)) CurrentTool = "delete"; 

        if (editMode == 2)
        {
            if (GUI.Toggle(new Rect(0, 57, width, 28), CurrentTool == "j_line", ToolbarExpanded ? "Line" : "LN", itemStyle)) CurrentTool = "j_line"; 
            if (GUI.Toggle(new Rect(0, 84, width, 28), CurrentTool == "j_arc", ToolbarExpanded ? "Arc" : "AR", itemStyle)) CurrentTool = "j_arc"; 
            if (GUI.Toggle(new Rect(0, 111, width, 28), CurrentTool == "j_curve", ToolbarExpanded ? "Curve" : "CV", itemStyle)) CurrentTool = "j_curve"; 
        }
        else if (editMode == 3)
        {
            if (GUI.Toggle(new Rect(0, 57, width, 28), CurrentTool == "h_normal", ToolbarExpanded ? "Normal" : "NR", itemStyle)) CurrentTool = "h_normal"; 
            if (GUI.Toggle(new Rect(0, 84, width, 28), CurrentTool == "h_catch", ToolbarExpanded ? "Catch" : "CA", itemStyle)) CurrentTool = "h_catch"; 
            // if (GUI.Toggle(new Rect(0, 111, width, 28), CurrentTool == "h_flick", ToolbarExpanded ? "Flick" : "FK", itemStyle)) CurrentTool = "h_flick"; 
        }
        else if (editMode == 4)
        {
            if (GUI.Toggle(new Rect(0, 57, width, 28), CurrentTool == "rail", ToolbarExpanded ? "Rail" : "RL", itemStyle)) CurrentTool = "rail"; 
        }
        
        if (GUI.Button(new Rect(0, position.height - 188, width, 28), ToolbarExpanded ? GetContent("Collapse", "tab_prev") : GetContent("Expand", "tab_next"), itemStyle)) ToolbarExpanded = !ToolbarExpanded;
        
    }
}