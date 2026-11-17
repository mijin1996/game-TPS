using System.Collections.Generic;
using UnityEngine;

public class GoblinSpawn : MonoBehaviour {
    public Goblin goblinPrefab;

    public GoblinData[] goblinDatas;
    public Transform[] spawnPoints;

    private List<Goblin> goblins = new List<Goblin>();
    private int wave;

    private void Update() {
        if (GM.ins != null && GM.ins.isGameOver) {
            return;
        }

        if (goblins.Count <= 0) {
            SpawnWave();
        }

        UpdateUI();
    }

    private void UpdateUI() {
        UIM.ins.UpdateWaveText(wave, goblins.Count);
    }

    private void SpawnWave() {
        wave++;

        int spawnCnt = Mathf.RoundToInt(wave * 1.5f);

        for (int i = 0; i < spawnCnt; i++) {
            CreateGoblin();
        }
    }

    private void CreateGoblin() {
        GoblinData goblinData = goblinDatas[Random.Range(0, goblinDatas.Length)];

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        Goblin goblin = Instantiate(goblinPrefab, spawnPoint.position, spawnPoint.rotation);

        goblin.Setup(goblinData);

        goblins.Add(goblin);

        goblin.onDeath += () => goblins.Remove(goblin);
        goblin.onDeath += () => Destroy(goblin.gameObject, 10f);
        goblin.onDeath += () => GM.ins.AddScore(100);
    }
}