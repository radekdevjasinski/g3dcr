using UnityEngine;

public class Flower : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        // Sprawdzamy, czy to pszczoła (musisz nadać jej Tag "Player")
        if (other.CompareTag("Player"))
        {
            // Odwołujemy się do Managera, żeby dodać energię
            GameManager.instance.CollectFlower();
            
            // Niszczymy ten kwiatek
            Destroy(gameObject);
        }
    }
}