using UnityEngine;
public class WhoDisablesMe : MonoBehaviour
{
    void OnDisable() { Debug.Log(name + " disabled @ " + Time.time); }
    void OnDestroy() { Debug.Log(name + " destroyed @ " + Time.time); }
}
