using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// 정해진 색으로 깜빡이는 효과. 글자와 그림 어디에든 붙일 수 있다
public class BlinkEffect : MonoBehaviour, IBlink {
    public Color blinkColor = Color.red;   // 깜빡일 때의 색
    public float blinkTime = 0.2f;         // 한 번 바뀌는 시간
    public int repeat = 3;                 // 반복 횟수

    private Graphic graphic;
    private Color originalColor;

    private void Awake() {
        graphic = GetComponent<Graphic>();
        originalColor = graphic.color;
    }

    private void OnDisable() {
        if (graphic != null) graphic.color = originalColor;
    }

    public void BlinkBlink() {
        StopAllCoroutines();
        graphic.color = originalColor;
        StartCoroutine(Blink());
    }

    private IEnumerator Blink() {
        // Realtime: 게임이 멈춰 있어도(timeScale = 0) 깜빡임은 계속됨
        for (int i = 0; i < repeat; i++) {
            graphic.color = blinkColor;
            yield return new WaitForSecondsRealtime(blinkTime);
            graphic.color = originalColor;
            yield return new WaitForSecondsRealtime(blinkTime);
        }
    }
}
