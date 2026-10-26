using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// 씬 이동만 담당한다. 버튼(Start·Restart·Home)이 이 함수들을 부른다
// 소리는 모르고, "씬 이동을 시작한다"고 알리기만 한다 (SM이 듣고 클릭음을 냄)
public class SceneChange : MonoBehaviour {
    public static event Action onChangeRequested;

    public float delay = 0.3f;   // 클릭음이 들리도록 기다리는 시간 (초)

    private const int introScene = 0;
    private const int playScene = 1;

    private bool isChanging;

    // ---- 듣기(구독): 설정 창의 나가기 → Intro ----
    private void OnEnable() {
        SettingsMenu.onGoHomeRequested += GameHome;
    }

    private void OnDisable() {
        SettingsMenu.onGoHomeRequested -= GameHome;
    }

    public void GameStart() { Change(playScene); }
    public void GameHome() { Change(introScene); }
    public void GameRestart() { Change(SceneManager.GetActiveScene().buildIndex); }

    public void Change(int index) {
        if (isChanging) return;   // 여러 번 눌러도 한 번만
        isChanging = true;
        onChangeRequested?.Invoke();
        StartCoroutine(ChangeAfterDelay(index));
    }

    // 멈춘 상태(timeScale = 0)에서도 기다릴 수 있게 Realtime 사용
    private IEnumerator ChangeAfterDelay(int index) {
        yield return new WaitForSecondsRealtime(delay);
        Time.timeScale = 1f;   // 새 씬은 항상 정상 속도로 시작
        SceneManager.LoadScene(index);
    }
}