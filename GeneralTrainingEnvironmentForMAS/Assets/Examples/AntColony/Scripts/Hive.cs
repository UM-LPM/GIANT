using UnityEngine;

public class Hive : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;
    public int antsRequiredForRepair;
    public bool needsMaintenance = false;
    public void Initialize(float currentHealth, int antsRequiredForRepair, bool needsMaintenance)
    {
        this.antsRequiredForRepair = antsRequiredForRepair;
        this.currentHealth = currentHealth;
        this.needsMaintenance = needsMaintenance;

    }
    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= maxHealth * 0.75f) 
        {
            needsMaintenance = true;
        }
    }

    public void Repair(float repairAmount)
    {
        currentHealth += repairAmount;
        if (currentHealth >= maxHealth)
        {
            currentHealth = maxHealth;
            needsMaintenance = false;
        }
    }

    private void Update()
    {
        if (Time.frameCount % 600 == 0) // Example damage every 10 seconds
        {
            TakeDamage(10f);
        }
    }
}
