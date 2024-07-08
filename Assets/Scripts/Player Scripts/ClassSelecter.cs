using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu]
public class ClassSelection : ScriptableObject
{
	[SerializeField] private int myClass;

	public int MyClass
	{
		get { return myClass; }
		set { myClass = value; }
	}

}
