using CraftedSolutions.MarBasSchema.Broker;
using CraftedSolutions.MarBasSchema.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CraftedSolutions.MarBasAPICore.Http
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
            return DigestExceptionsAsync(() => { return Task.FromResult(worker()); }, logger).Result;
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
            catch (StorageQuotaExceededException e)
            {
                logger?.LogWarning(e, "Storage error: {error}", e.Message);
                throw new HttpResponseException(StatusCodes.Status413PayloadTooLarge, e.Message);
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
                throw new HttpResponseException(StatusCodes.Status500InternalServerError, new ProblemDetails()
                {
                    Title = e.GetType().Name,
                    Detail = e.Message
                });
            }
        }
    }
}
