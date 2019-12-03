using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AdvertiserController : MonoBehaviour {

    public Slider numOfAdvertiser;
    public Slider observationDistSlider;
    public Slider pitchDistSlider;
    public Slider advertiseRateSlider;
    public Slider advertiseProbSlider;

    public Text numOfAdvertiserText;

    private Text observationDistText;
    private Text pitchDistText;
    private Text advertiseRateText;
    private Text advertiseProbText;

    private int index = 0;

    public GameObject advertiserPrefab;

    public List<GameObject> advertiserInstances;

    void Start() {
        advertiserInstances = new List<GameObject>();
        numOfAdvertiserText.text = numOfAdvertiser.value.ToString();
        observationDistText = observationDistSlider.transform.GetChild(3).gameObject.GetComponent<Text>();
        pitchDistText = pitchDistSlider.transform.GetChild(3).gameObject.GetComponent<Text>();
        advertiseRateText = advertiseRateSlider.transform.GetChild(3).gameObject.GetComponent<Text>();
        advertiseProbText = advertiseProbSlider.transform.GetChild(3).gameObject.GetComponent<Text>();
    }

    void SpawnAgents() {
        for (int i = advertiserInstances.Count; i < numOfAdvertiser.value; i++) {
            SpawnAdvertiser();
        }
    }

    void Update() {
        observationDistText.text = observationDistSlider.value.ToString();
        pitchDistText.text = pitchDistSlider.value.ToString();
        advertiseRateText.text = advertiseRateSlider.value.ToString();
        advertiseProbText.text = advertiseProbSlider.value.ToString();
        SpawnAgents();
        numOfAdvertiserText.text = numOfAdvertiser.value.ToString();
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
