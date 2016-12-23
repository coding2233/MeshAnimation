using UnityEngine;
using System.Collections;

public class demo5 : MonoBehaviour {

    public GameObject _Target;
	
	void Update () {
        if (_Target != null)
            _Target.transform.Rotate(0,0,Time.deltaTime*30);
	}
}
