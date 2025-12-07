using PIF.EBP.Core.FileManagement;
using PIF.EBP.Core.FileManagement.DTOs;
using System;
using System.Threading.Tasks;
using static PIF.EBP.Application.Shared.Enums;

namespace PIF.EBP.Application.AttendeeEvent.Implementation
{
    public class AttendeeEventAppService: IAttendeeEventAppService
    {
        private readonly IFileManagement _fileService;

        public AttendeeEventAppService(IFileManagement fileService)
        {
            _fileService = fileService;
        }

        public async Task<AttendeeEventDetailsDto> RetrieveAttendeesDetailsByRefId(string refId)
        {
            AttendeeEventDetailsDto attendeeEventDetailsDto = _fileService.RetrieveAttendeesDetailsByRefId(refId, "Seating");
            return attendeeEventDetailsDto;
        }
        public async Task<bool> UpdateAttendeesDetails(string refId, int newRSVPValue, string guestName, string guestRole)
        {
            string newRSVPText = Enum.GetName(typeof(RSVP), newRSVPValue);
            return _fileService.UpdateAttendeesDetails("Seating", refId, newRSVPText, guestName, guestRole);
        }
    }
}
