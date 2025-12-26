using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("Основные настройки")]
    public int damage = 10;
    public float range = 100f; // Дальность выстрела
    public float fireRate = 10f; // Выстрелов в секунду
    public int maxAmmo = 30; // Максимальный патрон в магазине
    public float reloadTime = 1.5f; // Время перезарядки

    [Header("Эффекты")]
    public ParticleSystem muzzleFlash; // Эффект дульного пламени
    public GameObject impactEffect; // Эффект попадания (дым, искры)

    [Header("Точка выстрела")]
    public Camera playerCamera; // Камера, из которой ведётся стрельба

    private int currentAmmo; // Текущие патроны
    private bool isReloading = false;
    private float nextTimeToFire = 0f; // Таймер для скорострельности

    void Start()
    {
        currentAmmo = maxAmmo;

        // Найти камеру, если не назначена
        if (playerCamera == null)
        {
            playerCamera = GetComponentInParent<Camera>();
            if (playerCamera == null)
            {
                playerCamera = Camera.main;
            }
        }
    }

    void Update()
    {
        // Проверка на перезарядку
        if (isReloading)
            return;

        // Автоматическая перезарядка, если патроны кончились
        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }

        // Стрельба: зажата ЛКМ для автоматического огня
        if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + 1f / fireRate;
            Shoot();
        }

        // Ручная перезарядка по клавише R
        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmo)
        {
            StartCoroutine(Reload());
        }
    }

    void Shoot()
    {
        // Воспроизвести эффект дульного пламени
        if (muzzleFlash != null)
            muzzleFlash.Play();

        // Уменьшить патроны
        currentAmmo--;

        // Создать луч (Raycast) из центра камеры
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, 
                           playerCamera.transform.forward, 
                           out hit, 
                           range))
        {
            // Проверить, попали ли во врага
            EnemyHealth enemy = hit.transform.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                // Нанести урон врагу
                enemy.TakeDamage(damage);
            }

            // Создать эффект попадания в точке удара
            if (impactEffect != null)
            {
                GameObject impact = Instantiate(impactEffect, 
                                               hit.point, 
                                               Quaternion.LookRotation(hit.normal));
                Destroy(impact, 2f); // Удалить эффект через 2 секунды
            }
        }
    }

    System.Collections.IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("Перезарядка...");

        // Анимация или звук перезарядки (здесь просто ждём)
        yield return new WaitForSeconds(reloadTime);

        currentAmmo = maxAmmo;
        isReloading = false;
        Debug.Log("Перезарядка завершена.");
    }

    // Метод для UI (покажем позже)
    public int GetCurrentAmmo() { return currentAmmo; }
    public int GetMaxAmmo() { return maxAmmo; }
}