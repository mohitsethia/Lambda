namespace SendEmailLambda.DTO;

public class TemplateEmailDTO
{
    public string sendFrom { get; set; }
    public string sendTo { get; set; }
    public string templateName { get; set; }
    public string templateData { get; set; }
}