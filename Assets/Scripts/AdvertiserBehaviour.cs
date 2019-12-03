using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AdvertiserBehaviour : MonoBehaviour {

    public GameObject flyerPrefab;

    private List<GameObject> flyerInstances;
    private List<BehaviorForce> forces;

    private ShopperBehavior shopperBehavior;
    private AdvertiserController advertiserController;

    public float obstacleDistance = 8f;
    public float magnitude;
    public float maxSpeed = 20.0f;
    public float maxForce = 20.0f;

    private float observationDist;
    private float pitchDist;
    private float advertiseRate;
    private float advertiseProb;

    public bool traversing = false;
    public bool shopping = false;

    public int numOfSales = 0;

    private Vector3 wanderForce;
    private Vector3 velocity;
    private Transform target;
    private Rigidbody rb;
    private ObjectController objectController;

    private bool flyeredSuccess;
    private float time = 0.0f;
    private float chaseTime = 0.0f;
    private float pitchTime = 0.0f;


    void Start() {
        velocity = Vector3.zero;            
        rb = GetComponent<Rigidbody>();
        forces = new List<BehaviorForce>();
        flyerInstances = new List<GameObject>();
        objectController = GameObject.FindGameObjectWithTag("TP").GetComponent<ObjectController>();
        advertiserController = GameObject.FindGameObjectWithTag("AP").GetComponent<AdvertiserController>();
    }

    void Update() {
        observationDist = advertiserController.observationDistSlider.value;
        pitchDist = advertiserController.pitchDistSlider.value;
        advertiseRate = advertiserController.advertiseRateSlider.value;
        advertiseProb = advertiserController.advertiseProbSlider.value;
               
        if (flyeredSuccess && target != null) {          
            if (chaseTime <= 5.0f) {
                Debug.DrawLine(transform.position, target.position, Color.cyan);
                Seek();
                if (Vector3.Distance(target.position, transform.position) <= pitchDist) {
                    pitchTime += Time.deltaTime;
                    if (pitchTime >= 4.0f) {
                        numOfSales++;
                        transform.GetComponentInChildren<TextMesh>().text = numOfSales.ToString();
                        flyeredSuccess = false;
                        pitchTime = 0;                       
                        target = null;
                    }
                } else {
                    chaseTime += Time.deltaTime;
                }               
            } else {
                Wander();
                flyeredSuccess = false;
                pitchTime = 0;
                chaseTime = 0f;
                target = null;

            }
        } else {            
            Wander();
            flyeredSuccess = findFlyeredShopper();
        }
        CollisionForce();
        Move();
    }

    void DropFlyer() {
        time += Time.deltaTime;
        if (Random.value < advertiseProb && time >= advertiseRate) {
            time = 0;
            GameObject flyer = Instantiate(flyerPrefab, transform.position, transform.rotation, null);
            flyerInstances.Add(flyer);
        }
    }

    bool findFlyeredShopper() {
        GameObject[] shoppers = GameObject.FindGameObjectsWithTag("Shopper");
        for (int index = 0; index < shoppers.Length; index++) {
            bool flyerAttract = shoppers[index].GetComponent<ShopperBehavior>().isFlyered;
            if (flyerAttract) {
                if (Vector3.Distance(shoppers[index].transform.position, transform.position) < observationDist) {
                    shopperBehavior = shoppers[index].GetComponent<ShopperBehavior>();
                    forces.Clear();
                    target = shoppers[index].transform;
                    return true;
                }
            }
        }
        return false;

    }

    void Seek() {
        Vector3 desiredVelocity = target.transform.position - transform.position;
        desiredVelocity = desiredVelocity.normalized * maxSpeed;
        desiredVelocity -= velocity;
        AddForce(2.0f, desiredVelocity);
    }

    private void Wander() {
        DropFlyer();
        //Give an initial velocity "SORT OF""
        if (transform.position.magnitude > 5f) {
            Vector3 directionToCenter = (new Vector3(-70f, 0, 15f) - transform.position).normalized;
            wanderForce = velocity.normalized + directionToCenter;
        } else if (Random.value < 0.5f) {
            wanderForce = GetRandomWanderForce();
        }
        Vector3 desiredVelocity = wanderForce;
        desiredVelocity = desiredVelocity.normalized * maxSpeed;
        desiredVelocity -= velocity;
        AddForce(2.0f, desiredVelocity);

    }

    private Vector3 GetRandomWanderForce() {
        Vector3 circleCenter = velocity.normalized;
        Vector3 randomPoint = Random.insideUnitCircle;

        Vector3 displacement = new Vector3(randomPoint.x, randomPoint.y) * 10f;
        displacement = Quaternion.LookRotation(velocity) * displacement;

        Vector3 wanderForce = circleCenter + displacement;

        return wanderForce;
    }

    void Move() {
        float totalWeight = 0;
        Vector3 steering = Vector3.zero;

        forces.ForEach(e => totalWeight += e.Weight);
        forces.ForEach(e => steering += e.Force * (e.Weight / totalWeight));

        steering = Vector3.ClampMagnitude(steering, maxForce);

        velocity += steering * Time.deltaTime;
        velocity.y = 0;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

        if (velocity != Vector3.zero)
            transform.forward = velocity.normalized;

        //Fixing the Bouncing Issue:
        magnitude = rb.GetRelativePointVelocity(transform.position).magnitude;
        if (magnitude > 15f) {
            rb.velocity = velocity;
            rb.angularVelocity = velocity;
        }

        transform.position += velocity * Time.deltaTime;
        forces.Clear();
        Debug.DrawRay(transform.position, transform.forward * 5);
    }

    private void CollisionForce() {

        Vector3 calcForce = Collision.CollisionWithWall(objectController.shopWallPosition, this.transform, obstacleDistance, maxSpeed)
            + Collision.CollisionWithWall(objectController.wallPosition, this.transform, obstacleDistance, maxSpeed, -1)
            + Collision.CollisionWithPlanter(objectController.planterPosition, this.transform, obstacleDistance, maxSpeed)
            + Collision.CollisionWithTable(objectController.tablePosition, this.transform, obstacleDistance, maxSpeed, -1);

        if (calcForce.magnitude > 0)
            AddForce(2.0f, calcForce);

        List<Vector3> agents = new List<Vector3>();

        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Advertiser")) {
            if (g.name == this.name)
                continue;
            agents.Add(g.transform.position);
        }

        calcForce = Collision.CollisionWithOtherAgent(agents, this.transform, 1f, maxSpeed, velocity);
        if (calcForce.magnitude > 0)
            AddForce(2.0f, calcForce);
    }

    public void AddForce(float weight, Vector3 force) {
        forces.Add(new BehaviorForce() { Weight = weight, Force = force });
    }
}
