using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActorManager : SingletonTemplate<ActorManager> {
    
    private Dictionary<long, Actor> actors = new Dictionary<long, Actor>(); 
    private Actor mainPlayer =null; 

    class AreaInfo
    {
        public int id;
        public List<Actor> actors = new List<Actor>();
    }

 
    /// <summary>
    /// 创建actor
    /// </summary>  
    public Actor CreateActor(int itemId, ActorType type,int campId,long actorId = 0)
    {
        Actor actor = null;
        actorId = actorId==0 ? (int)IdAssginer.getId(IdAssginer.IdType.ActorId) : actorId;
        //test
        switch (type)
        {
            case ActorType.Monster:
            case ActorType.Fly:
            case ActorType.Ship:
                actor = new Actor(itemId, type, actorId, campId);
                break;  
            case ActorType.Player:
            case ActorType.OtherPlayer:
                actor = new Actor(itemId,type,actorId, campId);
                break;
        }

        if (actor != null)
        {
            if (actors.ContainsKey(actorId))
            {
                Debug.LogError("重复添加actor:" + actorId);
            }
            else
            {
                actors[actorId] = actor;
                Debug.LogError("添加actor:" + actorId);
            }
            actor.Initalize();
        }
        return actor;
    }

    /// <summary>
    /// 切换主控制对象
    /// </summary> 
    public void SetPlayer(Actor actor)
    {
        mainPlayer = actor;
    }

    public Actor GetPlayer()
    {
        return mainPlayer;
    }

    public Actor GetActor(int id)
    {
        Actor actor = null;
        actors.TryGetValue(id, out actor);
        return actor;
    } 
 
    public Actor GetActorByServerID(int id)
    { 
        foreach (var v in actors)
        {
            if (v.Value.ServerId == id)
                return v.Value;
        }
        return null;
    } 

    /// <summary>
    /// test
    /// </summary>
    public void Update()
    { 
        var iter = actors.GetEnumerator(); 

        while (iter.MoveNext()) 
        {
            var actor = iter.Current.Value;
            if (actor != null )
            {
                actor.Update(Time.deltaTime);
            }     
        } 
    }

    /// <summary>
    /// test
    /// </summary>
    public void UpdateLogic(int delta)
    {
        var iter = actors.GetEnumerator();

        while (iter.MoveNext())
        {
            var actor = iter.Current.Value;
            if (actor != null)
            {
                actor.UpdateLogic(delta);
            }
        }
    }

    public void SyncActorTransform(int id, Vector3 pos, float dirY)
    {
        var actor  = GetActor(id);
        Actor ae = actor;
        if (ae != null)
        {
            ae.rcvSyncTransform(pos,dirY);
        }
    }
}
