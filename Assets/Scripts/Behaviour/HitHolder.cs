using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitHolder : MonoBehaviour
{
    public HitObject hit;
    public JudgeHolder parent;

    public SpriteRenderer sprite;

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
        if (tt < -200) StartCoroutine(Fade());
        sprite.color = new Color(1, 1, 1, 1 + tt / 200);
        if (tt < 0) tt = 0;
        transform.localPosition = parent.GetPosition(hit.Position);
        transform.position += new Vector3(hit.Velocity.x, -hit.Velocity.y, hit.Velocity.z) * tt / 1000;

        transform.eulerAngles = ChartPlayer.main.MainCamera.eulerAngles;
    }

    public IEnumerator Fade() {
        Destroy(gameObject);
        yield return null;
    }
}
