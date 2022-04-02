using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectingTaskCollision : MonoBehaviour
{

    public CollectingTask task;

    public string activateWith;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnCollisionEnter(Collision col)
    {
        double timestamp = UnixTime.GetTime();
        Debug.Log(col.gameObject.name);
        Debug.Log(col.gameObject);
        Debug.Log(col.gameObject);

        if (col.gameObject.name == activateWith)
        {
            //col.gameObject.GetComponent<CollisionDone>().isCollision = true;
            //task.collision(timestamp, trashColor);

            task.spawnNext();
            Destroy(this.gameObject);
        }
    }
}