using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockHighlight : MonoBehaviour
{
    [SerializeField] private MeshRenderer highlight;
    private float maxDistance = 8f;

    Vector3 coords;
    float distance;

    private void OnMouseEnter()
    {
        coords = Player.Instance.gameObject.transform.position;
        distance = Mathf.Abs(transform.position.x - coords.x) +
                         Mathf.Abs(transform.position.y - coords.y) +
                         Mathf.Abs(transform.position.z - coords.z);

        if (distance <= maxDistance)
            highlight.enabled = true;
    }

    private void OnMouseExit()
    {
        highlight.enabled = false;
    }
}
