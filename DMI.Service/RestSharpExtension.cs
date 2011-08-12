using System;
using System.Threading.Tasks;

namespace RestSharp
{
    public static class RestSharpExtension
    {
        public static Task<T> ExecuteTask<T>(this RestClient client, RestRequest request)
            where T : new()
        {
            var tcs = new TaskCompletionSource<T>(TaskCreationOptions.AttachedToParent);

            client.ExecuteAsync<T>(request, response => 
                {                    
                    if (response.Data != null)
                        tcs.TrySetResult(response.Data);
                    else
                        tcs.TrySetException(response.ErrorException);
                });

            return tcs.Task;
        }
    }
}
