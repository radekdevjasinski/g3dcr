using UnityEngine;
using UnityEngine.UI; 
using UnityEngine.SceneManagement; 
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager instance; 

    [Header("Ustawienia Gry")]
    public float maxEnergy = 10f; 
    public int totalFlowers = 50; 
    
    [Header("Referencje")]
    public GameObject[] flowersPrefab;
    public Terrain terrain;
    public Transform player; 

    [Header("UI")]
    public Slider energyBar;
    public TMPro.TMP_Text timeText;
    public GameObject gameOverPanel;

    public InputAction RestartInput;
    private float currentEnergy;
    private float timeAlive;
    private bool isGameOver = false;

    void Awake()
    {
        instance = this;
    }
    void OnEnable() { RestartInput.Enable(); }
    void OnDisable() { RestartInput.Disable(); }
    void Start()
    {
        currentEnergy = maxEnergy;
        gameOverPanel.SetActive(false);
        
        for (int i = 0; i < totalFlowers; i++)
        {
            SpawnFlower();
        }
    }

    void Update()
    {
        if (RestartInput.triggered) SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        if (isGameOver) return;

        // naliczanie czasu przeżycia
        timeAlive += Time.deltaTime;
        timeText.text = timeAlive.ToString("F2") + "s";

        // zmniejszanie energii
        currentEnergy -= Time.deltaTime;

        // aktualizacja paska (wartość od 0 do 1)
        if (energyBar != null)
            energyBar.value = currentEnergy / maxEnergy;

        // sprawdzenie przegranej
        if (currentEnergy <= 0)
        {
            GameOver();
        }
    }

    // metoda wywoływana przez kwiatek
    public void CollectFlower()
    {
        if (isGameOver) return;

        // resetujemy energię
        currentEnergy = maxEnergy; 

        // spawnujemy nowy kwiatek w losowym miejscu, żeby ich liczba była stała
        SpawnFlower();
    }
    // generowanie kwiatka w losowej pozycji na terenie
    void SpawnFlower()
    {
        if (terrain == null) return;

        // pobieramy wymiary terenu
        float terrainWidth = terrain.terrainData.size.x-20; // -20, żeby nie spawnować na krawędziach
        float terrainLength = terrain.terrainData.size.z-20;
        float terrainPosX = terrain.transform.position.x;
        float terrainPosZ = terrain.transform.position.z;

        // losujemy pozycję X i Z
        float randX = Random.Range(terrainPosX, terrainPosX + terrainWidth);
        float randZ = Random.Range(terrainPosZ, terrainPosZ + terrainLength);

        // obliczamy wysokość terenu w tym punkcie (Y)
        float yVal = terrain.SampleHeight(new Vector3(randX, 0, randZ));
        yVal += terrain.transform.position.y;
        Vector3 spawnPos = new Vector3(randX, yVal, randZ);

        // generujemy kwiatek
        GameObject flower = Instantiate(flowersPrefab[Random.Range(0, flowersPrefab.Length)], spawnPos, Quaternion.identity);
        flower.transform.SetParent(this.transform); 
        flower.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0); // losowa rotacja wokół Y
        flower.transform.localScale = Vector3.one * 0.5f; // zbyt duży prefab, skalujemy
        flower.transform.localScale = flower.transform.localScale * Random.Range(0.8f, 1.2f); // losowa skala
    }

    void GameOver()
    {
        isGameOver = true;
        gameOverPanel.SetActive(true);
        player.GetComponent<BeeFlight>().EndGame();
    }
}