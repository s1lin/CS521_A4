#Shilei Lin
==============
#COMP 521 Mordern Computer Games
==============
#Assignment 4
==============
#2019/12/03
==============

My implementaion for steering behavior involving adding forces with certain weight into a list, and caculating the sum of the force at the moving function:
```c#
float totalWeight = 0;
Vector3 steering = Vector3.zero;

forces.ForEach(e => totalWeight += e.Weight);
forces.ForEach(e => steering += e.Force * (e.Weight / totalWeight));

steering = Vector3.ClampMagnitude(steering, maxForce);

velocity += steering * Time.deltaTime;
velocity.y = 0;
velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
```		
#Steering behavior for both shopper and advertiser
==============
##Seek behavior:
==============
```c#
Vector3 desiredVelocity = target - transform.position;
desiredVelocity = desiredVelocity.normalized * maxSpeed;
desiredVelocity -= velocity;
AddForce(1.0f, desiredVelocity);
```		

##Collision Advoidence:
==============
```c#
Vector3 vecToCenter = obstaclePosition - transform.position;
float distance = vecToCenter.magnitude;
float zDirect = Vector3.Dot(transform.forward, vecToCenter);
float xDirect = Vector3.Dot(transform.right, vecToCenter);

//is it too far:
if (distance > obstacleDistance + obstacleRadius + agentRadius)
    return Vector3.zero;

//is it moving towards and too far from the x direction:
if (zDirect < 0 || Mathf.Abs(xDirect) > agentRadius + obstacleRadius)
    return Vector3.zero;

Debug.DrawLine(transform.position, obstaclePosition, Color.red);
return transform.right * (-Mathf.Sign(xDirect) * maxSpeed) * obstacleDistance / distance;
```

##Keep a distance with other agents
```c#
Vector3 calcForce = Vector3.zero;
//the amount of force is the reciprocal of the distance
agents.ForEach(e => calcForce += 10f < Vector3.Distance(transform.position, e) ? Vector3.zero : (transform.position - agentPosition).normalized / Vector3.Distance(transform.position, agentPosition));

calcForce /= agents.Count - 1;
calcForce = calcForce.normalized * maxSpeed - velocity;
```

But for advertisers, they need a wandering force in random directions:
```c#
Vector3 circleCenter = velocity.normalized;
Vector3 randomPoint = Random.insideUnitCircle;

//Give a random point on the "Ahead" circle to follow.
Vector3 displacement = new Vector3(randomPoint.x, randomPoint.y) * 10f;
displacement = Quaternion.LookRotation(velocity) * displacement;

Vector3 wanderForce = circleCenter + displacement;
return wanderForce;
```