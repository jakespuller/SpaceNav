using UnityEngine;
using System.Collections.Generic;

/*
 * 
 * This class saves and loads some data of a GameObject. 
 * It also works with nested objects.
 * Simply attach this script to the object you want to save the Transform of.
 * 
 * It will automatically load of Start, 
 * and save on when the application quits or level is changed.
 * 
 */
public class SaveScript : MonoBehaviour 
{
	// This is an ID which uniquely defines this object.
	public string uniqueID;
	
	/*
	 * This is where we'll save our data.
	 * 
	 * OnDestroy is called when the application is quit, and when the level is changed.
	 */
	void OnDestroy()
	{
		// Note: We use unique ID, appended by the name of the data we are
		// saving as the tag.
		
		// Save the position and rotation by saving the Transform.
		ES2.Save (this.transform, "myFile.txt?tag="+uniqueID+"transform");
		
		// We can also save some public variables in a script attached to this object,
		// if that object has that script attached.
		MyExampleScript myScript = GetComponent<MyExampleScript>();
		if(myScript != null)
		{
			ES2.Save(myScript.intArray, "myFile.txt?tag="+uniqueID+"intArray");
			ES2.Save(myScript.myString, "myFile.txt?tag="+uniqueID+"myString");
		}
	}
	
	/*
	 * This is where we'll load our data.
	 */
	void Start()
	{
		// If there's no data to load, exit early.
		if(!ES2.Exists("myFile.txt"))
			return;
		
		// Note: We use the same uniqueID to load the data as we used to save.
		
		// Load the Transform using the self-assigning function.
		ES2.Load<Transform>("myFile.txt?tag="+uniqueID+"transform", this.transform);
		
		// We can also save some public variables in a script attached to this object
		// if that object has that script attached.
		MyExampleScript myScript = GetComponent<MyExampleScript>();
		if(myScript != null)
		{
			myScript.intArray = ES2.LoadArray<int>("myFile.txt?tag="+uniqueID+"intArray");
			myScript.myString = ES2.Load<string>("myFile.txt?tag="+uniqueID+"myString");
		}
	}
}
