using ParkEase.Booking.Interfaces;

namespace ParkEase.Booking.BackgroundServices;








public class ExpiredBookingBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ExpiredBookingBackgroundService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);

    public ExpiredBookingBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<ExpiredBookingBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "ExpiredBookingBackgroundService started. Runs every {Interval} minutes.",
            _interval.TotalMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessExpiredBookingsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error in ExpiredBookingBackgroundService");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task ProcessExpiredBookingsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var bookingService = scope.ServiceProvider
            .GetRequiredService<IBookingService>();

        await bookingService.AutoCancelExpiredBookingsAsync();
    }
}
