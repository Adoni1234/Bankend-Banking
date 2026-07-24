using ArtemisBanking.Core.Application.Enums;

namespace ArtemisBanking.Core.Application.Dtos.Email;

public class EmailTemplateDataDto
{
    public EmailType Type { get; set; }
    public string To { get; set; }
    public Dictionary<string, string> Variables { get; set; } = new();
}
