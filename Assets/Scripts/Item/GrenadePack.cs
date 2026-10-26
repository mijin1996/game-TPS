using UnityEngine;

// 수류탄 보유 개수를 늘려주는 아이템
public class GrenadePack : MonoBehaviour, IItem {
    public int grenadeAmount = 1; // 추가할 수류탄 수

    public void Use(GameObject target) {
        PlayerGrenade playerGrenade = target.GetComponent<PlayerGrenade>();

        if (playerGrenade != null) {
            playerGrenade.AddGrenade(grenadeAmount);
        }

        Destroy(gameObject);
    }
}
