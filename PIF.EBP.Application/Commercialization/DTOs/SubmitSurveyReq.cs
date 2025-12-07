using System.Collections.Generic;

namespace PIF.EBP.Application.Commercialization.DTOs
{
    public class SubmitSurveyReq
    {
        public string RequestId { get; set; }
        public List<SurveyResult> SurveyResults { get; set; }
    }
    public class SurveyResult
    {
        public string Question { get; set; }
        public string Answer { get; set; }
    }
}
