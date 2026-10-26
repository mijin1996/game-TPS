using System.Collections;
using UnityEngine;

namespace ZombieHUD
{
    // PUBG HUD 루트(Canvas)에 붙는 컨트롤러
    // - 새 HUD 와 겹치는 기존 HUD(탄약 텍스트 박스, 체력 슬라이더)를 CanvasGroup 알파로 숨긴다.
    //   오브젝트를 비활성화/삭제하지 않으므로 기존 UIM, PlayerHealth 코드는 그대로 동작한다.
    // - 체크를 끄면 기존 HUD 가 다시 보인다 (되돌리기 쉬움).
    [DisallowMultipleComponent]
    public class PubgHudController : MonoBehaviour
    {
        [Tooltip("기존 HUD 의 탄약 표시와 체력 슬라이더를 숨깁니다.")]
        public bool hideLegacyHud = true;
        [Tooltip("추가로 숨길 오브젝트 (씬에 배치한 뒤 인스펙터에서 지정)")]
        public GameObject[] extraObjectsToHide;

        [Tooltip("중앙 상단 나침반과 겹치지 않도록 기존 Score 텍스트를 아래로 내리는 거리 (기존 HUD Canvas 단위, 0 이면 이동 안 함)")]
        public float legacyScoreOffsetY = -60f;

        private IEnumerator Start()
        {
            // PlayerHealth.OnEnable 등 다른 초기화가 끝난 뒤 처리
            yield return null;
            if (hideLegacyHud) HideLegacyHud();
            ShiftLegacyScore();
        }

        private void ShiftLegacyScore()
        {
            if (Mathf.Approximately(legacyScoreOffsetY, 0f)) return;
            UIM ui = FindFirstObjectByType<UIM>();
            if (ui == null || ui.scoreText == null) return;
            ui.scoreText.rectTransform.anchoredPosition += new Vector2(0f, legacyScoreOffsetY);
        }

        private void HideLegacyHud()
        {
            UIM ui = FindFirstObjectByType<UIM>();
            if (ui != null && ui.ammoText != null)
            {
                // "Ammo Display"(배경 이미지) 째로 숨김. 부모가 Canvas 면 텍스트만 숨김
                Transform t = ui.ammoText.transform;
                bool parentIsCanvas = t.parent == null || t.parent.GetComponent<Canvas>() != null;
                Hide(parentIsCanvas ? t.gameObject : t.parent.gameObject);
            }

            PlayerHealth player = FindFirstObjectByType<PlayerHealth>();
            if (player != null && player.healthSlider != null) Hide(player.healthSlider.gameObject);

            if (extraObjectsToHide != null)
            {
                foreach (GameObject go in extraObjectsToHide)
                {
                    if (go != null) Hide(go);
                }
            }
        }

        private static void Hide(GameObject go)
        {
            CanvasGroup group = go.GetComponent<CanvasGroup>();
            if (group == null) group = go.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;
        }
    }
}
