using Microsoft.Win32.SafeHandles;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Goblin : LivingEntity {
    public LayerMask whatIsTarget;

    private LivingEntity targetEntity;
    private NavMeshAgent navAgent;

    public ParticleSystem hitFx;
    public AudioClip deathSound;
    public AudioClip hitSound;

    private Animator goblinAni;
    private AudioSource goblinSfx;
    private Renderer goblinRenderer;

    public float damage = 20f;
    public float timeBetAttack = 0.5f;
    private float lastAttackTime;

    private bool hasTarget {
        get {
            if (targetEntity != null && !targetEntity.dead) {
                return true;
            }

            return false;
        }
    }

    private void Awake() {
        navAgent = GetComponent<NavMeshAgent>();
        goblinAni = GetComponent<Animator>();
        goblinSfx = GetComponent<AudioSource>();

        goblinRenderer = GetComponentInChildren<Renderer>();
    }

    public void Setup(GoblinData goblinData) {
        startingHealth = goblinData.health;
        health = goblinData.health;
        damage = goblinData.damage;
        navAgent.speed = goblinData.speed;
        goblinRenderer.material.color = goblinData.skinColor;
    }

    private void Start() {
        StartCoroutine(UpdatePath());
    }

    private void Update() {
        goblinAni.SetBool("HasTarget", hasTarget);
    }

    private IEnumerator UpdatePath() {
        while (!dead) {
            if(hasTarget) {
                navAgent.isStopped = false;
                navAgent.SetDestination(targetEntity.transform.position);
            } else {
                navAgent.isStopped = true;

                Collider[] colliders = Physics.OverlapSphere(transform.position, 20f, whatIsTarget);
                
                for (int i = 0; i < colliders.Length; i++) {
                    LivingEntity livingEntity = colliders[i].GetComponent<LivingEntity>();

                    if (livingEntity != null && !livingEntity.dead) {
                        targetEntity = livingEntity;

                        break;
                    }
                }
            }
            yield return new WaitForSeconds(0.25f);
        }
    }

    public override void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal) {
        if(!dead) {
            hitFx.transform.position = hitPoint;
            hitFx.transform.rotation = Quaternion.LookRotation(hitNormal);
            hitFx.Play();

            goblinSfx.PlayOneShot(hitSound);
        }
        
        base.OnDamage(damage, hitPoint, hitNormal);
    }

    public override void Die() {
        base.Die();

        Collider[] goblinCollliders = GetComponents<Collider>();
        for (int i = 0; i < goblinCollliders.Length; i++) {
            goblinCollliders[i].enabled = false;
        }

        navAgent.isStopped = true;
        navAgent.enabled = false;

        goblinAni.SetTrigger("Die");
        goblinSfx.PlayOneShot(deathSound);
    }

    private void OnTriggerStay(Collider other) {
        if(!dead && Time.time >= lastAttackTime + timeBetAttack) {
            LivingEntity attackTarget = other.GetComponent<LivingEntity>();

            if(attackTarget != null && attackTarget == targetEntity) {
                lastAttackTime = Time.time;

                Vector3 hitPoint = other.ClosestPoint(transform.position);
                Vector3 hitNormal = transform.position = other.transform.position;

                attackTarget.OnDamage(damage, hitPoint, hitNormal);
            }
        }
    }
}