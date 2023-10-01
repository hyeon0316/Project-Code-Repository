using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraManager : MonoBehaviour
{
    /*
1.public으로 선언 된 변수는 앞글자 대문자로 시작
2. private는 변수 앞에 "_"붙이고  소문자로 시작
3. 함수 안의 지역변수는 네이밍 규칙 상관없음
4. bool형 변수는 is~ or can~ 로 시작
번외로  나중에 구현해야 할 일을 스크립트에 적어 놓을때 //todo: 할 일(메모장 느낌)
*/
    //카메라 음직임 타입 테스트가 끝나면 사용하는 함수와 함께 주석
    public int CameraMovetype = 0;
    //플레이어 게임 오브젝트
    public GameObject Target;
    //백그라운드 오브젝트
    public GameObject BackgroundImg;
    //v2를 사용할 경우 카메라가 플레이어를 느리게 따라가는데 그 따라가는 속도롤 결정(0.1~ 0.04 사이로 생각)
    public float CameraMoveSpeed;

    //카메라 크기
    private Vector2 _cameraSize;
    //백그라운드의 최대값
    private Vector3 _maxsize;
    //백그라운드의 최소값
    private Vector3 _minsize;

    public BoxCollider FloorCollider;
    
    public float MaxCameratoPlayerX = 0.65f;
    
    public float MinCameratoPlayerX = 0.35f;
    
    public float MaxCameratoPlayerY = 0.8f;
    
    public float MinCameratoPlayerY = 0.45f;

    
    // Start is called before the first frame update
    public void Start()
    {
      /*
        orthographic 카메라와 perspective 카메라를 분리하여 2d 카메라와 3d 카메라 를 분리했다.
      */
        _cameraSize.y = transform.GetChild(0).GetComponent<Camera>().orthographicSize;
        _cameraSize.x = _cameraSize.y * transform.GetChild(0).GetComponent<Camera>().aspect;

        BackgroudUpdate();

       
    }


    public void LateUpdate()
    {
        CameraType();
    }

    public void BackgroudUpdate()
    {
        
        BackgroundImg = GameObject.Find("Background");
        _maxsize = new Vector2(BackgroundImg.GetComponent<MeshRenderer>().bounds.max.x,
            BackgroundImg.GetComponent<MeshRenderer>().bounds.max.y);
        _minsize = new Vector2(BackgroundImg.GetComponent<MeshRenderer>().bounds.min.x,
            BackgroundImg.GetComponent<MeshRenderer>().bounds.min.y);

        FloorCollider = GameObject.Find("FloorCollider").GetComponent<BoxCollider>();
        _maxsize = new Vector3(BackgroundImg.GetComponent<MeshRenderer>().bounds.max.x,
            BackgroundImg.GetComponent<MeshRenderer>().bounds.max.y, FloorCollider.bounds.max.z - 1.5f);

        _minsize = new Vector3(BackgroundImg.GetComponent<MeshRenderer>().bounds.min.x,
            BackgroundImg.GetComponent<MeshRenderer>().bounds.min.y, FloorCollider.bounds.min.z - 1.5f);
    }



    /// <summary>
    /// 여러가지 타입을 테스트 해보기 위한 switch 문
    /// </summary>
    private void CameraType()
    {
        switch (CameraMovetype)
        {
            case 0:
                CameraMove_v1();

                break;
            case 1:
                CameraMove_v2();
                break;
        }
    }


    public void ChangeCameraType()
    {
        StartCoroutine(ChangeCameraTypeCo());
    }
    private IEnumerator ChangeCameraTypeCo()
    {
        yield return new WaitForSeconds(0.1f);
        CameraMovetype = 1;
    }
    
    
    /// <summary>
    /// 카메라가 백그라운드를 넘어가는것을 막는것을 확인하는 함수
    /// </summary>
    /// <param name="size">플레이어의 x,y 값 </param>
    /// <returns>만약 받은 인자 x,y 값이 백그라운드 값을 초과하거나 감소했을경우 최대값, 최소값을 반환해준다.</returns>
    private Vector3 MaxMinSize(Vector3 size)
    {
        
        return new Vector3(
              Mathf.Clamp(size.x, _minsize.x + (_cameraSize.x), _maxsize.x - (_cameraSize.x))
            , Mathf.Clamp(size.y, _minsize.y + _cameraSize.y, _maxsize.y - _cameraSize.y)
            , Mathf.Clamp(size.z, _minsize.z , _maxsize.z )
            );
    }   

    /// <summary>
    /// 카메라 즉시 이동
    /// </summary>
    private void CameraMove_v1()
    {
        Vector2 playerposition = MaxMinSize(new Vector2(Target.transform.position.x, Target.transform.position.y));

        
        this.transform.position = new Vector3( playerposition.x, playerposition.y + 2 ,this.transform.position.z );
    }

    /// <summary>
    /// 카메라 천천히 이동
    /// </summary>
    private void CameraMove_v2()
    {
        Vector3 playerposition = MaxMinSize(new Vector3(Target.transform.position.x, Target.transform.position.y + 1.8f,Target.transform.position.z - 4.5f));

        //플레이어가 카메라 화면 좌우로 일정 이상 나간다면 카메라 위치를 다시 잡아주는 라인
        if(OutPlayertoCameraX())
        this.transform.position = Vector3.Lerp(this.transform.position, new Vector3(playerposition.x, this.transform.position.y, this.transform.position.z), CameraMoveSpeed * Time.deltaTime );


        //플레이어가 카메라 화면 상하로 일정 이상 나간다면 카메라 위치를 다시 잡아주는 라인
        if(OutPlayertoCameraY())
        this.transform.position = Vector3.Lerp(this.transform.position, new Vector3(this.transform.position.x, playerposition.y, this.transform.position.z), CameraMoveSpeed * Time.deltaTime);


        this.transform.position = Vector3.Lerp(this.transform.position, new Vector3(this.transform.position.x, this.transform.position.y, playerposition.z), CameraMoveSpeed * Time.deltaTime);

    }


    /// <summary>
    /// 플레이어가 카메라좌우로 일정량 이상 나갔는지 확인하는 함수
    /// </summary>
    /// <returns>플레이어가 일정량 이상 나갔다면 true 아니면 false를 반환한다.</returns>
    private bool OutPlayertoCameraX()
    {
        Vector3 P_position = Camera.main.WorldToScreenPoint(Target.transform.position);
        Vector2 cameraScale = new Vector2(Camera.main.pixelWidth, Camera.main.pixelHeight);


        if (P_position.x < cameraScale.x * MinCameratoPlayerX || P_position.x > cameraScale.x * MaxCameratoPlayerX)
            return true;
      
        return false;
    }

    /// <summary>
    /// 플레이어가 카메라 상하로 일정량 이상 나갔는지 확인하는 함수
    /// </summary>
    /// <returns>플레이어가 일정량 이상 나갔다면 true 아니면 false를 반환한다.</returns>
    private bool OutPlayertoCameraY()
    {
        Vector3 P_position = Camera.main.WorldToScreenPoint(Target.transform.position);
        Vector2 cameraScale = new Vector2(Camera.main.pixelWidth, Camera.main.pixelHeight);

        

        if (P_position.y < cameraScale.y * MinCameratoPlayerY || P_position.y > cameraScale.y * MaxCameratoPlayerY)
            return true;


        return false;
    }



}
