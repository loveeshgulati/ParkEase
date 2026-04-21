using MassTransit;
using ParkEase.Auth.Entities;
using ParkEase.Auth.Events;
using ParkEase.Auth.Interfaces;

namespace ParkEase.Auth.Consumers;

public class UserDeactivationRolledBackConsumer : IConsumer<UserDeactivationRolledBackEvent>
{
    private readonly IUserRepository _userRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ILogger<UserDeactivationRolledBackConsumer> _logger;

    public UserDeactivationRolledBackConsumer(
        IUserRepository userRepository,
        IAuditLogRepository auditLogRepository,
        ILogger<UserDeactivationRolledBackConsumer> logger)
    {
        _userRepository = userRepository;
        _auditLogRepository = auditLogRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserDeactivationRolledBackEvent> context)
    {
        var evt = context.Message;
        _logger.LogWarning("Rolling back deactivation for UserId={UserId}. Reason: {Reason}",
            evt.UserId, evt.Reason);

        var user = await _userRepository.FindByUserIdAsync(evt.UserId);
        if (user == null)
        {
            _logger.LogError("Rollback failed: User {UserId} not found", evt.UserId);
            return;
        }

        user.IsActive = true;
        user.Status = "ACTIVE";
        await _userRepository.UpdateAsync(user);

        await _auditLogRepository.CreateAsync(new AuditLog
        {
            ActorUserId = null,
            Action = "DEACTIVATION_ROLLED_BACK",
            TargetUserId = evt.UserId.ToString(),
            After = System.Text.Json.JsonSerializer.Serialize(new { IsActive = true, Status = "ACTIVE" }),
            Timestamp = evt.RolledBackAt,
            FailureReason = evt.Reason
        });

        _logger.LogInformation("User {UserId} reactivated via saga rollback", evt.UserId);
    }
}
