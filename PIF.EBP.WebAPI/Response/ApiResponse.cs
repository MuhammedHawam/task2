namespace PIF.EBP.WebAPI.Response
{
    public class ApiResponse
    {
        public int HttpCode { get; set; }
        public string Status { get; set; }
        public object Data { get; set; }
        public object Error { get; set; }

        public ApiResponse(int httpCode, string status, object data, object error)
        {
            HttpCode = httpCode;
            Status = status;
            Data = data;
            Error = error;
        }
    }

    public class ApiResponseError
    {
        public string Code { get; set; }
        public string MessageEn { get; set; }
        public string MessageAr { get; set; }
        
        public ApiResponseError(string messageEn ,string messageAr,string code)
        {
            Code =  code;
            MessageEn = messageEn;
            MessageAr = messageAr;
        }
        public ApiResponseError() { }
    }
}