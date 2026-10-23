using UnityEngine;
using System.Collections;


public class SM : MonoBehaviour {
    public static SM ins {
        get {
            if (sm == null) {
                sm = FindObjectOfType<SM>();
            }
            return sm;
        }
    }

    private static SM sm;

    [Header("Sound")]
    [SerializeField] private AudioSource bgm;
    [SerializeField] private AudioSource sfx;
    [SerializeField] private AudioClip buttonClickClip;
    [SerializeField] private AudioClip introClip;
    [SerializeField] private AudioClip gamePlayClip;
    [SerializeField] private AudioClip gameOverClip;
    
    

    private void Start() {
        AudioClip clip = introClip != null ? introClip : gamePlayClip;
        if (clip != null) {
            bgm.clip = clip;
            bgm.loop = true;
            bgm.Play();
        }
    }

    // ---- 재사용 가능한 공용 코루틴 ----
    // "클릭음 재생 → 대기 → 넘겨받은 동작 실행"을 누구나 이 함수로 쓸 수 있음
    public void CoroutineMethod(System.Action action) {
        StartCoroutine(SfxAfterSceneChange(action));
    }

    private IEnumerator SfxAfterSceneChange(System.Action action) {
        Sfx();
        yield return new WaitForSeconds(buttonClickClip.length);
        action?.Invoke();
    }
    //버튼클릭
    public void Sfx() {
        sfx.PlayOneShot(buttonClickClip);
    }

    public void PlayGameOverBgm() {
        bgm.clip = gameOverClip;
        bgm.Play();
    }
}