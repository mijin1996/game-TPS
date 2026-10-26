using UnityEngine;
using UnityEngine.Serialization;

// 소리만 담당한다. 다른 스크립트의 알림(이벤트)을 듣고 알아서 소리를 바꾼다
public class SM : MonoBehaviour {
    [Header("Sound")]
    [SerializeField, FormerlySerializedAs("bgm")] private AudioSource bgmSource;   // 배경 음악 재생기
    [SerializeField, FormerlySerializedAs("sfx")] private AudioSource sfxSource;   // 효과음 재생기
    [SerializeField] private AudioClip buttonClickClip;
    [SerializeField] private AudioClip introClip;
    [SerializeField] private AudioClip gamePlayClip;
    [SerializeField] private AudioClip gameOverClip;

    // ---- 듣기(구독) ----
    private void OnEnable() {
        GM.onGameOver += PlayGameOverClip;
        SceneChange.onChangeRequested += PlayButtonClickClip;   // 씬 이동 버튼을 누르면 클릭음
        SettingsMenu.onButtonClicked += PlayButtonClickClip;    // 설정 창 버튼을 누르면 클릭음
    }

    private void OnDisable() {
        GM.onGameOver -= PlayGameOverClip;
        SceneChange.onChangeRequested -= PlayButtonClickClip;
        SettingsMenu.onButtonClicked -= PlayButtonClickClip;
    }

    private void Start() {
        AudioClip startClip = introClip != null ? introClip : gamePlayClip;
        if (startClip != null) {
            bgmSource.clip = startClip;
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }

    private void PlayButtonClickClip() {
        if (buttonClickClip != null) sfxSource.PlayOneShot(buttonClickClip);
    }

    private void PlayGameOverClip() {
        bgmSource.clip = gameOverClip;
        bgmSource.Play();
    }
}