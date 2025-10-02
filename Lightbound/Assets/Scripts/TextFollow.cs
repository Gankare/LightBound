using UnityEngine;

public class TextFollow : MonoBehaviour
{
    public Transform player;     
    public float rotateSpeed = 5f; 
    public bool isActive = true;    

    void Update()
    {
        if (isActive && player != null)
        {
            Vector3 direction = player.position - transform.position;
            direction.y = 0; 

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                targetRotation *= Quaternion.Euler(0, 180, 0);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
            }
        }
    }
}
