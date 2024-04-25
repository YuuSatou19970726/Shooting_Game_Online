using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private Transform viewPoint;
    public float mouseSensitivity = 1f;

    private float verticalRotStore;
    private Vector2 mouseInput;

    public bool invertLook;

    public float moveSpeed = 5f, runSpeed = 8f;
    private float activeMoveSpeed;
    private Vector3 moveDir, movement;

    [SerializeField]
    CharacterController characterController;

    private new Camera camera;

    public float jumpForce = 12f, gravityMod = 2.5f;

    [SerializeField]
    private Transform groundCheckPoint;
    private bool isGrounded;
    [SerializeField]
    private LayerMask groundLayers;

    [SerializeField]
    private GameObject bulletImpact;
    // public float timerBetweenShots = 0.1f;
    private float shotCounter;
    public float muzzleDisplayTime;
    private float muzzleCounter;

    public float maxHeat = 10f, /*heatPerShot = 1f*/ coolRate = 4f, overheatCoolRate = 5f;
    private float heatCounter;
    private bool overHeated;

    public Gun[] allGuns;
    private int selectedGun;

    [SerializeField]
    private GameObject playerHit;

    public int maxHealth = 100;
    private int currentHealth;

    [SerializeField]
    private Animator anim;
    [SerializeField]
    private GameObject playerModel;
    [SerializeField]
    private Transform modelGunPoint, gunHolder;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        camera = Camera.main;
        UIController.instance.weaponTempSlider.maxValue = maxHeat;

        // SwitchGun();
        photonView.RPC(nameof(SetGun), RpcTarget.All, selectedGun);

        currentHealth = maxHealth;

        // Transform newTrans = SpawnManager.instance.GetSpawnPoint();
        // transform.position = newTrans.position;
        // transform.rotation = newTrans.rotation;

        if (photonView.IsMine)
        {
            playerModel.SetActive(false);
            UIController.instance.healthSlider.maxValue = maxHealth;
            UIController.instance.healthSlider.value = currentHealth;
        }
        else
        {
            gunHolder.parent = modelGunPoint;
            gunHolder.localPosition = Vector3.zero;
            gunHolder.localRotation = Quaternion.identity;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            Rotation();
            Movement();
            Event();
        }
    }

    void LateUpdate()
    {
        MoveToCamera();
    }

    void Rotation()
    {
        mouseInput = new Vector2(Input.GetAxisRaw(MouseAxis.MOUSE_X), Input.GetAxisRaw(MouseAxis.MOUSE_Y)) * mouseSensitivity;
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + mouseInput.x, transform.rotation.eulerAngles.z);


        verticalRotStore += mouseInput.y;
        verticalRotStore = Mathf.Clamp(verticalRotStore, -60f, 60f);
        if (invertLook)
            viewPoint.rotation = Quaternion.Euler(verticalRotStore, viewPoint.rotation.eulerAngles.y, viewPoint.rotation.eulerAngles.z);
        else
            viewPoint.rotation = Quaternion.Euler(-verticalRotStore, viewPoint.rotation.eulerAngles.y, viewPoint.rotation.eulerAngles.z);
    }

    void Movement()
    {
        moveDir = new Vector3(Input.GetAxisRaw(Axis.HORIZONTAL), 0f, Input.GetAxisRaw(Axis.VERTICAL));

        if (Input.GetKey(KeyCode.LeftShift))
        {
            activeMoveSpeed = runSpeed;
        }
        else
        {
            activeMoveSpeed = moveSpeed;
        }

        float yVel = movement.y;
        movement = ((transform.forward * moveDir.z) + (transform.right * moveDir.x)).normalized * activeMoveSpeed;
        movement.y = yVel;

        if (characterController.isGrounded)
            movement.y = 0f;

        isGrounded = Physics.Raycast(groundCheckPoint.position, Vector3.down, 0.25f, groundLayers);

        if (Input.GetButtonDown(GetButton.BUTTON_JUMP) && isGrounded)
        {
            movement.y = jumpForce;
        }

        movement.y += Physics.gravity.y * Time.deltaTime * gravityMod;

        // transform.position += movement * moveSpeed * Time.deltaTime;
        characterController.Move(movement * Time.deltaTime);

        anim.SetBool(Animations.GROUNDED_BOOL, isGrounded);
        anim.SetFloat(Animations.SPEED_FLOAT, moveDir.magnitude);
    }

    private void Event()
    {
        if (allGuns[selectedGun].muzzleFlash.activeInHierarchy)
        {
            muzzleCounter -= Time.deltaTime;

            if (muzzleCounter <= 0)
                allGuns[selectedGun].muzzleFlash.SetActive(false);
        }

        if (!overHeated)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Shoot();
            }

            if (Input.GetMouseButton(0))
            {
                shotCounter -= Time.deltaTime;
                if (shotCounter <= 0)
                {
                    Shoot();
                }
            }

            heatCounter -= coolRate * Time.deltaTime;
        }
        else
        {
            heatCounter -= overheatCoolRate * Time.deltaTime;
            if (heatCounter <= 0)
            {
                overHeated = false;
                UIController.instance.overheatedMessage.gameObject.SetActive(false);
            }
        }

        if (heatCounter < 0)
            heatCounter = 0;

        UIController.instance.weaponTempSlider.value = heatCounter;

        if (Input.GetAxisRaw(MouseAxis.MOUSE_SCROLLWHEEL) > 0f)
        {
            selectedGun++;

            if (selectedGun >= allGuns.Length)
            {
                selectedGun = 0;
            }

            // SwitchGun();
            photonView.RPC(nameof(SetGun), RpcTarget.All, selectedGun);
        }
        else if (Input.GetAxisRaw(MouseAxis.MOUSE_SCROLLWHEEL) < 0f)
        {
            selectedGun--;

            if (selectedGun < 0)
            {
                selectedGun = allGuns.Length - 1;
            }

            // SwitchGun();
            photonView.RPC(nameof(SetGun), RpcTarget.All, selectedGun);
        }

        for (int i = 0; i < allGuns.Length; i++)
        {
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                selectedGun = i;
                // SwitchGun();
                photonView.RPC(nameof(SetGun), RpcTarget.All, selectedGun);
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else if (Cursor.lockState == CursorLockMode.None)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }

    void Shoot()
    {
        Ray ray = camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        ray.origin = camera.transform.position;

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Debug.Log("Hit: " + hit.collider.gameObject);

            if (hit.collider.gameObject.CompareTag(Tags.PLAYER_TAG))
            {
                PhotonNetwork.Instantiate(playerHit.name, hit.point, Quaternion.identity);
                hit.collider.gameObject.GetPhotonView().RPC(nameof(DealDamage), RpcTarget.All, photonView.Owner.NickName, allGuns[selectedGun].shotDamage);
            }
            else
            {
                GameObject bulletImpactObject = Instantiate(bulletImpact, hit.point + (hit.normal * 0.002f), Quaternion.LookRotation(hit.normal, Vector3.up));
                Destroy(bulletImpactObject, 10f);
            }
        }

        shotCounter = allGuns[selectedGun].timeBetweenShots;

        heatCounter += allGuns[selectedGun].heatPerShot;
        if (heatCounter >= maxHeat)
        {
            heatCounter = maxHeat;
            overHeated = true;

            UIController.instance.overheatedMessage.gameObject.SetActive(true);
        }

        allGuns[selectedGun].muzzleFlash.SetActive(true);
        muzzleCounter = muzzleDisplayTime;
    }

    void SwitchGun()
    {
        foreach (Gun gun in allGuns)
        {
            gun.gameObject.SetActive(false);
        }

        allGuns[selectedGun].gameObject.SetActive(true);
    }

    void MoveToCamera()
    {
        if (photonView.IsMine)
        {
            camera.transform.position = viewPoint.position;
            camera.transform.rotation = viewPoint.rotation;
        }
    }

    void TakeDamage(string damageByPlayer, int damageAmount)
    {
        if (photonView.IsMine)
        {
            currentHealth -= damageAmount;
            if (currentHealth <= 0)
            {
                PlayerSpawner.instance.Die(damageByPlayer);
            }
            UIController.instance.healthSlider.value = currentHealth;
        }

    }

    // PUN
    [PunRPC]
    public void DealDamage(string damageByPlayer, int damageAmount)
    {
        TakeDamage(damageByPlayer, damageAmount);
    }

    [PunRPC]
    public void SetGun(int gunToSwitchTo)
    {
        if (gunToSwitchTo < allGuns.Length)
        {
            selectedGun = gunToSwitchTo;
            SwitchGun();
        }
    }

}
