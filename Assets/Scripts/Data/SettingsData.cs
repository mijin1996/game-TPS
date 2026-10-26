using UnityEngine;

// 음량 저장소. 씬이 바뀌어도, 게임을 껐다 켜도 유지된다
public static class SettingsData {
    public static float masterVolume { get; private set; } = 1f;
    public static bool isSoundOn { get; private set; } = true;

    // 첫 씬이 열리기 전에 저장된 음량을 적용
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Load() {
        SetMasterVolume(PlayerPrefs.GetFloat("Settings_MasterVolume", 1f));
    }

    public static void SetMasterVolume(float volume) {
        masterVolume = Mathf.Clamp01(volume);
        AudioListener.volume = masterVolume;   // 게임의 모든 소리에 적용
        PlayerPrefs.SetFloat("Settings_MasterVolume", masterVolume);
    }

    public static void SetSoundOn(bool on) {
        isSoundOn = on;
        AudioListener.volume = on ? masterVolume : 0f;
        PlayerPrefs.SetInt("Settings_SoundOn", on ? 1 : 0);
    }

    public static void Save() {
        PlayerPrefs.Save();
    }
}
