namespace TicketingSystem.SharedKernel.DTOs
{
    public class SendEmailResponse
    {
        public bool IsSuccess { get; set; }
        public string MessageId { get; set; }
        public string Response { get; set; }
    }
}
