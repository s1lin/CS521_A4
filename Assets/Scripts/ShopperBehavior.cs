using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopperBehavior : MonoBehaviour {

    public struct BehaviorForce {
        public float Weight { get; set; }
        public Vector3 Force { get; set; }
    }

    //flyer:
    public Material flyered;
    public Material notFlyered;
    private GameObject flyer;
    public bool flyerAttract = false;

    public float obstacleDistance = 8f;

    public float maxSpeed = 20.0f;
    public float maxForce = 20.0f;

    public Vector3 velocity;
    public float magnitude;
    private Vector3 target;
    private List<GameObject> flyers;
    public List<Vector3> agents;
    private List<BehaviorForce> forces = new List<BehaviorForce>();
    private TPSpawn tp;
    private Chair chair;
    private Rigidbody rb;
    private int shopVisited;

    public bool traversing = false;
    public bool shopping = false;

    public float shopTime = 0.0f;
    public float sitTime = 0.0f;
    public float flyerTime = 0.0f;
    public Vector3 caclForce;
    public float caclForceMagnitude;
    

    void Start() {
        tp = GameObject.FindGameObjectWithTag("TP").GetComponent<TPSpawn>();

        velocity = Vector3.zero;
        rb = GetComponent<Rigidbody>();
        target = new Vector3(99f, 0, Random.Range(-15f, 15f));
        
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
        flyerAttract = false;
        velocity = Vector3.zero;
        transform.GetComponent<Renderer>().material = notFlyered;
    }

    void Update() {
        Debug.DrawLine(transform.position, target);
        if (flyerAttract && flyerTime < 2.0f) {
            velocity = Vector3.zero;
            transform.GetComponent<Renderer>().material = flyered;
            if (flyer != null) {
                velocity = Vector3.zero;
                flyers.Remove(flyer);
                Destroy(flyer);
            }
            if (flyerTime < 2.0f) {
                flyerTime += Time.deltaTime;
            } else {
                flyerTime = 0;
                //flyerAttract = false;
            }
        } else {
            if (!flyerAttract) {
                flyerAttract = CollisionWithFlyer();
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
                        chair = tp.GetEmptyChair();
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
                                tp.ChairReset(chair);
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
        Vector3 calcForce = Collision.CollisionWithWall(tp.wallPosition, this.transform, obstacleDistance, maxSpeed)
            + Collision.CollisionWithWall(tp.shopWallPosition, this.transform, obstacleDistance, maxSpeed, shopVisited)
            + Collision.CollisionWithChair(tp.chairs, this.transform, obstacleDistance, maxSpeed, tableNum)
            + Collision.CollisionWithPlanter(tp.planterPosition, this.transform, obstacleDistance, maxSpeed)
            + Collision.CollisionWithTable(tp.tablePosition, this.transform, obstacleDistance, maxSpeed, tableNum);

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

        foreach (BehaviorForce force in forces) {
            totalWeight += force.Weight;
        }

        Vector3 appliedForce = Vector3.zero;

        foreach (BehaviorForce force in forces) {
            Vector3 apply = force.Force;
            apply *= force.Weight / totalWeight;
            appliedForce += apply;
        }

        appliedForce = Vector3.ClampMagnitude(appliedForce, maxForce);
        caclForce = appliedForce;
        caclForceMagnitude = caclForce.magnitude;
        velocity += appliedForce * Time.deltaTime;
        velocity.y = 0;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

        if (velocity != Vector3.zero)
            transform.forward = velocity.normalized;
        
        magnitude = rb.GetRelativePointVelocity(transform.position).magnitude;
        if (magnitude > 25f) {
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
