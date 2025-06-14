using Unity.Entities;

public struct AgentDecrementer : IComponentData
{
    public static AgentDecrementer Decrement()
    {
        UI.agentsKilled++;
        
        return new AgentDecrementer();
    }
}