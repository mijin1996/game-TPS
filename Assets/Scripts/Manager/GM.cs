using UnityEngine;

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

    private void Awake() {
        if (ins != this) {
            Destroy(gameObject);
        }
    }

    private void Start() {
        FindObjectOfType<PlayerHealth>().onDeath += EndGame;
    }

    public void AddScore(int newScore) {
        if (!isGameOver) {
            score += newScore;
            UIM.ins.UpdateScoreText(score);
        }
    }

    public void EndGame() {
        isGameOver = true;
        UIM.ins.SetActiveGameOverUI(true);
    }
}