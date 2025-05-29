using System.ComponentModel.DataAnnotations;

namespace Common.Core.Base;

public class Report : BaseEntity
{
    [Required][MaxLength(10)] public string Code { get; set; } //Code of report

    [Required][MaxLength(250)] public string FileName { get; set; } //Name of report file

    public string Query { get; set; } //Query or Sp

    [Required][MaxLength(250)] public string ReportPath { get; set; } //Directory or Name Rdl
}
