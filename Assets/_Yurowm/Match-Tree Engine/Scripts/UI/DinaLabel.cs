using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

[RequireComponent (typeof (Text))]
public class DinaLabel : MonoBehaviour {

    static public Dictionary<string, Word> words = new Dictionary<string,Word>();
    static bool initialized = false;

    Text label;
 
    public string text;
    public float updateDelay = 0;
    float lastTime = 0;
    public string[] keys;

	void Awake () {
        if (!initialized)
            Initialize();
        label = GetComponent<Text>();
	}

    static void Initialize() {
        words.Add("CurrentLevel", () => {return LevelProfile.main.level.ToString();});
        words.Add("CurrentScore", () => {return SessionAssistant.main.score.ToString();});
        words.Add("FirstStarScore", () => {return LevelProfile.main.firstStarScore.ToString();});
        words.Add("SecondStarScore", () => {return LevelProfile.main.secondStarScore.ToString();});
        words.Add("ThirdStarScore", () => {return LevelProfile.main.thirdStarScore.ToString();});
        words.Add("BestScore", () => {return ProfileAssistant.main.local_profile.GetScore(LevelProfile.main.level).ToString();});
        words.Add("BlockCount", () => {return GameObject.FindObjectsOfType<Block> ().Length.ToString();});
        words.Add("BlockCountTotal", () => {return SessionAssistant.main.blockCountTotal.ToString();});
        words.Add("JellyCount", () => {return GameObject.FindObjectsOfType<Jelly> ().Length.ToString();});
        words.Add("JellyCountTotal", () => {return SessionAssistant.main.jellyCountTotal.ToString();});
        words.Add("SugarCount", () => {return SessionAssistant.main.targetSugarDropsCount.ToString();});
        words.Add("SugarCountTotal", () => {return LevelProfile.main.targetSugarDropsCount.ToString();});
        words.Add("CurrentMoves", () => {return SessionAssistant.main.movesCount.ToString();});
        words.Add("CurrentTime", () => {return Utils.ToTimerFormat(SessionAssistant.main.timeLeft);});
        words.Add("WaitingStatus", () => {return Utils.waitingStatus;});
        words.Add("TargetModeName", () => {return SessionAssistant.main.GetTargetModeName();});
        words.Add("BoosterSelectedName", () => {return BerryStoreAssistant.main.GetItemByID(BoosterButton.boosterSelectedId).name;});
        words.Add("BoosterSelectedPackDescription", () => {return BerryStoreAssistant.main.GetItemByID(BoosterButton.boosterSelectedId).description;});
        words.Add("LivesCount", () => {return ProfileAssistant.main.local_profile["live"].ToString();});
        words.Add("ColorCollections", () => {
            string r = "";
            foreach (int i in SessionAssistant.main.countOfEachTargetCount)
                r += (r.Length > 0 ? "," : "") + Mathf.Max(0, i).ToString();
            return r;
        });
        words.Add("NextLiveTimer", () => {
            System.TimeSpan span = ProfileAssistant.main.local_profile.next_live_time - System.DateTime.Now;
            if (span.TotalSeconds <= 0) return "00:00";
            return string.Format("{0:00}:{1:00}", span.Minutes, span.Seconds);
        });

        initialized = true;
    }
	
	void OnEnable () {
        UpdateLabel();
	}

    void Update () {
        if (updateDelay <= 0) return;
        if (lastTime + updateDelay > Time.unscaledTime) return;
        lastTime = Time.unscaledTime;
        UpdateLabel();
    }

    void UpdateLabel() {
        object[] w = new object[keys.Length];
        for (int i = 0; i < keys.Length; i++) {
            if (words.ContainsKey(keys[i]))
                w[i] = words[keys[i]].Invoke();
            else
                w[i] = "";
        }
        label.text = string.Format(text, w);
    }

    public delegate string Word();
}
