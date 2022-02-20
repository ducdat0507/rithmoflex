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
    [Header("Main")]
    public Transform MainCamera;
    public AudioSource AudioPlayer;
    [Space]
    [Header("Objects")]
    public JudgeGroupHolder GroupSample;
    public JudgeHolder JudgeSample;
    public HitHolder NormalHit;
    public HitHolder CatchHit;
    public HitHolder FlickHit;
    [Space]
    public TMP_Text SongNameText;
    public TMP_Text SongArtistText;
    public TMP_Text DifficultyText;
    public Image ProgressBar;
    public TMP_Text ScoreText;
    public TMP_Text ComboText;
    [Space]
    [Header("Data")]
    public float TrackTime;
    public float SyncThreshold;
    public bool Autoplay;
    public float RawScore;
    public float MaxRawScore;
    public int Combo;

    Dictionary<string, JudgeGroupHolder> Groups = new Dictionary<string, JudgeGroupHolder>();

    void Awake() {
        main = this;
        System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
    }

    // Start is called before the first frame update
    void Start()
    {
        CurrentChart = Instantiate(_PlayableSong).Charts[ChartID];
        AudioPlayer.clip = _PlayableSong.Clip;
        AudioPlayer.Play();

        float width = (Screen.height / (float)Screen.width) * 960;
        float ratio = Mathf.Max(1, width / 960 * 16 / 9);
        float scale =  (Mathf.Tan(Mathf.PI * 1 / 3) * 3.375f);
        MainCamera.position = new Vector3(0, 0, -scale * ratio);

        SongNameText.text = _PlayableSong.SongName;
        SongArtistText.text = _PlayableSong.SongArtist;
        DifficultyText.text = CurrentChart.DifficultyLevel;

        foreach (JudgeGroup jg in CurrentChart.Groups) {
            JudgeGroupHolder jgh = Instantiate(GroupSample, transform);
            jgh.group = jg;
            Groups[jg.Name] = jgh;
        }

        foreach (Judge j in CurrentChart.Judges) {
            foreach (HitObject ho in j.Objects) {
                MaxRawScore += ho.Type == HitObject.HitType.Catch ? 1 : 3;
            }
        }
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
        ProgressBar.fillAmount = TrackTime / AudioPlayer.clip.length;

        while (true)
        {
            if (CurrentChart.Judges.Count > 0 && TrackTime * 1000 > CurrentChart.Judges[0].Offset) 
            {
                Judge j = CurrentChart.Judges[0];
                Transform p = transform;
                if (j.Group != "" && Groups.ContainsKey(j.Group)) p = Groups[j.Group].transform;
                JudgeHolder jh = Instantiate(JudgeSample, p);
                jh.judge = j;
                CurrentChart.Judges.RemoveAt(0);
            }
            else break;
        }
    }

    public void Score(float multi, float acc) {
        RawScore += multi * acc;
        Combo = acc > 0 ? Combo + 1 : 0;

        ScoreText.text = (RawScore / MaxRawScore * 1e6).ToString("0000000") + "<size=10><b>ppm";
        ComboText.text = Combo > 0 ? Combo.ToString("0") : "";
    }
}
