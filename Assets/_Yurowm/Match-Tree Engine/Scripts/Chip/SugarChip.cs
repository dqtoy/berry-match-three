using UnityEngine;
using System.Collections;

public class SugarChip : MonoBehaviour {

    public Chip chip;
    bool mMatching = false;
    public bool matching {
        set {
            if (value == mMatching)
                return;
            mMatching = value;
            if (mMatching)
                SessionAssistant.main.matching++;
            else
                SessionAssistant.main.matching--;
        }
        get {
            return mMatching;
        }
    }

    bool _live = false;
    public static int live_count = 0;
    public bool live {
        set {
            if (value == _live)
                return;
            _live = value;
            if (_live)
                live_count++;
            else
                live_count--;
        }
        get {
            return _live;
        }
    }

    void OnDestroy() {
        matching = false;
        live = false;
    }

    void Awake() {
        chip = GetComponent<Chip>();
        chip.chipType = "SugarChip";
        live = true;
        AudioAssistant.Shot("SugarCreate");
    }

    void Update() {
        if (chip.destroying) return;
        if (chip.move) return;
        if (!chip.parentSlot) return;
        if (!SessionAssistant.main.CanIWait()) return;
        if (chip.parentSlot.slot.sugarDropSlot) {
            chip.destroyable = true;
            SessionAssistant.main.targetSugarDropsCount++;
            chip.DestroyChip();
        }
    }

    // Coroutine destruction / activation
    IEnumerator DestroyChipFunction() {

        matching = true;
        AudioAssistant.Shot("SugarCrush");

        yield return new WaitForSeconds(0.2f);
        matching = false;

        chip.ParentRemove();


        float velocity = 0;
        Vector3 impuls = new Vector3(Random.Range(-3f, 3f), Random.Range(1f, 5f), 0);
        impuls += chip.impulse;
        chip.impulse = Vector3.zero;
        foreach (SpriteRenderer sprite in GetComponentsInChildren<SpriteRenderer>())
            sprite.sortingLayerName = "UI";


        float rotationSpeed = Random.Range(-30f, 30f);
        float growSpeed = Random.Range(0.2f, 0.8f);

        while (transform.position.y > -10) {
            velocity += Time.deltaTime * 20;
            velocity = Mathf.Min(velocity, 40);
            transform.position += impuls * Time.deltaTime * transform.localScale.x;
            transform.position -= Vector3.up * Time.deltaTime * velocity * transform.localScale.x;
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
            transform.localScale += Vector3.one * growSpeed * Time.deltaTime;
            yield return 0;
        }

        Destroy(gameObject);
    }
}
