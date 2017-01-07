using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using Pathfinding.RVO;

[System.Serializable]
public class Actor { 
    [Describe("actor唯一id")]
    public long ActorId { get; set; }

    [Describe("服务器id")]
    public int ServerId { get; set; }

    [Describe("队伍id")]
    public int CampId { get; private set; }

    [Describe("显示的GameObject")]
    GameObject ShowObj;

    //BehaviorAgent behaviorTree;

    [Describe("actor 类型")]
    public ActorType AType;
     
    [Describe("actor位置,通过Int3整数计算")]
    Int3 position = Int3.zero; 
    public Int3 Position
    {
        get
        { 
            return position;
        }
        set
        {
            position = value;
            ActorTransform.position = position.vec3;
        }
    }

    public bool hasReachedNavEdge;
    public Int1 groundY = 0;

    /// <summary>
    /// 
    /// </summary> 
    public Actor(int itemId, ActorType type, long actorId,int campId)
    { 
        this.ActorId = actorId;
        CampId = campId;   
        AType = type;
    }   

    /// <summary>
    /// 
    /// </summary>
    public virtual Transform ActorTransform
    {
        get
        {
            if (ShowObj != null)
                return ShowObj.transform;
            return null;
        }
    }

 
    /// <summary>
    /// 
    /// </summary>
    public virtual void Initalize()
    {
        LoadRes();

        ShowObj.getOrAddComponent<ActorHolder>().Owner = this;  
        InitState();
        LoadAI();

        CreateNavAgent();
    }

    public virtual void InitState()
    {
        //stateMachine.RegisterState(GameState.ActorIdle.ToString(),new ActorIdleState());
        ////stateMachine.RegisterState(GameState.ActorDead.ToString(), new Actord()); 
        //stateMachine.RegisterState(GameState.ActorMove.ToString(), new ActorMoveState()); 
        //stateMachine.RegisterState(GameState.ActorAttack.ToString(), new ActorAttackState());
        //stateMachine.RegisterState(GameState.ActorMoveToActor.ToString(), new ActorMoveToActorState());
        //stateMachine.RegisterState(GameState.ActorDead.ToString(), new ActorDeadState());
        //stateMachine.RegisterState(GameState.ActorBeHit.ToString(), new ActorHurtState());
        //stateMachine.RegisterState(GameState.ActorFackToActor.ToString(), new ActorFaceToActorState());

    }

    /// <summary>
    /// 加载必要资源
    /// </summary>
    public virtual void LoadRes()
    {
        GameObject prefab = null;
        switch (AType)
        {
            case ActorType.Monster:
                prefab = ResHelper.LoadModel("soldier1");
                break;
            case ActorType.Fly:
                prefab = ResHelper.LoadModel("soldier3");
                break;
            case ActorType.Ship:
                prefab = ResHelper.LoadModel("soldier2");
                break;
            case ActorType.Player: 
            case ActorType.OtherPlayer: 
                prefab = ResHelper.LoadModel("asu");
                break;
        }
        if (prefab != null)
        {
            ShowObj  = GameObject.Instantiate(prefab);
            Position = (Int3)ShowObj.transform.position;
        } 
    }

    public virtual void Hurt(int value, bool isBomb = false)
    {
       
    } 
        

    public virtual Vector3 GetCurPos()
    {
        return ActorTransform.position;
    } 

    /// <summary>
    /// 加载ai资源
    /// </summary>
    public virtual void LoadAI()
    {
        //behaviorTree = showObj.getOrAddComponent<BehaviorTree>(); 
        //behaviorTree.ExternalBehavior = Resources.Load<ExternalBehavior>("Behavior/soldier");
        //behaviorTree.StartWhenEnabled = false;
        //behaviorTree.RestartWhenComplete = true;
        //behaviorTree.PauseWhenDisabled = true; 
        //behaviorTree.EnableBehavior();
        //behaviorTree.Start();
    }
   
    public virtual bool IsDead()
    { 
        return false;
    }

    public void UpdateLogic(int delta)
    {
        controller.UpdateLogic(delta);
    }

    public void ExecMove(Int3 dir,int speed)
    { 
        controller.Move(dir * speed); 
    }

    int oldDegree = 0;
    public virtual void Update(float delta)
    {
       
    }


    Seeker seeker;
    FunnelModifier fmodifier;
    RVOController controller;
    ActorPathFinding pathFinding;
    void CreateNavAgent()
    {
        var actorShowObj = ActorTransform.gameObject;
        seeker = actorShowObj.getOrAddComponent<Seeker>();
        fmodifier = actorShowObj.getOrAddComponent<FunnelModifier>();
        controller = actorShowObj.getOrAddComponent<RVOController>();
        controller.center = new Int3(0, 1600, 0);
        Position = (Int3)new Vector3(0,13,-41);
        controller.EnsureActorAndSimulator();
    }

    /// <summary>
    /// 接受从服务器同步的transform信息
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="dirY"></param>
    public void rcvSyncTransform(Vector3 pos, float dirY)
    {
       
    }

    public virtual void Dispose()
    {  
        GameObject.Destroy(ShowObj);
    }

    /// <summary>
    /// 播放动画
    /// </summary>
    public virtual void PlayAni(string name,float blendTime,float speed,WrapMode mode)
    {
       
    }

    /// <summary>
    /// test 使用普通技能
    /// </summary>
    public void UseSkill()
    { 
    }
}
