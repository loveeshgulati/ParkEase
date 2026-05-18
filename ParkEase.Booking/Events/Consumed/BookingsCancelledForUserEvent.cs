namespace ParkEase.Booking.Events.Consumed;




public class BookingsCancelledForUserEvent
{
    public Guid SagaCorrelationId { get; set; }
    public int UserId { get; set; }
    public int CancelledCount { get; set; }
}
