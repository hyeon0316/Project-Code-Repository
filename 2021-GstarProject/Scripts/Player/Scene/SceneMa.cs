using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneMa : MonoBehaviour
{
    public string sceneName;
    public Vector3 spwanVector;
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            Player.inst.isMove = false;
            Player.inst.isSkill = true;
            Player.inst.animator.SetBool("isMove", false);
            Player.inst.rigidbody.useGravity = false;
            other.transform.position = spwanVector;
            LoadingSceneManager.LoadScene(sceneName);
            
            //SceneManager.LoadScene(sceneName);
        }
    }
}
