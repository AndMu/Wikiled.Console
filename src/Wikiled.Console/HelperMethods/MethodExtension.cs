using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Wikiled.Console.HelperMethods
{
    public static class MethodExtension
    {
        public static void ForgetOrThrow(this Task task, ILogger logger)
        {
            task.ContinueWith(t => { logger.LogError("Error", t.Exception); }, TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}
