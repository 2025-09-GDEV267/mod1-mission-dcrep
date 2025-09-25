using System.Collections;
using System.Collections.Generic;
//using System.Numerics;
using UnityEngine;

public class Slingshot : MonoBehaviour {

    [Header("Inscribed")]
    public GameObject projectilePrefab;
    public float velocityMult = 10f;
    public GameObject projLinePrefab;

    [Header("Dynamic")]
    public GameObject launchPoint;
    public Vector3 launchPos;
    public GameObject projectile;
    public bool aimingMode;

   LineRenderer bandLine;

   void Awake()
   {
      Transform launchPointTrans = transform.Find("LaunchPoint");
      launchPoint = launchPointTrans.gameObject;
      launchPoint.SetActive(false);
      launchPos = launchPointTrans.position;

      CreateBand();
   }

   void CreateBand()
   {
      Transform leftArmTfm = this.transform.Find("LeftArm");
      GameObject leftArmGO = leftArmTfm.gameObject;

      bandLine = leftArmGO.AddComponent<LineRenderer>();
      bandLine.positionCount = 3;
      Vector3 leftArmPos = leftArmTfm.position;
      Bounds leftArmBounds = leftArmGO.GetComponent<Renderer>().bounds;
      //leftArmPos.y += 0.7f;
      float bandAttachY = leftArmPos.y + leftArmBounds.size.y / 3;
      leftArmPos.y = bandAttachY;

      Transform rightArmTfm = this.transform.Find("RightArm");
      //GameObject rightArmGO = rightArmTfm.gameObject;

      Vector3 rightArmPos = rightArmTfm.position;

      //rightArmPos.y += 0.7f;
      rightArmPos.y = bandAttachY;
      bandLine.SetPosition(0, leftArmPos);
      bandLine.SetPosition(1, launchPos);
      bandLine.SetPosition(2, rightArmPos);
      // Color still needs material set (otherwise it shows up as purple/pink)..
      bandLine.material = new Material(Shader.Find("Sprites/Default"));
      bandLine.startColor = Color.red;
      bandLine.endColor = Color.red;
      bandLine.startWidth = 0.1f;
      bandLine.endWidth = 0.1f;
      bandLine.enabled = false;
    }

   void OnMouseEnter()
   {
      //print("Slingshot:OnMouseEnter()");
      launchPoint.SetActive(true);
    }

    void OnMouseExit() {
        //print("Slingshot:OnMouseExit()");
        launchPoint.SetActive(false);
    }

   void OnMouseDown()
   {

      aimingMode = true;

      projectile = Instantiate(projectilePrefab) as GameObject;

      projectile.transform.position = launchPos;

      projectile.GetComponent<Rigidbody>().isKinematic = true;

      FollowCam.SWITCH_VIEW(FollowCam.eView.slingshot);
   }

   void Update()
   {

      if (!aimingMode) return;

      Vector3 mousePos2D = Input.mousePosition;
      mousePos2D.z = -Camera.main.transform.position.z;
      Vector3 mousePos3D = Camera.main.ScreenToWorldPoint(mousePos2D);

      Vector3 mouseDelta = mousePos3D -launchPos;

      float maxMagnitude = this.GetComponent<SphereCollider>().radius; // - 0.2f;
      if (mouseDelta.magnitude > maxMagnitude){
         mouseDelta.Normalize();
         mouseDelta*= maxMagnitude;
      }

      Vector3 projPos = launchPos + mouseDelta;
      projectile.transform.position = projPos;

      // Show band and set midpoint (of 3-point line) to projectile position
      bandLine.enabled = true;
      bandLine.SetPosition(1, projPos);

      if (Input.GetMouseButtonUp(0))
      {

         aimingMode = false;
         bandLine.enabled = false;
         Rigidbody projRB = projectile.GetComponent<Rigidbody>();
         projRB.isKinematic = false;
         projRB.collisionDetectionMode = CollisionDetectionMode.Continuous;
         projRB.linearVelocity = -mouseDelta * velocityMult;
         // Switch to slingshot view immediately before setting POI (switch-view issue otherwise)
         FollowCam.SWITCH_VIEW(FollowCam.eView.slingshot);
         FollowCam.POI = projectile;
         Instantiate<GameObject>(projLinePrefab, projectile.transform);
         projectile = null;
         MissionDemolition.SHOT_FIRED();
      }
   }
}
