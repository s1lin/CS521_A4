using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopperController : MonoBehaviour {

    public int spawnRate;
    public int numOfShoppers = 1;

    public GameObject shopperPrefab;

    public List<GameObject> shopperInstances;

    void Start() {
        shopperInstances = new List<GameObject>();
       
        StartCoroutine(SpawnAgents());
    }

    IEnumerator SpawnAgents() {
        for (int i = 0; i < numOfShoppers; i++) {
            SpawnShopper(i);
            yield return new WaitForSeconds(2);
        }
    }

    void Update() {
        for (int i = 0; i < shopperInstances.Count; i++) {
            float x = shopperInstances[i].transform.position.x;
            float y = shopperInstances[i].transform.position.y;
            float z = shopperInstances[i].transform.position.z;
            //outofBound
            if (x > 98f || Mathf.Abs(y) > 0.5f || Mathf.Abs(z) > 55f) {
                Destroy(shopperInstances[i]);
                shopperInstances.RemoveAt(i);
                SpawnShopper(i);
            }
        }
    }

    void SpawnShopper(int i) {
        float zIndex = Random.Range(-15f, 15f);
        Vector3 pos = new Vector3(-99f, 0, zIndex);
        GameObject shopper = Instantiate(shopperPrefab, pos, Quaternion.identity, transform);       
        shopper.name = "shopper" + i;
        shopperInstances.Add(shopper);
    }
}
