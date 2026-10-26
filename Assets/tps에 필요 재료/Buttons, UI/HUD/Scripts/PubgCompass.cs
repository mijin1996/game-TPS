using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ZombieHUD
{
    // 화면 중앙 상단의 배틀그라운드 스타일 나침반(방위 띠)
    // - 0~360도 눈금/숫자/방위 문자(N, NE, E ...)를 런타임에 한 번 생성하고
    // - 매 프레임 카메라 방향에 맞춰 띠(strip)를 좌우로 이동시킨다.
    [DisallowMultipleComponent]
    public class PubgCompass : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("방위를 읽을 Transform. 비워두면 게임 카메라를 자동으로 찾습니다.")]
        public Transform headingSource;
        [Tooltip("눈금이 생성되는 부모. Viewport(RectMask2D) 안에 있어야 합니다.")]
        public RectTransform strip;
        [Tooltip("현재 방위 숫자 (예: 280)")]
        public TextMeshProUGUI headingText;
        [Tooltip("눈금 라벨 폰트. 비워두면 headingText 의 폰트를 사용합니다.")]
        public TMP_FontAsset font;

        [Header("Scale")]
        [Tooltip("1도당 픽셀 수. 값이 클수록 띠가 넓게 퍼집니다.")]
        public float pixelsPerDegree = 3.5f;
        [Tooltip("맵의 북쪽이 월드 +Z 가 아닐 때 보정값(도)")]
        public float northOffset = 0f;

        [Header("Ticks")]
        public int minorTickStep = 5;
        public int labelStep = 15;
        public Vector2 minorTickSize = new Vector2(2f, 6f);
        public Vector2 majorTickSize = new Vector2(2f, 12f);
        public float labelFontSize = 15f;
        public float cardinalFontSize = 20f;
        public float labelOffsetY = 16f;

        [Header("Colors")]
        public Color tickColor = new Color(1f, 1f, 1f, 0.55f);
        public Color labelColor = new Color(1f, 1f, 1f, 0.7f);
        public Color cardinalColor = Color.white;
        public Color northColor = new Color(1f, 0.82f, 0.2f, 1f); // PUBG 처럼 N 은 노란색

        public float Heading { get; private set; }

        private bool built;
        private int lastShownHeading = -1;

        private void Awake()
        {
            Build();
        }

        // 눈금 생성. -180 ~ 540 도 범위를 만들어 두어 0/360 경계에서도 끊김 없이 스크롤된다.
        private void Build()
        {
            if (built || strip == null) return;
            built = true;

            if (font == null && headingText != null) font = headingText.font;
            int step = Mathf.Max(1, minorTickStep);
            int lblStep = Mathf.Max(step, labelStep);

            for (int deg = -180; deg < 540; deg += step)
            {
                int norm = ((deg % 360) + 360) % 360;
                bool isLabel = norm % lblStep == 0;
                string cardinal = GetCardinal(norm);
                float x = deg * pixelsPerDegree;

                // 눈금
                Image tick = CreateChild<Image>("Tick_" + deg, strip);
                tick.raycastTarget = false;
                tick.color = norm == 0 ? northColor : tickColor;
                RectTransform tr = tick.rectTransform;
                tr.anchorMin = tr.anchorMax = new Vector2(0.5f, 1f);
                tr.pivot = new Vector2(0.5f, 1f);
                tr.sizeDelta = isLabel ? majorTickSize : minorTickSize;
                tr.anchoredPosition = new Vector2(x, 0f);

                if (!isLabel) continue;

                // 라벨 (숫자 또는 방위 문자)
                TextMeshProUGUI label = CreateChild<TextMeshProUGUI>("Label_" + deg, strip);
                label.raycastTarget = false;
                if (font != null) label.font = font;
                label.alignment = TextAlignmentOptions.Top;
                label.textWrappingMode = TextWrappingModes.NoWrap;
                label.overflowMode = TextOverflowModes.Overflow;

                if (cardinal != null)
                {
                    label.text = cardinal;
                    label.fontSize = cardinalFontSize;
                    label.fontStyle = FontStyles.Bold;
                    label.color = norm == 0 ? northColor : cardinalColor;
                }
                else
                {
                    label.text = norm.ToString();
                    label.fontSize = labelFontSize;
                    label.color = labelColor;
                }

                RectTransform lr = label.rectTransform;
                lr.anchorMin = lr.anchorMax = new Vector2(0.5f, 1f);
                lr.pivot = new Vector2(0.5f, 1f);
                lr.sizeDelta = new Vector2(48f, 24f);
                lr.anchoredPosition = new Vector2(x, -labelOffsetY);
            }
        }

        private void LateUpdate()
        {
            if (strip == null) return;

            if (headingSource == null)
            {
                Camera cam = HudCameraUtil.FindGameplayCamera();
                if (cam == null) return;
                headingSource = cam.transform;
            }

            Heading = Mathf.Repeat(HudCameraUtil.GetYaw(headingSource) + northOffset, 360f);
            strip.anchoredPosition = new Vector2(-Heading * pixelsPerDegree, strip.anchoredPosition.y);

            if (headingText != null)
            {
                int shown = Mathf.RoundToInt(Heading) % 360;
                if (shown != lastShownHeading)
                {
                    lastShownHeading = shown;
                    headingText.SetText("{0}", shown); // SetText 는 GC 할당이 없음
                }
            }
        }

        private static string GetCardinal(int deg)
        {
            switch (deg)
            {
                case 0: return "N";
                case 45: return "NE";
                case 90: return "E";
                case 135: return "SE";
                case 180: return "S";
                case 225: return "SW";
                case 270: return "W";
                case 315: return "NW";
                default: return null;
            }
        }

        private static T CreateChild<T>(string name, Transform parent) where T : Component
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.layer = parent.gameObject.layer;
            go.transform.SetParent(parent, false);
            return go.AddComponent<T>();
        }
    }
}
