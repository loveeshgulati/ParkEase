using MassTransit;
using ParkEase.ParkingLot.Events.Published;

namespace ParkEase.ParkingLot.Sagas;



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



public class SendLotApprovalNotificationCommand
{
    public Guid CorrelationId { get; set; }
    public int LotId { get; set; }
    public int ManagerId { get; set; }
    public string LotName { get; set; } = string.Empty;
    public bool IsApproved { get; set; }
    public string? RejectionReason { get; set; }
}



public class LotApprovalNotificationSentEvent
{
    public Guid SagaCorrelationId { get; set; }
    public int LotId { get; set; }
}













public class LotApprovalSaga : MassTransitStateMachine<LotApprovalSagaState>
{
    public State Notifying { get; private set; } = null!;
    public State Completed { get; private set; } = null!;

    public Event<LotApprovedEvent> LotApproved { get; private set; } = null!;
    public Event<LotRejectedEvent> LotRejected { get; private set; } = null!;
    public Event<LotApprovalNotificationSentEvent> NotificationSent { get; private set; } = null!;

    public LotApprovalSaga()
    {
        InstanceState(x => x.CurrentState);

        Event(() => LotApproved,
            x => x.CorrelateById(ctx => NewId.NextGuid()));

        Event(() => LotRejected,
            x => x.CorrelateById(ctx => NewId.NextGuid()));

        Event(() => NotificationSent,
            x => x.CorrelateById(ctx => ctx.Message.SagaCorrelationId));

        
        Initially(
            When(LotApproved)
                .Then(ctx =>
                {
                    ctx.Saga.LotId = ctx.Message.LotId;
                    ctx.Saga.ManagerId = ctx.Message.ManagerId;
                    ctx.Saga.LotName = ctx.Message.LotName;
                    ctx.Saga.AdminId = ctx.Message.ApprovedByAdminId;
                    ctx.Saga.InitiatedAt = ctx.Message.ApprovedAt;
                })
                .PublishAsync(ctx => ctx.Init<SendLotApprovalNotificationCommand>(new
                {
                    ctx.Saga.CorrelationId,
                    ctx.Saga.LotId,
                    ctx.Saga.ManagerId,
                    ctx.Saga.LotName,
                    IsApproved = true,
                    RejectionReason = (string?)null
                }))
                .TransitionTo(Notifying),

            
            When(LotRejected)
                .Then(ctx =>
                {
                    ctx.Saga.LotId = ctx.Message.LotId;
                    ctx.Saga.ManagerId = ctx.Message.ManagerId;
                    ctx.Saga.LotName = ctx.Message.LotName;
                    ctx.Saga.InitiatedAt = ctx.Message.RejectedAt;
                })
                .PublishAsync(ctx => ctx.Init<SendLotApprovalNotificationCommand>(new
                {
                    ctx.Saga.CorrelationId,
                    ctx.Saga.LotId,
                    ctx.Saga.ManagerId,
                    ctx.Saga.LotName,
                    IsApproved = false,
                    ctx.Message.Reason
                }))
                .TransitionTo(Notifying)
        );

        During(Notifying,
            When(NotificationSent)
                .TransitionTo(Completed)
        );

        SetCompletedWhenFinalized();
    }
}
