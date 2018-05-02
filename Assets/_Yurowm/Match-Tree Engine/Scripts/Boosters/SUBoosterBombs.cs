﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SUBoosterBombs : MonoBehaviour {

	void Start () {
        string id = "bombs";
        ProfileAssistant.main.local_profile[id]--;
        SUBoosterButton.bag.Remove(id);
        StartCoroutine(BoosterRountine());
	}

    IEnumerator BoosterRountine() {
        int total = 4;
        int count = total;
        while (count > 0) {
            yield return StartCoroutine(Utils.WaitFor(SessionAssistant.main.CanIWait, 0.3f));
            FieldAssistant.main.AddPowerup("SimpleBomb");

            yield return StartCoroutine(Utils.WaitFor(SessionAssistant.main.CanIWait, 0.3f));
            FieldAssistant.main.AddPowerup("CrossBomb");

            count--;
            while (SessionAssistant.main.GetResource() * total >= count)
                yield return 0;
        }

        Destroy(gameObject);
    }

}
