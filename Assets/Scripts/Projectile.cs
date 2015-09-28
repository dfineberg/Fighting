using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour {

    public float speed;
    public float lifetime;
    public float knockbackForce;
    public float stunTime;
    public float damage;
    public Vector2 localPos;
    [HideInInspector]
    public Direction direction;

    int hitTrigger;
    float timer = 0f;
    new Rigidbody2D rigidbody;
    Animator animator;
    new Collider2D collider;


    void Start()
    {
        hitTrigger = Animator.StringToHash("Hit");
        rigidbody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        collider = GetComponent<Collider2D>();
    }



    void Update()
    {
        if (timer >= lifetime)
        {
            DestroyMe();
        }
        else
        {
            transform.Translate(Vector2.right * speed * Time.deltaTime);
            timer += Time.deltaTime;
        }
    }



    void Hit()
    {
        animator.SetTrigger(hitTrigger);
        rigidbody.velocity = Vector2.zero;
        collider.enabled = false;
    }



    public void DestroyMe()
    {
        Destroy(gameObject);
    }



    void OnTriggerEnter2D(Collider2D col)
    {
        Hit();

        if (col.gameObject.CompareTag("Player"))
        {
            CharacterController hitChar = col.GetComponent<CharacterController>();
            hitChar.Stun(DirectionUtility.GetVector(direction) * knockbackForce, stunTime);
        }
    }
}
