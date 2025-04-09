using System;
using UnityEngine;

public enum Map
{
   Default
}

public enum GameMode
{

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
   public GameInfo userGamePreferences = new GameInfo();

}

[Serializable]
public class GameInfo
{
   public Map map;
   public GameMode gameMode;
   public GameQueue gameQueue;
}
