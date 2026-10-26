using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// 웨이브마다 고블린을 생성한다. UI·점수는 모르고, 이벤트로 알리기만 한다
public class GoblinSpawn : MonoBehaviour {
    public Goblin goblinPrefab;

    public GoblinData[] goblinDatas;
    public Transform[] spawnPoints;
    public GameObject spawnEffect;

    public int maxWave = 10;
    public int cntPerWave = 10;

    // ---- 알림(이벤트) ----
    public static event Action<int> onWaveStarted;          // 새 웨이브 시작 (웨이브 번호)
    public static event Action<int, int> onEnemyCountChanged; // 웨이브 번호, 남은 적 수
    public static event Action onGoblinKilled;              // 고블린 한 마리 죽음

    private List<Goblin> goblins = new List<Goblin>();
    private int wave;
    private bool isGameOver;

    // ---- 듣기(구독) ----
    private void OnEnable() {
        GM.onGameOver += OnGameOver;
    }

    private void OnDisable() {
        GM.onGameOver -= OnGameOver;
    }

    private void OnGameOver() {
        isGameOver = true;
    }

    private void Update() {
        if (isGameOver) return;

        if (goblins.Count <= 0 && wave < maxWave) {
            SpawnWave();
        }
    }

    private void SpawnWave() {
        wave++;
        onWaveStarted?.Invoke(wave);

        int spawnCnt = cntPerWave * (int)Mathf.Pow(2, wave - 1); // wave마다 고블린이 배로 늘어남 📌
        for (int i = 0; i < spawnCnt; i++) {
            CreateGoblin();
        }

        onEnemyCountChanged?.Invoke(wave, goblins.Count);
    }

    private void CreateGoblin() {
        GoblinData goblinData = goblinDatas[UnityEngine.Random.Range(0, goblinDatas.Length)];
        Transform spawnPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];

        Goblin goblin = Instantiate(goblinPrefab, spawnPoint.position, spawnPoint.rotation);

        if (spawnEffect != null) {
            Vector3 groundPos = spawnPoint.position;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(spawnPoint.position, out hit, 10f, NavMesh.AllAreas)) {
                groundPos = hit.position;
            }
            GameObject fx = Instantiate(spawnEffect, groundPos + Vector3.up * 0.05f, Quaternion.identity);
            Destroy(fx, 3f);
        }

        goblin.Setup(goblinData);
        goblins.Add(goblin);

        goblin.onDeath += () => OnGoblinDeath(goblin);
    }

    private void OnGoblinDeath(Goblin goblin) {
        goblins.Remove(goblin);
        Destroy(goblin.gameObject, 10f);
        onGoblinKilled?.Invoke();
        onEnemyCountChanged?.Invoke(wave, goblins.Count);
    }
}