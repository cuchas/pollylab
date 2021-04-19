using System;
using System.ComponentModel.DataAnnotations;

public class Activation {
    [Required(ErrorMessage = "Informe o state")]
    public string State { get; set; }

    [Required(ErrorMessage = "Informe o Status")]
    public string Status { get; set; }

    [Required]
    [Range(0, 99999)]
    public int Id { get; set; }

    [Required]
    public DateTime Date { get; set; }
}