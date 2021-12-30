using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Timestamp 
{
    public int Time;
    public int Duration;
    public string ID;
    public float Target;
    public string Easing = "Linear";
}

public class TimestampType {
    public string ID;
    public string Name;
    public Func<IStoryboardable, float> Get;
    public Action<IStoryboardable, float> Set;
}

[Serializable]
public class Storyboard 
{
    public List<Timestamp> Timestamps = new List<Timestamp>();

    public void Add(Timestamp timestamp) {
        Timestamps.Add(timestamp);
        Timestamps.Sort((x, y) => x.Time.CompareTo(y.Time));
    }

    public List<Timestamp> FromType(string type) {
        return Timestamps.FindAll(x => x.ID == type);
    }
}

public class Ease {
    public string ID;
    public string Name;
    public Func<float, float> Get;

    public static Ease[] Eases = {
        new Ease {
            ID = "Linear",
            Name = "Linear",
            Get = (x) => x,
        },
        new Ease {
            ID = "SineIn",
            Name = "Sine In",
            Get = (x) => 1 - Mathf.Cos((x * Mathf.PI) / 2),
        },
        new Ease {
            ID = "SineOut",
            Name = "Sine Out",
            Get = (x) => Mathf.Sin((x * Mathf.PI) / 2),
        },
        new Ease {
            ID = "SineInOut",
            Name = "Sine In Out",
            Get = (x) => (1 - Mathf.Cos(x * Mathf.PI)) / 2,
        },
        new Ease {
            ID = "QuadIn",
            Name = "Quad In",
            Get = (x) => x * x,
        },
        new Ease {
            ID = "QuadOut",
            Name = "Quad Out",
            Get = (x) => 1 - Mathf.Pow(1 - x, 2),
        },
        new Ease {
            ID = "QuadInOut",
            Name = "Quad In Out",
            Get = (x) => x < 0.5f ? 2 * x * x : 1 - Mathf.Pow(-2 * x + 2, 2) / 2,
        },
        new Ease {
            ID = "CubicIn",
            Name = "Cubic In",
            Get = (x) => x * x * x,
        },
        new Ease {
            ID = "CubicOut",
            Name = "Cubic Out",
            Get = (x) => 1 - Mathf.Pow(1 - x, 3),
        },
        new Ease {
            ID = "CubicInOut",
            Name = "Cubic In Out",
            Get = (x) => x < 0.5f ? 4 * x * x * x : 1 - Mathf.Pow(-2 * x + 2, 3) / 2,
        },
        new Ease {
            ID = "QuartIn",
            Name = "Quart In",
            Get = (x) => x * x * x * x,
        },
        new Ease {
            ID = "QuartOut",
            Name = "Quart Out",
            Get = (x) => 1 - Mathf.Pow(1 - x, 4),
        },
        new Ease {
            ID = "QuartInOut",
            Name = "Quart In Out",
            Get = (x) => x < 0.5f ? 8 * x * x * x * x : 1 - Mathf.Pow(-2 * x + 2, 4) / 2,
        },
    };
}

public abstract class IStoryboardable
{
    public Storyboard Storyboard = new Storyboard();

    public static TimestampType[] TimestampTypes = {};

    TimestampType[] tts;
    
    public IStoryboardable Get(float time) {
        if (tts == null) tts = (TimestampType[])this.GetType().GetField("TimestampTypes").GetValue(null);
        IStoryboardable obj = (IStoryboardable)this.MemberwiseClone();
        foreach(TimestampType tst in tts) try {
            List<Timestamp> sb = Storyboard.FromType(tst.ID);
            float value = tst.Get(this);
            foreach (Timestamp ts in sb) 
            {
                if (time >= ts.Time + ts.Duration) value = ts.Target;
                else if (time > ts.Time) 
                {
                    value = Mathf.Lerp(value, ts.Target, Array.Find(Ease.Eases, (x) => x.ID == ts.Easing).Get((time - ts.Time) / ts.Duration));
                    break;
                }
                else break;
            }
            tst.Set(obj, value);
        } catch (Exception e) {
            Debug.LogError(this.GetType() + " " + tst.ID + "\n" + e);
        }
        return obj;
    }

    Dictionary<string, float> currentValues;
    float currentTime;
    public void Advance (float time) 
    {
        if (tts == null) tts = (TimestampType[])this.GetType().GetField("TimestampTypes").GetValue(null);
        if (currentValues == null) 
        {
            currentValues = new Dictionary<string, float>();
            foreach (TimestampType tst in tts) 
            {
                currentValues.Add(tst.ID, tst.Get(this));
            }
        }
        foreach(TimestampType tst in tts) try {
            float value = currentValues[tst.ID];
            while (true) 
            {
                Timestamp ts = Storyboard.Timestamps.Find(x => x.ID == tst.ID);
                if (ts == null || (time < ts.Time && currentTime < ts.Time)) break;
                else if (time < ts.Time + ts.Duration)
                {
                    value = Mathf.Lerp(value, ts.Target, Array.Find(Ease.Eases, (x) => x.ID == ts.Easing).Get((time - ts.Time) / ts.Duration));
                    break;
                }
                else
                {
                    currentValues[tst.ID] = value = ts.Target;
                    Storyboard.Timestamps.Remove(ts);
                }
            }
            tst.Set(this, value);
        } catch (Exception e) {
            Debug.LogError(this.GetType() + " " + tst.ID + "\n" + e);
        }
        currentTime = time;
    }
}
