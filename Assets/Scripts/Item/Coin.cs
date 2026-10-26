using System;
using UnityEngine;

public class Coin : MonoBehaviour, IItem {
    // ---- 알림(이벤트): 코인을 먹음 (얻은 점수) → GM이 듣고 점수에 더함 ----
    public static event Action<int> onCollected;

    public int score = 200;

    public void Use(GameObject target) {
        onCollected?.Invoke(score);
        Destroy(gameObject);
    }
}