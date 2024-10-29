using Collector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpComponent : MonoBehaviour
{
    public PowerUpType PowerUpType;
    public RobostrikeEnvironmentController RobocodeEnvironmentController { get; set; }

    private void Awake()
    {
        RobocodeEnvironmentController = transform.parent.GetComponent<RobostrikeEnvironmentController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != this.gameObject.layer)
            return;

        AgentComponent otherAgent;
        other.gameObject.TryGetComponent<AgentComponent>(out otherAgent);

        if (otherAgent != null)
        {
            if (RobocodeEnvironmentController.PowerUpPickedUp(this, PowerUpType, otherAgent))
                Destroy(this.gameObject);
        }

    }
}

public enum PowerUpType
{
    Health,
    Ammo,
    Shield
}
