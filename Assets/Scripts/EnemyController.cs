using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    Animator animator;
    // Variable related Hor, Ver movement
    public bool vertical;
    public float speed;
    Rigidbody2D rigidbody2d;

    // Variable related TimePatrol
    public float changeTime = 3.0f;
    float timer;
    int direction = 1;
    bool aggressive = true;

    //Sound Effect 
    AudioSource audioSource;
    public AudioClip fixSound;
    public AudioClip enemyHit1;
    public AudioClip enemyHit2;

    //Particle Effect
    public ParticleSystem smokeEffect;
    

    // Start is called before the first frame update
    void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        timer = changeTime;
    }

    //FixedUpdate has the same call rate as the physics system
    void FixedUpdate()
    {
        if(!aggressive){
            return;
        }

        timer -= Time.deltaTime;

        if (timer < 0){
            direction = -direction;
            timer = changeTime;
        }

        Vector2 position = rigidbody2d.position;
        
        if(vertical){
            position.y = position.y + speed * direction * Time.deltaTime;
            animator.SetFloat("Move X", 0);
            animator.SetFloat("Move Y", direction);
        }
        else{
            position.x = position.x + speed * direction * Time.deltaTime;
            animator.SetFloat("Move X", direction);
            animator.SetFloat("Move Y", 0);
        }

        rigidbody2d.MovePosition(position);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.gameObject.GetComponent<PlayerController>();

        if(player != null){
            if(aggressive){
                player.ChangeHealth(-1);
            }
        }
    }

    public void Fix()
    {
        rigidbody2d.simulated = true;
        animator.SetTrigger("Fixed");
        audioSource.Stop();
        AudioClip randomHit = Random.value > 0.5f ? enemyHit1 : enemyHit2;
        audioSource.PlayOneShot(randomHit);
        if (aggressive == true){
            audioSource.PlayOneShot(fixSound);
        }
        aggressive = false;
        smokeEffect.Stop();
    }

    public void Dead()
    {
        Destroy(gameObject);
    }
}
