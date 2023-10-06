using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerShip_NJ : InputListenerBase
{
    public float moveSpeed = 8f,
        projectileSpeed = 20f,
        projectileLifetime = 3f, // Lifetime of Projectile in seconds
        worldBorder = 0.2f,
        fireCooldown = 0.12f, // Cooldown time in seconds
        mineLifetime = 10f; // Lifetime of Mine in seconds
    private float lastFireTime = 0;
    public int maxProjectiles = 20, nbMinePlace = 0, maxMine = 5;
    public GameObject projectilePrefab, minePrefab;

    private Rigidbody2D rb;
    private Vector3 screenBounds;
    private List<GameObject> projectiles = new List<GameObject>();
    //public Projectile projectilePrefab;
    //private List<Projectile> projectiles = new List<Projectile>();
    private Vector2 _mousePositionAtFrame;
    private bool tireLaserActif = false;
    private GameObject newProjectile;
    Vector3 mouseWorldPosition;
    Vector2 fireDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        InitializeProjectilePool();
    }
    private void Start()
    {
        //if change size of window/screen in game ?
        screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
        screenBounds.x -= worldBorder;
        screenBounds.y -= worldBorder;
        //screenBoundsXMax = screenBounds.x; screenBoundsYMax = screenBounds.y;
        //Debug.Log("screenBounds: " + screenBounds); //screenBounds: (8.89, 5.00, -10.00) on 4k in Unity
    }
    private void HandleProjectileCollision(Collider2D other)
    {
        Debug.Log("Collision avec : " + other.gameObject.name);
    }
    private void Update()
    {
        //CheckProjectileLifetime();
        CheckProjectileOutOfBounds();

        if (tireLaserActif)
        {
            FireContinuousLaser();
        }
    }

    private void InitializeProjectilePool()
    {
        for (int i = 0; i < maxProjectiles; i++)
        {
            //Projectile newProjectile = Instantiate(projectilePrefab, Vector3.zero, Quaternion.identity);
            newProjectile = Instantiate(projectilePrefab, Vector3.zero, Quaternion.identity);
            newProjectile.SetActive(false);
            //newProjectile.OnCollision += HandleProjectileCollision;
            projectiles.Add(newProjectile);
        }
    }

    private void CheckProjectileOutOfBounds()
    {
        //foreach (Projectile projectile in projectiles)
        foreach (GameObject projectile in projectiles)
            {
            if (projectile.activeInHierarchy && (projectile.transform.position.x < -screenBounds.x ||
                projectile.transform.position.x > screenBounds.x ||
                projectile.transform.position.y < -screenBounds.y ||
                projectile.transform.position.y > screenBounds.y))
            {
                //projectile.DesActive();
                projectile.SetActive(false);
            }
        }
    }
    private void CheckProjectileLifetime()
    {
        //foreach (Projectile projectile in projectiles)
        foreach (GameObject projectile in projectiles)
        {
            // Decrease the remaining lifetime of the projectile
            //projectile.remainingLifetime -= Time.deltaTime;

            // Check if the projectile's lifetime has expired
            //if (projectile.remainingLifetime <= 0f)
            {
                //projectile.DesActive();
                projectile.SetActive(false);
            }
        }
    }

    public override void ProcessMouseButtonDown(int _button)
    {
        if (_button == 0) // tire simple
        {
            //Projectile newProjectile = GetInactiveProjectile();
            newProjectile = GetInactiveProjectile();

            if (newProjectile != null)
            {
                newProjectile.transform.position = transform.position; // + transform.forward;
                newProjectile.transform.rotation = transform.rotation;
                newProjectile.SetActive(true);

                mouseWorldPosition = Camera.main.ScreenToWorldPoint(new Vector3(_mousePositionAtFrame.x, _mousePositionAtFrame.y, Camera.main.nearClipPlane));
                fireDirection = (mouseWorldPosition - newProjectile.transform.position).normalized;

                Rigidbody2D _rb = newProjectile.GetComponent<Rigidbody2D>();
                _rb.velocity = fireDirection * projectileSpeed;
            }
        }
        else if (_button == 1)
        {
            tireLaserActif = true;
            //FireContinuousLaser();
        }
    }
    private void FireContinuousLaser()
    {
        // Check if enough time has passed since the last shot
        if (Time.time - lastFireTime >= fireCooldown)
        {
            lastFireTime = Time.time;
            GameObject newProjectile = GetInactiveProjectile(); // Obtenir un projectile inactif
            if (newProjectile != null)
            {
                newProjectile.transform.position = transform.position;
                newProjectile.transform.rotation = transform.rotation;
                newProjectile.SetActive(true);

                mouseWorldPosition = Camera.main.ScreenToWorldPoint(new Vector3(_mousePositionAtFrame.x, _mousePositionAtFrame.y, 0));
                fireDirection = (mouseWorldPosition - transform.position).normalized;

                Rigidbody2D _rb = newProjectile.GetComponent<Rigidbody2D>();
                _rb.velocity = fireDirection * projectileSpeed;
            }
        }
        //else { Debug.Log("FireCooldown:" + fireCooldown+ " - Time:" + Time.time + " - LastFireTime:" + lastFireTime);        }
    }
    //private Projectile GetInactiveProjectile()
    private GameObject GetInactiveProjectile()
    {
        //foreach (Projectile projectile in projectiles)
        foreach (GameObject projectile in projectiles)
        {
            if (!projectile.activeInHierarchy)
            {
                return projectile;
            }
        }
        return null;
    }

    public override void ProcessMouseButtonUp(int _button)
    {
        //Debug.Log("ProcessMouseButtonUp");
        if (_button == 1)
        {
            tireLaserActif = false;
        }
    }
    public override void ProcessMousePosition(Vector2 _mousePosition)
    {
        _mousePositionAtFrame = _mousePosition;

        Vector3 mouseWorldPosition = new Vector3(Camera.main.ScreenToWorldPoint(_mousePosition).x,
            Camera.main.ScreenToWorldPoint(_mousePosition).y, 0) - transform.position; //enlever la position actuelle du joueur compar� � 0.0
        transform.rotation = Quaternion.LookRotation(Vector3.forward, mouseWorldPosition);
    }
    public override void ProcessInputAxes(Vector2 _inputAxes)
    {
        if (Math.Abs(transform.position.x) >= 0 && Math.Abs(transform.position.x) <= screenBounds.x && Math.Abs(transform.position.y) >= 0 && Math.Abs(transform.position.y) <= screenBounds.y)
            rb.velocity = new Vector2(_inputAxes.x * moveSpeed, _inputAxes.y * moveSpeed);
        else {
            ShipOutOfBorder();
        }
    }
    private void ShipOutOfBorder()
    {
        Vector3 newPosition = transform.position;

        if (transform.position.x < -screenBounds.x)
        {
            // Si le vaisseau d�passe � gauche, le faites appara�tre � droite de l'�cran
            newPosition.x = screenBounds.x;
        }
        else if (transform.position.x > screenBounds.x)
        {
            // Si le vaisseau d�passe � droite, le faites appara�tre � gauche de l'�cran
            newPosition.x = -screenBounds.x;
        }

        if (transform.position.y < -screenBounds.y)
        {
            // Si le vaisseau d�passe en bas, le faites appara�tre en haut de l'�cran
            newPosition.y = screenBounds.y;
        }
        else if (transform.position.y > screenBounds.y)
        {
            // Si le vaisseau d�passe en haut, le faites appara�tre en bas de l'�cran
            newPosition.y = -screenBounds.y;
        }

        transform.position = newPosition;
    }
    public override void ProcessKeyCodeDown(KeyCode _keyCode)
    {
        if (_keyCode == KeyCode.Space && nbMinePlace < maxMine)
        {
            nbMinePlace++;
            GameObject mine = Instantiate(minePrefab, transform.position, Quaternion.identity);
            StartCoroutine(DestroyMineAfterDelay(mine, mineLifetime));
        }
    }
    private IEnumerator DestroyMineAfterDelay(GameObject _objectToDestroy, float _lifetime)
    {
        yield return new WaitForSeconds(_lifetime);
        if (_objectToDestroy != null)
        {
            Destroy(_objectToDestroy);
            nbMinePlace--;
        }
    }
    public override void ProcessKeyCodeUp(KeyCode _keyCode)
    {
        Debug.Log("space up");
    }

}
