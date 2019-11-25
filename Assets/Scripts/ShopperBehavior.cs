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
    public float maxSeeAhead = 20f;
    public float maxAvoidForce = 10.0f;

    public Vector3 velocity;
    public float magnitude;
    private Vector3 target;
    private List<Vector3> tables;
    private List<Vector3> planters;
    private List<GameObject> flyers;

    private List<BehaviorForce> forces = new List<BehaviorForce>();
    private TPSpawn tp;
    private Chair chair;

    public bool traversing = false;
    public bool shopping = false;

    public float shopTime = 0.0f;
    public float sitTime = 0.0f;
    public float flyerTime = 0.0f;

    void Start() {        
        velocity = Vector3.zero;  

        target = new Vector3(99f, 0, Random.Range(-15f, 15f));
        tp = GameObject.FindGameObjectWithTag("TP").GetComponent<TPSpawn>();
        planters = tp.planterPosition;
        tables = tp.tablePosition;

        float action = Random.Range(0f, 1f);
        traversing = action >= 0.5f;
        shopping = action < 0.5f;
        if (shopping) {
            int shopVisited = Random.Range(0, 16);
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
                Collision();
                Move();
                if (chair != null) {
                    chair.transform.GetComponent<CapsuleCollider>().enabled = true;
                }
            }

            if (shopping) {
                if (Vector3.Distance(target, transform.position) > obstacleDistance && chair == null) {
                    Seek();
                    Collision();
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
                        if (Vector3.Distance(target, transform.position) > 3f) {                            
                            Seek();
                            CollisionWithOtherTable();
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
                                tp.ChairReset(chair);
                            }
                        }                        
                    }
                }
            }
        }
    }

    Vector3 CollisionWithWall() {
        Vector3 calcForce = Vector3.zero;

        //for (int i = 0; i < 2; i++) {
        //    float dist = Mathf.Abs(tp.wallPosition[i].z - 32f - transform.position.z);
        //    if (dist > 5f)
        //        continue;

        //    Debug.DrawLine(transform.position, new Vector3(transform.position.x, 0, tp.wallPosition[i].z - 32f), Color.red);
        //    calcForce += transform.right * -maxSpeed * obstacleDistance / dist;
        //}

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
        return calcForce;
    }
    Vector3 CollisionWithPlanter() {
        Vector3 calcForce = Vector3.zero;

        for (int i = 0; i < planters.Count; i++) {

            float obRadius = 5f;
            float agentRadius = 5f;

            Vector3 vecToCenter = planters[i] - transform.position;
            vecToCenter.y = 0;
            float dist = vecToCenter.magnitude;

            if (dist > obstacleDistance + obRadius + agentRadius)
                continue;

            if (Vector3.Dot(transform.forward, vecToCenter) < 0)
                continue;

            float rightDotVTC = Vector3.Dot(vecToCenter, transform.right);

            if (Mathf.Abs(rightDotVTC) > agentRadius + obRadius)
                continue;

            Debug.DrawLine(transform.position, planters[i], Color.red);

            if (rightDotVTC > 0)
                calcForce += transform.right * -maxSpeed * obstacleDistance / dist;
            else
                calcForce += transform.right * maxSpeed * obstacleDistance / dist;
        }
        return calcForce;
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

    void CollisionWithOtherTable() {
        Vector3 calcForce = CollisionWithWall();
        calcForce += CollisionWithPlanter();

        for (int i = 0; i < tables.Count; i++) {
            if (i == chair.parent)
                continue;
            float obRadius = 8f;
            float agentRadius = 8f;

            Vector3 vecToCenter = tables[i] - transform.position;
            vecToCenter.y = 0;
            float dist = vecToCenter.magnitude;

            if (dist > obstacleDistance + obRadius + agentRadius)
                continue;

            if (Vector3.Dot(transform.forward, vecToCenter) < 0)
                continue;

            float rightDotVTC = Vector3.Dot(vecToCenter, transform.right);

            if (Mathf.Abs(rightDotVTC) > agentRadius + obRadius)
                continue;

            Debug.DrawLine(transform.position, tables[i], Color.red);

            if (rightDotVTC > 0)
                calcForce += transform.right * -maxSpeed * obstacleDistance / dist;
            else
                calcForce += transform.right * maxSpeed * obstacleDistance / dist;
        }

        if (calcForce.magnitude > 0)
            AddForce(2.0f, calcForce);
    }

    private void Collision() {

        Vector3 calcForce = CollisionWithWall();
        calcForce += CollisionWithPlanter();

        for (int i = 0; i < tables.Count; i++) {

            float obRadius = 8f;
            float agentRadius = 8f;

            Vector3 vecToCenter = tables[i] - transform.position;
            vecToCenter.y = 0;
            float dist = vecToCenter.magnitude;

            if (dist > obstacleDistance + obRadius + agentRadius)
                continue;

            if (Vector3.Dot(transform.forward, vecToCenter) < 0)
                continue;

            float rightDotVTC = Vector3.Dot(vecToCenter, transform.right);

            if (Mathf.Abs(rightDotVTC) > agentRadius + obRadius)
                continue;

            Debug.DrawLine(transform.position, tables[i], Color.red);

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
        if (magnitude > 14) {
            velocity = Vector3.zero;
        }
        magnitude = velocity.magnitude;
        transform.position += velocity * Time.deltaTime;
        forces.Clear();
        Debug.DrawRay(transform.position, transform.forward * 5);
    }

    

    public void AddForce(float weight, Vector3 force) {
        forces.Add(new BehaviorForce() { Weight = weight, Force = force });
    }  

}
