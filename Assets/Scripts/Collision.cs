using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collision : MonoBehaviour {

    public static Vector3 CollisionWithWall(List<Vector3> wallPosition, Transform transform, float obstacleDistance, float maxSpeed) {
        Vector3 calcForce = Vector3.zero;

        //for (int i = 0; i < 2; i++) {
        //    float dist = Mathf.Abs(tp.wallPosition[i].z - 32f - transform.position.z);
        //    if (dist > 5f)
        //        continue;

        //    Debug.DrawLine(transform.position, new Vector3(transform.position.x, 0, tp.wallPosition[i].z - 32f), Color.red);
        //    calcForce += transform.right * -maxSpeed * obstacleDistance / dist;
        //}

        for (int i = 2; i < wallPosition.Count; i++) {

            float obRadius = 3f;
            float agentRadius = 3f;

            Vector3 vecToCenter = wallPosition[i] - transform.position;
            vecToCenter.y = 0;
            float dist = vecToCenter.magnitude;

            if (dist > obstacleDistance + obRadius + agentRadius)
                continue;

            if (Vector3.Dot(transform.forward, vecToCenter) < 0)
                continue;

            float rightDotVTC = Vector3.Dot(vecToCenter, transform.right);

            if (Mathf.Abs(rightDotVTC) > agentRadius + obRadius)
                continue;

            Debug.DrawLine(transform.position, wallPosition[i], Color.red);

            if (rightDotVTC > 0)
                calcForce += transform.right * -maxSpeed * obstacleDistance / dist;
            else
                calcForce += transform.right * maxSpeed * obstacleDistance / dist;
        }
        return calcForce;
    }

    public static Vector3 CollisionWithWall(List<Vector3> wallPosition, Transform transform, float obstacleDistance, float maxSpeed, int shopVisited) {
        Vector3 calcForce = Vector3.zero;

        for (int i = 0; i < wallPosition.Count; i++) {
            if (i == shopVisited)
                continue;
            float obRadius = 3f;
            float agentRadius = 3f;

            Vector3 vecToCenter = wallPosition[i] - transform.position;
            vecToCenter.y = 0;
            float dist = vecToCenter.magnitude;

            if (dist > obstacleDistance + obRadius + agentRadius)
                continue;

            if (Vector3.Dot(transform.forward, vecToCenter) < 0)
                continue;

            float rightDotVTC = Vector3.Dot(vecToCenter, transform.right);

            if (Mathf.Abs(rightDotVTC) > agentRadius + obRadius)
                continue;

            Debug.DrawLine(transform.position, wallPosition[i], Color.red);

            if (rightDotVTC > 0)
                calcForce += transform.right * -maxSpeed * obstacleDistance / dist;
            else
                calcForce += transform.right * maxSpeed * obstacleDistance / dist;
        }
        return calcForce;
    }


    public static Vector3 CollisionWithPlanter(List<Vector3> planters, Transform transform, float obstacleDistance, float maxSpeed) {
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

    public static Vector3 CollisionWithTable(List<Vector3> tables, Transform transform, float obstacleDistance, float maxSpeed, int tableNum) {
        Vector3 calcForce = Vector3.zero;
        for (int i = 0; i < tables.Count; i++) {
            if (i == tableNum)
                continue;
            float obRadius = 10f;
            float agentRadius = 10f;

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
        return calcForce;
    }

    public static Vector3 CollisionWithChair(List<Chair> chairs, Transform transform, float obstacleDistance, float maxSpeed, int tableNum) {
        Vector3 calcForce = Vector3.zero;
        for (int i = 0; i < chairs.Count; i++) {
            if (chairs[i].parent == tableNum)
                continue;
            float obRadius = 5f;
            float agentRadius = 5f;

            Vector3 vecToCenter = chairs[i].transform.position - transform.position;
            vecToCenter.y = 0;
            float dist = vecToCenter.magnitude;

            if (dist > obstacleDistance + obRadius + agentRadius)
                continue;

            if (Vector3.Dot(transform.forward, vecToCenter) < 0)
                continue;

            float rightDotVTC = Vector3.Dot(vecToCenter, transform.right);

            if (Mathf.Abs(rightDotVTC) > agentRadius + obRadius)
                continue;

            Debug.DrawLine(transform.position, chairs[i].transform.position, Color.red);

            if (rightDotVTC > 0)
                calcForce += transform.right * -maxSpeed * obstacleDistance / dist;
            else
                calcForce += transform.right * maxSpeed * obstacleDistance / dist;
        }
        return calcForce;
    }

    public static Vector3 CollisionWithOtherAgent(List<Vector3> agents, Transform transform, float obstacleDistance, float maxSpeed, Vector3 velocity) {

        Vector3 separationForce = Vector3.zero;
        int index = 0;

        // separation
        foreach (Vector3 c in agents) {

            float distance = Vector3.Distance(transform.position, c);
            if (distance < 10f) {
                separationForce += (transform.position - c).normalized / distance;
                Debug.DrawLine(transform.position, c, Color.red);
                index++;
            }
        }
        if (index == 0)
            return Vector3.zero;

        separationForce /= agents.Count - 1;
        separationForce = separationForce.normalized * maxSpeed - velocity;
        return separationForce;
    }
}
