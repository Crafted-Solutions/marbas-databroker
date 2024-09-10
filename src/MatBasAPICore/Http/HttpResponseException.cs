using MarBasSchema.Broker;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MarBasAPICore.Http
{
    public sealed class HttpResponseException : Exception
    {
        public HttpResponseException(int statusCode, object? value = null) =>
            (StatusCode, Value) = (statusCode, value);

        public HttpResponseException(UnauthorizedAccessException e) =>
            (StatusCode, Value) = (StatusCodes.Status403Forbidden, e.Message);

        public int StatusCode { get; }

        public object? Value { get; }

        public static void Throw503IfOffline(IProfileProvider? profileProvider)
        {
            if (null == profileProvider || !profileProvider.Profile.IsOnline)
            {
                throw new HttpResponseException(StatusCodes.Status503ServiceUnavailable);
            }
        }

        public static T DigestExceptions<T>(Func<T> worker, ILogger? logger = null)
        {
            return DigestExceptionsAsync<T>(() => { return Task.FromResult(worker()); }, logger).Result;
        }

        public static async Task<T> DigestExceptionsAsync<T>(Func<Task<T>> worker, ILogger? logger = null)
        {
            try
            {
                return await worker();
            }
            catch (UnauthorizedAccessException e)
            {
                throw new HttpResponseException(e);
            }
            catch (ArgumentException e)
            {
                logger?.LogWarning(e, "Argument validation failed");
                throw new HttpResponseException(StatusCodes.Status400BadRequest, e.Message);
            }
            catch (HttpResponseException)
            {
                throw;
            }
            catch (Exception e)
            {
                logger?.LogError(e, "Unexpected error occured");
                throw new HttpResponseException(StatusCodes.Status500InternalServerError, e.GetType().Name);
            }
        }
    }
}
