using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

[System.Serializable]
public class BuildableTiles
{
    public String Title;
    public TileBase tile;
    public GameObject prefab;
    public int price;
    public int tileHealth;
}
[System.Serializable]
public class EnemySpawn
{
    public GameObject EnemyPrefab;
    public int Count;
    public int alreadySpawned = 0;
}
[System.Serializable]
public class WaveEnemies
{
    public EnemySpawn[] Enemies;
    public float healthBoost = 1;
}
public class GameManager : MonoBehaviour
{
    public WaveEnemies[] waveSpawns =
    {
        new WaveEnemies {},
    };
    public GameObject StartObject;
    public GameObject TargetObject;
    public Tilemap tilemap_Rock;
    public Tilemap tilemap_Buildings;
    public GameObject DebugFlowArrow;
    public GameObject FlowDebugContainer;
    public GameObject LightmapTile;
    public GameObject LightmapContainer;
    public GameObject PrefabContainer;
    public GameObject EnemyContainer;
    public bool renderDebug;
    public TextMeshProUGUI CashUI;
    public TextMeshProUGUI WaveUI;
    public TextMeshProUGUI SelectedItem;
    public Transform BaseHealthBar;
    public GameObject ReadyButton;
    public GameObject DoubleSpeedButton;
    public TextMeshProUGUI DoubleSpeedText;
    public int cash = 100;
    public int selectedItem = 0;
    public bool waveEnd = true;
    public bool Ready = false;
    public float TimeScale = 1;

    public int wave = 0;
    public float waveStartDelay = 10;
    private float waveEndTime = 0;
    public int currentWaveElement = 0;
    public int enemiesRemaining = 0;
    private float lastEnemySpawned = 0;
    private bool allEnemiesSpawned = true;
    public float MaxBaseHealth = 30;
    public float baseHealth;
    public float baseDamageRange = 3;

    public float debugNumber = 0;



    private Camera cam;
    private Dictionary<Vector3Int, float> tileHealth = new();
    private Dictionary<Vector3Int, float> tileLight = new();
    private Dictionary<Vector3Int, Vector2> flowMap = new();
    public Dictionary<Vector3Int, Vector2> FlowMap => flowMap;
    public BuildableTiles[] Items =
    {
    };


    public Vector2 getFlowDirection(Vector3 pos)
    {
        if (flowMap.ContainsKey(tilemap_Rock.WorldToCell(pos)))
        {
            return flowMap[tilemap_Rock.WorldToCell(pos)];
        }
        else
        {
            return Vector2.zero;
        }
    }
    public void AddCash(int value)
    {
        cash += value;
    }
    public void DamageBase(float value)
    {
        baseHealth -= value;
        if (baseHealth < 0)
        {
            baseHealth = 0;
            string currentSceneName = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(currentSceneName);
        }
        BaseHealthBar.localPosition = new Vector3(-(1-(baseHealth/MaxBaseHealth))*0.5f,0.75f,0);
        BaseHealthBar.localScale = new Vector3(1-(1-(baseHealth/MaxBaseHealth)),0.1f,1);
        //Debug.Log(baseHealth);
    }
    public TileBase getTile1(Vector3 pos, Vector3Int tileOffset)
    {
        return tilemap_Rock.GetTile(tilemap_Rock.WorldToCell(pos)+tileOffset);
    }
    public TileBase getTile2(Vector3 pos, Vector3Int tileOffset)
    {
        return tilemap_Buildings.GetTile(tilemap_Buildings.WorldToCell(pos)+tileOffset);
    }
    public void PressReady()
    {
        ReadyButton.SetActive(false);
        DoubleSpeedButton.SetActive(true);
        DoubleSpeedText.text = ">";
        TimeScale = 1;
        Ready = true;
    }
    public void PressDoubleSpeed()
    {
        if (TimeScale == 1)
        {
            TimeScale = 2;
            DoubleSpeedText.text = ">>";
        }
        else if (TimeScale == 2)
        {
            TimeScale = 4;
            DoubleSpeedText.text = ">>>>";
        }
        else
        {
            TimeScale = 1;
            DoubleSpeedText.text = ">";
        }
    }

    void createFlowMap()
    {
        //reset flowmap
        foreach (Vector3Int position in tilemap_Rock.cellBounds.allPositionsWithin)
        {
            flowMap[position] = Vector2.zero;
        }

        flowMap[tilemap_Rock.WorldToCell(TargetObject.transform.position)] = Vector2.up;
        //Vector3 pos = TargetObject.transform.position;
        //Debug.Log(pos);
        //Vector3Int CurrentCell = tilemap.WorldToCell(pos);
        //Debug.Log(CurrentCell);
        //TileBase tile = tilemap.GetTile(CurrentCell);
        //Debug.Log(tile);

        //loop over everything in the tilemap and update the flowmap until every tile has been touched
        void CheckAllTiles(bool ignoreAllBuilds)
        {
            bool tileChanged = true;
            while (tileChanged == true)
            {
                tileChanged = false;
                
                foreach (Vector3Int position in tilemap_Rock.cellBounds.allPositionsWithin)
                {
                    TileBase tile = tilemap_Rock.GetTile(position);
                    TileBase tile2 = tilemap_Buildings.GetTile(position);

                    bool allowPosition = true;

                    if (ignoreAllBuilds == true)
                    {
                        if (tile != null)
                        {
                            allowPosition = false;
                        }
                    }
                    else
                    {
                        if (tile != null || tile2 != null)
                        {
                            allowPosition = false;
                        }
                    }




                    if (allowPosition == true)
                    {
                        if (flowMap[position] == Vector2.zero)
                        {
                            if (flowMap.ContainsKey(position + new Vector3Int(1,0,0)))
                            {
                                if (flowMap[position + new Vector3Int(1,0,0)] != Vector2.zero)
                                {
                                    flowMap[position] = Vector2.right;
                                    tileChanged = true;
                                    continue;
                                }
                            }
                            if (flowMap.ContainsKey(position + new Vector3Int(0,1,0)))
                            {
                                if (flowMap[position + new Vector3Int(0,1,0)] != Vector2.zero)
                                {
                                    flowMap[position] = Vector2.up;
                                    tileChanged = true;
                                    continue;
                                }
                            }
                            if (flowMap.ContainsKey(position + new Vector3Int(-1,0,0)))
                            {
                                if (flowMap[position + new Vector3Int(-1,0,0)] != Vector2.zero)
                                {
                                    flowMap[position] = Vector2.left;
                                    tileChanged = true;
                                    continue;
                                }
                            }
                            if (flowMap.ContainsKey(position + new Vector3Int(0,-1,0)))
                            {
                                if (flowMap[position + new Vector3Int(0,-1,0)] != Vector2.zero)
                                {
                                    flowMap[position] = Vector2.down;
                                    tileChanged = true;
                                    continue;
                                }
                            }

                            //flowMapDebug();
                            //yield return new WaitForSeconds(0.002f);
                        }
                    }
                    
                    // Do something with the tile
                    //Debug.Log($"Tile at {position}: {tile.name}");
                }
            }
        }

        CheckAllTiles(false);
        CheckAllTiles(true);
        
        
        //Vector3 pos = TargetObject.transform.position;
        //Debug.Log(pos);
        //Vector3Int CurrentCell = tilemap.WorldToCell(pos);
        //Debug.Log(CurrentCell);
        //TileBase tile = tilemap.GetTile(CurrentCell);
        //Debug.Log(tile);
        
    }
    void flowMapDebug()
    {
        foreach (Transform child in FlowDebugContainer.transform) {
			GameObject.Destroy(child.gameObject);
		}
        foreach (Vector3Int position in tilemap_Rock.cellBounds.allPositionsWithin)
        {
            if (flowMap[position] != Vector2.zero)
            {
                GameObject arrow = Instantiate(
                    DebugFlowArrow,
                    tilemap_Rock.CellToWorld(position) + new Vector3(0.125f,0.125f,0),
                    Quaternion.identity
                );
                arrow.transform.SetParent(FlowDebugContainer.transform);

                Vector2 Direction = flowMap[position];
                if (Direction == Vector2.right)
                {
                    arrow.transform.rotation = Quaternion.Euler(0,0,0);
                }
                if (Direction == Vector2.left)
                {
                    arrow.transform.rotation = Quaternion.Euler(0,0,180);
                }
                if (Direction == Vector2.up)
                {
                    arrow.transform.rotation = Quaternion.Euler(0,0,90);
                }
                if (Direction == Vector2.down)
                {
                    arrow.transform.rotation = Quaternion.Euler(0,0,-90);
                }
            }
        }
    }

    void updateLightmap()
    {
        //default values
        foreach (Vector3Int position in tilemap_Rock.cellBounds.allPositionsWithin)
        {
            TileBase tile = tilemap_Rock.GetTile(position);
            TileBase tile2 = tilemap_Buildings.GetTile(position);
            bool isRock = false;
            if (tile != null || tile2 != null)
            {
                isRock = true;
            }
            if (isRock == false)
            {
                tileLight[position] = 1;
            }
            else
            {
                tileLight[position] = 0;
            }
        }


        void CheckAllTiles()
        {


            bool tileChanged = true;
            while (tileChanged == true)
            {
                tileChanged = false;
                
                foreach (Vector3Int position in tilemap_Rock.cellBounds.allPositionsWithin)
                {
                    TileBase tile = tilemap_Rock.GetTile(position);
                    TileBase tile2 = tilemap_Buildings.GetTile(position);

                    bool isRock = false;

                    if (tile != null || tile2 != null)
                    {
                        isRock = true;
                    }

                    if (isRock == true)
                    {
                        {
                            float highestBrightness = 0;

                            void checkNearbyTile(Vector3Int checkPosition)
                            {
                                if (tileLight.TryGetValue(checkPosition, out float nearbyBrightness) && nearbyBrightness > highestBrightness)
                                {
                                    highestBrightness = nearbyBrightness;
                                }
                            }

                            checkNearbyTile(position + new Vector3Int(1,0,0));
                            checkNearbyTile(position + new Vector3Int(0,1,0));
                            checkNearbyTile(position + new Vector3Int(-1,0,0));
                            checkNearbyTile(position + new Vector3Int(0,-1,0));

                            float propagatedBrightness = 0;
                            if (highestBrightness < 0.25f)
                            {
                                propagatedBrightness = Mathf.Max(0, highestBrightness - 0.025f);
                            }
                            else
                            {
                                propagatedBrightness = Mathf.Max(0, highestBrightness - 0.1f);
                            }
                            if (propagatedBrightness > tileLight[position])
                            {
                                tileLight[position] = propagatedBrightness;
                                tileChanged = true;
                            }
                        }
                        /*if (tileLight[position] == 2)
                        {
                            bool foundNearby = false;
                            float highestBrighness = 0;
                            Vector3Int highestBrightnessPosition = Vector3Int.zero;

                            void checkNearbyTile(Vector3Int checkPosition)
                            {
                                if (tileLight.ContainsKey(checkPosition))
                                {
                                    if (tileLight[checkPosition] > highestBrighness)
                                    {
                                        if (tileLight[checkPosition] != 2)
                                        {
                                            highestBrightnessPosition = checkPosition;
                                            highestBrighness = tileLight[checkPosition];
                                            tileChanged = true;
                                            foundNearby = true;
                                        }

                                    }
                                }

                                return;
                            }
                            
                            checkNearbyTile(position + new Vector3Int(1,0,0));
                            checkNearbyTile(position + new Vector3Int(0,1,0));
                            checkNearbyTile(position + new Vector3Int(-1,0,0));
                            checkNearbyTile(position + new Vector3Int(0,-1,0));

                            if (foundNearby == true)
                            {
                                tileLight[position] = tileLight[highestBrightnessPosition] - 0.1f;
                            }
                            //flowMapDebug();
                            //yield return new WaitForSeconds(0.002f);
                        }*/
                    }
                    
                    // Do something with the tile
                    //Debug.Log($"Tile at {position}: {tile.name}");
                }
            }
        }

        CheckAllTiles();




        foreach (Transform child in LightmapContainer.transform) {
			GameObject.Destroy(child.gameObject);
		}
        foreach (Vector3Int position in tilemap_Rock.cellBounds.allPositionsWithin)
        {
            //int lightRange = 5;

            GameObject LightTile = Instantiate(
                LightmapTile,
                tilemap_Rock.CellToWorld(position) + new Vector3(0.125f,0.125f,0),
                Quaternion.identity
            );
            LightTile.transform.SetParent(LightmapContainer.transform);

            SpriteRenderer lightTileRenderer = LightTile.GetComponent<SpriteRenderer>();
            Color lightTileColor = lightTileRenderer.color;
            lightTileColor.a = 1 - Mathf.Clamp01(tileLight[position]);
            //Debug.Log(tileLight[position]);
            lightTileRenderer.color = lightTileColor;

        }

    }
    public bool damageTile(Vector3 pos, Vector3Int tileOffset, float damage)
    {
        Vector3Int position = tilemap_Buildings.WorldToCell(pos)+tileOffset;
        TileBase tile2 = tilemap_Buildings.GetTile(position);
        tileHealth[position] -= damage;

        //Debug.Log(tileHealth[position]);
        if (tileHealth[position] < 0)
        {
            tileHealth[position] = 0;
            tilemap_Buildings.SetTile(position, null);
            foreach (Transform child in PrefabContainer.transform) {
                if (child.position-new Vector3(0.125f,0.125f,0) == tilemap_Rock.CellToWorld(position))
                {
                    Destroy(child.gameObject);
                }
            }

            createFlowMap();
            if (renderDebug == true)
            {
                flowMapDebug();
            }
        }
        return false;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cam = Camera.main;
        //StartCoroutine(createFlowMap());
        createFlowMap();
        updateLightmap();
        if (renderDebug == true)
        {
            flowMapDebug();
        }

        //set tile health
        foreach (Vector3Int position in tilemap_Rock.cellBounds.allPositionsWithin)
        {
            tileHealth[position] = 0;
        }
        baseHealth = MaxBaseHealth;
        
    }

    // Update is called once per frame
    void Update()
    {

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if ( scroll != 0f )
        {
            if ( scroll > 0 )
            {
                selectedItem ++;
            }
            else
            {
                selectedItem --;
            }
            if (selectedItem > Items.Length-1)
            {
                selectedItem = 0;
            }
            if (selectedItem < 0)
            {
                selectedItem = Items.Length-1;
            }
        }

        if (Input.GetMouseButton(0))
        {
            if (waveEnd == true)
            {
                Vector3Int MouseTilePos = tilemap_Buildings.WorldToCell(cam.ScreenToWorldPoint(Input.mousePosition));
                TileBase tile = tilemap_Rock.GetTile(MouseTilePos);
                TileBase tile2 = tilemap_Buildings.GetTile(MouseTilePos);
                
                if (tile == null)
                {
                    if (tile2 == null)
                    {
                        if (cash >= Items[selectedItem].price)
                        {
                            //Debug.Log(Items[0].tile);
                            if (Items[selectedItem].tile != null)
                            {
                                tilemap_Buildings.SetTile(MouseTilePos, Items[selectedItem].tile);
                            }
                            if (Items[selectedItem].prefab != null)
                            {
                                GameObject prefab = Instantiate(
                                    Items[selectedItem].prefab,
                                    tilemap_Rock.CellToWorld(MouseTilePos) + new Vector3(0.125f,0.125f,0),
                                    Quaternion.identity
                                );
                                prefab.transform.SetParent(PrefabContainer.transform);
                            }
                            
                            tileHealth[MouseTilePos] = Items[selectedItem].tileHealth;
                            cash -= Items[selectedItem].price;

                            //createFlowMap();
                            //if (renderDebug == true)
                            //{
                            //    flowMapDebug();
                            //}
                        }
                    }
                }
            }
        }
        if (Input.GetMouseButton(1))
        {
            Vector3Int MouseTilePos = tilemap_Buildings.WorldToCell(cam.ScreenToWorldPoint(Input.mousePosition));
            TileBase tile = tilemap_Rock.GetTile(MouseTilePos);
            TileBase tile2 = tilemap_Buildings.GetTile(MouseTilePos);

            if (tile2 != null)
            {
                tilemap_Buildings.SetTile(MouseTilePos, null);
                foreach (Transform child in PrefabContainer.transform) {
                    if (child.position-new Vector3(0.125f,0.125f,0) == tilemap_Rock.CellToWorld(MouseTilePos))
			        {
                        Destroy(child.gameObject);
                    }
		        }
                cash += 1;
            }

            createFlowMap();
            if (renderDebug == true)
            {
                flowMapDebug();
            }
        }

        if (allEnemiesSpawned == false)
        {
            if (Time.time - 0.1 > lastEnemySpawned)
            {
                Transform RandomSpawn = StartObject.transform.GetChild(
                    UnityEngine.Random.Range(0, StartObject.transform.childCount)
                );
                
                GameObject enemy = Instantiate(
                    waveSpawns[wave-1].Enemies[currentWaveElement].EnemyPrefab,
                    RandomSpawn.transform.position + RandomSpawn.transform.up*UnityEngine.Random.Range(-0.5f, 0.5f),
                    Quaternion.identity
                );
                enemy.transform.SetParent(EnemyContainer.transform);
                enemy.GetComponent<GenericEnemy>().startingHealth *= waveSpawns[wave-1].healthBoost;

                waveSpawns[wave-1].Enemies[currentWaveElement].alreadySpawned += 1;
                if (waveSpawns[wave-1].Enemies[currentWaveElement].alreadySpawned >= waveSpawns[wave-1].Enemies[currentWaveElement].Count)
                {
                    currentWaveElement += 1;
                    if (currentWaveElement >= waveSpawns[wave-1].Enemies.Length)
                    {
                        currentWaveElement = 0;
                        allEnemiesSpawned = true;
                        //wave += 1;
                    }
                }

                enemiesRemaining += 1;
                lastEnemySpawned = Time.time;
            }
        }
        if (allEnemiesSpawned == true)
        {
            if (enemiesRemaining == 0)
            {
                if (waveEnd == false)
                {
                    waveEndTime = Time.time;
                    waveEnd = true;
                    ReadyButton.SetActive(true);
                    DoubleSpeedButton.SetActive(false);
                    Ready = false;

                    baseHealth += 1;
                    if (baseHealth > MaxBaseHealth)
                    {
                        baseHealth = MaxBaseHealth;
                    }
                    BaseHealthBar.localPosition = new Vector3(-(1-(baseHealth/MaxBaseHealth))*0.5f,0.75f,0);
                    BaseHealthBar.localScale = new Vector3(1-(1-(baseHealth/MaxBaseHealth)),0.1f,1);
                }
            }
        }
        if (waveEnd == true)
        {
            if (Ready == true)
            {
                wave += 1;
                allEnemiesSpawned = false;
                currentWaveElement = 0;
                waveEnd = false;

                createFlowMap();
                if (renderDebug == true)
                {
                    flowMapDebug();
                }
                updateLightmap();
            }
        }
        

        CashUI.text = cash.ToString();
        SelectedItem.text = Items[selectedItem].Title + " - " + Items[selectedItem].price.ToString();
        if (waveEnd == true)
        {
            int secondsLeft = Mathf.RoundToInt(waveEndTime - Time.time + waveStartDelay);
            //WaveUI.text = "Wave " + wave.ToString() + ", " + " Starting in " + secondsLeft.ToString();
            WaveUI.text = "Wave " + wave.ToString();
        }
        else
        {
            WaveUI.text = "Wave " + wave.ToString() + ", " + enemiesRemaining + " Enemies left";
        }


        if (tileLight.ContainsKey(tilemap_Rock.WorldToCell(cam.ScreenToWorldPoint(Input.mousePosition))))
        {
            Vector3Int MouseTilePos2 = tilemap_Rock.WorldToCell(cam.ScreenToWorldPoint(Input.mousePosition));
            debugNumber = tileLight[MouseTilePos2];
        }

        
            
        //createFlowMap();
    }
}
