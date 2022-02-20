using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Chart
{
    public string DifficultyName = "Normal";
    public string DifficultyLevel = "6";

    public List<JudgeGroup> Groups = new List<JudgeGroup>();
    public List<Judge> Judges = new List<Judge>();

    public Chart() {
        
    }
}

[System.Serializable]
public class Judge : IStoryboardable {
    public int Offset = 0;
    public int Duration = 2000;
    public List<HitObject> Objects = new List<HitObject>();
    public JudgeType Type;
    public Vector3 Position;
    public float Length;
    public float Rotation;
    public float ArcAngle;
    public string Group;

    public float Opacity = 1;

    public enum JudgeType
    {
        Line,
        Arc,
        Curve
    }

    public new static TimestampType[] TimestampTypes = {
        new TimestampType {
            ID = "Position_X",
            Name = "Position X",
            Get = (x) => ((Judge)x).Position.x,
            Set = (x, a) => { ((Judge)x).Position.x = a; },
        },
        new TimestampType {
            ID = "Position_Y",
            Name = "Position Y",
            Get = (x) => ((Judge)x).Position.y,
            Set = (x, a) => { ((Judge)x).Position.y = a; },
        },
        new TimestampType {
            ID = "Position_Z",
            Name = "Position Z",
            Get = (x) => ((Judge)x).Position.z,
            Set = (x, a) => { ((Judge)x).Position.z = a; },
        },
        new TimestampType {
            ID = "Length",
            Name = "Length, Radius",
            Get = (x) => ((Judge)x).Length,
            Set = (x, a) => { ((Judge)x).Length = a; },
        },
        new TimestampType {
            ID = "Rotation",
            Name = "Rotation",
            Get = (x) => ((Judge)x).Rotation,
            Set = (x, a) => { ((Judge)x).Rotation = a; },
        },
        new TimestampType {
            ID = "ArcAngle",
            Name = "Arc Angle",
            Get = (x) => ((Judge)x).ArcAngle,
            Set = (x, a) => { ((Judge)x).ArcAngle = a; },
        },
        new TimestampType {
            ID = "Opacity",
            Name = "Opacity",
            Get = (x) => ((Judge)x).Opacity,
            Set = (x, a) => { ((Judge)x).Opacity = a; },
        },
    };
}

[System.Serializable]
public class JudgeGroup : IStoryboardable {
    public string Name;
    public Vector3 Position;
    public float Rotation;

    public new static TimestampType[] TimestampTypes = {
        new TimestampType {
            ID = "Position_X",
            Name = "Position X",
            Get = (x) => ((JudgeGroup)x).Position.x,
            Set = (x, a) => { ((JudgeGroup)x).Position.x = a; },
        },
        new TimestampType {
            ID = "Position_Y",
            Name = "Position Y",
            Get = (x) => ((JudgeGroup)x).Position.y,
            Set = (x, a) => { ((JudgeGroup)x).Position.y = a; },
        },
        new TimestampType {
            ID = "Position_Z",
            Name = "Position Z",
            Get = (x) => ((JudgeGroup)x).Position.z,
            Set = (x, a) => { ((JudgeGroup)x).Position.z = a; },
        },
        new TimestampType {
            ID = "Rotation",
            Name = "Rotation",
            Get = (x) => ((JudgeGroup)x).Rotation,
            Set = (x, a) => { ((JudgeGroup)x).Rotation = a; },
        },
    };
}

[System.Serializable]
public class HitObject : IStoryboardable {
    public int Offset = 0;
    public float AppearTime = 2000;
    public float Position;
    public Vector3 Velocity;
    public HitType Type;
    public CoordinateMode CoordinateMode;

    public float Opacity = 1;
    
    public enum HitType
    {
        Normal,
        Catch,
        Flick,
    }

    public List<RailTimestamp> Rail = new List<RailTimestamp>();

    public new static TimestampType[] TimestampTypes = {
        new TimestampType {
            ID = "Position",
            Name = "Position",
            Get = (x) => ((HitObject)x).Position,
            Set = (x, a) => { ((HitObject)x).Position = a; },
        },
        new TimestampType {
            ID = "Velocity_X",
            Name = "Velocity X",
            Get = (x) => ((HitObject)x).Velocity.x,
            Set = (x, a) => { ((HitObject)x).Velocity.x = a; },
        },
        new TimestampType {
            ID = "Velocity_Y",
            Name = "Velocity Y",
            Get = (x) => ((HitObject)x).Velocity.y,
            Set = (x, a) => { ((HitObject)x).Velocity.y = a; },
        },
        new TimestampType {
            ID = "Velocity_Z",
            Name = "Velocity Z",
            Get = (x) => ((HitObject)x).Velocity.z,
            Set = (x, a) => { ((HitObject)x).Velocity.z = a; },
        },
        new TimestampType {
            ID = "Opacity",
            Name = "Opacity",
            Get = (x) => ((HitObject)x).Opacity,
            Set = (x, a) => { ((HitObject)x).Opacity = a; },
        },
    };
}

[System.Serializable]
public class RailTimestamp : IStoryboardable {
    public int Offset = 0;
    public float Position;
    public Vector3 Velocity;
    

    public new static TimestampType[] TimestampTypes = {
        new TimestampType {
            ID = "Position",
            Name = "Position",
            Get = (x) => ((RailTimestamp)x).Position,
            Set = (x, a) => { ((RailTimestamp)x).Position = a; },
        },
        new TimestampType {
            ID = "Velocity_X",
            Name = "Velocity X",
            Get = (x) => ((RailTimestamp)x).Velocity.x,
            Set = (x, a) => { ((RailTimestamp)x).Velocity.x = a; },
        },
        new TimestampType {
            ID = "Velocity_Y",
            Name = "Velocity Y",
            Get = (x) => ((RailTimestamp)x).Velocity.y,
            Set = (x, a) => { ((RailTimestamp)x).Velocity.y = a; },
        },
        new TimestampType {
            ID = "Velocity_Z",
            Name = "Velocity Z",
            Get = (x) => ((RailTimestamp)x).Velocity.z,
            Set = (x, a) => { ((RailTimestamp)x).Velocity.z = a; },
        },
    };
}

public enum CoordinateMode {
    Local,
    Group,
    Global,
}
