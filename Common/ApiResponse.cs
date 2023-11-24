namespace netbusters.Common
{
    public class ApiResponse
    {
        public string Message { get; set; }
        public object ResponseData { get; set; }

        public ApiResponse(string message, object responseData)
        {
            Message = message;
            ResponseData = responseData;
        }
    }
}
