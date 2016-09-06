using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*******************************************************************************
    効果音（SE)管理クラス。オブジェクトにアタッチすることなくどこでも使用できる

    ＜使い方＞
    ◆再生：SoundControl.Instance.playSE(SE登録名)
    　SE登録名（キー）はstring。SE登録名（キー）はこのクラスの初期化で指定しておく

　　◆初期化
      private SoundControl()にて
      SEを予め登録しておく。 登録には、呼び出しに指定するキーとファイル名を指定する
      ・「SE登録名（キー）」
      ・「SEのファイル名（Resourcesフォルダからのパスを記述」

********************************************************************************/
public class SoundControl : MonoBehaviour
{
    private static SoundControl mInstance;
    GameObject soundPlayerObj;
    AudioSource audioSource;
    Dictionary<string, AudioClipInfo> audioClips = new Dictionary<string, AudioClipInfo>();

    /// <summary>
    /// privateコンストラクタ
    /// SEの登録処理を記述しておくこと
    /// </summary>
    private SoundControl()
    {
        // ++++++++++++++ audioClips ++++++++++++++++++++++++++++++
        // １．第１引数：登録名（キー）呼び出しの際のこのキーを指定する
        // ２．第２引数：AudioClipInfo

        // ++++++++++++++ AudioClipInfo ++++++++++++++++++++++++++++++
        // ３．第１引数：音声ファイル名。実際のファイル名（Resources以下のパスを含めて記述する)
        // ４．第２引数：登録名（キー）。上記の１と同じものを指定する
        // ５．第３引数：同時にならすSE数（通上は変更不要）
        // ６．第４引数：初期ボリューム（通上は変更不要）

        // ++++++++++++++ 使用例 ++++++++++++++++++++++++++++++
        //audioClips.Add("SE_OK", new AudioClipInfo("sound/ok", "SE_OK", 10, 2.0f));
    }
    public static SoundControl Instance
    {
        get
        {
            if (mInstance == null)
            {
                GameObject go = new GameObject("SoundControl");
                mInstance = go.AddComponent<SoundControl>();
            }
            return mInstance;
        }
    }
    class SEInfo
    {
        public int index;
        public float curTime;
        public float volume;
        public SEInfo(int _index, float _curtime, float _volume)
        {
            index = _index;
            curTime = _curtime;
            volume = _volume;
        }
    }

    // AudioClip information
    class AudioClipInfo
    {
        public SortedList<int, SEInfo> stockList = new SortedList<int, SEInfo>();
        public List<SEInfo> playingList = new List<SEInfo>();
        public int maxSENum = 10;        // 同時最大発音数
        public float initVolume = 1.0f;  // 1個目のボリューム
        public float attenuate = 0.0f;   // 合成時減衰率

        public string resourceName;
        public string name;
        public AudioClip clip;

        public AudioClipInfo(string resourceName, string name)
        {
            this.resourceName = resourceName;
            this.name = name;

        }

        public AudioClipInfo(string resourceName, string name, int maxSENum, float initVolume)
        {
            this.resourceName = resourceName;
            this.name = name;
            this.maxSENum = maxSENum;

            this.initVolume = initVolume;
            attenuate = calcAttenuateRate();

            // create stock list
            for (int i = 0; i < maxSENum; i++)
            {
                SEInfo seInfo = new SEInfo(i, 0.0f, initVolume * Mathf.Pow(attenuate, i));
                stockList.Add(seInfo.index, seInfo);
            }
        }

        float calcAttenuateRate()
        {
            float n = maxSENum;
            return NewtonMethod.run(
                delegate (float p)
                {
                    return (1.0f - Mathf.Pow(p, n)) / (1.0f - p) - 1.0f / initVolume;
                },
                delegate (float p)
                {
                    float ip = 1.0f - p;
                    float t0 = -n * Mathf.Pow(p, n - 1.0f) / ip;
                    float t1 = (1.0f - Mathf.Pow(p, n)) / ip / ip;
                    return t0 + t1;
                },
                0.9f, 100
            );
        }
    }

    public void Update()
    {
        // playing SE update

        foreach (AudioClipInfo info in audioClips.Values)
        {
            List<SEInfo> newList = new List<SEInfo>();

            foreach (SEInfo seInfo in info.playingList)
            {
                seInfo.curTime = seInfo.curTime - Time.deltaTime;
                if (seInfo.curTime > 0.0f)
                    newList.Add(seInfo);
                else
                    info.stockList.Add(seInfo.index, seInfo);
            }
            info.playingList = newList;
        }
    }
    public bool playSE(string seName)
    {
        if (audioClips.ContainsKey(seName) == false)
            return false; // not register

        AudioClipInfo info = audioClips[seName];

        // Load
        if (info.clip == null)
            info.clip = (AudioClip)Resources.Load(info.resourceName);

        if (soundPlayerObj == null)
        {
            soundPlayerObj = new GameObject("SoundControl");
            audioSource = soundPlayerObj.AddComponent<AudioSource>();
        }

        float len = info.clip.length / 10;

        if (info.stockList.Count > 0)
        {

            SEInfo seInfo = info.stockList.Values[0];
            seInfo.curTime = len;
            info.playingList.Add(seInfo);

            // remove from stock
            info.stockList.Remove(seInfo.index);

            // Play SE
            audioSource.PlayOneShot(info.clip, seInfo.volume);
            return true;
        }

        return true;
    }
    public void StopSE()
    {
        //Debug.Log("STOPSE:"+ Time.deltaTime);
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }
    public class NewtonMethod
    {
        public delegate float Func(float x);

        public static float run(Func func, Func derive, float initX, int maxLoop)
        {
            float x = initX;
            for (int i = 0; i < maxLoop; i++)
            {
                float curY = func(x);
                if (curY < 0.00001f && curY > -0.00001f)
                    break;
                x = x - curY / derive(x);
            }
            return x;
        }
    }
}