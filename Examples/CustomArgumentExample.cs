#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ParrelSync.Example
{
    public class CustomArgumentExample : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            if (ClonesManager.IsClone())
            {
                Debug.Log("This is a clone project.");

                string customArgument = ClonesManager.GetArgument();
                Debug.Log("The custom argument of this clone project is: " + customArgument);
                //Do what ever you need with the argument string
            }
            else
            {
                Debug.Log("This is the original project.");
            }
        }
    }
}
#endif