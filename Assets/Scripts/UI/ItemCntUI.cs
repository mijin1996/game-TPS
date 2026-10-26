using UnityEngine;
using TMPro; // TextMeshPro 관련 코드

// 각 버튼 옆에 "내가 가진 개수"를 작은 숫자로 표시한다
// 총·수류탄이 보내는 알림(이벤트)을 듣고, 바뀔 때만 글자를 갱신한다
public class ItemCntUI : MonoBehaviour {
    [Header("각 버튼 옆에 표시할 텍스트 (비워두면 표시하지 않음)")]
    public TMP_Text fireCntText;    // 발사 버튼 — 현재 탄창 탄알 수
    public TMP_Text reloadCntText;  // 재장전 버튼 — 남은 전체 탄알 수
    public TMP_Text grenadeCntText; // 수류탄 버튼 — 보유 수류탄 수

    private int lastAmmo = -1;
    private int lastGrenade = -1;

    // ---- 듣기(구독) ----
    private void OnEnable() {
        Gun.onAmmoChanged += UpdateAmmo;
        PlayerGrenade.onGrenadeCntChanged += UpdateGrenade;
    }

    private void OnDisable() {
        Gun.onAmmoChanged -= UpdateAmmo;
        PlayerGrenade.onGrenadeCntChanged -= UpdateGrenade;
    }

    private void UpdateAmmo(int magAmmo, int ammoRemain) {
        if (fireCntText != null) fireCntText.text = magAmmo.ToString();
        if (reloadCntText != null) {
            reloadCntText.text = ammoRemain.ToString();
            if (lastAmmo >= 0 && ammoRemain > lastAmmo) reloadCntText.GetComponent<IPop>()?.PopPop();   // 늘어났을 때만
        }
        lastAmmo = ammoRemain;
    }

    private void UpdateGrenade(int grenadeCnt) {
        if (grenadeCntText == null) return;
        grenadeCntText.text = grenadeCnt.ToString();
        if (lastGrenade >= 0 && grenadeCnt > lastGrenade) grenadeCntText.GetComponent<IPop>()?.PopPop();
        lastGrenade = grenadeCnt;
    }
}