using UnityEngine;

public class Lighting : MonoBehaviour
{
    public Material lightingMat;
    public Transform position;
	private static Lighting instance;
 
	private void Awake()
	{
		if(instance == null)
		{
			instance = this;
		}
		else
		{
			Destroy(gameObject);
		}
	}

	private void Update()
	{
        lightingMat.SetVector("_Player_Pos", new Vector3(0, 1, 0));
	}
}
