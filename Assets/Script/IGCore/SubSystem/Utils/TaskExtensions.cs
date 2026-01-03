using System.Threading.Tasks;

public static class TaskExtensions
{
    /// <summary>
    /// No await and prevent warning CS4014.
    /// </summary>
    public static void Forget(this Task task)
    {
    }
}