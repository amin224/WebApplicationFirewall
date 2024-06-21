namespace WebApplicationFirewallUE.Models;

public class LogTraceOperation
{
    public LogTraceOperation(bool attack, string type)
    {
        Attack = attack;
        Type = type;
    }
    
    public LogTraceOperation()
    {
    }
    
    public bool Attack { get; set; }
    public string Type { get; set; }
}