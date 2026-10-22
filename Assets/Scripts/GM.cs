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
    public bool isGameover { get; private set; }

    private void Awake() {
        if (ins != this) {
            Destroy(gameObject);
        }
    }

    private void Start() {
        FindObjectOfType<PlayerHealth>().onDeath += EndGame;
    }

    public void AddScore(int newScore) {
        if (!isGameover) {
            score += newScore;
            UIM.ins.UpdateScoreText(score);
        }
    }

    public void EndGame() {
        isGameover = true;
        UIM.ins.SetActiveGameoverUI(true);
    }
}