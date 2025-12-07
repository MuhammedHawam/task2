using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.Core.FileManagement.DTOs;
using System.Threading.Tasks;

namespace PIF.EBP.Application.AttendeeEvent
{
    public interface IAttendeeEventAppService : ITransientDependency
    {
        Task<AttendeeEventDetailsDto> RetrieveAttendeesDetailsByRefId(string refId);
        Task<bool> UpdateAttendeesDetails(string refId, int RSVP, string guestName, string guestRole);
    }
}
