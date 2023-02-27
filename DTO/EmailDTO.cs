namespace SendEmailLambda.DTO;

public class EmailDTO
{
    public string sendFrom { get; set; }

    public string emailSubject { get; set; }

    public string sendTo { get; set; }

    public string htmlBody { get; set; }
    
    public string textBody { get; set; }
}