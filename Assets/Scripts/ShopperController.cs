using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopperController : MonoBehaviour {

    public Slider spawnRate;
    public Text spawnRateText;
    private int index = 0;
    private bool isWait = false;

    public GameObject shopperPrefab;

    public List<GameObject> shopperInstances;

    void Start() {
        shopperInstances = new List<GameObject>();
        spawnRateText.text = spawnRate.value.ToString();
    }

    IEnumerator SpawnAgents() {
        isWait = true;
        SpawnShopper();
        yield return new WaitForSeconds(5f / spawnRate.value);
        isWait = false;
    }

    void Update() {
        spawnRateText.text = spawnRate.value.ToString();
        if (!isWait) {
            if(spawnRate.value != 0)
                StartCoroutine(SpawnAgents());
            for (int i = 0; i < shopperInstances.Count; i++) {
                float x = shopperInstances[i].transform.position.x;
                float y = shopperInstances[i].transform.position.y;
                float z = shopperInstances[i].transform.position.z;
                //outofBound
                if (x > 98f || Mathf.Abs(y) > 0.5f || Mathf.Abs(z) > 55f) {
                    Destroy(shopperInstances[i]);
                    shopperInstances.RemoveAt(i);
                }
            }
        }
    }

    public void SpawnShopper() {
        float zIndex = Random.Range(-15f, 15f);
        Vector3 pos = new Vector3(-99f, 0, zIndex);
        GameObject shopper = Instantiate(shopperPrefab, pos, Quaternion.identity, transform);
        shopper.name = "shopper" + index;
        index++;
        shopperInstances.Add(shopper);
    }
}
