using UnityEngine;
using UnityEngine.Tilemaps;

public class GenericEnemy : MonoBehaviour
{
    private GameManager GameManager;
    public float startingHealth = 15;
    private float health;
    public float MovementSpeed = 5;
    public float MovementFriction = 0.05f;
    public float Damage = 1;
    public float DamageCooldown = 0.25f;
    public bool UseBullets = false;
    public GameObject bullet;
    private float lastDamage = 0;
    private bool dead = false;
    private Rigidbody2D rb;
    private Vector3 CharacterVelocity = Vector3.zero;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        GameManager = FindAnyObjectByType<GameManager>();
        health = startingHealth;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 TargetVelocity = GameManager.getFlowDirection(transform.position);
        rb.linearVelocity = rb.linearVelocity + new Vector2(TargetVelocity.x*MovementSpeed*Time.deltaTime*GameManager.TimeScale ,TargetVelocity.y*MovementSpeed*Time.deltaTime*GameManager.TimeScale);
        rb.linearVelocity = rb.linearVelocity / (1+(MovementFriction*Time.deltaTime));

        if (rb.linearVelocity.sqrMagnitude > 0f)
        {
            float rotation = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0f, 0f, rotation);
        }

        if (dead == false)
        {
            if (Time.time - DamageCooldown/GameManager.TimeScale > lastDamage)
            {
                if (UseBullets == true)
                {
                    GameObject newBullet = Instantiate(
                        bullet,
                        transform.position + transform.forward*0.5f,
                        transform.rotation
                    );
                }
                else
                {                    
                    TileBase TargetTile2 = GameManager.getTile2(transform.position, new Vector3Int(Mathf.RoundToInt(TargetVelocity.x), Mathf.RoundToInt(TargetVelocity.y), 0));

                    if (TargetTile2 != null)
                    {
                        GameManager.damageTile(transform.position, new Vector3Int(Mathf.RoundToInt(TargetVelocity.x), Mathf.RoundToInt(TargetVelocity.y), 0), 1);
                    }
                }
                lastDamage = Time.time;
            }

            if ((transform.position - GameManager.TargetObject.transform.position).magnitude < GameManager.baseDamageRange)
            {
                dead = true;
                GameManager.DamageBase(Damage);
                Destroy(gameObject);
                GameManager.enemiesRemaining -= 1;
            }
        }

    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Bullet")
        {
            if (dead == false)
            {
                GenericBullet BulletScript = collision.gameObject.GetComponent<GenericBullet>();
                health -= BulletScript.damage;
                Destroy(collision.gameObject);
                if (health <= 0)
                {
                    Destroy(gameObject);
                    GameManager.enemiesRemaining -= 1;
                    dead = true;
                }
                else
                {
                    GameManager.AddCash(Mathf.RoundToInt(BulletScript.damage/6));
                }
            }
        }
    }
}
