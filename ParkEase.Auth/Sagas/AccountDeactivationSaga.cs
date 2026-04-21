using MassTransit;
using ParkEase.Auth.Events;

namespace ParkEase.Auth.Sagas;

// ─── Saga State ───────────────────────────────────────────────────────────────
public class AccountDeactivationSagaState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string SagaCurrentState { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateTime InitiatedAt { get; set; }
    public string? FailureReason { get; set; }
}

// ─── Saga Commands ────────────────────────────────────────────────────────────
public class CancelBookingsForUserCommand
{
    public Guid CorrelationId { get; set; }
    public int UserId { get; set; }
}

public class SendDeactivationNotificationCommand
{
    public Guid CorrelationId { get; set; }
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
}

// ─── Saga Response Events (from other services) ───────────────────────────────
public class BookingsCancelledForUserEvent
{
    public Guid SagaCorrelationId { get; set; }
    public int UserId { get; set; }
    public int CancelledCount { get; set; }
}

public class BookingCancellationFailedEvent
{
    public Guid SagaCorrelationId { get; set; }
    public int UserId { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class DeactivationNotificationSentEvent
{
    public Guid SagaCorrelationId { get; set; }
    public int UserId { get; set; }
}

// ─── Saga State Machine ───────────────────────────────────────────────────────

/// <summary>
/// AccountDeactivationSaga
///
/// Flow:
///   Step 1: auth-service marks user inactive        (local)
///   Step 2: booking-service cancels active bookings (cross-service)
///   Step 3: notification-service notifies user      (cross-service)
///
/// Compensation:
///   If Step 2 fails → reactivate user (rollback Step 1)
/// </summary>
public class AccountDeactivationSaga : MassTransitStateMachine<AccountDeactivationSagaState>
{
    public State Deactivating { get; private set; } = null!;
    public State BookingsCancelled { get; private set; } = null!;
    public State Completed { get; private set; } = null!;
    public State Failed { get; private set; } = null!;

    public Event<UserDeactivatedEvent> UserDeactivated { get; private set; } = null!;
    public Event<BookingsCancelledForUserEvent> BookingsCancelledForUser { get; private set; } = null!;
    public Event<BookingCancellationFailedEvent> BookingCancellationFailed { get; private set; } = null!;
    public Event<DeactivationNotificationSentEvent> NotificationSent { get; private set; } = null!;

    public AccountDeactivationSaga()
    {
        InstanceState(x => x.SagaCurrentState);

        Event(() => UserDeactivated,
            x => x.CorrelateById(ctx => NewId.NextGuid()));

        Event(() => BookingsCancelledForUser,
            x => x.CorrelateById(ctx => ctx.Message.SagaCorrelationId));

        Event(() => BookingCancellationFailed,
            x => x.CorrelateById(ctx => ctx.Message.SagaCorrelationId));

        Event(() => NotificationSent,
            x => x.CorrelateById(ctx => ctx.Message.SagaCorrelationId));

        Initially(
            When(UserDeactivated)
                .Then(ctx =>
                {
                    ctx.Saga.UserId = ctx.Message.UserId;
                    ctx.Saga.Email = ctx.Message.Email;
                    ctx.Saga.InitiatedAt = ctx.Message.DeactivatedAt;
                })
                .PublishAsync(ctx => ctx.Init<CancelBookingsForUserCommand>(new
                {
                    ctx.Saga.CorrelationId,
                    ctx.Message.UserId
                }))
                .TransitionTo(Deactivating)
        );

        During(Deactivating,
            When(BookingsCancelledForUser)
                .PublishAsync(ctx => ctx.Init<SendDeactivationNotificationCommand>(new
                {
                    ctx.Saga.CorrelationId,
                    ctx.Saga.UserId,
                    ctx.Saga.Email
                }))
                .TransitionTo(BookingsCancelled),

            When(BookingCancellationFailed)
                .Then(ctx => ctx.Saga.FailureReason = ctx.Message.Reason)
                .PublishAsync(ctx => ctx.Init<UserDeactivationRolledBackEvent>(new
                {
                    ctx.Saga.UserId,
                    ctx.Message.Reason,
                    RolledBackAt = DateTime.UtcNow
                }))
                .TransitionTo(Failed)
        );

        During(BookingsCancelled,
            When(NotificationSent)
                .TransitionTo(Completed)
        );

        SetCompletedWhenFinalized();
    }
}
