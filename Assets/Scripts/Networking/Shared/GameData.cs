using System;
using UnityEngine;

public enum Map
{
   Default
}

public enum GameMode
{

}

public enum Team
{
    Deathmatch = -1,
    TeamA = 0,
    TeamB = 1
}

public enum GameQueue
{
   Solo,
   Team
}

[Serializable]
public class UserData 
{
   public string userName;
   public string userAuthId;
   public int teamId;
   
}



[Serializable]
public class GameInfo
{
   public Map map;
   public GameMode gameMode;
   public GameQueue gameQueue;
}
