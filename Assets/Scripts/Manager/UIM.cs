using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIM : MonoBehaviour {
    public static UIM ins {
        get {
            if (uim == null) {
                uim = FindObjectOfType<UIM>();
            }

            return uim;
        }
    }

    private static UIM uim;

    public Text ammoText;
    public Text scoreText;
    public Text waveText;
    public GameObject gameOverUI;
    public GameObject pauseUI;

    public void UpdateAmmoText(int magAmmo, int remainAmmo) {
        ammoText.text = magAmmo + "/" + remainAmmo;
    }

    public void UpdateScoreText(int newScore) {
        scoreText.text = "Score : " + newScore;
    }

    public void UpdateWaveText(int waves, int cnt) {
        waveText.text = "Wave : " + waves + "\nEnemy Left : " + cnt;
    }

    public void SetActivePauseUI(bool active) {      // 추가
        pauseUI.SetActive(active);
    }
    
    public void SetActiveGameOverUI(bool active) {
        gameOverUI.SetActive(active);
    }

    public void GameRestart() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

/*
UIM의 umi와 ins가 있는 이유 (instance될 것들임) ⭐⭐ “static 안에서는 static만 쓸 수 있어서” 둘다 소속이 같은거야
uim(정적 필드): UIM을 저장
ins가(정적 프로퍼티): 저장된 걸 꺼내 줌

*/