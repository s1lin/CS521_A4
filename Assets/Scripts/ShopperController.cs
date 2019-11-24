using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopperController : MonoBehaviour {

    public int spawnRate;
    public int numOfShoppers = 1;

    public GameObject shopperPrefab;

    private List<GameObject> shopperInstances;

    void Start() {
        shopperInstances = new List<GameObject>();
        StartCoroutine(SpawnAgents());
    }

    IEnumerator SpawnAgents() {
        for (int i = 0; i < numOfShoppers; i++) {
            SpawnShopper();
            yield return new WaitForSeconds(2);
        }
    }

    void Update() {
        for (int i = 0; i < shopperInstances.Count; i++) {
            float x = shopperInstances[i].transform.position.x;
            if (x > 98f) {
                Destroy(shopperInstances[i]);
                shopperInstances.RemoveAt(i);
                SpawnShopper();
            }
        }
    }

    void SpawnShopper() {
        float zIndex = Random.Range(-15f, 15f);
        Vector3 pos = new Vector3(-99f, 0, zIndex);
        GameObject shopper = Instantiate(shopperPrefab, pos, Quaternion.identity, transform);
        shopperInstances.Add(shopper);
    }
}
