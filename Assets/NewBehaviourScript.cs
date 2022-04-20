using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scorpio.Timer;
public class NewBehaviourScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(Time.time);
        TimerManager.Instance.AddGameTimerMS(1000, (timer, args, fixedArgs) => {
            Debug.Log(Time.time);
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
