﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdvertiserBehaviour : MonoBehaviour {

    public struct BehaviorForce {
        public float Weight { get; set; }
        public Vector3 Force { get; set; }
    }

    public GameObject flyerPrefab;

    private List<GameObject> flyerInstances;
    private ShopperBehavior shopperBehavior;

    public float obstacleDistance = 8f;

    public float maxSpeed = 20.0f;
    public float maxForce = 20.0f;
    public float maxAvoidForce = 10.0f;

    public float observationDist = 25.0f;
    public float pitchDist = 5.0f;
    public float advertiseRate = 5f;
    public float advertiseProb = 0.9f;

    private Vector3 wanderForce;
    private Vector3 velocity;
    private Transform target;
    private List<Vector3> path;
    private List<Vector3> obstacles;

    private List<BehaviorForce> forces = new List<BehaviorForce>();
    private TPSpawn tp;
    private Chair chair;

    public bool traversing = false;
    public bool shopping = false;

    private float time = 0.0f;
    private float chaseTime = 0.0f;
    private float pitchTime = 0.0f;

    private bool flyeredSuccess;
    public int numOfSales = 0;

    void Start() {
        velocity = Vector3.zero;
        flyerInstances = new List<GameObject>();

        tp = GameObject.FindGameObjectWithTag("TP").GetComponent<TPSpawn>();
        obstacles = tp.planterPosition;
        obstacles.AddRange(tp.tablePosition);
    }

    // Update is called once per frame
    void Update() {
        DropFlyer();
        if (flyeredSuccess && target != null && shopperBehavior.flyerAttract) {
            //chaseTime += Time.deltaTime;
            //if (chaseTime <= 4f) {
            Seek();
            Pitch();
            //} else {
            //flyeredSuccess = false;
            //Wander();
            //}
        } else {
            Wander();
            flyeredSuccess = findFlyeredShopper();
        }
        Collision();
        Move();
    }
    void Pitch() {
        if(Vector3.Distance(target.position, transform.position) <= pitchDist) {
            pitchTime += Time.deltaTime;
            if(pitchTime >= 4.0f) {
                numOfSales++;
                flyeredSuccess = false;
                shopperBehavior.Reset();                
                pitchTime = 0;
            }
        }
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
        int index = 0;
        for (index = 0; index < shoppers.Length; index++) {
            bool flyerAttract = shoppers[index].GetComponent<ShopperBehavior>().flyerAttract;
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
        var desiredVelocity = target.transform.position - transform.position;
        desiredVelocity = desiredVelocity.normalized * maxSpeed;
        desiredVelocity -= velocity;
        AddForce(1.0f, desiredVelocity);
    }

    private void Wander() {
        if (transform.position.magnitude > 5f) {
            var directionToCenter = (Vector3.zero - transform.position).normalized;
            wanderForce = velocity.normalized + directionToCenter;
        } else if (Random.value < 0.5f) {
            wanderForce = GetRandomWanderForce();
        }
        var desiredVelocity = wanderForce;
        desiredVelocity = desiredVelocity.normalized * maxSpeed;
        desiredVelocity -= velocity;
        AddForce(1.0f, desiredVelocity);

    }

    private Vector3 GetRandomWanderForce() {
        var circleCenter = velocity.normalized;
        var randomPoint = Random.insideUnitCircle;

        var displacement = new Vector3(randomPoint.x, randomPoint.y) * 8f;
        displacement = Quaternion.LookRotation(velocity) * displacement;

        var wanderForce = circleCenter + displacement;

        return wanderForce;
    }

    public void setAngle(Vector3 vector, float value) {
        var len = vector.magnitude;
        vector.x = Mathf.Cos(value) * len;
        vector.z = Mathf.Sin(value) * len;
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
