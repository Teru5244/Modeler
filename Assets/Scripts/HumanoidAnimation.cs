using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanoidAnimation : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject body; // assigned through Inspector pane
    public GameObject head;
    public GameObject leftArm;
    public GameObject rightArm;
    public GameObject leftLeg;
    public GameObject rightLeg;
    private Vector3 rotation1 = Vector3.zero;
    private Vector3 rotation2 = Vector3.zero;
    private Vector3 rotation3 = Vector3.zero;
    private bool headDirection = true;
    private bool limbDirection = true;
    private Vector3 movement;
    private bool rotate;
    private bool walk;
    private bool nod;

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (rotate)
        {
            movement = new Vector3(0, 1, 0);
            rotation1 += movement;
            rotation2 += movement;
            rotation3 += movement;
            body.transform.rotation = Quaternion.Euler(rotation1);
        }
        
        if (walk)
        {
            movement = new Vector3(0, 0, 2);
            if (rotation2.z >= 30)
            {
                limbDirection = false;
            } 
            else if (rotation2.z <= -30)
            {
                limbDirection = true;
            }
            
            if (limbDirection)
            {
                rotation2 += movement;
            } 
            else
            {
                rotation2 -= movement;
            }
            rightArm.transform.rotation = Quaternion.Euler(rotation2);
            leftArm.transform.rotation = Quaternion.Euler(new Vector3(rotation2.x, rotation2.y, -rotation2.z));
            leftLeg.transform.rotation = Quaternion.Euler(rotation2);
            rightLeg.transform.rotation = Quaternion.Euler(new Vector3(rotation2.x, rotation2.y, -rotation2.z));
        }
        
        if (nod)
        {
            movement = new Vector3(0, 0, 1);
            if (rotation3.z >= 0)
            {
                headDirection = true;
            } 
            else if (rotation3.z <= -20)
            {
                headDirection = false;
            }

            if (headDirection)
            {
                rotation3 -= movement;
            } 
            else
            {
                rotation3 += movement;
            }
            head.transform.rotation = Quaternion.Euler(rotation3);
        }
    }

    public void BodyRotate()
    {
        if (rotate)
        {
            rotate = false;
        }
        else
        {
            rotate = true;
        }
    }

    public void Walk()
    {
        if (walk)
        {
            walk = false;
        }
        else
        {
            walk = true;
        }
    }
    
    public void Nod()
    {
        if (nod)
        {
            nod = false;
        }
        else
        {
            nod = true;
        }
    }
}
