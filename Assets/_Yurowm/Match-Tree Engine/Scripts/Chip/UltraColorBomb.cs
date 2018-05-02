using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// The class is responsible for logic ColorBomb
[RequireComponent(typeof(Chip))]
public class UltraColorBomb : IBomb {

    Chip chip;
    int birth; // Event count at the time of birth SessionAssistant.main.eventCount
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
    }

    void Awake() {
        chip = GetComponent<Chip>();
        chip.chipType = "UltraColorBomb";
        anim = GetComponent<Animation>();
        birth = SessionAssistant.main.eventCount;
        AudioAssistant.Shot("CreateColorBomb");
    }

    // Coroutine destruction / activation
    IEnumerator DestroyChipFunction() {
        if (birth == SessionAssistant.main.eventCount) {
            chip.destroying = false;
            yield break;
        }


        SimpleChip[] chips = FindObjectsOfType<SimpleChip>();
        if (chips.Length == 0)
            yield break;
        chip.id = chips[Random.Range(0, chips.Length)].chip.id;
        chip.chipType = "SimpleChip";
        yield return StartCoroutine(UltraColorMixRoutine(chip));
    }

    public List<Chip> GetDangeredChips(List<Chip> stack) {
        if (stack.Contains(chip))
            return stack;

        stack.Add(chip);

        int sx = chip.parentSlot.slot.x;
        int sy = chip.parentSlot.slot.y;

        Slot s;

        for (int x = 0; x < FieldAssistant.main.field.width; x++) {
            for (int y = 0; y < FieldAssistant.main.field.height; y++) {
                if (y == sy && x == sx)
                    continue;
                s = Slot.GetSlot(x, y);
                if (s && s.GetChip() && s.GetChip().id == chip.id) {
                    stack = s.GetChip().GetDangeredChips(stack);
                }
            }
        }
        return stack;
    }

    #region Mixes
    void UltraColorMix(Chip secondary) {
        StartCoroutine(UltraColorMixRoutine(secondary));
    }

    IEnumerator UltraColorMixRoutine(Chip secondary) {
        matching = true;
        chip.destroyable = false;

        anim.Play("UltraColorBump");
        AudioAssistant.Shot("ColorBombCrush");

        int width = FieldAssistant.main.field.width;
        int height = FieldAssistant.main.field.height;

        int sx = chip.parentSlot.slot.x;
        int sy = chip.parentSlot.slot.y;

        Slot s;

        FieldAssistant.main.JellyCrush(sx, sy);

        Color color = Color.black;
        if (secondary.id == Mathf.Clamp(0, 5, secondary.id))
            color = Chip.colors[secondary.id];


        List<Chip> target = new List<Chip>();
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                if (y == sy && x == sx)
                    continue;
                s = Slot.GetSlot(x, y);
                if (s == null || s.GetChip() == null || s.GetChip() == secondary || s.GetChip().chipType != "SimpleChip")
                    continue;
                if (secondary.chipType == "UltraColorBomb" || s.GetChip().id == secondary.id) {
                    if (secondary.chipType != "SimpleChip" && secondary.chipType != "UltraColorBomb") {
                        Chip pu = FieldAssistant.main.AddPowerup(x, y, secondary.chipType);
                        pu.can_move = false;
                    }
                    yield return new WaitForSeconds(0.02f);
                    if (s.GetChip()) {
                        Lightning.CreateLightning(3, transform, s.GetChip().transform, color != Color.black ? color : Chip.colors[s.GetChip().id]);
                        target.Add(s.GetChip());
                    }
                }
            }
        }

        yield return new WaitForSeconds(0.1f);

        SessionAssistant.main.EventCounter();
        foreach(Chip t in target) {
            if (t.destroying)
                continue;
            t.SetScore(0.3f);
            FieldAssistant.main.BlockCrush(t.parentSlot.slot.x, t.parentSlot.slot.y, true);
            FieldAssistant.main.JellyCrush(t.parentSlot.slot.x, t.parentSlot.slot.y);
            t.DestroyChip();
            yield return new WaitForSeconds(0.02f);
        }

        yield return new WaitForSeconds(0.1f);

        FieldAssistant.main.JellyCrush(sx, sy);

        while (anim.isPlaying)
            yield return 0;

        matching = false;

        chip.ParentRemove();
        chip.HideChip(false);
    }
    #endregion
}