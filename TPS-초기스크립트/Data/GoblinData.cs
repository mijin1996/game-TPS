using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable/GoblinData", fileName = "Goblin Data")]
public class GoblinData : ScriptableObject {
    public float health = 100f;
    public float damage = 20f;
    public float speed = 2f;
    public Color skinColor = Color.white;
}
