namespace ParkEase.Booking.Events.Consumed;




public class BookingCancellationFailedEvent
{
    public Guid SagaCorrelationId { get; set; }
    public int UserId { get; set; }
    public string Reason { get; set; } = string.Empty;
}
