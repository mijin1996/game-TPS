using UnityEngine;
using System.Linq;

public class GM : MonoBehaviour {
    public static GM ins {
        get {
            if (gm == null) {
                gm = FindObjectOfType<GM>();
            }
            return gm;
        }
    }

    private static GM gm;

    private int score = 0;
    public bool isGameOver { get; private set; }

    public bool isPaused { get; private set; } // 정지

    private void Awake() {
        if (ins != this) {
            Destroy(gameObject);
        }
    }

    private void Start() {
        FindObjectOfType<PlayerHealth>().onDeath += EndGame;
    }
    
    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape) && !isGameOver) {
            SetPause(!isPaused);
        }
    }

    public void AddScore(int newScore) {
        if (!isGameOver) {
            score += newScore;
            UIM.ins.UpdateScoreText(score);
        }
    }

    public void SetPause(bool pause) {
        isPaused = pause;

        var freezables = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<IFreeze>();
        foreach (IFreeze f in freezables) {
            f.Freeze(pause);                     // 고블린이든 수류탄이든 상관없이 전부
        }

        UIM.ins.SetActivePauseUI(pause);
        if (pause) SM.ins.PlayPauseSfx();
    }


    public void EndGame() {
        isGameOver = true;
        UIM.ins.SetActiveGameOverUI(true);
    }
}