using UnityEngine;
using UnityEngine.UI;

public class Weapon : MonoBehaviour
{
    [Header("Настройки")]
    public float damage = 10f;
    public float range = 100f;
    public float fireRate = 10f;
    public int maxAmmo = 30;
    public float reloadTime = 1.5f;

    [Header("Прицел")]
    public Image crosshairImage; 

    [Header("UI")]
    public Text ammoText; 

    [Header("Звуки")]
    public AudioClip shootSound;
    public AudioClip reloadSound;
    private AudioSource audioSource;

    private int currentAmmo;
    private bool isReloading = false;
    private float nextTimeToFire = 0f;

    //ПРИЦЕЛ
    public Texture2D crosshairTexture; 
    public Vector2 size = new Vector2(32, 32); 

    //ТЕКСТ
    public Weapon weapon; // Ссылка на оружие
    [Header("Позиция на экране")]
    public Vector2 position = new Vector2(10, 100); 
    public bool rightAlign = true;
    [Header("Настройки текста")]
    public int fontSize = 22;
    
    void Start()
    {
        currentAmmo = maxAmmo;
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        UpdateUI();
    }

    void Update()
    {
        if (isReloading) return;

        if (currentAmmo <= 0)
        {
            StartReload();
            return;
        }

        if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + 1f / fireRate;
            Shoot();
        }

        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmo)
        {
            StartReload();
        }
    }

    void Shoot()
    {
        if (shootSound != null)
        {
            audioSource.PlayOneShot(shootSound);
        }
        
        currentAmmo--;

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, 
                           Camera.main.transform.forward, 
                           out hit, 
                           range))
        {
            EnemyHealth enemy = hit.transform.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage((int)damage);
            }
        }
        
        UpdateUI();
    }

    void StartReload()
    {
        if (isReloading || currentAmmo >= maxAmmo) return;
        
        isReloading = true;
        
        if (reloadSound != null)
        {
            audioSource.PlayOneShot(reloadSound);
        }
        
        Invoke("FinishReload", reloadTime);
    }

    void FinishReload()
    {
        currentAmmo = maxAmmo;
        isReloading = false;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (ammoText != null)
        {
            ammoText.text = $"{currentAmmo} / {maxAmmo}";
        }
    }

    // Методы для получения информации (для других скриптов)
    public int GetCurrentAmmo() { return currentAmmo; }
    public int GetMaxAmmo() { return maxAmmo; }
    public bool IsReloading() { return isReloading; }
    public float GetReloadProgress()
    {
        if (!isReloading) return 1f;
        return 0f;
    }

    void OnGUI()
    {
        if (crosshairTexture == null) return;
        
        // Позиция в центре экрана
        float x = Screen.width / 2 - size.x / 2;
        float y = Screen.height / 2 - size.y / 2;
        
        // Рисуем прицел
        GUI.DrawTexture(new Rect(x, y, size.x, size.y), crosshairTexture);

        if (weapon == null)
        {
            Debug.LogWarning("Weapon не назначен в AmmoUI!");
            return;
        }
        
        // Получаем данные из оружия
        int currentAmmo = weapon.GetCurrentAmmo();
        int maxAmmo = weapon.GetMaxAmmo();
        bool isReloading = weapon.IsReloading();
        
        // Создаем стиль текста
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = fontSize;
        style.fontStyle = FontStyle.Bold;
        
        // Формируем текст
        string ammoText = $"ПАТРОНЫ: {currentAmmo} / {maxAmmo}";
        string statusText = "";
        
        if (isReloading)
        {
            statusText = "СТАТУС: ПЕРЕЗАРЯДКА";
        }
        else if (currentAmmo == 0)
        {
            statusText = "СТАТУС: ПУСТО!";
        }
        else if (currentAmmo <= maxAmmo / 4)
        {
            statusText = "СТАТУС: МАЛО ПАТРОНОВ";
        }
        else
        {
            statusText = "СТАТУС: ГОТОВ";
        }
        
        // Вычисляем ширину текста
        Vector2 textSize1 = style.CalcSize(new GUIContent(ammoText));
        Vector2 textSize2 = style.CalcSize(new GUIContent(statusText));
        float maxWidth = Mathf.Max(textSize1.x, textSize2.x);
        
        // Позиция (правый нижний угол или левый верхний)
        float xPos, yPos;
        
        if (rightAlign)
        {
            // Правый нижний угол
            xPos = Screen.width - maxWidth - position.x;
            yPos = Screen.height - textSize1.y - textSize2.y - position.y - 10;
        }
        else
        {
            // Левый верхний угол (как у волн)
            xPos = position.x;
            yPos = position.y;
        }
        
        // Отображаем информацию о патронах
        GUI.Label(new Rect(xPos, yPos, maxWidth, textSize1.y), ammoText, style);
        GUI.Label(new Rect(xPos, yPos + textSize1.y + 5, maxWidth, textSize2.y), statusText, style);
        
        // Дополнительно: прогресс перезарядки (если есть)
        if (isReloading)
        {
            string reloadText = "ЗАРЯЖАЮ...";
            Vector2 reloadSize = style.CalcSize(new GUIContent(reloadText));
            GUI.Label(new Rect(xPos, yPos + textSize1.y + textSize2.y + 15, reloadSize.x, reloadSize.y), 
                     reloadText, style);
        }
    }
}
