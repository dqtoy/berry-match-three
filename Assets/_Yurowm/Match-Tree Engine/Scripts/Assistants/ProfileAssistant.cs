using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class ProfileAssistant : MonoBehaviour {

    public static ProfileAssistant main;


    public UserProfile local_profile;

    public void UnlockAllLevels() {
        local_profile = UserProfileUtils.ReadProfileFromDevice();
        local_profile.current_level = 9999;
        UserProfileUtils.WriteProfileOnDevice(local_profile);
        if (LevelMap.main)
            LevelMap.main.UpdateMapParameters();
    }

    public void ClearData() {
        local_profile = new UserProfile();
        PlayerPrefs.DeleteAll();
        UserProfileUtils.WriteProfileOnDevice(local_profile);
        if (LevelMap.main)
            LevelMap.main.UpdateMapParameters();
        if (Application.isPlaying)
            ItemCounter.RefreshAll();
    }

    void Awake() {
        main = this;

        UIAssistant.onShowPage += TryToSaveProfile;

        DebugPanel.AddDelegate("Clear Data", ClearData);
        DebugPanel.AddDelegate("Unlock all levels", UnlockAllLevels);
    }

    void Update() {
        if (local_profile == null)
            return;
        foreach (KeyValuePair<string, int> pair in local_profile.inventory)
            DebugPanel.Log(pair.Key, "Items", pair.Value);
    }

    void Start() {
        local_profile = UserProfileUtils.ReadProfileFromDevice();
        if (PlayerPrefs.GetInt("FirstPass") != 1) {
            StartCoroutine(FirstPass());
            return;
        }
    }

    IEnumerator FirstPass() {
        while (CPanel.uiAnimation > 0)
            yield return 0;
        yield return 0;
        
        local_profile["live"] = LiveSystemAssistant.lives_limit;
        local_profile["seed"] = 10000;

        LevelProfile.main = Level.all[1];
        FieldAssistant.main.StartLevel();
    }

    void TryToSaveProfile(string page) {
        UserProfileUtils.WriteProfileOnDevice(local_profile);
    }

    public void SaveUserInventory() {
        StartCoroutine(SaveUserInventoryRoutine());
    }

    IEnumerator SaveUserInventoryRoutine() {
        yield return 0;
        UserProfileUtils.WriteProfileOnDevice(local_profile);
    }
}
public class UserProfile {

    public System.DateTime lastSave = new DateTime();

    public string name = "";
    public int current_level = 1;
    public System.DateTime next_live_time = new DateTime();

    public Dictionary<string, int> inventory = new Dictionary<string, int>();
    public Dictionary<int, int> score = new Dictionary<int, int>();
    public string img = "";
    public override string ToString() {
        string report = "";
        report += "Name: " + name + ", ";
        report += "Current level: " + current_level + ", ";
        report += "Score count: " + score.Count + ", ";
        report += "Last save: " + lastSave.ToString() + ", ";
        report += "Next live time: " + next_live_time.ToShortDateString();
        return report;
    }

    public int GetScore(int level_number) {
        if (!score.ContainsKey(level_number))
            return 0;
        return score[level_number];
    }

    public void SetScore(int level_number, int value) {
        if (!score.ContainsKey(level_number))
            score.Add(level_number, 0);
        score[level_number] = Mathf.Max(score[level_number], value);
    }

    public bool IsEmpty() {
        if (current_level > 1)
            return false;
        foreach (int count in inventory.Values)
            if (count > 0)
                return false;
        return true;
    }

    public int this[string index] {
        get {
            if (inventory.ContainsKey(index))
                return inventory[index];
            return 0;
        }
        set {
            if (!inventory.ContainsKey(index))
                inventory.Add(index, 0);
            inventory[index] = value;
        }
    }

}

public class UserProfileUtils {
    public static void WriteProfileOnDevice(UserProfile profile) {
        PlayerPrefs.SetString("Profile_name", profile.name);
        PlayerPrefs.SetInt("Profile_current_level", profile.current_level);

        profile.lastSave = System.DateTime.UtcNow;
        PlayerPrefs.SetString("Profile_last_save", profile.lastSave.ToBinary().ToString());
        PlayerPrefs.SetString("Profile_next_live_time", profile.next_live_time.ToBinary().ToString());
        string inventory = string.Join(";", profile.inventory.Select(
                p => string.Format(
                    "{0}:{1}",
                    p.Key,
                    p.Value.ToString()
                    )).ToArray<string>());
        PlayerPrefs.SetString("Profile_inventory", inventory);
        string score = string.Join(";", profile.score.Select(
            p => string.Format(
                "{0}:{1}",
                p.Key,
                p.Value.ToString()
                )).ToArray<string>());
        PlayerPrefs.SetString("Profile_score", score);
        PlayerPrefs.Save();
    }

    public static UserProfile ReadProfileFromDevice() {
        UserProfile profile = new UserProfile();

        profile.name = PlayerPrefs.GetString("Profile_name");
        profile.current_level = PlayerPrefs.GetInt("Profile_current_level");
        if (profile.current_level == 0)
            profile.current_level = 1;

        string lastSave = PlayerPrefs.GetString("Profile_last_save");
        if (lastSave.Length > 0)
            profile.lastSave = System.DateTime.FromBinary(long.Parse(lastSave));

        string next_live_time = PlayerPrefs.GetString("Profile_next_live_time");
        if (next_live_time.Length > 0)
            profile.next_live_time = System.DateTime.FromBinary(long.Parse(next_live_time));

        string inventory = PlayerPrefs.GetString("Profile_inventory");
        if (inventory.Length > 0)
            profile.inventory = inventory
                 .Split(';')
                 .Select(s => s.Split(':'))
                 .ToDictionary(
                    p => p[0],
                    p => int.Parse(p[1])
                );

        string score = PlayerPrefs.GetString("Profile_score");
        if (score.Length > 0)
            profile.score = score
             .Split(';')
             .Select(s => s.Split(':'))
             .ToDictionary(
                p => int.Parse(p[0]),
                p => int.Parse(p[1])
            );
        return profile;
    }

    public static UserProfile GetLastVersion(UserProfile a, UserProfile b) {
        if (a.lastSave >= b.lastSave)
            return a;
        else
            return b;
    }
}
