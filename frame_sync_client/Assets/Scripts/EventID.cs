using System;


public enum EventID
{ 
    OnNetConnect,
    OnNetDisConnect,
    SceneLoadFinish,  

    OnRightJoystickMove,
    OnRightJoystickTouchStart,
    OnRightJoystickTouchEnd, 

    WindowOpen,
    WindowClose,

    //组队相关事件
    RefreshTeamInfo,
    //离开队伍事件（被踢或主动退出）
    LeaveTeam,
    //队伍解散消息
    DismissTeam,
}

/// 新手引导事件.
public enum GuideEventId
{
    NewbieUpdated = 4000000,
    GuideFinished,
    OnStepBegin,
    OnStepFinish,
    GuideException,      //异常处理
    DoGuideStep,         //完成步骤.
}

/// 新手引导触发事件.
public enum GuideStepEventId
{
    AutoTrigger = 5000000,
    OnWindowOpen,       //当某个窗口打开  和eventid区分开 主要是想独立新手引导逻辑
    OnWindowClose,      //当某个窗口关闭
}