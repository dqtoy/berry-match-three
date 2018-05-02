using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

// The class is responsible for logic CrossBomb
[RequireComponent(typeof(Chip))]
public class Ladybird : IBomb {
    public static List<Slot> targetStack = new List<Slot>();

    Chip chip;
    int birth; // Event count at the time of birth SessionAssistant.main.eventCount
    int branchCount;

    public Transform directionSprite;

    Slot target;
    public string seed = "";
    bool targeting = false;

    Animation anim;

    bool mMatching = false;
    bool matching {
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
    void OnDestroy() {
        matching = false;
        if (target != null && targetStack.Contains(target)) {
            targetStack.Remove(target);
        }

    }

    void Awake() {
        anim = GetComponent<Animation>();
        chip = GetComponent<Chip>();
        chip.chipType = "Ladybird";
        birth = SessionAssistant.main.eventCount;
        AudioAssistant.Shot("LadybirdCreate");
    }

    IEnumerator Targeting() {
        targeting = true;
        yield return new WaitForSeconds(0.3f);
        int eventCount = 0;
        while (true) {
            if (!targeting)
                yield break;
            if (target)
                Debug.DrawLine(transform.position, target.transform.position, Color.cyan);
            if (!target) {
                if (target) {
                    targetStack.Remove(target);
                    target = null;
                }
                if (SessionAssistant.main.gravity == 0) {
                    target = FindTarget();
                    targetStack.Add(target);
                    eventCount = SessionAssistant.main.eventCount;
                }
            }
            yield return 0;
        }
    
    }

    // Coroutine destruction / activation
    IEnumerator DestroyChipFunction() {
        if (birth == SessionAssistant.main.eventCount) {
            chip.destroying = false;
            yield break;
        }

        matching = true;

        anim.Play("LadybirdBump");
        AudioAssistant.Shot("LadybirdCreate");

        foreach (SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>())
            sr.sortingLayerName = "Foreground";

        if (chip.parentSlot) {
            int sx = chip.parentSlot.slot.x;
            int sy = chip.parentSlot.slot.y;

            FieldAssistant.main.BlockCrush(sx, sy, false);
            FieldAssistant.main.JellyCrush(sx, sy);
        }


        chip.ParentRemove();

        float speed = 1;
        float angular_speed = 5;
        float frequency = Random.Range(2f, 8f);

        matching = false;
        chip.gravity = false;

        StartCoroutine(Targeting());

        float angle = 0;
        float distance;
        while (true) {
            if (target)
                distance = Mathf.Clamp((transform.position - target.transform.position).magnitude / 10, 0, 1);
            else
                distance = 0.2f;
            speed += 4 * Time.deltaTime;
            speed = Mathf.Min(speed, Mathf.Lerp(2, 8, distance));
            angular_speed += 5 * Time.deltaTime;

            if (target) {
                angular_speed = Mathf.Min(angular_speed, Mathf.Lerp(2, 30, distance));
                angle = Vector3.Angle(directionSprite.up, target.transform.position - transform.position);
                if (Vector3.Angle(-directionSprite.right, target.transform.position - transform.position) > 90)
                    angle *= -1;
            } else {
                angular_speed = Mathf.Min(angular_speed, 2);
                angle = Mathf.MoveTowards(angle, 3, Time.deltaTime);
            }
            directionSprite.Rotate(0, 0, (angle + 60f * Mathf.Sin(Time.time * frequency)) * angular_speed * Time.deltaTime);
            //directionSprite.Rotate(0, 0, angle * angular_speed * Time.deltaTime);
            transform.position += directionSprite.up * speed * Time.deltaTime;

            if (target && (target.transform.position - transform.position).sqrMagnitude <= 0.1f)
                break;

            yield return 0;
    
        }

        SessionAssistant.main.EventCounter();

        Slot _target = target;
        if (seed != "") {
            Chip pu = FieldAssistant.main.AddPowerup(_target.x, _target.y, seed);
            pu.can_move = false;
            _target.SetChip(pu);
            yield return 0;
            SessionAssistant.main.EventCounter();
        }
        Crush(_target.x, _target.y);

        AudioAssistant.Shot("LadybirdCrush");
        SessionAssistant.main.EventCounter();

        targeting = false;

        targetStack.Remove(target);

        yield return new WaitForSeconds(0.1f);

        anim.Play("LadybirdCrush");
        AnimationAssistant.main.Explode(transform.position, 5, 7);

        while (anim.isPlaying)
            yield return 0;
        
        Destroy(gameObject);
    }

    Slot FindTarget() {
        switch (LevelProfile.main.target) {
            case FieldTarget.Block:
                Block[] blocks = GameObject.FindObjectsOfType<Block>();
                if (blocks.Length > 0)
                    return blocks[Random.Range(0, blocks.Length)].slot;
                break;
            case FieldTarget.Color:
            case FieldTarget.Jelly:
            case FieldTarget.None:
            case FieldTarget.SugarDrop:
                List<Chip> chips = new List<Chip>(GameObject.FindObjectsOfType<Chip>());
                int potential = -1;
                int z = 0;
                List<Chip> resultChip = new List<Chip>();
                foreach (Chip c in chips) {
                    if (c.chipType == "Ladybird" || c.chipType == "SugarChip")
                        continue;
                    if (c.destroying || !c.parentSlot || targetStack.Contains(c.parentSlot.slot))
                        continue;
                    z = c.GetPotencial();
                    if (potential == z)
                        resultChip.Add(c);
                    if (potential < z) {
                        resultChip.Clear();
                        resultChip.Add(c);
                        potential = z;
                    }
                }
                if (resultChip.Count > 0)
                    return resultChip[Random.Range(0, resultChip.Count)].parentSlot.slot;
                break;
        }

        Slot[] targets = GameObject.FindObjectsOfType<Slot>();
        Slot result = targets[Random.Range(0, targets.Length)];
        while ((chip.parentSlot && result == chip.parentSlot.slot) || result.GetChip() == null) {
            result = targets[Random.Range(0, targets.Length)];
        }
        return result;
    }

    bool Crush(int x, int y) {
        Slot s = Slot.GetSlot(x, y);
        FieldAssistant.main.BlockCrush(x, y, false);
        FieldAssistant.main.JellyCrush(x, y);
        if (s && s.GetChip()) {
            s.GetChip().SetScore(3f);
            s.GetChip().DestroyChip();
            AnimationAssistant.main.Explode(s.transform.position, 3, 7);
        }
        return x >= 0 && y >= 0 && x < FieldAssistant.main.field.width && y < FieldAssistant.main.field.height;
    }

    public List<Chip> GetDangeredChips(List<Chip> stack) {
        if (stack.Contains(chip))
            return stack;

        stack.Add(chip);
        return stack;
    }

    #region Mixes
    public void LadybirdsMix(Chip secondary) {
        StartCoroutine(LadybirdsMixRoutine(secondary));
    }

    IEnumerator LadybirdsMixRoutine(Chip secondary) {
        matching = true;
        chip.destroyable = false;

        yield return 0;

        List<Chip> ladies = new List<Chip>();
        int count = 5;
        if (secondary.chipType == "Ladybird")
            count = 10;
        for (int i = 0; i <= count; i++) {
            Chip l = ContentAssistant.main.GetItem<Chip>("Ladybird" + Chip.chipTypes[Random.value > 0.5f ? chip.id : secondary.id]);
            l.transform.position = chip.parentSlot.transform.position;
            l.transform.localScale = Vector3.one;
            l.transform.SetParent(Slot.folder);
            l.transform.FindChild("LadybirdBody").rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
            if (secondary.chipType != "Ladybird")
                l.GetComponent<Ladybird>().seed = secondary.chipType;
            ladies.Add(l);
        }

        chip.Minimize();

        foreach (Chip l in ladies) {
            Animation a = l.GetComponent<Animation>();
            while (a.isPlaying)
                yield return 0;
        }
        SessionAssistant.main.EventCounter();


        matching = false;
        chip.HideChip(false);
    }
    #endregion

}