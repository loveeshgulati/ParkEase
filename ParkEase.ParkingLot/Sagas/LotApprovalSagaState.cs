using MassTransit;

namespace ParkEase.ParkingLot.Sagas;

// Saga State

public class LotApprovalSagaState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = string.Empty;
    public int LotId { get; set; }
    public int ManagerId { get; set; }
    public string LotName { get; set; } = string.Empty;
    public int AdminId { get; set; }
    public DateTime InitiatedAt { get; set; }
}
