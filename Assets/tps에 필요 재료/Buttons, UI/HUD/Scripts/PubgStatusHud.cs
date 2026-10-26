using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ZombieHUD
{
    // 화면 중앙 하단의 배틀그라운드 스타일 상태바
    // - 탄창에 남은 탄(크게) | 남은 전체 탄(작게)
    // - 체력바 (피격 시 빨간 잔상이 잠시 후 줄어드는 연출, 체력이 낮으면 빨갛게 깜빡임)
    // 기존 PlayerHealth / Gun 을 "읽기만" 하므로 기존 스크립트는 수정하지 않는다.
    [DisallowMultipleComponent]
    public class PubgStatusHud : MonoBehaviour
    {
        [Header("Sources (비워두면 자동 탐색)")]
        public PlayerHealth playerHealth;
        public PlayerShooter playerShooter;

        [Header("Health")]
        [Tooltip("Image Type = Filled / Horizontal")]
        public Image healthFill;
        [Tooltip("피격 잔상. Image Type = Filled / Horizontal")]
        public Image healthDamageTrail;
        public Color healthNormalColor = new Color(0.93f, 0.93f, 0.93f, 1f);
        public Color healthLowColor = new Color(0.9f, 0.18f, 0.14f, 1f);
        [Range(0f, 1f)] public float lowHealthThreshold = 0.3f;
        [Tooltip("잔상이 줄어들기 시작하기까지 대기 시간(초)")]
        public float trailDelay = 0.4f;
        [Tooltip("잔상이 줄어드는 속도 (초당 비율)")]
        public float trailSpeed = 0.8f;

        [Header("Ammo")]
        public TextMeshProUGUI magText;
        public TextMeshProUGUI reserveText;
        [Tooltip("재장전 중에만 켜지는 오브젝트")]
        public GameObject reloadIndicator;
        public Color ammoNormalColor = Color.white;
        public Color ammoLowColor = new Color(1f, 0.62f, 0.2f, 1f);
        public Color ammoEmptyColor = new Color(0.95f, 0.22f, 0.18f, 1f);
        [Range(0f, 1f)] public float lowAmmoThreshold = 0.3f;
        [Tooltip("발사 시 탄 숫자가 튀는 크기")]
        public float firePunchScale = 1.15f;

        [Header("Misc")]
        [Tooltip("사망 시 HUD 를 흐리게 할 CanvasGroup")]
        public CanvasGroup canvasGroup;

        private float trailValue = -1f;
        private float trailHoldUntil;
        private float lastHealth01 = -1f;
        private int lastMag = int.MinValue;
        private int lastReserve = int.MinValue;
        private bool lastReloading;
        private float nextSourceSearch;

        private void Start()
        {
            if (reloadIndicator != null) reloadIndicator.SetActive(false);
        }

        private void Update()
        {
            ResolveSources();
            UpdateHealth();
            UpdateAmmo();
        }

        private void ResolveSources()
        {
            if (playerHealth != null && playerShooter != null) return;
            if (Time.unscaledTime < nextSourceSearch) return; // 플레이어가 없을 때 매 프레임 검색 방지
            nextSourceSearch = Time.unscaledTime + 1f;

            if (playerHealth == null) playerHealth = FindFirstObjectByType<PlayerHealth>();
            if (playerShooter == null && playerHealth != null) playerShooter = playerHealth.GetComponent<PlayerShooter>();
            if (playerShooter == null) playerShooter = FindFirstObjectByType<PlayerShooter>();
        }

        private void UpdateHealth()
        {
            if (playerHealth == null) return;

            float max = Mathf.Max(1f, playerHealth.startingHealth);
            // RestoreHealth 는 최대치 제한이 없으므로 표시값은 0~1 로 제한
            float h01 = Mathf.Clamp01(playerHealth.health / max);

            // 잔상 계산
            if (trailValue < 0f) trailValue = h01;
            if (lastHealth01 >= 0f && h01 < lastHealth01 - 0.0001f) trailHoldUntil = Time.time + trailDelay;
            if (h01 >= trailValue) trailValue = h01; // 회복 시 즉시 맞춤
            else if (Time.time >= trailHoldUntil) trailValue = Mathf.MoveTowards(trailValue, h01, trailSpeed * Time.deltaTime);
            lastHealth01 = h01;

            if (healthDamageTrail != null) healthDamageTrail.fillAmount = trailValue;

            if (healthFill != null)
            {
                healthFill.fillAmount = h01;
                if (h01 <= lowHealthThreshold && h01 > 0f)
                {
                    Color c = healthLowColor;
                    c.a = 0.65f + 0.35f * Mathf.Sin(Time.time * 8f); // 저체력 깜빡임
                    healthFill.color = c;
                }
                else
                {
                    healthFill.color = h01 <= 0f ? healthLowColor : healthNormalColor;
                }
            }

            if (canvasGroup != null)
            {
                float goal = playerHealth.dead ? 0.35f : 1f;
                canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, goal, Time.deltaTime * 2f);
            }
        }

        private void UpdateAmmo()
        {
            Gun gun = playerShooter != null ? playerShooter.gun : null;
            if (gun == null) return;

            int mag = gun.magAmmo;
            int reserve = gun.ammoRemain;
            int capacity = gun.gunData != null ? Mathf.Max(1, gun.gunData.magCapacity) : Mathf.Max(1, mag);
            bool reloading = gun.state == Gun.State.Reloading;

            if (magText != null)
            {
                if (mag != lastMag)
                {
                    // 탄이 줄었을 때(=발사) 숫자를 살짝 키워 타격감 연출
                    if (lastMag != int.MinValue && mag < lastMag)
                        magText.rectTransform.localScale = Vector3.one * firePunchScale;

                    magText.SetText("{0}", mag);
                    lastMag = mag;
                }

                Transform mt = magText.rectTransform;
                mt.localScale = Vector3.Lerp(mt.localScale, Vector3.one, 1f - Mathf.Exp(-18f * Time.unscaledDeltaTime));

                Color color;
                if (mag <= 0) color = ammoEmptyColor;
                else if (mag <= capacity * lowAmmoThreshold) color = ammoLowColor;
                else color = ammoNormalColor;
                color.a = reloading ? 0.4f : 1f;
                magText.color = color;
            }

            if (reserveText != null && reserve != lastReserve)
            {
                reserveText.SetText("{0}", reserve);
                reserveText.color = reserve <= 0 ? ammoEmptyColor : new Color(1f, 1f, 1f, 0.7f);
                lastReserve = reserve;
            }

            if (reloadIndicator != null && reloading != lastReloading)
            {
                reloadIndicator.SetActive(reloading);
                lastReloading = reloading;
            }
        }
    }
}
