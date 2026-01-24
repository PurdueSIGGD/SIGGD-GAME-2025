using UnityEngine;

// literally just a holder that will identify the root object for a placeable item
// this is done so that we can fully delete the whole object from the parent down when we pick up the placeable.
// the root object exists so that we can change the pivot of the placeable object to the bottom center for placement purposes
//  - akarsh :)
public class PlaceableRoot : MonoBehaviour {}