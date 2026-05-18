namespace ParkEase.Booking.Events.Consumed;






public class CancelBookingsForUserCommand
{
    public Guid CorrelationId { get; set; }
    public int UserId { get; set; }
}
