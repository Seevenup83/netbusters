// Services/ApiResponseService.cs
namespace netbusters.Services
{
    // This service class is used to create a standardized API response structure.
    public class ApiResponseService
    {
        // Indicates if the response represents a successful operation.
        public bool Status { get; set; }

        // A message providing additional information about the response.
        public string Message { get; set; }

        // The data payload of the response.
        public object Data { get; set; }

        // A list of error messages (if any).
        public List<string> Errors { get; set; }

        // Constructor to initialize the ApiResponseService object.
        public ApiResponseService(bool status, string message, object data = null, List<string> errors = null)
        {
            Status = status;
            Message = message;
            Data = data;
            Errors = errors ?? new List<string>();
        }

        // Factory method to create a successful response.
        public static ApiResponseService Success(string message, object data = null)
        {
            return new ApiResponseService(true, message, data);
        }

        // Factory method to create a failure response.
        public static ApiResponseService Failure(string message, List<string> errors = null)
        {
            return new ApiResponseService(false, message, data: null, errors);
        }
    }
}