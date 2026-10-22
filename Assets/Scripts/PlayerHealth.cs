using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : LivingEntity {
    public Slider healthSlider;

    public AudioClip deathClip;
    public AudioClip hitClip;
    public AudioClip itemPickupClip;

    private AudioSource playerSfx;
    private Animator playerAni;

    private PlayerMovement playerMovement;
    private PlayerShooter playerShooter;

    private void Awake() {
        playerAni = GetComponent<Animator>();
        playerSfx = GetComponent<AudioSource>();

        playerMovement = GetComponent<PlayerMovement>();
        playerShooter = GetComponent<PlayerShooter>();
    }

    protected override void OnEnable() {
        base.OnEnable();

        healthSlider.gameObject.SetActive(true);
        healthSlider.maxValue = startingHealth;
        healthSlider.value = health;

        playerMovement.enabled = true;
        playerShooter.enabled = true;
    }

    public override void RestoreHealth(float newHealth) {
        base.RestoreHealth(newHealth);
        healthSlider.value = health;
    }

    public override void OnDamage(float damage, Vector3 hitPoint, Vector3 hitDirection) {
        if (!dead) {
            playerSfx.PlayOneShot(hitClip);
        }
        
        base.OnDamage(damage, hitPoint, hitDirection);
        healthSlider.value = health;
    }

    public override void Die() {
        base.Die();

        healthSlider.gameObject.SetActive(false);

        playerSfx.PlayOneShot(deathClip);
        playerAni.SetTrigger("Die");

        playerMovement.enabled = false;
        playerShooter.enabled = false;
    }

    private void OnTriggerEnter(Collider other) {
        if (!dead) {
            IItem item = other.GetComponent<IItem>();

            if(item != null) {
                item.Use(gameObject);
                playerSfx.PlayOneShot(itemPickupClip);
            }
        }
    }
}