using UnityEngine;

public class LogFuel : MonoBehaviour
{
    public float fuelAmount = 20f;      // 이 장작이 주는 연료
    public bool destroyOnBurn = true;   // 태우면 파괴할지

    public void Consume(CampfireController fire)
    {
        if (!fire) return;
        fire.AddFuel(fuelAmount);

        if (destroyOnBurn) Destroy(gameObject);
        else gameObject.SetActive(false);
    }
}
