using System.ComponentModel.DataAnnotations;

namespace DiscordBot.Features.DailyStats.Job;

public record DailyStatsJobConfig
{
    [Required]
    public required string Cron { get; init; }
};
