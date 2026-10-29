using Unity.VisualScripting;
using UnityEngine;

public class GenericTurret : MonoBehaviour
{
    private GameManager GameManager;
    public float idleRotation = 0.5f;
    public float range = 50;
    public float fireRate = 0.15f;
    public GameObject Bullet;
    private GameObject Target;
    private GameObject EnemyContainer;
    private float lastFire = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager = FindAnyObjectByType<GameManager>();
        EnemyContainer = GameObject.FindWithTag("EnemyContainer");
    }

    // Update is called once per frame
    void Update()
    {
        //find closest enemy
        float closestDistance = range;
        Transform ClosestEnemy = null;
        foreach (Transform child in EnemyContainer.transform)
        {
            float magnitude = (child.position - transform.position).sqrMagnitude;
            //Debug.Log(magnitude);
            if (magnitude < closestDistance)
            {
                closestDistance = magnitude;
                ClosestEnemy = child;
            }
        }
        if (ClosestEnemy != null)
        {
            Target = ClosestEnemy.gameObject;
        }
        else
        {
            Target = null;
        }

        if (Target == null)
        {
            transform.Rotate(0, 0, idleRotation * Time.deltaTime*GameManager.TimeScale);
        }
        else
        {
            Vector3 direction = Target.transform.position - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);

            if (Time.time - fireRate/GameManager.TimeScale > lastFire)
            {
                GameObject newBullet = Instantiate(
                    Bullet,
                    transform.position + transform.forward*0.5f,
                    transform.rotation
                );

                lastFire = Time.time;
            }
        }
    }
}
