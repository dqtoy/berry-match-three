using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SUBoosterLadybird : MonoBehaviour {
    void Start() {
        string id = "ladybird";
        ProfileAssistant.main.local_profile[id]--;
        SUBoosterButton.bag.Remove(id);
        StartCoroutine(BoosterRountine());
    }

    IEnumerator BoosterRountine() {
        int total = 10;
        int count = total;
        float angle;
        while (count > 0) {
            yield return StartCoroutine(Utils.WaitFor(SessionAssistant.main.CanIWait, 0.3f));
            GameObject ladybird = ContentAssistant.main.GetItem("Ladybird" + Chip.chipTypes[Random.Range(0, 6)]);
            angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            ladybird.transform.position = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * 6;
            SessionAssistant.main.EventCounter();
            ladybird.GetComponent<Chip>().DestroyChip();
            count--;
            while (SessionAssistant.main.GetResource() * total >= count)
                yield return 0;
        }

        Destroy(gameObject);
    }
}
