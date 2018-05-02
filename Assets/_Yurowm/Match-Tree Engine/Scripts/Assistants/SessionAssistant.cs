using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GoogleMobileAds;
using GoogleMobileAds.Api;

// Game logic class
public class SessionAssistant : MonoBehaviour {

	public static SessionAssistant main;
    public int blockCountTotal;
    public int jellyCountTotal;

    public bool squareCombination = true;
    public List<Combinations> combinations = new List<Combinations>();
    public List<PowerUps> powerups = new List<PowerUps>();
    public List<Mix> mixes = new List<Mix>();

	List<Solution> solutions = new List<Solution>();

	public int animate = 0; // Number of current animation
	public int matching = 0; // Number of current matching process
	public int gravity = 0; // Number of current falling chips process
	public int lastMovementId;
	public int movesCount; // Number of remaining moves
	public int swapEvent; // After each successed swap this parameter grows by 1 
	public int[] countOfEachTargetCount = {0,0,0,0,0,0};// Array of counts of each color matches. Color ID is index.
	public float timeLeft; // Number of remaining time
	public int eventCount; // Event counter
	public int score = 0; // Current score
	public int[] colorMask = new int[6]; // Mask of random colors: color number - colorID
    public int targetSugarDropsCount;
    public int creatingSugarDropsCount;
	public bool isPlaying = false;
	public bool outOfLimit = false;
	public bool reachedTheTarget = false;
    public int creatingSugarTask = 0;
    public bool firstChipGeneration = false;

    public int stars;

    bool targetRoutineIsOver = false;
	bool limitationRoutineIsOver = false;
	
	bool wait = false;
	public static int scoreC = 10; // Score multiplier
	
	private InterstitialAd interstitial;

    // Use this for initialization
	
	private void RequestInterstitial()
    {
        string adUnitId = "ca-app-pub-9738690958049285/9382208653";
        // Create an interstitial.
        interstitial = new InterstitialAd(adUnitId);
        // Load an interstitial ad.
		AdRequest request = new AdRequest.Builder().Build();
        interstitial.LoadAd(request);
    }
	
	private void ShowInterstitial()
    {
        if (interstitial.IsLoaded())
        {
            interstitial.Show();
        }
    }
	
	void  Awake (){
		main = this;
        combinations.Sort((SessionAssistant.Combinations a, SessionAssistant.Combinations b) => {
            if (a.priority < b.priority)
                return -1;
            if (a.priority > b.priority)
                return 1;
            return 0;
        });

        DebugPanel.AddDelegate("Complete the level", () => {
            if (isPlaying) {
                reachedTheTarget = true;
                movesCount = 0;
                timeLeft = 0;
                score = LevelProfile.main.thirdStarScore;
            }
        });

        DebugPanel.AddDelegate("Fail the level", () => {
            if (isPlaying) {
                reachedTheTarget = false;
                limitationRoutineIsOver = true;
                movesCount = 0;
                timeLeft = 0;
            }
        });
    }

    void Update () {
        DebugPanel.Log("Matching", "Session", matching);
        DebugPanel.Log("Gravity", "Session", gravity);
        DebugPanel.Log("Animate", "Session", animate);
    }

    void OnApplicationPause(bool pauseStatus) {
        if (isPlaying)
            UIAssistant.main.ShowPage("Pause");

    }

    // Reset variables
    public static void Reset() {
        main.animate = 0;
        main.gravity = 0;
        main.matching = 0;

        main.stars = 0;

        main.eventCount = 0;
        main.lastMovementId = 0;
        main.swapEvent = 0;
        main.score = 0;
        main.firstChipGeneration = true;

        main.isPlaying = false;
        main.movesCount = LevelProfile.main.moveCount;
        main.timeLeft = LevelProfile.main.duration;
        main.countOfEachTargetCount = new int[] { 0, 0, 0, 0, 0, 0};
        main.creatingSugarTask = 0;


        main.reachedTheTarget = false;
		main.outOfLimit = false;

		main.targetRoutineIsOver = false;
		main.limitationRoutineIsOver = false;

        AnimationAssistant.main.iteraction = true;
	}

	// Add extra moves (booster effect)
	public void AddExtraMoves () {
		if (!isPlaying) return;
        if (ProfileAssistant.main.local_profile["move"] == 0) return;
        ProfileAssistant.main.local_profile["move"]--;
        ItemCounter.RefreshAll();
        movesCount += 5;
        Continue();
	}

    // Add extra time (booster effect)
    public void AddExtraTime () {
		if (!isPlaying) return;
        if (ProfileAssistant.main.local_profile["time"] == 0) return;
        ProfileAssistant.main.local_profile["time"]--;
        ItemCounter.RefreshAll();
		timeLeft += 15;
        Continue();
	}

    public void MixChips(Chip a, Chip b) {
        Mix mix = Mix.FindMix(a.chipType, b.chipType);
        if (mix == null)
            return;
        Chip target = null;
        Chip secondary = null;
        if (a.chipType == mix.pair.a) {
            target = a;
            secondary = b;
        }
        if (b.chipType == mix.pair.a) {
            target = b;
            secondary = a;
        }


        if (target == null) {
            Debug.LogError("It can't be mixed, because there is no target chip");
            return;
        }
        b.parentSlot.SetChip(target);
        secondary.HideChip(false);
        target.SendMessage(mix.function, secondary);
    }

	// Resumption of gameplay
	public void Continue () {
        UIAssistant.main.ShowPage("Field");
		wait = false;
	}

	// Starting next level
	public void PlayNextLevel() {
		if (CPanel.uiAnimation > 0) return;
        StartCoroutine(PlayLevelRoutine(LevelProfile.main.level + 1));
	}

    IEnumerator PlayLevelRoutine(int level) {
        yield return StartCoroutine(QuitCoroutine());
        while (CPanel.uiAnimation > 0)
            yield return 0;
        Level.LoadLevel(level);
    }

	// Restart the current level
	public void RestartLevel() {
		if (CPanel.uiAnimation > 0) return;
        StartCoroutine(PlayLevelRoutine(LevelProfile.main.level));
	}

	// Starting a new game session
	public void StartSession(FieldTarget sessionType, Limitation limitationType) {
		RequestInterstitial();
		
		StopAllCoroutines (); // Ending of all current coroutines

        isPlaying = true;

        blockCountTotal = GameObject.FindObjectsOfType<Block>().Length;

		switch (limitationType) { // Start corresponding coroutine depending on the limiation mode
			case Limitation.Moves: StartCoroutine(MovesLimitation()); break;
			case Limitation.Time: StartCoroutine(TimeLimitation());break;
		}

		switch (sessionType) { // Start corresponding coroutine depending on the target level
            case FieldTarget.None:
                StartCoroutine(TargetSession(() => {
                    return true;
                }));
                break;
            case FieldTarget.Jelly:
                jellyCountTotal = FindObjectsOfType<Jelly>().Length;
                Jelly.need_to_update_potentials = true;
                StartCoroutine(TargetSession(() => {
                    return FindObjectsOfType<Jelly>().Length == 0;
                }));
                break;
            case FieldTarget.Block:
                StartCoroutine(TargetSession(() => {
                    return FindObjectsOfType<Block>().Length == 0;
                }));
                break;
            case FieldTarget.Color:
                for (int i = 0; i < LevelProfile.main.countOfEachTargetCount.Length; i++)
                    countOfEachTargetCount[colorMask[i]] = LevelProfile.main.countOfEachTargetCount[i];
                StartCoroutine(TargetSession(() => {
                    foreach (int c in countOfEachTargetCount)
                        if (c > 0)
                            return false;
                    return true;
                }));
                break;
            case FieldTarget.SugarDrop:
                targetSugarDropsCount = 0;
                creatingSugarDropsCount = LevelProfile.main.targetSugarDropsCount;
                StartCoroutine(TargetSession(() => {
                    return targetSugarDropsCount >= LevelProfile.main.targetSugarDropsCount && GameObject.FindObjectsOfType<SugarChip>().Length == 0;
                }));
                break;
        }

		StartCoroutine (BaseSession()); // Base routine of game session
		StartCoroutine (ShowingHintRoutine()); // Coroutine display hints
		StartCoroutine (ShuffleRoutine()); // Coroutine of mixing chips at the lack moves
		StartCoroutine (FindingSolutionsRoutine()); // Coroutine of finding a solution and destruction of existing combinations of chips
		StartCoroutine (IllnessRoutine()); // Coroutine of Weeds logic

        GameCamera.main.ShowField();
        UIAssistant.main.ShowPage("Field");
    }

	IEnumerator BaseSession () {
		while (!limitationRoutineIsOver && !targetRoutineIsOver) {
			yield return 0;
		}

		// Checking the condition of losing
		if (!reachedTheTarget) {
			yield return StartCoroutine(GameCamera.main.HideFieldRoutine());
			FieldAssistant.main.RemoveField();
			ShowLosePopup();
			yield break;
		}

        AnimationAssistant.main.iteraction = false;

        yield return new WaitForSeconds(0.2f);
        UIAssistant.main.ShowPage("TargetIsReached");
        AudioAssistant.Shot("TargetIsReached");
        yield return StartCoroutine(Utils.WaitFor(() => CPanel.uiAnimation == 0, 0.4f));
        UIAssistant.main.ShowPage("Field");
        		
		// Conversion of the remaining moves into bombs and activating them
		yield return StartCoroutine(BurnLastMovesToPowerups());
		
		yield return StartCoroutine(Utils.WaitFor(CanIWait, 1f));
		
		// Ending the session, showing win popup
		yield return StartCoroutine(GameCamera.main.HideFieldRoutine());
		FieldAssistant.main.RemoveField();
        StartCoroutine(YouWin());
	}

    IEnumerator TargetSession(System.Func<bool> func) {
        reachedTheTarget = false;
        int score_tatget = LevelProfile.main.target == FieldTarget.None ? LevelProfile.main.thirdStarScore : LevelProfile.main.firstStarScore;
        while (!outOfLimit && (score < score_tatget || !func.Invoke())) {
            yield return new WaitForSeconds(0.33f);
            if (GetResource() == 0 && score >= LevelProfile.main.firstStarScore && func.Invoke())
                reachedTheTarget = true;
        }

        if (score >= LevelProfile.main.firstStarScore && func.Invoke())
            reachedTheTarget = true;

        targetRoutineIsOver = true;
    }


	#region Limitation Modes Logic	

	// Game session with limited time
	IEnumerator TimeLimitation() {
		outOfLimit = false;

		// Waiting until the rules of the game are carried out
		while (timeLeft > 0 && !targetRoutineIsOver) {
            if (Time.timeScale == 1)
                timeLeft -= 1f;
            timeLeft = Mathf.Max(timeLeft, 0);
            if (timeLeft <= 5)
                AudioAssistant.Shot("TimeWarrning");
            yield return new WaitForSeconds(1f);

            if (timeLeft <= 0) {
                do
                    yield return StartCoroutine(Utils.WaitFor(CanIWait, 1f));
                while (FindObjectsOfType<Chip>().ToList().Find(x => x.destroying) != null);
                if (!reachedTheTarget) {
                    UIAssistant.main.ShowPage("NoMoreMoves");
                    AudioAssistant.Shot("NoMoreMoves");
                    wait = true;
                    // Pending the decision of the player - lose or purchase additional time
                    while (wait)
                        yield return new WaitForSeconds(0.5f);

                }
			}
		}

		yield return StartCoroutine(Utils.WaitFor(CanIWait, 1f));

		if (timeLeft <= 0) outOfLimit = true;

		limitationRoutineIsOver = true;
	}

	// Game session with limited count of moves
	IEnumerator MovesLimitation() {
		outOfLimit = false;
		
		// Waiting until the rules of the game are carried out
        while (movesCount > 0) {
            yield return new WaitForSeconds(1f);
            if (movesCount <= 0) {
                do
				    yield return StartCoroutine(Utils.WaitFor(CanIWait, 1f));
                while (FindObjectsOfType<Chip>().ToList().Find(x => x.destroying) != null);
                if (!reachedTheTarget) {
                    UIAssistant.main.ShowPage("NoMoreMoves");
                    AudioAssistant.Shot("NoMoreMoves");
                    wait = true;
                    // Pending the decision of the player - lose or purchase additional time
                    while (wait)
                        yield return new WaitForSeconds(0.5f);

                }
			}
		}

		yield return StartCoroutine(Utils.WaitFor(CanIWait, 1f));
		
		outOfLimit = true;
		limitationRoutineIsOver = true;
	}

	#endregion


	// Coroutine of searching solutions and the destruction of existing combinations
	IEnumerator FindingSolutionsRoutine () {
		List<Solution> solutions;
        int id = 0;

		while (true) {
            if (isPlaying) {

                yield return StartCoroutine(Utils.WaitFor(() => lastMovementId > id, 0.2f));

                id = lastMovementId;
                solutions = FindSolutions();
                if (solutions.Count > 0) {
                    MatchSolutions(solutions);
                } else
                    yield return StartCoroutine(Utils.WaitFor(CanIMatch, 0.1f));
            } else
                yield return 0;
		}
	}

	// Coroutine of conversion of the remaining moves into bombs and activating them
	IEnumerator BurnLastMovesToPowerups ()
	{
		yield return StartCoroutine(CollapseAllPowerups ());

		int newBombs = 0;
		switch (LevelProfile.main.limitation) {
			case Limitation.Moves: newBombs = movesCount; break;
			case Limitation.Time: newBombs = Mathf.CeilToInt(timeLeft / 3); break;
		}

		int count;
		while (newBombs > 0) {
			count = Mathf.Min(newBombs, 8);
			while (count > 0) {
				count --;
				newBombs --;
				movesCount --;
				timeLeft -= 3;
                timeLeft = Mathf.Max(timeLeft, 0);
				switch (Random.Range(0, 2)) {
				case 0: FieldAssistant.main.AddPowerup("SimpleBomb"); break;
				case 1: FieldAssistant.main.AddPowerup("CrossBomb"); break;
				}
				yield return new WaitForSeconds(0.1f);
			}
            yield return StartCoroutine(Utils.WaitFor(CanIWait, 0.5f));
			yield return StartCoroutine(CollapseAllPowerups ());
		}
	}

	// Weeds logic
	IEnumerator IllnessRoutine () {
        Weed.lastWeedCrush = swapEvent;
		Weed.seed = 0;

        int last_swapEvent = swapEvent;

		yield return new WaitForSeconds(1f);

        while (Weed.all.Count > 0) {
            yield return StartCoroutine(Utils.WaitFor(() => swapEvent > last_swapEvent, 0.1f));
            last_swapEvent = swapEvent;
            yield return StartCoroutine(Utils.WaitFor(CanIWait, 0.1f));
            if (Weed.lastWeedCrush < swapEvent) {
                Weed.seed += swapEvent - Weed.lastWeedCrush;
                Weed.lastWeedCrush = swapEvent;
            }
            Weed.Grow();
		}
	}

	// Ending the session at user
	public void Quit() {
		if (CPanel.uiAnimation > 0) return;
		StopAllCoroutines ();
		StartCoroutine(QuitCoroutine());
		}

	// Coroutine of ending the session at user
	IEnumerator QuitCoroutine() {
        isPlaying = false;
        
        if (GameCamera.main.playing) {
            UIAssistant.main.ShowPage("Field");

		    yield return StartCoroutine(GameCamera.main.HideFieldRoutine());
        
		    FieldAssistant.main.RemoveField();
            
            while (CPanel.uiAnimation > 0)
                yield return 0;
        }

        Utils.waitingStatus = "Cleaning";

        UIAssistant.main.ShowPage("Loading");

        while (CPanel.uiAnimation > 0)
            yield return 0;

        yield return new WaitForSeconds(0.5f);
        UIAssistant.main.ShowPage("LevelList");
		ShowInterstitial();
	}

	// Coroutine of activation all bombs in playing field
	IEnumerator CollapseAllPowerups () {
		yield return StartCoroutine(Utils.WaitFor(CanIWait, 0.5f));
		List<Chip> powerUp = FindPowerups ();
		while (powerUp.Count > 0) {
            powerUp = powerUp.FindAll(x => !x.destroying);
            if (powerUp.Count > 0) {
			    SessionAssistant.main.EventCounter();
			    powerUp[Random.Range(0, powerUp.Count)].DestroyChip();
            }
			yield return StartCoroutine(Utils.WaitFor(CanIWait, 0.5f));
			powerUp = FindPowerups ();
		}
		yield return StartCoroutine(Utils.WaitFor(CanIWait, 0.5f));
	}

	// Finding bomb function
	List<Chip> FindPowerups ()
	{
		return FindObjectsOfType<IBomb>().Select(x => x.GetComponent<Chip>()).ToList();
    }

	// Showing lose popup
	void ShowLosePopup ()
	{
		AudioAssistant.Shot ("YouLose");
        isPlaying = false;
        GameCamera.main.HideField();
        UIAssistant.main.ShowPage("YouLose");
	}

	// Showing win popup
	IEnumerator YouWin ()
	{
		AudioAssistant.Shot ("YouWin");
        PlayerPrefs.SetInt("FirstPass", 1);
		isPlaying = false;

        ProfileAssistant.main.local_profile["live"]++;

        if (ProfileAssistant.main.local_profile.current_level == LevelProfile.main.level) {
            if (Level.all.ContainsKey(ProfileAssistant.main.local_profile.current_level + 1)) {
                ProfileAssistant.main.local_profile.current_level++;
            }
        }

        ProfileAssistant.main.local_profile.SetScore(LevelProfile.main.level, score);

        GameCamera.main.HideField();
        
        yield return 0;

        while (CPanel.uiAnimation > 0)
            yield return 0;

        yield return 0;

        UIAssistant.main.ShowPage("YouWin");
        
        UserProfileUtils.WriteProfileOnDevice(ProfileAssistant.main.local_profile);
    }

    public void ShowYouWinPopup() {
        bool bestScore = false;

        if (ProfileAssistant.main.local_profile.GetScore(LevelProfile.main.level) < score) {
            ProfileAssistant.main.local_profile.SetScore(LevelProfile.main.level, score);
            bestScore = true;
        }

        UIAssistant.main.ShowPage(bestScore ? "YouWinBestScore" : "YouWin");
    }

	// Conditions to start animation
	public bool CanIAnimate (){
		return isPlaying && gravity == 0 && matching == 0;
	}

	// Conditions to start matching
	public bool CanIMatch (){
        return isPlaying && animate == 0 && gravity == 0;
	}

	// Conditions to start falling chips
	public bool CanIGravity (){
        return isPlaying && ((animate == 0 && matching == 0) || gravity > 0);
	}

	// Conditions for waiting player's actions
	public bool CanIWait (){
        return isPlaying && animate == 0 && matching == 0 && gravity == 0;
	}

	void  AddSolution ( Solution s  ){
		solutions.Add(s);
	}

	// Event counter
	public void  EventCounter (){
		eventCount ++;
	}

	// Search function possible moves
	public List<Move> FindMoves (){
		List<Move> moves = new List<Move>();
		if (!FieldAssistant.main.gameObject.activeSelf) return moves;
		if (LevelProfile.main == null) return moves;

		int x;
		int y;
		int width = LevelProfile.main.width;
		int height = LevelProfile.main.height;
		Move move;
		Solution solution;
        int potential;
		SlotForChip slot;
		string chipTypeA = "";
		string chipTypeB = "";
		
		// horizontal
		for (x = 0; x < width - 1; x++)
		for (y = 0; y < height; y++) {
			if (!FieldAssistant.main.field.slots[x,y]) continue;
			if (!FieldAssistant.main.field.slots[x+1,y]) continue;
			if (FieldAssistant.main.field.blocks[x,y] > 0) continue;
			if (FieldAssistant.main.field.blocks[x+1,y] > 0) continue;
			if (FieldAssistant.main.field.chips[x,y] == FieldAssistant.main.field.chips[x+1,y]) continue;
			if (FieldAssistant.main.field.chips[x,y] == -1 && FieldAssistant.main.field.chips[x+1,y] == -1) continue;
			if (FieldAssistant.main.field.wallsV[x,y]) continue;
			move = new Move();
			move.fromX = x;
			move.fromY = y;
			move.toX = x + 1;
			move.toY = y;
			AnalizSwap(move);

            Solution solutionA = null;
            Solution solutionB = null;
            
            slot = Slot.GetSlot(move.fromX, move.fromY).GetComponent<SlotForChip>();
			chipTypeA = slot.chip == null ? "SimpleChip" : slot.chip.chipType;
			
            potential = 0;

            solution = slot.MatchAnaliz();
			if (solution != null) {
                solutionA = solution;
                potential = solution.potential;
			}

            solution = slot.MatchSquareAnaliz();
            if (solution != null && potential < solution.potential) {
                solutionA = solution;
                potential = solution.potential;
            }

            move.potencial += potential;

            slot = Slot.GetSlot(move.toX, move.toY).GetComponent<SlotForChip>();
			chipTypeB = slot.chip == null ? "SimpleChip" : slot.chip.chipType;

            potential = 0;
            solution = slot.MatchAnaliz();
            if (solution != null) {
                solutionB = solution;
                potential = solution.potential;
            }

            solution = slot.MatchSquareAnaliz();
            if (solution != null && potential < solution.potential) {
                solutionB = solution;
                potential = solution.potential;
            }

            move.potencial += potential;

            if (solutionA != null && solutionB != null)
                move.solution = solutionA.potential >= solutionB.potential ? solutionA : solutionB;
            else
                move.solution = solutionA != null ? solutionA : solutionB;

			AnalizSwap(move);
            if (Mix.ContainsThisMix(chipTypeA, chipTypeB))
                move.potencial += 100;
			if (move.potencial > 0 || (chipTypeA != "SimpleChip" &&  chipTypeB != "SimpleChip")) 
				moves.Add(move);		
		}
		
		// vertical
		for (x = 0; x < width; x++)
		for (y = 0; y < height - 1; y++) {
			if (!FieldAssistant.main.field.slots[x,y]) continue;
			if (!FieldAssistant.main.field.slots[x,y+1]) continue;
			if (FieldAssistant.main.field.blocks[x,y] > 0) continue;
			if (FieldAssistant.main.field.blocks[x,y+1] > 0) continue;
			if (FieldAssistant.main.field.chips[x,y] == FieldAssistant.main.field.chips[x,y+1]) continue;
			if (FieldAssistant.main.field.chips[x,y] == -1 && FieldAssistant.main.field.chips[x,y+1] == -1) continue;
			if (FieldAssistant.main.field.wallsH[x,y]) continue;
			move = new Move();
			move.fromX = x;
			move.fromY = y;
			move.toX = x;
			move.toY = y + 1;

            AnalizSwap(move);

            Solution solutionA = null;
            Solution solutionB = null;

            slot = Slot.GetSlot(move.fromX, move.fromY).GetComponent<SlotForChip>();
            chipTypeA = slot.chip == null ? "SimpleChip" : slot.chip.chipType;

            potential = 0;

            solution = slot.MatchAnaliz();
            if (solution != null) {
                solutionA = solution;
                potential = solution.potential;
            }

            solution = slot.MatchSquareAnaliz();
            if (solution != null && potential < solution.potential) {
                solutionA = solution;
                potential = solution.potential;
            }

            move.potencial += potential;

            slot = Slot.GetSlot(move.toX, move.toY).GetComponent<SlotForChip>();
            chipTypeB = slot.chip == null ? "SimpleChip" : slot.chip.chipType;

            potential = 0;
            solution = slot.MatchAnaliz();
            if (solution != null) {
                solutionB = solution;
                potential = solution.potential;
            }

            solution = slot.MatchSquareAnaliz();
            if (solution != null && potential < solution.potential) {
                solutionB = solution;
                potential = solution.potential;
            }

            move.potencial += potential;

            if (solutionA != null && solutionB != null)
                move.solution = solutionA.potential >= solutionB.potential ? solutionA : solutionB;
            else
                move.solution = solutionA != null ? solutionA : solutionB;

            AnalizSwap(move);
            if (Mix.ContainsThisMix(chipTypeA, chipTypeB))
                move.potencial += 100;
			if (move.potencial > 0 || (chipTypeA != "SimpleChip" &&  chipTypeB != "SimpleChip")) 
				moves.Add(move);		
            }

		return moves;
	}

    //public List<Move> FindMoves() {
    //    List<Move> moves = new List<Move>();
    //    if (!FieldAssistant.main.gameObject.activeSelf)
    //        return moves;
    //    if (LevelProfile.main == null)
    //        return moves;

    //    int x;
    //    int y;
    //    int width = LevelProfile.main.width;
    //    int height = LevelProfile.main.height;
    //    Move move;
    //    Solution solution;
    //    SlotForChip slot;
    //    string chipTypeA = "";
    //    string chipTypeB = "";

    //    // horizontal
    //    for (x = 0; x < width - 1; x++)
    //        for (y = 0; y < height; y++) {
    //            if (!FieldAssistant.main.field.slots[x, y])
    //                continue;
    //            if (!FieldAssistant.main.field.slots[x + 1, y])
    //                continue;
    //            if (FieldAssistant.main.field.blocks[x, y] > 0)
    //                continue;
    //            if (FieldAssistant.main.field.blocks[x + 1, y] > 0)
    //                continue;
    //            if (FieldAssistant.main.field.chips[x, y] == FieldAssistant.main.field.chips[x + 1, y])
    //                continue;
    //            if (FieldAssistant.main.field.chips[x, y] == -1 && FieldAssistant.main.field.chips[x + 1, y] == -1)
    //                continue;
    //            if (FieldAssistant.main.field.wallsV[x, y])
    //                continue;
    //            move = new Move();
    //            move.fromX = x;
    //            move.fromY = y;
    //            move.toX = x + 1;
    //            move.toY = y;
    //            AnalizSwap(move);
    //            slot = FieldAssistant.main.GetSlot(move.fromX, move.fromY).GetComponent<SlotForChip>();
    //            chipTypeA = slot.chip == null ? "SimpleChip" : slot.chip.chipType;
    //            solution = slot.MatchAnaliz();
    //            if (solution != null) {
    //                move.potencial += solution.potencial;
    //                move.solution = solution;
    //            }
    //            slot = FieldAssistant.main.GetSlot(move.toX, move.toY).GetComponent<SlotForChip>();
    //            solution = slot.MatchAnaliz();
    //            chipTypeB = slot.chip == null ? "SimpleChip" : slot.chip.chipType;
    //            if (solution != null && (move.potencial < solution.potencial || move.solution == null))
    //                move.solution = solution;
    //            if (solution != null)
    //                move.potencial += solution.potencial;
    //            AnalizSwap(move);
    //            if (BombMixEffect.ContainsPair(chipTypeA, chipTypeB))
    //                move.potencial += 100;
    //            if (move.potencial != 0 || (chipTypeA != "SimpleChip" && chipTypeB != "SimpleChip"))
    //                moves.Add(move);
    //        }

    //    // vertical
    //    for (x = 0; x < width; x++)
    //        for (y = 0; y < height - 1; y++) {
    //            if (!FieldAssistant.main.field.slots[x, y])
    //                continue;
    //            if (!FieldAssistant.main.field.slots[x, y + 1])
    //                continue;
    //            if (FieldAssistant.main.field.blocks[x, y] > 0)
    //                continue;
    //            if (FieldAssistant.main.field.blocks[x, y + 1] > 0)
    //                continue;
    //            if (FieldAssistant.main.field.chips[x, y] == FieldAssistant.main.field.chips[x, y + 1])
    //                continue;
    //            if (FieldAssistant.main.field.chips[x, y] == -1 && FieldAssistant.main.field.chips[x, y + 1] == -1)
    //                continue;
    //            if (FieldAssistant.main.field.wallsH[x, y])
    //                continue;
    //            move = new Move();
    //            move.fromX = x;
    //            move.fromY = y;
    //            move.toX = x;
    //            move.toY = y + 1;

    //            AnalizSwap(move);
    //            slot = FieldAssistant.main.GetSlot(move.fromX, move.fromY).GetComponent<SlotForChip>();
    //            chipTypeA = slot.chip == null ? "SimpleChip" : slot.chip.chipType;
    //            solution = slot.MatchAnaliz();
    //            if (solution != null) {
    //                move.potencial += solution.potencial;
    //                move.solution = solution;
    //            }
    //            slot = FieldAssistant.main.GetSlot(move.toX, move.toY).GetComponent<SlotForChip>();
    //            solution = slot.MatchAnaliz();
    //            chipTypeB = slot.chip == null ? "SimpleChip" : slot.chip.chipType;
    //            if (solution != null && (move.potencial < solution.potencial || move.solution == null))
    //                move.solution = solution;
    //            if (solution != null)
    //                move.potencial += solution.potencial;
    //            AnalizSwap(move);
    //            if (BombMixEffect.ContainsPair(chipTypeA, chipTypeB))
    //                move.potencial += 100;
    //            if (move.potencial != 0 || (chipTypeA != "SimpleChip" && chipTypeB != "SimpleChip"))
    //                moves.Add(move);
    //        }

    //    return moves;
    //}

	// change places 2 chips in accordance with the move for the analysis of the potential of this move
	void  AnalizSwap (Move move){
		SlotForChip slot;
		Chip fChip = GameObject.Find("Slot_" + move.fromX + "x" + move.fromY).GetComponent<Slot>().GetChip();
		Chip tChip = GameObject.Find("Slot_" + move.toX + "x" + move.toY).GetComponent<Slot>().GetChip();
		if (!fChip || !tChip) return;
		slot = tChip.parentSlot;
		fChip.parentSlot.SetChip(tChip);
		slot.SetChip(fChip);
	}

    void MatchSolutions(List<Solution> solutions) {
        if (!isPlaying) return;
        solutions.Sort(delegate(Solution x, Solution y) {
            if (x.potential == y.potential)
                return 0;
            else if (x.potential > y.potential)
                return -1;
            else
                return 1;
        });

        int width = FieldAssistant.main.field.width;
        int height = FieldAssistant.main.field.height;
        
        bool[,] mask = new bool[width,height];
        string key;
        Slot slot;

        for (int x = 0; x < width; x++) 
            for (int y = 0; y < height; y++) {
                mask[x, y] = false;
                key = x.ToString() + "_" + y.ToString();
                if (Slot.all.ContainsKey(key)) {
                    slot = Slot.all[key];
                    if (slot.GetChip())
                        mask[x, y] = true;
                }
            }

        List<Solution> final_solutions = new List<Solution>();

        bool breaker;
        foreach (Solution s in solutions) {
            breaker = false;
            foreach (Chip c in s.chips) {
                if (!mask[c.parentSlot.slot.x, c.parentSlot.slot.y]) {
                    breaker = true;
                    break;
                }
            }
            if (breaker)
                continue;

            final_solutions.Add(s);

            foreach (Chip c in s.chips)
                mask[c.parentSlot.slot.x, c.parentSlot.slot.y] = false;
        }

        foreach (Solution solution in final_solutions) {
            EventCounter();
        
            Jelly jelly;
            int puID = -1;

            foreach (Chip chip in solution.chips) {
                if (chip.id == solution.id || chip.id == 10) {
                    if (!chip.parentSlot)
                        continue;


                    slot = chip.parentSlot.slot;



                    if (!chip.IsMatcheble())
                        break;

                    if (chip.movementID > puID)
                        puID = chip.movementID;
                    chip.SetScore(Mathf.Pow(2, solution.count - 3) / solution.count);
                    if (!slot.GetBlock())
                        FieldAssistant.main.BlockCrush(slot.x, slot.y, true);
                    chip.DestroyChip();
                    jelly = slot.GetJelly();
                    if (jelly)
                        jelly.JellyCrush();
                    }
                }

            solution.chips.Sort(delegate(Chip a, Chip b) {
                return a.movementID > b.movementID ? -1 : a.movementID == b.movementID ? 0 : 1;
            });

            breaker = false;
            foreach (Combinations combination in combinations) {
                if (combination.square && !solution.q)
                    continue;
                if (!combination.square) {
                    if (combination.horizontal && !solution.h)
                        continue;
                    if (combination.vertical && !solution.v)
                        continue;
                    if (combination.minCount > solution.count)
                        continue;
                }

                // For additional logic
                switch (combination.tag) {
                    case "Tag1":
                        Debug.Log("Tag 1 combination..."); break;
                    case "Tag2":
                        Debug.Log("Tag 2 combination..."); break;
                    case "Tag3":
                        Debug.Log("Tag 3 combination..."); break;
                }

                foreach (Chip ch in solution.chips)
                    if (ch.chipType == "SimpleChip") {
                        FieldAssistant.main.GetNewBomb(ch.parentSlot.slot.x, ch.parentSlot.slot.y, combination.powerup, ch.parentSlot.slot.transform.position, solution.id);
                        breaker = true;
                        break;
                    }
                if (breaker)
                    break;
            }
        }
    }
	
	public int GetMovementID (){
		lastMovementId ++;
		return lastMovementId;
	}
	
	public int GetMovesCount (){
		return movesCount;
	}

    public float GetResource() {
        switch (LevelProfile.main.limitation) {
            case Limitation.Moves:
                return 1f * movesCount / LevelProfile.main.moveCount;
            case Limitation.Time:
                return 1f * timeLeft / LevelProfile.main.duration;
        }
        return 1f;
    }

	// Coroutine of call mixing chips in the absence of moves
	IEnumerator ShuffleRoutine () {
		int shuffleOrder = 0;
		float delay = 1;
		while (true) {
			yield return StartCoroutine(Utils.WaitFor(CanIWait, delay));
            List<Chip> chips = new List<Chip>(FindObjectsOfType<Chip>());
			if (eventCount > shuffleOrder && !targetRoutineIsOver && chips.Find(x => x.destroying) == null) {
				shuffleOrder = eventCount;
				yield return StartCoroutine(Shuffle(false));
			}
		}
	}


    void RawShuffle(List<Slot> slots) {
        EventCounter();
        int targetID;
        for (int j = 0; j < slots.Count; j++) {
            targetID = Random.Range(0, j - 1);
            if (!slots[j].GetChip() || !slots[targetID].GetChip())
                continue;
            if (slots[j].GetChip().chipType == "SugarChip" || slots[targetID].GetChip().chipType == "SugarChip")
                continue;
            AnimationAssistant.main.SwapTwoItemNow(slots[j].GetChip(), slots[targetID].GetChip());
        }
    }


	// Coroutine of mixing chips
	public IEnumerator Shuffle (bool f) {
		bool force = f;

		List<Move> moves = FindMoves();
		if (moves.Count > 0 && !force)
			yield break;
		if (!isPlaying)
			yield break;

		isPlaying = false;

        List<Slot> slots = new List<Slot>(Slot.all.Values);
        
		Dictionary<Slot, Vector3> positions = new Dictionary<Slot, Vector3> ();
        foreach (Slot slot in slots)
			positions.Add (slot, slot.transform.position);

        float t = 0;
        while (t < 1) {
            t += Time.unscaledDeltaTime * 3;
            Slot.folder.transform.localScale = Vector3.one * Mathf.Lerp(1, 0.6f, EasingFunctions.easeInOutQuad(t));
            Slot.folder.transform.eulerAngles = Vector3.forward * Mathf.Lerp(0, Mathf.Sin(Time.unscaledTime * 40) * 3, EasingFunctions.easeInOutQuad(t));

            yield return 0;
        }


        if (f || moves.Count == 0) {
            f = false;
            RawShuffle(slots);
        }

        moves = FindMoves();
		List<Solution> solutions = FindSolutions ();

        int itrn = 0;
        int targetID;
        while (solutions.Count > 0 || moves.Count == 0) {
            if (itrn > 100) {
                ShowLosePopup();
                yield break;
            }
            if (solutions.Count > 0) {
                for (int s = 0; s < solutions.Count; s++) {
                    targetID = Random.Range(0, slots.Count - 1);
                    if (slots[targetID].GetChip() && slots[targetID].GetChip().chipType != "SugarChip" && slots[targetID].GetChip().id != solutions[s].id)
                        AnimationAssistant.main.SwapTwoItemNow(solutions[s].chips[Random.Range(0, solutions[s].chips.Count - 1)], slots[targetID].GetChip());
                }
            } else 
                RawShuffle(slots);

            moves = FindMoves();
            solutions = FindSolutions();
            itrn++;
            Slot.folder.transform.eulerAngles = Vector3.forward * Mathf.Sin(Time.unscaledTime * 40) * 3;

            yield return 0;
        }

        t = 0;
        AudioAssistant.Shot("Shuffle");
        while (t < 1) {
            t += Time.unscaledDeltaTime * 3;
            Slot.folder.transform.localScale = Vector3.one * Mathf.Lerp(0.6f, 1, EasingFunctions.easeInOutQuad(t));
            Slot.folder.transform.eulerAngles = Vector3.forward * Mathf.Lerp(Mathf.Sin(Time.unscaledTime * 40) * 3, 0, EasingFunctions.easeInOutQuad(t));
            yield return 0;
        }

        Slot.folder.transform.localScale = Vector3.one;
        Slot.folder.transform.eulerAngles = Vector3.zero;

		isPlaying = true;
	}

	// Function of searching possible solutions
	List<Solution> FindSolutions() {
		List<Solution> solutions = new List<Solution> ();
		Solution zsolution;
		foreach(SlotForChip slot in GameObject.FindObjectsOfType<SlotForChip>()) {
			zsolution = slot.MatchAnaliz();
			if (zsolution != null) solutions.Add(zsolution);
			zsolution = slot.MatchSquareAnaliz();
			if (zsolution != null) solutions.Add(zsolution);
		}
		return solutions;
	}

	// Coroutine of showing hints
	IEnumerator ShowingHintRoutine () {
		int hintOrder = 0;
		float delay = 5;

        yield return new WaitForSeconds(1f);

        while (!reachedTheTarget) {
            while (!isPlaying)
                yield return 0;
			yield return StartCoroutine(Utils.WaitFor(CanIWait, delay));
			if (eventCount > hintOrder) {
				hintOrder = eventCount;
				ShowHint();
			}
		}
	}

	// Showing random hint
	void  ShowHint (){
		if (!isPlaying) return;
		List<Move> moves = FindMoves();

        foreach (Move move in moves) {
            Debug.DrawLine(Slot.GetSlot(move.fromX, move.fromY).transform.position, Slot.GetSlot(move.toX, move.toY).transform.position, Color.red, 10);
        
        }


		if (moves.Count == 0) return;

		Move bestMove = moves[ Random.Range(0, moves.Count) ];

		if (bestMove.solution == null) return;

        foreach (Chip chip in bestMove.solution.chips)
            chip.Flashing(eventCount);
	}

    [System.Serializable]
    public class PowerUps {
        public string name = "";
        public string contentName = "";
        public bool color = true;
        public int levelEditorID = 0;
        public string levelEditorName = "";
    }

    [System.Serializable]
    public class Combinations {
        public int priority = 0;
        public string powerup;
        public bool horizontal = true;
        public bool vertical = true;
        public bool square = false;
        public int minCount = 4;
        public string tag = "";

    }

	// Class with information of solution
	public class Solution {
		//   T
		//   T
		// LLXRR  X - center of solution
		//   B
		//   B

		public int count; // count of chip combination (count = T + L + R + B + X)
		public int potential; // potential of solution
		public int id; // ID of chip color
        public List<Chip> chips = new List<Chip>();

		// center of solution
		public int x;
		public int y;

		public bool v; // is this solution is vertical?  (v = L + R + X >= 3)
		public bool h; // is this solution is horizontal? (h = T + B + X >= 3)
        public bool q;
        //public int posV; // number on right chips (posV = R)
        //public int negV; // number on left chips (negV = L)
        //public int posH; // number on top chips (posH = T)
        //public int negH; // number on bottom chips (negH = B)
	}

	// Class with information of move
	public class Move {
		//
		// A -> B
		//

		// position of start chip (A)
		public int fromX;
		public int fromY;
		// position of target chip (B)
		public int toX;
		public int toY;

		public Solution solution; // solution of this move
		public int potencial; // potential of this move
	}

    public string GetTargetModeName() {
        switch (LevelProfile.main.target) {
            case FieldTarget.None:
                return "Target Score";
            case FieldTarget.Block:
                return "Block Crush";
            case FieldTarget.Color:
                return "Color Collection";
            case FieldTarget.Jelly:
                return "Jelly Crush";
            case FieldTarget.SugarDrop:
                return "Sugar Drop";
        }
        return "Unknown";
    }

    [System.Serializable]
    public class Mix {
        public Pair pair = new Pair("", "");

        public string function;

        public bool Compare(string _a, string _b) {
            return pair == new Pair(_a, _b);
        }

        public static bool ContainsThisMix(string _a, string _b) {
            return main.mixes.Find(x => x.Compare(_a, _b)) != null;
        }

        public static Mix FindMix(string _a, string _b) {
            return main.mixes.Find(x => x.Compare(_a, _b));
        }
    }
}
