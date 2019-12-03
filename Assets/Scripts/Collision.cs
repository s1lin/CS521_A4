using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collision : MonoBehaviour {

    private static float agentRadius = 3f;

    private static Vector3 CalculateForce(float obstacleRadius, float agentRadius, Vector3 obstaclePosition, Transform transform, float obstacleDistance, float maxSpeed) {
        Vector3 vecToCenter = obstaclePosition - transform.position;
        float distance = vecToCenter.magnitude;
        float zDirect = Vector3.Dot(transform.forward, vecToCenter);
        float xDirect = Vector3.Dot(transform.right, vecToCenter);

        //is it too far:
        if (distance > obstacleDistance + obstacleRadius + agentRadius)
            return Vector3.zero;

        //is it moving towards:
        if (zDirect < 0 || Mathf.Abs(xDirect) > agentRadius + obstacleRadius)
            return Vector3.zero;

        Debug.DrawLine(transform.position, obstaclePosition, Color.red);
        return transform.right * (-Mathf.Sign(xDirect) * maxSpeed) * obstacleDistance / distance;
    }

    private static Vector3 CalculateForce(Transform transform, Vector3 agentPosition) {
        Debug.DrawLine(transform.position, agentPosition, Color.blue);
        return (transform.position - agentPosition).normalized / Vector3.Distance(transform.position, agentPosition);
    }

    public static Vector3 CollisionWithWall(List<Vector3> wallPosition, Transform transform, float obstacleDistance, float maxSpeed) {
        Vector3 calcForce = Vector3.zero;
        float obstacleRadius = 3f;

        for (int i = 2; i < wallPosition.Count; i++) {
            calcForce += CalculateForce(obstacleRadius, agentRadius, wallPosition[i], transform, obstacleDistance, maxSpeed);
        }

        return calcForce;
    }

    public static Vector3 CollisionWithWall(List<Vector3> wallPosition, Transform transform, float obstacleDistance, float maxSpeed, int shopVisited) {
        Vector3 calcForce = Vector3.zero;
        float obstacleRadius = 3f;

        for (int i = 0; i < wallPosition.Count; i++) {
            if (i != shopVisited)
                calcForce += CalculateForce(obstacleRadius, agentRadius, wallPosition[i], transform, obstacleDistance, maxSpeed);
        }
        return calcForce;
    }

    public static Vector3 CollisionWithPlanter(List<Vector3> planters, Transform transform, float obstacleDistance, float maxSpeed) {
        Vector3 calcForce = Vector3.zero;
        float obstacleRadius = 5f;

        planters.ForEach(e => calcForce += CalculateForce(obstacleRadius, agentRadius, e, transform, obstacleDistance, maxSpeed));

        return calcForce;
    }

    public static Vector3 CollisionWithTable(List<Vector3> tables, Transform transform, float obstacleDistance, float maxSpeed, int tableNum) {
        Vector3 calcForce = Vector3.zero;

        float obstacleRadius = 10f;

        for (int i = 0; i < tables.Count; i++) {
            if (i != tableNum) {
                calcForce += CalculateForce(obstacleRadius, agentRadius, tables[i], transform, obstacleDistance, maxSpeed);
            }
        }
        return calcForce;
    }

    public static Vector3 CollisionWithChair(List<Chair> chairs, Transform transform, float obstacleDistance, float maxSpeed, int tableNum) {
        Vector3 calcForce = Vector3.zero;
        float obstacleRadius = 8f;

        chairs.ForEach(e => calcForce += e.parent == tableNum ? Vector3.zero : CalculateForce(obstacleRadius, agentRadius, e.transform.position, transform, obstacleDistance, maxSpeed));

        //if (Mathf.Abs(rightDotVTC) > agentRadius + obstacleRadius)
        //    continue;
        return calcForce;
    }

    public static Vector3 CollisionWithOtherAgent(List<Vector3> agents, Transform transform, float obstacleDistance, float maxSpeed, Vector3 velocity) {

        Vector3 calcForce = Vector3.zero;

        agents.ForEach(e => calcForce += 13f < Vector3.Distance(transform.position, e) ? Vector3.zero : CalculateForce(transform, e));
    
        calcForce /= agents.Count - 1;
        calcForce = calcForce.normalized * maxSpeed - velocity;
        return calcForce;
    }
}
