using UnityEngine;

public class Chair {

    public Transform transform;

    public int index;
    public int parent;

    public bool isAssigned;

    public Chair(Transform t, int index, int parent, bool isAssigned) {
        transform = t;
        this.index = index;
        this.parent = parent;
        this.isAssigned = isAssigned;
    }

}
