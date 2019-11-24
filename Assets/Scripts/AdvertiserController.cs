using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdvertiserController: MonoBehaviour {

    public int spawnRate;
    public int numOfAdvertiser = 1;

    public GameObject advertiserPrefab;

    private List<GameObject> advertiserInstances;

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
            float x = advertiserInstances[i].transform.position.x;
            if (x > 98f) {
                Destroy(advertiserInstances[i]);
                advertiserInstances.RemoveAt(i);
                SpawnAdvertiser();
            }
        }
    }

    void SpawnAdvertiser() {
        float zIndex = Random.Range(-15f, 15f);
        Vector3 pos = new Vector3(-99f, 0, zIndex);
        GameObject shopper = Instantiate(advertiserPrefab, pos, Quaternion.identity, transform);
        advertiserInstances.Add(shopper);
    }
}
