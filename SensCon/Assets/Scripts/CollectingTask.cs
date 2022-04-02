using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectingTask : MonoBehaviour
{

    public int maxObjects = 10;

    private int counter = 0;

    public GameObject gameArea;

    public GameObject objectList;

    public GameObject endObject;

    public Material endMaterial;

    // Start is called before the first frame update
    void Start()
    {
        spawnNext();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void spawnNext() {

        if (counter < maxObjects)
        {
            GameObject obj = Instantiate(objectList.transform.GetChild(Random.Range(0, objectList.transform.childCount)).gameObject);
            obj.name = obj.name.Replace("(Clone)", "") + counter;
            obj.transform.parent = this.transform;
            obj.transform.position = GetCollisionFreePosition();
            counter++;
        }
        else {
            endObject.GetComponent<MeshRenderer>().material = endMaterial;
        }
    }

    // Getting new random collision free position for next ball
    public Vector3 GetCollisionFreePosition()
    {
        Transform t = gameArea.transform;
        Vector3 newPosition = new Vector3(UnityEngine.Random.Range(t.position.x - (t.localScale.x / 2), t.position.x + (t.localScale.x / 2)),
                                          UnityEngine.Random.Range(t.position.y - (t.localScale.y / 2), t.position.y + (t.localScale.y / 2)),
                                          UnityEngine.Random.Range(t.position.z - (t.localScale.z / 2), t.position.z + (t.localScale.z / 2)));
        Collider[] collisions = Physics.OverlapSphere(newPosition, .5f);
        List<Collider> activeCollisions = new List<Collider>();

        foreach (Collider item in collisions)
        {
            if (item.tag == "wall" || item.tag == "obstacle")
            {
                activeCollisions.Add(item);
            }
        }

        // Move object to center until collision-free
        int iterations = 0;
        int threshhold = 100;
        while (activeCollisions.Count > 0 & iterations < threshhold)
        {
            iterations++;
            for (int i = 0; i < 3; i++)
            {
                if (Vector3.up[i] == 0f)
                {
                    if (newPosition[i] > 0)
                    {
                        newPosition[i] -= .01f;
                    }
                    else if (newPosition[i] < 0)
                    {
                        newPosition[i] += .01f;
                    }
                }
            }

            collisions = Physics.OverlapSphere(newPosition, .5f);
            activeCollisions = new List<Collider>();

            foreach (Collider item in collisions)
            {
                if (item.tag == "wall" || item.tag == "obstacle")
                {
                    activeCollisions.Add(item);
                }
            }
        }

        if (iterations >= threshhold)
        {
            newPosition = new Vector3(0f, .5f, 0f);
            iterations = 0;
        }

        return newPosition;
    }
}
