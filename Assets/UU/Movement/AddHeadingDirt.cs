using UnityEngine;
using PLib;
using PLib.Rand;

public class AddHeadingDirt : MonoBehaviour {

	public	float	initialMagnitude	=	45;
	public	float	updateMagnitude		=	45;
	public	float	spontaniusMagnitude	=	45;
	public	float	spontaniusChance	=	0.5f;

	void Start () {
		if (initialMagnitude == 0) return;
		transform.Rotate(PRand.RandomPosToNeg(initialMagnitude), PRand.RandomPosToNeg(initialMagnitude), Random.value * 360);
	}

	void LateUpdate () {
		transform.Rotate(PRand.RandomPosToNeg(updateMagnitude) * Time.deltaTime, 
		                 PRand.RandomPosToNeg(updateMagnitude) * Time.deltaTime,
		                 0);

		if (!PRand.ChancePerSec(spontaniusChance)) return;

		transform.Rotate(PRand.RandomPosToNeg(spontaniusMagnitude), 
		                 PRand.RandomPosToNeg(spontaniusMagnitude),
		                 0);

	}
}
