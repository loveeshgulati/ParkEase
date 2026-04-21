using MassTransit;
using ParkEase.ParkingLot.Events;

namespace ParkEase.ParkingLot.Sagas;

// Saga State Machine

/// <summary>
/// LotApprovalSaga
///
/// Flow:
///   Step 1: admin approves/rejects lot  (local — in ParkingLotService)
///   Step 2: notification-service notifies manager (cross-service)
///
/// Simple 2-step saga — no complex compensation needed here.
/// If notification fails, lot stays approved; manager just doesn't get notified.
/// </summary>
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

        // ── Lot Approved ──────────────────────────────────────────────────────
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

            // ── Lot Rejected ──────────────────────────────────────────────────
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
