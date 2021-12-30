using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChartPlayer : MonoBehaviour
{

    public static ChartPlayer main;

    public PlayableSong _PlayableSong;
    public int ChartID;
    public Chart CurrentChart { get; private set; }

    [Space]
    public Transform MainCamera;
    public JudgeHolder JudgeSample;
    public HitHolder NormalHit;
    public HitHolder CatchHit;
    public HitHolder FlickHit;
    [Space]
    public AudioSource AudioPlayer;
    public float TrackTime;
    public float SyncThreshold;
    [Space]
    public TMP_Text SongNameText;
    public TMP_Text DifficultyText;
    public Slider ProgressBar;

    void Awake() {
        main = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        CurrentChart = Instantiate(_PlayableSong).Charts[ChartID];
        AudioPlayer.clip = _PlayableSong.Clip;
        AudioPlayer.Play();

        float width = (Screen.height / (float)Screen.width) * 960;
        float ratio = Mathf.Max(1, width / 930 * 16 / 9);
        float scale =  (Mathf.Tan(Mathf.PI * 1 / 3) * 3.375f);
        MainCamera.position = new Vector3(0, scale * 15 / 960, -scale * ratio);

        SongNameText.text = _PlayableSong.SongName;
        DifficultyText.text = CurrentChart.DifficultyLevel;
    }

    // Update is called once per frame
    void Update()
    {
        TrackTime += Time.deltaTime;
        if (TrackTime < 0) 
        {
            if (AudioPlayer.isPlaying) AudioPlayer.Pause(); 
        }
        else if (TrackTime < AudioPlayer.clip.length) 
        {
            if (!AudioPlayer.isPlaying) AudioPlayer.Play();
            if (Mathf.Abs(TrackTime - AudioPlayer.time) >= SyncThreshold) AudioPlayer.time = TrackTime;
        }
        ProgressBar.value = TrackTime / AudioPlayer.clip.length;

        while (true)
        {
            if (CurrentChart.Judges.Count > 0 && TrackTime * 1000 > CurrentChart.Judges[0].Offset) 
            {
                Judge j = CurrentChart.Judges[0];
                JudgeHolder jh = Instantiate(JudgeSample, transform);
                jh.judge = j;
                CurrentChart.Judges.RemoveAt(0);
            }
            else break;
        }
    }
}
