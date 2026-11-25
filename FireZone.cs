using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FireZone : MonoBehaviour
{
    public CampfireController campfire;

    void Reset()
    {
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        var log = other.GetComponentInParent<LogFuel>();
        if (log != null)
        {
            log.Consume(campfire);
        }
    }
}
