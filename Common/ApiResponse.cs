namespace netbusters.Utilities
{
    public class ApiResponse
    {
        public int Code { get; set; }
        public string Message { get; set; }

        public ApiResponse(int code, string message)
        {
            Code = code;
            Message = message;
        }
    }
}
