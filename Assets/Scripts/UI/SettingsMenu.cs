using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// 설정 창: 음량 조절과 퍼센트 표시, 소리 끄기, 나가기
// 게임 멈춤·클릭음·씬 이동은 직접 하지 않고, 이벤트로 알리기만 한다
//   onOpenChanged     → GM이 듣고 일시정지 / 재개
//   onButtonClicked   → SM이 듣고 클릭음
//   onGoHomeRequested → SceneChange가 듣고 Intro로 이동
public class SettingsMenu : MonoBehaviour {
    // ---- 알림(이벤트) ----
    public static event Action<bool> onOpenChanged;    // 설정 창 열림(true) / 닫힘(false)
    public static event Action onButtonClicked;        // 설정 창 버튼을 누름
    public static event Action onGoHomeRequested;      // Play 씬에서 나가기 → Intro로

    public static SettingsMenu ins { get; private set; }

    public GameObject root;            // 설정 창 전체
    public GameObject settingsPanel;   // 음량 조절 패드 (비워두면 root 안의 "SettingsPanel"을 자동으로 찾음)
    public Button closeButton;         // X 버튼
    public Button exitButton;          // 나가기
    public Slider masterVolumeSlider;  // 음량 슬라이더
    public TMP_Text volumeValueText;   // "70%" 글자

    public Button soundButton;
    public Image soundIcon;
    public Sprite soundOnSprite;
    public Sprite soundOffSprite;

    public bool isOpen => root.activeSelf;

    private void Awake() {
        ins = this;
        if (settingsPanel == null) {
            Transform panel = root.transform.Find("SettingsPanel");
            if (panel != null) settingsPanel = panel.gameObject;
        }
        root.SetActive(false);
        closeButton.onClick.AddListener(Close);
        exitButton.onClick.AddListener(Exit);
        masterVolumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        soundButton.onClick.AddListener(ToggleSound);
    }

    public void Open() {
        onButtonClicked?.Invoke();
        root.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(true);   // 하이어라키에서 꺼 두어도 열 때는 항상 보이게
        masterVolumeSlider.SetValueWithoutNotify(SettingsData.masterVolume);
        ShowPercent();
        soundIcon.sprite = SettingsData.isSoundOn ? soundOnSprite : soundOffSprite;
        onOpenChanged?.Invoke(true);
    }

    public void Close() {
        onButtonClicked?.Invoke();
        root.SetActive(false);
        SettingsData.Save();
        onOpenChanged?.Invoke(false);
    }

    // 슬라이더를 움직일 때: 소리 없이 음량과 글자만 바꾼다
    private void OnVolumeChanged(float v) {
        SettingsData.SetMasterVolume(v);
        ShowPercent();
    }

    private void ShowPercent() {
        volumeValueText.text = Mathf.RoundToInt(SettingsData.masterVolume * 100f) + "%";
    }

    private void ToggleSound() {
        onButtonClicked?.Invoke();                 // 끄기 전에 클릭 소리 먼저
        SettingsData.SetSoundOn(!SettingsData.isSoundOn);
        soundIcon.sprite = SettingsData.isSoundOn ? soundOnSprite : soundOffSprite;
    }

    // 나가기 버튼: Play 씬에서는 Intro 화면으로, Intro에서는 게임 종료
    private void Exit() {
        SettingsData.Save();
        if (SceneManager.GetActiveScene().buildIndex != 0) {
            onGoHomeRequested?.Invoke();
            return;
        }
        onButtonClicked?.Invoke();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}