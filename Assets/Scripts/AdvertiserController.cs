using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdvertiserController: MonoBehaviour {

    public int spawnRate;
    public int numOfAdvertiser = 1;
    private int index = 0;

    public GameObject advertiserPrefab;

    public List<GameObject> advertiserInstances;

    void Start() {
        advertiserInstances = new List<GameObject>();
        StartCoroutine(SpawnAgents());
    }

    IEnumerator SpawnAgents() {
        for (int i = 0; i < numOfAdvertiser; i++) {
            SpawnAdvertiser();
            yield return new WaitForSeconds(2);
        }
    }

    void Update() {
        for (int i = 0; i < advertiserInstances.Count; i++) {
            int sales = advertiserInstances[i].GetComponent<AdvertiserBehaviour>().numOfSales;
            float x = advertiserInstances[i].transform.position.x;
            float y = advertiserInstances[i].transform.position.y;
            float z = advertiserInstances[i].transform.position.z;
            //outofBound
            if (x > 98f || Mathf.Abs(y) > 0.5f || Mathf.Abs(z) > 55f || sales == 3) {
                Destroy(advertiserInstances[i]);
                advertiserInstances.RemoveAt(i);
                SpawnAdvertiser();
            }
        }
    }

    void SpawnAdvertiser() {
        float zIndex = Random.Range(-15f, 15f);
        float xIndex = Random.Range(-90f, -45f);
        Vector3 pos = new Vector3(xIndex, 0, zIndex);
        GameObject advertiser = Instantiate(advertiserPrefab, pos, Quaternion.identity, transform);
        advertiser.name = "advertiser" + index;
        index++;
        advertiserInstances.Add(advertiser);
    }
}
