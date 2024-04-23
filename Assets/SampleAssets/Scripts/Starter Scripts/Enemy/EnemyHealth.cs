﻿using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EnemyHealth : MonoBehaviour
{
    [Header("Settings")]
    public bool EnemyHealthBar = false;
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Health Bar for Enemy")]
    public float padding = 2f;
    public Vector2 Dimensions;
    public GameObject HealthBar;

    public GameObject HealthBar_fill;
    public RectTransform canvasRectTransform;

    [Header("Item Drop Settings")]
    public GameObject itemPrefab;
    public int numberOfItemsToDrop = 3;

    [Header("Audio Settings")]
    public AudioClip deathSound; // The sound that plays upon death

    public GameObject fadeOutImage;

    public AudioSource BossMusic;
    [HideInInspector] public AudioSource DeathSource;

    private Image healthBarImage;
    private RectTransform healthRectTransform;
    private Animator anim;
    private SpriteRenderer spriteRender;

    [Range(0, 1)]
	public float VolumeLevel = 1;

    public float animationDelay = 2.0f;

    void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
        spriteRender = GetComponent<SpriteRenderer>();
        SetupAudio();
        
        if (EnemyHealthBar)
        {
            SetupHealthBar();
        }
    }

    void Update()
    {
        
    }
    void AssignParent(GameObject obj)
	{
		obj.transform.parent = transform;
	}
    void SetupAudio(){

        GameObject DeathGameObject = new GameObject("DeathAudioSource");
		AssignParent(DeathGameObject);
		DeathSource = DeathGameObject.AddComponent<AudioSource>();
		DeathSource.clip = deathSound;
		DeathSource.volume = VolumeLevel;
		DeathSource.loop = false;
    }
    public void DecreaseHealth(int value)
    {
        currentHealth -= value;
        StartCoroutine(dmgFlicker());
        if (currentHealth <= 0)
        {
            HandleDeath();
        }
        if (EnemyHealthBar)
            UpdateHealthBar();
    }

    private IEnumerator DestroyAfterAnimation()
    {
        yield return new WaitForSeconds(animationDelay); // Wait for death animation to play out
        DropItems();
        Destroy(gameObject);
        if (EnemyHealthBar) Destroy(HealthBar.gameObject);
    }
    private	IEnumerator Transition() {
        Animator anim = fadeOutImage.GetComponent<Animator>();
        anim.SetTrigger("StartFadeOut"); // Make sure the trigger name matches the one in the Animator
        // Wait for the animation to finish
        yield return new WaitForSeconds(2); // Adjust this time based on the animation length

		anim.ResetTrigger("StartFadeOut");
		anim.ResetTrigger("StartFadeIn");

        SceneManager.LoadScene(1);
	}

    private IEnumerator HandleDeathSequence()
    {
        yield return StartCoroutine(DestroyAfterAnimation());
        StartCoroutine(Transition());
    }

    IEnumerator FadeOutBossMusic(float fadeTime)
    {
        float startVolume = BossMusic.volume;

        while (BossMusic.volume > 0)
        {
            BossMusic.volume -= startVolume * Time.deltaTime / fadeTime;
            yield return null;
        }

        BossMusic.Stop();
        }

    IEnumerator dmgFlicker()
    {
        spriteRender.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRender.color = Color.white;
        yield return new WaitForSeconds(0.1f);
        spriteRender.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRender.color = Color.white;
        yield return new WaitForSeconds(0.1f);
        spriteRender.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRender.color = Color.white;
    }

    private void UpdateHealthBar()
    {
        float fillAmount = (float)currentHealth / maxHealth;
        HealthBar_fill.GetComponent<Image>().fillAmount = fillAmount;
    }

    private void SetupHealthBar()
    {
        HealthBar.SetActive(true);
        UpdateHealthBar();
    }

    private void HandleDeath()
    {
        currentHealth = 0;
        CapsuleCollider2D collider = GetComponent<CapsuleCollider2D>();
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        DeathSource.Play(); // Play the death sound

        gameObject.layer = 9;

        if (rb != null)
        {
            rb.Sleep();
        }
        anim.SetBool("isDead", true);
        StartCoroutine(FadeOutBossMusic(3.0f));
        StartCoroutine(HandleDeathSequence());
    }

    private void DropItems()
    {
        for (int i = 0; i < numberOfItemsToDrop; i++)
        {
            Vector3 spawnPosition = transform.position + new Vector3(Random.Range(-0.5f, 0.5f), 0, 0);
            Instantiate(itemPrefab, spawnPosition, Quaternion.identity);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent(out Damager weapon))
        {
            if (weapon.alignmnent == Damager.Alignment.Player || weapon.alignmnent == Damager.Alignment.Environment)
            {
                DecreaseHealth(weapon.damageValue);

                if (EnemyHealthBar)
                    UpdateHealthBar();
            }
        }
    }
}
