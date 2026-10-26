using System;
using UnityEngine;

// 게임 상태(점수·게임오버·일시정지)만 관리한다
// UI·소리는 직접 부르지 않고, 이벤트로 "무슨 일이 생겼다"고 알리기만 한다
public class GM : MonoBehaviour {
    public static GM ins {
        get {
            if (gm == null) {
                gm = FindAnyObjectByType<GM>();
            }
            return gm;
        }
    }

    private static GM gm;

    // ---- 알림(이벤트) ----
    public static event Action<int> onScoreChanged;   // 점수가 바뀜 (새 점수)
    public static event Action onGameOver;            // 게임오버

    private int score = 0;
    public bool isGameOver { get; private set; }
    public bool isPaused { get; private set; }

    private void Awake() {
        if (ins != this) {
            Destroy(gameObject);
        }
    }

    // ---- 듣기(구독) ----
    private void OnEnable() {
        GoblinSpawn.onGoblinKilled += OnGoblinKilled;
        Coin.onCollected += AddScore;
        SettingsMenu.onOpenChanged += SetPause;       // 설정 창이 열리면 멈춤, 닫히면 진행
    }

    private void OnDisable() {
        GoblinSpawn.onGoblinKilled -= OnGoblinKilled;
        Coin.onCollected -= AddScore;
        SettingsMenu.onOpenChanged -= SetPause;
    }

    private void Start() {
        FindAnyObjectByType<PlayerHealth>().onDeath += EndGame;
        onScoreChanged?.Invoke(score);
    }

    private void OnGoblinKilled() {
        AddScore(100);
    }

    public void AddScore(int newScore) {
        if (!isGameOver) {
            score += newScore;
            onScoreChanged?.Invoke(score);
        }
    }

    // 게임 일시정지는 이 함수 하나로만 함
    // timeScale = 0이면 고블린 이동·애니메이션·물리·수류탄 타이머가 전부 멈춤
    public void SetPause(bool pause) {
        isPaused = pause;
        Time.timeScale = pause ? 0f : 1f;
    }

    public void EndGame() {
        isGameOver = true;
        onGameOver?.Invoke();
    }

    // 멈춘 채로 씬이 바뀌어도 다음 씬이 멈춰 있지 않게
    private void OnDestroy() {
        if (gm == this) Time.timeScale = 1f;
    }
}