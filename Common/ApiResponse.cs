namespace netbusters.Common
{
    public class ApiResponse
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
        public List<string> Errors { get; set; }

        public ApiResponse(bool status, string message, object data = null, List<string> errors = null)
        {
            Status = status;
            Message = message;
            Data = data;
            Errors = errors ?? new List<string>();
        }

        // Factory methods for success and error responses
        public static ApiResponse Success(string message, object data = null)
        {
            return new ApiResponse(true, message, data);
        }

        public static ApiResponse Failure(string message, List<string> errors = null)
        {
            return new ApiResponse(false, message, data: null, errors);
        }
    }
}
