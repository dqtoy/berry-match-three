using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Jelly element on playing field
public class Jelly : MonoBehaviour {

	public int level = 1; // Level of jelly. From 1 to 3. Each "JellyCrush"-call fall level by one. If it becomes zero, this jelly will be destroyed.
	public Sprite[] sprites; // Images of jellies of different levels. The size of the array must be equal to 3
	SpriteRenderer sr;
    bool destroying = false;
    Animation anim;
    public string crush_effect;

    static Jelly main;
    void Update() {
        if (main == null) {
            main = this;
            StartCoroutine(UpdatePotentialsRoutine());
        }
        if (main != this)
            return;
    }
    
    void Start() {
		sr = GetComponent<SpriteRenderer>();
        sr.sprite = sprites[level - 1];
        anim = GetComponent<Animation>();
        AnimationSpeed speed = GetComponentInChildren<AnimationSpeed>();
        speed.speed = Random.Range(0.4f, 0.8f);
        speed.offset = Random.Range(0f, 1f);
	}

	// Crush block funtion
	public void JellyCrush (){
        if (level == 1) {
            AudioAssistant.Shot("JellyCrush");
            StartCoroutine(DestroyingRoutine());
			return;
		}
        level--;
        anim.Play("JellyCrush");
        AudioAssistant.Shot("JellyrHit");
		sr.sprite = sprites[level-1];
	}

    IEnumerator DestroyingRoutine() {
        destroying = true;
        need_to_update_potentials = true;

        GameObject o = ContentAssistant.main.GetItem(crush_effect);
        o.transform.position = transform.position;

        anim.Play("BlockDestroy");
        while (anim.isPlaying) {
            yield return 0;
        }

        Destroy(gameObject);
    }

    static Dictionary<Slot, int> jelly_potentials = new Dictionary<Slot, int>();
    public static bool need_to_update_potentials = false;
    IEnumerator UpdatePotentialsRoutine() {
        while (true) {
            while (!need_to_update_potentials)
                yield return 0;
            yield return StartCoroutine(Utils.WaitFor(SessionAssistant.main.CanIWait, 0.1f));
            UpdatePotentials();
        }
    }

    static void UpdatePotentials() {
        if (Application.isEditor) {
            jelly_potentials.Clear();
            foreach (Slot slot in Slot.all.Values)
                jelly_potentials.Add(slot, 0);
            foreach (Jelly jelly in GameObject.FindObjectsOfType<Jelly>()) {
                if (jelly.destroying)
                    continue;
                foreach (Slot slot in new List<Slot>(jelly_potentials.Keys)) {
                    if (jelly_potentials[slot] == 1000)
                        continue;
                    if (slot.transform.position == jelly.transform.position)
                        jelly_potentials[slot] = 1000;
                    else
                        jelly_potentials[slot] = Mathf.Max(jelly_potentials[slot],
                            Mathf.CeilToInt(9 - Mathf.Abs(jelly.transform.position.x - slot.transform.position.x) -
                            Mathf.Abs(jelly.transform.position.y - slot.transform.position.y)));
                }
            }
            need_to_update_potentials = false;
        }
    }

    public static int GetPotention(Slot slot) {
        if (Application.isEditor) {
            if (!jelly_potentials.ContainsKey(slot))
                UpdatePotentials();
            return jelly_potentials[slot];
        }
        else
            return slot.GetJelly() ? 100 : 0;
    }
}