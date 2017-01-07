using UnityEngine;
using System.Collections;
using Pathfinding;
using Pathfinding.RVO;

public class ASTest : SingletonMonoBehaviour<ASTest> {

    Seeker seeker;
    FunnelModifier fmodifier;
    RVOController controller;
    ActorPathFinding pathFinding;

    public Transform target;
     
   // Actor actor;

    protected override void Awake()
    {
        base.Awake();
        Logger.Init(true, null, false);
        IOTools.Init();
       // ScriptManager.Singleton.Init();   
    }

    void Start ()
    {
        IOTools.Init();
        BundleAsset.LoadAb("common" + IOTools.abSuffix);

         var actor = ActorManager.Singleton.CreateActor(0, ActorType.Player, 1,1000);
        ActorManager.Singleton.SetPlayer(actor);
       // CreateNavAgent();
       // pathFinding = new ActorPathFinding();
        //pathFinding.Init(seeker,controller,target);

        MyTileHandlerHelper.Singleton.Start();
    }


    void Update()
    {
        ActorManager.Singleton.Update(); 
    } 
	public void UpdateLogic (int delta) {
        //pathFinding.Update();
        //Int3 i3 = new Int3(20, 0, 0);
        //Int1 y = 0;
        //actor.ActorTransform.position += (Vector3)PathfindingUtility.Move(actor, i3, out y);
        MyTileHandlerHelper.Singleton.Update();
        // controller.Move(new Int3(0,0,5000));
        // controller.UpdateLogic(delta);  

        ActorManager.Singleton.UpdateLogic(delta);

        RVOSimulator.GetInstance().UpdateLogic(delta);
    }

 
}
