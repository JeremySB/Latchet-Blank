using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PlayerController : MonoBehaviour
{
    AudioSource jump;
    AudioSource jumpBoost;
    AudioSource shieldBoost;
    AudioSource damageBoost;
    AudioSource wallHit;

    public Material indicatorBlue;
    public Material indicatorRed;

    bool isDead;
    Camera cam;
    private bool shotOnCooldown;
    private CharacterController controller;
    private UIManager UI;
    private float lerpTime = 0;
    private float yVelocity;
    private GameObject latchedSurface;
    private int health;
    private Vector3 gunEnabledPosition;

    private Renderer jumpIndicatorRenderer;

    public bool isShooting;
    public bool gunEnabled;
    public enum PlayerState { Walking, Jumping, Latched };
    public enum WallBonus { None, Shield, Damage, Jump };
    public float gravity = -9.8f;
    public float jumpCooldownTime;
    public float jumpingSpeed = 45.0f;
    public float jumpRange = 130.0f;
    public float maximumVertical = 180;
    public float minimumVertical = 0;
    public float sensitivityX = 5;
    public float sensitivityY = 5;

    public float shotCooldownTime;

    public float terminalVelocity = 30;

    public float walkSpeed = 6.0f;
    public GunEffects gun;
    public ParticleSystem jumpParticles;
    public ParticleSystem shieldWallParticles;
    public ParticleSystem damageWallParticles;
    public ParticleSystem jumpWallParticles;
    public PlayerState state = PlayerState.Walking;
    Text hp;
    public Transform blanksWaypoints;
    public Transform gunBarrelPoint;
    public GameObject jumpIndicatorPrefabBlue;
    public GameObject jumpIndicatorPrefabRed;
    private GameObject jumpIndicator;
    public Transform gunTransform;
    public Transform mainCamera;
    public Vector3 jumpDirection = new Vector3();
    public WallBonus wallBonus = WallBonus.None;
    public int startingHealth = 500;

    void Start()
    {
        controller = this.GetComponent<CharacterController>();
        cam = mainCamera.transform.GetComponent<Camera>();
        AudioSource[] sources = this.GetComponents<AudioSource>();
        jumpIndicator = Instantiate(jumpIndicatorPrefabBlue);
        jumpIndicatorRenderer = jumpIndicator.GetComponent<Renderer>();
        jumpIndicatorRenderer.enabled = false;
        UI = FindObjectOfType<UIManager>();
        jump = sources[0];
        jumpBoost = sources[1];
        shieldBoost = sources[2];
        damageBoost = sources[3];
        wallHit = sources[4];
        health = startingHealth;
        shotOnCooldown = false;
        hp = FindObjectOfType<UIManager>().hpText;
        hp.text = "Health: " + health.ToString();
        isDead = false;
        gunEnabledPosition = gunTransform.localPosition;
        StartCoroutine(disableGun());
        gunEnabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isDead)
        {
            UpdateMouse();
            UpdateTeleport();
            UpdateMovement();
            UpdateShooting();
        }
        else
        {
            state = PlayerState.Walking;
            wallBonus = WallBonus.None;
            StartCoroutine(disableGun());
            isShooting = false;
            if (transform.rotation.x > -45)
            {
                transform.eulerAngles = new Vector3(Mathf.LerpAngle(transform.eulerAngles.x, -90, Time.deltaTime), transform.eulerAngles.y, transform.eulerAngles.z);

                Vector3 movement = new Vector3(0, -9.8f, 0) * Time.deltaTime;
                movement.y = Mathf.Clamp(movement.y, -terminalVelocity, float.MaxValue);

                controller.Move(movement);
            }

        }
        var rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
        blanksWaypoints.transform.rotation = rotation;

        if (health >= 0)
        {
            hp.text = "Health: " + health.ToString();
        }
    }

    private void UpdateMouse()
    {
        if (Time.timeScale < 0.01f) return;
        float rotationX = transform.localEulerAngles.x - Input.GetAxis("Mouse Y") * sensitivityY;

        if (rotationX < 270 && rotationX > 90)
        {
            rotationX = transform.localEulerAngles.x;
        }

        float rotationY = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivityX;

        transform.localEulerAngles = new Vector3(rotationX, rotationY, 0);
    }

    private void UpdateTeleport()
    {
        if (Time.timeScale < 0.01f) return;
        RaycastHit hit;
        if (Physics.SphereCast(transform.position, 0.3f, transform.forward, out hit))
        {
            if (state != PlayerState.Jumping && Input.GetKeyUp(KeyCode.Space))
            {
                jumpIndicatorRenderer.enabled = false;
                if (hit.transform.gameObject != latchedSurface && hit.transform.gameObject.tag != "Enemy" &&
                    (hit.distance <= jumpRange || wallBonus == WallBonus.Jump))
                {
                    state = PlayerState.Jumping;
                    //latchedSurface = null;
                    jumpDirection = transform.forward * jumpingSpeed;
                    lerpTime = 0;
                    jump.Play();
                    jumpParticles.Play();
                    jumpWallParticles.Stop();
                    shieldWallParticles.Stop();
                    damageWallParticles.Stop();
                    wallBonus = WallBonus.None;
                    if (!gunEnabled)
                    {
                        StartCoroutine(enableGun());
                    }
                }
            }
            else if (state != PlayerState.Jumping && Input.GetKey(KeyCode.Space))
            {
                if (hit.transform.gameObject != latchedSurface && hit.transform.gameObject.tag != "Enemy" &&
               (hit.distance <= jumpRange || wallBonus == WallBonus.Jump))
                {
                    jumpIndicatorRenderer.enabled = true;

                    if (hit.transform.gameObject.tag == "Unlatchable")
                    {
                        jumpIndicatorRenderer.material = indicatorRed;
                    }
                    else
                    {
                        jumpIndicatorRenderer.material = indicatorBlue;
                    }
                    jumpIndicator.transform.position = hit.point;
                    jumpIndicator.transform.rotation = Quaternion.LookRotation(hit.normal);
                    Debug.Log(hit.normal);
                }
                else
                {
                    jumpIndicatorRenderer.enabled = false;
                }
            }
        }
    }

    private void UpdateMovement()
    {
        if (Time.timeScale < 0.01f) return;
        float deltaX = Input.GetAxis("Horizontal");
        float deltaZ = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(deltaX, 0, deltaZ);

        if (state == PlayerState.Jumping)
        {
            transform.forward = jumpDirection;
            movement = jumpDirection;
            cam.fieldOfView = Mathf.Lerp(60, 75, lerpTime);
            lerpTime += Time.deltaTime;
            movement *= Time.deltaTime;

        }

        else if (state == PlayerState.Walking)
        {
            movement = Vector3.Normalize(movement) * walkSpeed;
            movement *= Time.deltaTime;
            movement = transform.TransformDirection(movement);

            yVelocity += gravity * Time.deltaTime;
            movement.y = yVelocity;
        }

        else if (state == PlayerState.Latched)
        {
            movement = Vector3.zero;
            if (Input.GetAxis("Unlatch") != 0)
            {
                state = PlayerState.Walking;
                wallBonus = WallBonus.None;
                jumpWallParticles.Stop();
                shieldWallParticles.Stop();
                damageWallParticles.Stop();
                StartCoroutine(disableGun());
                isShooting = false;
                latchedSurface = null;
            }
        }
        controller.Move(movement);
    }

    private void UpdateShooting()
    {
        if (Time.timeScale < 0.01f)
        {
            isShooting = false;
            return;
        };
        if (wallBonus == WallBonus.Damage)
        {
            gun.playerDamage = 30;
        }
        else
        {
            gun.playerDamage = 15;
        }

        if (Input.GetButtonDown("Fire1") && wallBonus != WallBonus.Shield && state != PlayerState.Walking)
        {
            isShooting = true;
        }
        else if (Input.GetButtonUp("Fire1"))
        {
            isShooting = false;
        }
        if (isShooting && !shotOnCooldown)
        {
            Vector3 point = new Vector3(cam.pixelWidth / 2, cam.pixelHeight / 2, 0);
            Ray ray = cam.ScreenPointToRay(point);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                Quaternion targetRotation = Quaternion.LookRotation(gunBarrelPoint.position - hit.point);
                gunBarrelPoint.rotation = targetRotation;
                gunBarrelPoint.Rotate(Random.Range(-1, 2), Random.Range(-1, 2), Random.Range(-1, 2));
                GameObject hitObject = hit.transform.gameObject;
                gun.fireBullet();
            }
            else
            {
                gun.fireBullet();
            }
            shotOnCooldown = true;
            StartCoroutine(cooldown());
        }
    }

    private IEnumerator cooldown()
    {
        yield return new WaitForSeconds(shotCooldownTime);
        shotOnCooldown = false;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {

        if (state == PlayerState.Jumping && hit.transform.gameObject != latchedSurface)
        {
            cam.fieldOfView = 60;
            jumpParticles.Stop();
            wallHit.Play();
            if (hit.transform.tag == "Unlatchable" || hit.transform.tag == "Enemy")
            {
                state = PlayerState.Walking;
                latchedSurface = null;
                if (gunEnabled)
                {
                    StartCoroutine(disableGun());
                }
            }
            else
            {
                state = PlayerState.Latched;
                latchedSurface = hit.gameObject;
                switch (latchedSurface.tag)
                {
                    case "ShieldWall":
                        wallBonus = WallBonus.Shield;
                        shieldBoost.Play();
                        shieldWallParticles.Play();
                        StartCoroutine(disableGun());
                        break;
                    case "DamageWall":
                        wallBonus = WallBonus.Damage;
                        damageBoost.Play();
                        damageWallParticles.Play();
                        break;
                    case "JumpWall":
                        jumpBoost.Play();
                        jumpWallParticles.Play();
                        wallBonus = WallBonus.Jump;
                        break;
                    default:
                        wallBonus = WallBonus.None;
                        break;
                }
            }
        }
        if (controller.collisionFlags == CollisionFlags.Below)
        {
            yVelocity = 0;
        }
    }

    public void Damage(int damage)
    {
        if (!isDead && wallBonus != WallBonus.Shield)
        {
            UI.DamageFlash();
            health -= damage;
        }
        if (health <= 0)
        {
            isDead = true;
            Messenger.Broadcast(GameEvent.PLAYER_DEATH);
            StartCoroutine(death());
        }
    }

    private IEnumerator death()
    {
        yield return new WaitForSeconds(1.5f);
    }

    private IEnumerator disableGun()
    {
        Vector3 startingPos = gunTransform.transform.localPosition;
        float elapsedTime = 0f;
        gunEnabled = false;
        while (elapsedTime < 1.0f)
        {
            gunTransform.transform.localPosition = new Vector3(gunEnabledPosition.x, Mathf.Lerp(startingPos.y, gunEnabledPosition.y - 2.0f, elapsedTime / 1.0f), gunEnabledPosition.z);
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }
    private IEnumerator enableGun()
    {
        Vector3 startingPos = gunTransform.transform.localPosition;
        float elapsedTime = 0f;
        gunEnabled = true;
        while (elapsedTime < 1.0f)
        {
            gunTransform.transform.localPosition = new Vector3(gunEnabledPosition.x, Mathf.Lerp(startingPos.y, gunEnabledPosition.y, elapsedTime / 1.0f), gunEnabledPosition.z);
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }
}
