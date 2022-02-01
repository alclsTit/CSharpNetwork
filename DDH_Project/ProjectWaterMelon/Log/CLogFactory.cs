
namespace ProjectWaterMelon.Log
{ 
    /// <summary>
    /// Factory Method Design Pattern
    /// </summary>
    public interface ILogFactory
    {
        CLogger GetLogger();
    }
}
