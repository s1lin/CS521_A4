using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopperBehavior : MonoBehaviour {

    //flyer:
    public Material flyered;
    public Material notFlyered;

    public bool isFlyered = false;

    public float obstacleDistance = 8f;

    public float maxSpeed = 20.0f;
    public float maxForce = 20.0f;
    public float magnitude;

    public Vector3 velocity;

    private List<Vector3> agents;
    private List<GameObject> flyers;
    private List<BehaviorForce> forces;

    private Vector3 target;
    private GameObject flyer;
    private Rigidbody rb;

    private ObjectController objectController;
    private Chair chair;

    private int shopVisited;

    public bool traversing = false;
    public bool shopping = false;

    private float shopTime = 0.0f;
    private float sitTime = 0.0f;
    private float flyerTime = 0.0f;

    void Start() {

        objectController = GameObject.FindGameObjectWithTag("TP").GetComponent<ObjectController>();
        target = new Vector3(99f, 0, Random.Range(-15f, 15f));
        forces = new List<BehaviorForce>();
        rb = GetComponent<Rigidbody>();

        velocity = Vector3.zero;

        float action = Random.value;
        traversing = action >= 0.5f;
        shopping = action < 0.5f;
        if (shopping) {
            shopVisited = Random.Range(0, 16);
            target = GameObject.FindGameObjectsWithTag("Shop")[shopVisited].transform.position;
        }
    }
    public void Reset() {
        flyerTime = 0;
        isFlyered = false;
        velocity = Vector3.zero;
        transform.GetComponent<Renderer>().material = notFlyered;
    }

    void Update() {
        Debug.DrawLine(transform.position, target);
        if (isFlyered && flyerTime < 2.0f) {
            velocity = Vector3.zero;
            transform.GetComponent<Renderer>().material = flyered;
            if (flyer != null) {
                velocity = Vector3.zero;
                flyers.Remove(flyer);
                Destroy(flyer);
            }
            //Flyered, Wait 2s;
            if (flyerTime < 2.0f) {
                flyerTime += Time.deltaTime;
            }
        } else if (flyerTime >= 2.0f) {
            flyerTime = 0;
            isFlyered = false;
        } else {

            if (!isFlyered) {
                isFlyered = CollisionWithFlyer();
            }

            if (traversing) {
                Seek();
                CollisionAvoidance(-1, -1);
                Move();
                if (chair != null) {
                    chair.transform.GetComponent<CapsuleCollider>().enabled = true;
                }
            }

            if (shopping) {
                if (Vector3.Distance(target, transform.position) > obstacleDistance && chair == null) {
                    Seek();
                    CollisionAvoidance(-1, shopVisited);
                    Move();
                } else {
                    if (chair == null) {
                        chair = objectController.GetEmptyChair();
                        target = chair.transform.position;
                        velocity = Vector3.zero;
                        forces.Clear();
                    }
                    //Inside Shop, Wait 1s;
                    if (shopTime <= 1.0f)
                        shopTime += Time.deltaTime;
                    else {
                        if (Vector3.Distance(target, transform.position) > 4f) {
                            Seek();
                            CollisionAvoidance(chair.parent, shopVisited);
                            Move();
                        } else {
                            transform.position = target;
                            chair.transform.GetComponent<CapsuleCollider>().enabled = false;
                            velocity = Vector3.zero;
                            forces.Clear();
                            if (sitTime < 3.0f) {
                                sitTime += Time.deltaTime;
                            } else {
                                target = new Vector3(99f, 0, Random.Range(-15f, 15f));
                                traversing = true;
                                shopping = false;
                                Seek();
                                CollisionAvoidance(chair.parent, -1);
                                Move();
                                objectController.ChairReset(chair);
                            }
                        }
                    }
                }
            }
        }
    }

    bool CollisionWithFlyer() {
        flyers = new List<GameObject>();
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Flyer")) {
            flyers.Add(g);
        }

        if (flyers.Count == 0)
            return false;

        flyer = flyers.Find(e => Vector3.Distance(e.transform.position, transform.position) < 2f);

        return flyer != null;
    }

    void CollisionAvoidance(int tableNum, int shopVisited) {
        Vector3 calcForce = Collision.CollisionWithWall(objectController.wallPosition, this.transform, obstacleDistance, maxSpeed)
            + Collision.CollisionWithWall(objectController.shopWallPosition, this.transform, obstacleDistance, maxSpeed, shopVisited)
            + Collision.CollisionWithChair(objectController.chairs, this.transform, obstacleDistance, maxSpeed, tableNum)
            + Collision.CollisionWithPlanter(objectController.planterPosition, this.transform, obstacleDistance, maxSpeed)
            + Collision.CollisionWithTable(objectController.tablePosition, this.transform, obstacleDistance, maxSpeed, tableNum);

        if (calcForce.magnitude > 0)
            AddForce(2.0f, calcForce);

        agents = new List<Vector3>();

        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Shopper")) {
            if (g.name == this.name)
                continue;
            agents.Add(g.transform.position);
        }

        calcForce = Collision.CollisionWithOtherAgent(agents, this.transform, 1f, maxSpeed, velocity);
        if (calcForce.magnitude > 0)
            AddForce(0.5f, calcForce);
    }

    void Seek() {
        var desiredVelocity = target - transform.position;
        desiredVelocity = desiredVelocity.normalized * maxSpeed;
        desiredVelocity -= velocity;
        AddForce(1.0f, desiredVelocity);
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
        if (magnitude > 20f) {
            rb.velocity = velocity;
            rb.angularVelocity = velocity;
        }

        transform.position += velocity * Time.deltaTime;
        forces.Clear();
        Debug.DrawRay(transform.position, transform.forward * 5);
    }

    public void AddForce(float weight, Vector3 force) {
        forces.Add(new BehaviorForce() { Weight = weight, Force = force });
    }

}
