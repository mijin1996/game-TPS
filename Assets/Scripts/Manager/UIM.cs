using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// 화면(글자·게임오버 창) 표시만 담당한다
// 다른 스크립트가 보내는 알림(이벤트)을 듣고 알아서 화면을 바꾼다
public class UIM : MonoBehaviour {
    public Text ammoText;
    public Text scoreText;
    public Text waveText;
    public GameObject gameOverUI;

    public Text wavePopText;
    private float fadeTime = 0.5f;
    private float showTime = 1.5f;

    // ---- 듣기(구독) ----
    private void OnEnable() {
        GM.onScoreChanged += UpdateScoreText;
        GM.onGameOver += ShowGameOverUI;
        GoblinSpawn.onWaveStarted += OnWaveStarted;
        GoblinSpawn.onEnemyCountChanged += UpdateWaveText;
        Gun.onAmmoChanged += UpdateAmmoText;
    }

    private void OnDisable() {
        GM.onScoreChanged -= UpdateScoreText;
        GM.onGameOver -= ShowGameOverUI;
        GoblinSpawn.onWaveStarted -= OnWaveStarted;
        GoblinSpawn.onEnemyCountChanged -= UpdateWaveText;
        Gun.onAmmoChanged -= UpdateAmmoText;
    }

    private void UpdateAmmoText(int magAmmo, int remainAmmo) {
        if (ammoText != null) ammoText.text = magAmmo + "/" + remainAmmo;
    }

    private void UpdateScoreText(int newScore) {
        if (scoreText == null) return;
        scoreText.text = "Score : " + newScore;
        if (newScore > 0) {
            scoreText.GetComponent<IPop>()?.PopPop();
            scoreText.GetComponent<IBlink>()?.BlinkBlink();
        }
    }

    private void UpdateWaveText(int wave, int cnt) {
        if (waveText != null) waveText.text = "Wave : " + wave + "\nEnemy Left : " + cnt;
    }

    // 새 웨이브: wave 글자 깜빡임 📌 + 가운데 WAVE 팝업
    private void OnWaveStarted(int wave) {
        if (waveText != null) waveText.GetComponent<IBlink>()?.BlinkBlink();
        ShowWavePopup(wave);
    }

    private void ShowWavePopup(int wave) {
        if (wavePopText == null) return;
        SetActiveAll(wavePopText.gameObject);   // 꺼 둔 상태여도 보일 때만 켜짐
        wavePopText.text = "WAVE " + wave;
        StopCoroutine(nameof(WavePopup));
        StartCoroutine(nameof(WavePopup));
    }

    private IEnumerator WavePopup() {
        Color c = wavePopText.color;

        // 페이드 인
        for (float t = 0f; t < fadeTime; t += Time.deltaTime) {
            c.a = t / fadeTime;
            wavePopText.color = c;
            yield return null;
        }
        c.a = 1f;
        wavePopText.color = c;

        yield return new WaitForSeconds(showTime);

        // 페이드 아웃
        for (float t = 0f; t < fadeTime; t += Time.deltaTime) {
            c.a = 1f - t / fadeTime;
            wavePopText.color = c;
            yield return null;
        }
        c.a = 0f;
        wavePopText.color = c;
        wavePopText.gameObject.SetActive(false);   // 다 보여 준 뒤 다시 꺼짐
    }

    private void ShowGameOverUI() {
        if (gameOverUI != null) SetActiveAll(gameOverUI);   // 글자·Restart·Home 버튼까지 전부 켜짐
    }

    // 자신과 안에 있는 자식들을 모두 켬
    private void SetActiveAll(GameObject target) {
        foreach (Transform t in target.GetComponentsInChildren<Transform>(true)) {
            t.gameObject.SetActive(true);
        }
    }
}