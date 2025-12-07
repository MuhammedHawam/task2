using PIF.EBP.Application.Feedback.DTOs;
using PIF.EBP.Core.DependencyInjection;
using System.Threading.Tasks;

namespace PIF.EBP.Application.Feedback
{
    public interface IFeedbackAppService : ITransientDependency
    {
        Task<string> CreateFeedback(FeedbackDto feedbackDto);
    }
}
