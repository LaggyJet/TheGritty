using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClassSelection : MonoBehaviour
{
	[SerializeField] private int myClass;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public int MyClass
	{
		get { return myClass; }
		set { myClass = value; }
	}

}
