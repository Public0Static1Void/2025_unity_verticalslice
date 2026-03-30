using UnityEngine;

public class SC_MarkUnequipper : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<SC_MarksPlacer>().RemoveMarks();
        }
    }
}
