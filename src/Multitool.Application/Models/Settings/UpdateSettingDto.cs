namespace Multitool.Application.Models.Settings;

/// <summary>
/// Body of PATCH /api/Settings/{module}/{key}. The value is sent as a string and
/// validated/parsed by the backend according to the setting definition.
/// </summary>
public record UpdateSettingDto(string Value);
