using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JudgeGroupHolder : MonoBehaviour
{
    public JudgeGroup group;

    // Start is called before the first frame update
    void Start()
    {
        Update();
    }

    // Update is called once per frame
    void Update()
    {
        group.Advance(ChartPlayer.main.TrackTime * 1000);
        transform.position = new Vector3(group.Position.x - 6, -group.Position.y + 3.375f, group.Position.z);
        transform.eulerAngles = Vector3.back * group.Rotation;
    }
}
