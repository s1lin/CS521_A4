using System.Collections.Generic;
using UnityEngine;

public class ObjectController : MonoBehaviour {

    public GameObject planterPrefab;

    //Different Material For Chair
    public Material assigned;
    public Material notAssigned;

    private List<GameObject> tableInstances;
    private List<GameObject> planterInstances;
    public GameObject[] tablePrefabs;

    public List<Vector3> tablePosition;
    public List<Vector3> planterPosition;
    public List<Vector3> wallPosition;
    public List<Vector3> shopWallPosition;

    public List<Chair> chairs;

    void Start() {
        GeneratePosition();
        SpawnTable();
        GeneratePlanterPosition();
        SpawnPlanter();

        wallPosition = new List<Vector3>();
        GameObject[] walls = GameObject.FindGameObjectsWithTag("Wall");
        foreach (GameObject g in walls) {
            wallPosition.Add(g.transform.position);
        }

        shopWallPosition = new List<Vector3>();
        GameObject[] shopWalls = GameObject.FindGameObjectsWithTag("ShopWall");
        foreach (GameObject g in shopWalls) {
            shopWallPosition.Add(g.transform.position);
        }
    }

    void GeneratePosition() {
        tablePosition = new List<Vector3>();
        Vector3 pos = new Vector3(Random.Range(-50f, 70f), 0, Random.Range(-15f, 15f));

        int numOfTable = Random.Range(3, 5);
        for (int i = 0; i < numOfTable; i++) {
            while (tablePosition.Find(e => Vector3.Distance(e, pos) < 30f) != new Vector3()) {
                pos = new Vector3(Random.Range(-50f, 70f), 0, Random.Range(-15f, 15f));

            }
            tablePosition.Add(pos);
        }
    }

    void GeneratePlanterPosition() {
        planterPosition = new List<Vector3>();
        Vector3 pos = new Vector3(Random.Range(-50f, 70f), 0, Random.Range(-15f, 15f));

        int numOfPlanter = Random.Range(2, 6);
        for (int i = 0; i < numOfPlanter; i++) {
            while (tablePosition.Find(e => Vector3.Distance(e, pos) < 20f) != new Vector3() ||
                planterPosition.Find(e => Vector3.Distance(e, pos) < 21f) != new Vector3()) {
                pos = new Vector3(Random.Range(-50f, 70f), 0, Random.Range(-15f, 15f));
            }
            planterPosition.Add(pos);
        }
    }

    void SpawnTable() {

        chairs = new List<Chair>();
        tableInstances = new List<GameObject>();
        for (int i = 0; i < tablePosition.Count; i++) {
            int r = Random.Range(0, tablePrefabs.Length);
            GameObject table = Instantiate(tablePrefabs[r], transform);
            table.transform.position = tablePosition[i];
            tableInstances.Add(table);
            for (int j = 1; j < table.transform.childCount; j++) {
                Chair chair = new Chair(table.transform.GetChild(j), j, i, false);
                chairs.Add(chair);
            }
        }
    }

    void SpawnPlanter() {
        planterInstances = new List<GameObject>();
        for (int i = 0; i < planterPosition.Count; i++) {
            GameObject planter = Instantiate(planterPrefab, transform);
            planter.transform.position = planterPosition[i];
            planterInstances.Add(planter);
        }
    }

    public Chair GetEmptyChair() {
        int chairIndex = Random.Range(0, chairs.Count);
        while (chairs[chairIndex].isAssigned) {
            chairIndex = Random.Range(0, chairs.Count);
        }
        chairs[chairIndex].isAssigned = true;
        chairs[chairIndex].transform.GetComponent<Renderer>().material = assigned;
        return chairs[chairIndex];
    }

    public void ChairReset(Chair chair) {
        chair.isAssigned = false;
        chair.transform.GetComponent<Renderer>().material = notAssigned;
    }
}