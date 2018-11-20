namespace Sitecore.Support.EmailCampaign.Model.Messaging
{
  using UnsubscribeMessageOrigin = Sitecore.EmailCampaign.Model.Messaging.UnsubscribeMessage;

  public class UnsubscribeMessage : UnsubscribeMessageOrigin
  {
    public string MessageLanguage { get; set; }
  }
}