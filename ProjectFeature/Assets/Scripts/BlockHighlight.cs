using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockHighlight : MonoBehaviour
{
    [SerializeField] private MeshRenderer highlight;

    private void OnMouseEnter()
    {
        highlight.enabled = true;
    }

    private void OnMouseExit()
    {
        highlight.enabled = false;
    }
}
