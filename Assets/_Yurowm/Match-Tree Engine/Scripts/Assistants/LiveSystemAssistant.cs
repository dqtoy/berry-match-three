using UnityEngine;
using System.Collections;
using System;

public class LiveSystemAssistant : MonoBehaviour {

    public static readonly TimeSpan refilling_time = new TimeSpan(0, 30, 0);
    public static readonly int lives_limit = 5;
	void Start () {
        StartCoroutine(LiveSystemRoutine());
	}

    IEnumerator LiveSystemRoutine() {
        while (ProfileAssistant.main.local_profile == null)
            yield return 0;

        while (true) {
            while (ProfileAssistant.main.local_profile["live"] < lives_limit && ProfileAssistant.main.local_profile.next_live_time <= DateTime.Now) {
                ProfileAssistant.main.local_profile["live"]++;
                ProfileAssistant.main.local_profile.next_live_time += refilling_time;
                ItemCounter.RefreshAll();
            }
            if (ProfileAssistant.main.local_profile["live"] >= lives_limit)
                ProfileAssistant.main.local_profile.next_live_time = DateTime.Now + refilling_time;
            yield return new WaitForSeconds(1);
        }
    }
}
