using TMPro;
using UnityEngine;
using System.Collections;

public class SC_Test_Mark_Stone : MonoBehaviour
{
    public float rotDuration = 0.5f; //Cuanto tiempo tarda en hacer el giro (seg? min?)
    public float rotAngle = -90f; 

    private Rigidbody _rb;
    private Quaternion _startRot;
    private Quaternion _targetRot;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        StartCoroutine(ActivatePillar());
    }

    private IEnumerator ActivatePillar()
    {
        _startRot = _rb.rotation;
        _targetRot = _startRot * Quaternion.Euler(rotAngle, 0f, 0f);

        float elapsed = 0f;

        while (elapsed < rotDuration) 
        {
            elapsed += Time.fixedDeltaTime;
            float t = elapsed / rotDuration;

            Quaternion newRot = Quaternion.Slerp(_startRot, _targetRot, t);
            _rb.MoveRotation(newRot);

            yield return new WaitForFixedUpdate();
        }
        _rb.MoveRotation(_targetRot);
    }
    /*Para Ice (rigidbodies y interactables) 
     * Collider[] colls = Physics.OverlapSphere(transform.position, range);
        foreach (Collider coll in colls)
        {
            if (coll.name == name) continue;

            rigidbody rb = coll.GetComponent<rigidbody>();

        }*/

    //Crear SC ZoneFreeze

    //Update funcional de MarksPlacer:
    /*
     *     private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            UnlockMark(marks[(int)ElementTypes.WATER].element.el_type);
            UnlockMark(marks[(int)ElementTypes.ELECTRICITY].element.el_type);
            UnlockMark(marks[(int)ElementTypes.GRAVITY].element.el_type);
            UnlockMark(marks[(int)ElementTypes.AIR].element.el_type);
            UnlockMark(marks[(int)ElementTypes.FIRE].element.el_type);
            UnlockMark(marks[(int)ElementTypes.STONE].element.el_type);
        }
        if (placing_mark)
        {
            //Horizontal del jugador
            Vector3 new_pos;
            if (Physics.Raycast(player.position, Camera.main.transform.forward, out RaycastHit hit, range_placing, layer_placeable, QueryTriggerInteraction.Ignore))
            {
                new_pos = hit.point;
                mark_placeholder.transform.rotation = Quaternion.LookRotation(hit.normal);
                mark_placeholder_image.color = Color.white;
            }
            else
            {
                new_pos = player.position + Camera.main.transform.forward * range_placing;
                mark_placeholder_image.color = Color.black;
                mark_placeholder.transform.LookAt(Camera.main.transform);
            }
            // Moves the mark to the camera, overlapping with collisions
            mark_placeholder.transform.position = Vector3.Lerp(mark_placeholder.transform.position, new_pos, Time.deltaTime * 30);

        }
        // Make the activatios order arrow look to the player
        if (placed_marks.Count > 0)
        {
            transform_arrow.LookAt(transform);
        }
    }
     */
}