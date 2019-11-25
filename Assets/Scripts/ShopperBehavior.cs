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
    private float flyeredWait = 2f;
    public bool flyerAttract = false;

    public float obstacleDistance = 8f;

    public float maxSpeed = 20.0f;
    public float maxForce = 20.0f;
    public float maxSeeAhead = 20f;
    public float maxAvoidForce = 10.0f;

    private float speed;
    private Vector3 steering;
    private Vector3 velocity;
    private Vector3 destination;
    private Vector3 desiredVelocity;
    private Vector3 target;
    private List<Vector3> obstacles;
    private List<GameObject> flyers;

    private List<BehaviorForce> forces = new List<BehaviorForce>();
    private TPSpawn tp;
    private Chair chair;

    public bool traversing = false;
    public bool shopping = false;

    private float time = 0.0f;

    void Start() {
        speed = Random.Range(0.17f, maxSpeed);
        velocity = Vector3.zero;
        steering = Vector3.zero;
        desiredVelocity = Vector3.zero;
        float zIndex = Random.Range(-15f, 15f);
        target = new Vector3(99f, 0, zIndex);
        tp = GameObject.FindGameObjectWithTag("TP").GetComponent<TPSpawn>();
        obstacles = tp.planterPosition;
        obstacles.AddRange(tp.tablePosition);

        float action = Random.Range(0f, 1f);
        traversing = action >= 0.5f;
        shopping = action < 0.5f;
        if (shopping) {
            int shopVisited = Random.Range(0, 16);
            target = GameObject.FindGameObjectsWithTag("Shop")[shopVisited].transform.position;
        }
    }

    // Update is called once per frame
    void Update() {

        if (flyerAttract && time < flyeredWait) {
            velocity = Vector3.zero;
            transform.GetComponent<Renderer>().material = flyered;
            if (flyer != null) {
                transform.position = flyer.transform.position;
                flyers.Remove(flyer);
                Destroy(flyer);
            }
            if (time < flyeredWait) {
                time += Time.deltaTime;
            } else {
                time = 0;
                //flyerAttract = false;
            }
        } else {
            if (!flyerAttract) {
                flyerAttract = CollisionWithFlyer();
            }

            if (traversing) {
                Seek();
                Collision();
                Move();
            }

            if (shopping) {
                if (Vector3.Distance(target, transform.position) > 15f && chair == null) {
                    Seek();
                    Collision();
                    Move();
                } else {
                    if (chair == null)
                        chair = tp.GetEmptyChair();
                    //Not to assign chair twice
                    if (chair != null) {
                        target = chair.transform.position;
                        if (Vector3.Distance(target, transform.position) > 5f) {
                            Seek();
                            CollisionWithOtherTable(chair.parent);
                            Move();
                        } else {
                            float zIndex = Random.Range(-15f, 15f);
                            target = new Vector3(99f, 0, zIndex);
                            traversing = true;
                            shopping = false;
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

    void CollisionWithOtherTable(int tableNum) {
        Vector3 calcForce = Vector3.zero;
        for (int i = 2; i < tp.wallPosition.Count; i++) {

            float obRadius = 5f;
            float agentRadius = 5f;

            Vector3 vecToCenter = tp.wallPosition[i] - transform.position;
            vecToCenter.y = 0;
            float dist = vecToCenter.magnitude;

            if (dist > obstacleDistance + obRadius + agentRadius)
                continue;

            if (Vector3.Dot(transform.forward, vecToCenter) < 0)
                continue;

            float rightDotVTC = Vector3.Dot(vecToCenter, transform.right);

            if (Mathf.Abs(rightDotVTC) > agentRadius + obRadius)
                continue;

            Debug.DrawLine(transform.position, tp.wallPosition[i], Color.red);

            if (rightDotVTC > 0)
                calcForce += transform.right * -maxSpeed * obstacleDistance / dist;
            else
                calcForce += transform.right * maxSpeed * obstacleDistance / dist;
        }

        if (calcForce.magnitude > 0)
            AddForce(2.0f, calcForce);
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

        velocity += appliedForce * Time.deltaTime;
        velocity.y = 0;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

        if (velocity != Vector3.zero)
            transform.forward = velocity.normalized;
        transform.position += velocity * Time.deltaTime;
        forces.Clear();
        Debug.DrawRay(transform.position, transform.forward * 5);

    }

    private void Collision() {

        Vector3 calcForce = Vector3.zero;

        for (int i = 0; i < obstacles.Count; i++) {

            float obRadius = 10f;
            float agentRadius = 10f;

            Vector3 vecToCenter = obstacles[i] - transform.position;
            vecToCenter.y = 0;
            float dist = vecToCenter.magnitude;

            if (dist > obstacleDistance + obRadius + agentRadius)
                continue;

            if (Vector3.Dot(transform.forward, vecToCenter) < 0)
                continue;

            float rightDotVTC = Vector3.Dot(vecToCenter, transform.right);

            if (Mathf.Abs(rightDotVTC) > agentRadius + obRadius)
                continue;

            Debug.DrawLine(transform.position, obstacles[i], Color.red);

            if (rightDotVTC > 0)
                calcForce += transform.right * -maxSpeed * obstacleDistance / dist;
            else
                calcForce += transform.right * maxSpeed * obstacleDistance / dist;
        }
        for (int i = 0; i < 2; i++) {
            float dist = Mathf.Abs(tp.wallPosition[i].z - transform.position.z);
            if (dist > 5f)
                continue;

            Debug.DrawLine(transform.position, new Vector3(transform.position.x, 0, tp.wallPosition[i].z), Color.red);
            calcForce += transform.right * -1 * obstacleDistance / dist;
        }


        for (int i = 2; i < tp.wallPosition.Count; i++) {

            float obRadius = 5f;
            float agentRadius = 5f;

            Vector3 vecToCenter = tp.wallPosition[i] - transform.position;
            vecToCenter.y = 0;
            float dist = vecToCenter.magnitude;

            if (dist > obstacleDistance + obRadius + agentRadius)
                continue;

            if (Vector3.Dot(transform.forward, vecToCenter) < 0)
                continue;

            float rightDotVTC = Vector3.Dot(vecToCenter, transform.right);

            if (Mathf.Abs(rightDotVTC) > agentRadius + obRadius)
                continue;

            Debug.DrawLine(transform.position, tp.wallPosition[i], Color.red);

            if (rightDotVTC > 0)
                calcForce += transform.right * -maxSpeed * obstacleDistance / dist;
            else
                calcForce += transform.right * maxSpeed * obstacleDistance / dist;
        }

        if (calcForce.magnitude > 0)
            AddForce(2.0f, calcForce);
    }

    public void AddForce(float weight, Vector3 force) {
        forces.Add(new BehaviorForce() { Weight = weight, Force = force });
    }

    private bool lineIntersectsCircle(Vector3 ahead, Vector3 ahead2, Vector3 objectPostion) {
        // the property "center" of the obstacle is a Vector3D.
        return Vector3.Distance(objectPostion, ahead) <= 10f || Vector3.Distance(objectPostion, ahead2) <= 10f;
    }

}
