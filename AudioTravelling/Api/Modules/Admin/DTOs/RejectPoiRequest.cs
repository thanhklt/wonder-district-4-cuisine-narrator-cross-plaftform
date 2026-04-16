using System.ComponentModel.DataAnnotations;

namespace Api.Modules.Admin.DTOs;

public class RejectPoiRequest
{
    [Required]
    public string Note { get; set; } = string.Empty;
}