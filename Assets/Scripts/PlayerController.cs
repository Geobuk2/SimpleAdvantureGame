using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    //Variables related to player chararcter movement
    public InputAction MoveAction;
    Rigidbody2D rigidbody2d;
    Vector2 move;
    public float speed = 3.0f;

    //Variable related to the health system
    public int maxHealth = 5;
    public int health { get { return currentHealth; } }
    int currentHealth;

    //Variable related to temporary invincibility
    public float timeInvincible = 2.0f;
    bool isInvinsible;
    float damageCooldown;

    // Variables related to animation
    Animator animator;
    Vector2 moveDirection = new Vector2(1, 0);

    //Varialbes related to NPC dialogue
    public GameObject projectilePrefab;
    public InputAction launchAction;

    //Start is called before the first frame update
    public InputAction talkAction;

    //In Game sounds
    AudioSource audioSource;
    public AudioClip launchSound;
    public AudioClip hitSound;
    public AudioSource footStepSound;
    private bool IsMoving;

    
    // Start is called before the first frame update
    void Start()
    {
        MoveAction.Enable();
        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        
        currentHealth = maxHealth;

        launchAction.Enable();
        launchAction.performed += Launch;

        talkAction.Enable();
        talkAction.performed += FindFriend;

        audioSource = GetComponent<AudioSource>();
        footStepSound = gameObject.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        move = MoveAction.ReadValue<Vector2>();

        if(!Mathf.Approximately(move.x, 0.0f) || !Mathf.Approximately(move.y, 0.0f)){
            moveDirection.Set(move.x, move.y);
            moveDirection.Normalize();
        }

        animator.SetFloat("Move X", moveDirection.x);
        animator.SetFloat("Move Y", moveDirection.y);
        animator.SetFloat("Speed", move.magnitude);

        if (isInvinsible){
            damageCooldown -= Time.deltaTime;
            if (damageCooldown < 0) {
                isInvinsible = false;
            }
        }



        //Getting Key Input
        if (Input.GetAxis("Vertical") != 0 || Input.GetAxis("Horizontal") != 0) {
            IsMoving = true;
        }         
        else {
            IsMoving = false;
        }

        // if player is moving and audiosource is not playing play it
        if (IsMoving && !footStepSound.isPlaying){  
            footStepSound.Play();
        }
        // if player is not moving and audiosource is playing stop it
        if (!IsMoving) { 
            footStepSound.Stop(); 
        }
    }

    //FixedUpdate has the same call rate as the physics system
    void FixedUpdate()
    {
        Vector2 move = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        Vector2 position = (Vector2)rigidbody2d.position + move * speed * Time.deltaTime;
        rigidbody2d.MovePosition(position);
    }

    public void ChangeHealth (int amount)
    {
        if (amount < 0){
            if (isInvinsible){
                return;
            }
            isInvinsible = true;
            damageCooldown = timeInvincible;
            animator.SetTrigger("Hit");
            audioSource.PlayOneShot(hitSound);
        }


        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        UIHandler.instance.SetHealthValue(currentHealth / (float)maxHealth);

        if(currentHealth <= 0){
            Destroy(gameObject);
        }
    }

    void Launch(InputAction.CallbackContext context)
    {
        GameObject projectileObject = Instantiate(projectilePrefab, rigidbody2d.position + Vector2.up * 0.5f, Quaternion.identity);
        Projectile projectile = projectileObject.GetComponent<Projectile>();
        projectile.Launch(moveDirection, 300);

        animator.SetTrigger("Launch");
        audioSource.PlayOneShot(launchSound);
    }

    void FindFriend (InputAction.CallbackContext context)
    {
        RaycastHit2D hit = Physics2D.Raycast(rigidbody2d.position + Vector2.up * 0.2f, moveDirection, 1.5f, LayerMask.GetMask("NPC"));
        
        if (hit.collider != null){
            NonPlayerCharacter character = hit.collider.GetComponent<NonPlayerCharacter>();
            
            if (character != null){
                UIHandler.instance.DisplayDialogue();
            }
        }
    }

    public void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);  
    }

}
