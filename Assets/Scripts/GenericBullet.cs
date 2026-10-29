using UnityEngine;
using UnityEngine.Tilemaps;

public class GenericBullet : MonoBehaviour
{
    private GameManager GameManager;
    public float damage = 6;
    public float range = 60;
    public float speed = 2f;
    private float travelledDistance = 0;
    public bool EnemyBullet;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager = FindAnyObjectByType<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        travelledDistance += speed * GameManager.TimeScale * Time.deltaTime;
        transform.position += transform.up * speed * GameManager.TimeScale * Time.deltaTime;

        if (EnemyBullet == true)
        {
            TileBase TargetTile2 = GameManager.getTile2(transform.position, new Vector3Int(0, 0, 0));

            if (TargetTile2 != null)
            {
                GameManager.damageTile(transform.position, new Vector3Int(0, 0, 0), damage);
                Destroy(gameObject);
            }
        }

        if (travelledDistance > range)
        {
            Destroy(gameObject);
        }
    }
}
