using MassTransit;
using ParkEase.Auth.Events;
using ParkEase.Auth.Interfaces;

namespace ParkEase.Auth.Consumers;

public class UserDeactivationRolledBackConsumer : IConsumer<UserDeactivationRolledBackEvent>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserDeactivationRolledBackConsumer> _logger;

    public UserDeactivationRolledBackConsumer(
        IUserRepository userRepository,
        ILogger<UserDeactivationRolledBackConsumer> logger)
    {
        _userRepository = userRepository;
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

        _logger.LogInformation("User {UserId} reactivated via saga rollback", evt.UserId);
    }
}
