using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]        // simple “Open” bool parameter
public class DoorController : MonoBehaviour
{
    private Animator anim;
    private bool isOpen = false;
    private BoxCollider collider;
    void Awake()
    {
        anim = GetComponent<Animator>();
        collider= GetComponent<BoxCollider>();
    }

    public void TryOpen()
    {
        if (isOpen) return;
        Debug.Log("Trying to open");
        isOpen = true;
        collider.enabled = false;
        anim.SetTrigger("Open");
    }
}