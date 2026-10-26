using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// 글자를 커졌다가 원래대로 되돌리는 효과. 어떤 UI 글자·그림에든 붙일 수 있다
public class PopEffect : MonoBehaviour, IPop {
    public float popScale = 1.4f;
    public float effectDuration = 0.3f;
    public Color flashColor = Color.yellow;
    public int repeat = 1;               // 반복 횟수

    private Graphic graphic;             // Text와 TMP_Text 둘 다 받는 공통 종류
    private Color originalColor;
    private Vector3 originalScale;

    private void Awake() {
        graphic = GetComponent<Graphic>();
        originalColor = graphic.color;
        originalScale = transform.localScale;
    }

    public void PopPop() {
        StopAllCoroutines();
        StartCoroutine(Pop());
    }

    private IEnumerator Pop() {
        for (int i = 0; i < repeat; i++) {
            for (float t = 0f; t < effectDuration; t += Time.deltaTime) {
                float p = t / effectDuration;
                transform.localScale = Vector3.Lerp(originalScale * popScale, originalScale, p);
                graphic.color = Color.Lerp(flashColor, originalColor, p);
                yield return null;
            }
        }
        transform.localScale = originalScale;
        graphic.color = originalColor;
    }
}
