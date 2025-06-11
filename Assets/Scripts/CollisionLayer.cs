public enum CollisionLayer
{
    //lovely magic numbers
    
    //it is found by representing each bit as a physics layer, and converting
    //the binary into an int. 1 means collision, 0 means no collision
    Default = 1,
    Team1Enemies = 1792,
    Team2Enemies = 1664,
    Team3Enemies = 1408,
    Team4Enemies = 896,
    AllEnemies = 1920
}

public static class GetCollisionLayer
{
    public static uint Please(Team team)
    {
        switch (team)
        {
            case Team.TEAM_1:
                return (uint)CollisionLayer.Team1Enemies;
            case Team.TEAM_2:
                return (uint)CollisionLayer.Team2Enemies;
            case Team.TEAM_3:
                return (uint)CollisionLayer.Team3Enemies;
            case Team.TEAM_4:
                return (uint)CollisionLayer.Team4Enemies;
                        
        }

        //bad things if this happens
        return 0;
    }
}
