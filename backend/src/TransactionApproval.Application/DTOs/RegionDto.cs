namespace TransactionApproval.Application.DTOs;

/// <summary>A supported region as exposed to clients.</summary>
public record RegionDto(string Code, string Name, string TimeZoneId);
