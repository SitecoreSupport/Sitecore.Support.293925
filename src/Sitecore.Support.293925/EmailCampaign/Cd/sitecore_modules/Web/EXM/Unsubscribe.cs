namespace Sitecore.Support.EmailCampaign.Cd.sitecore_modules.Web.EXM
{
  using Sitecore.Modules.EmailCampaign.Core;
  using Sitecore.XConnect;
  using System;
  using UnsubscribeOrigin = Sitecore.EmailCampaign.Cd.sitecore_modules.Web.EXM.Unsubscribe;
  using SupportUnsubscribeMessage = Model.Messaging.UnsubscribeMessage;
  using Sitecore.Data.Items;
  using Sitecore.Data;
  using Sitecore.Web;
  using Sitecore.Sites;
  using Sitecore.Links;

  public class Unsubscribe : UnsubscribeOrigin
  {
    protected override string UnsubscribeContact(ContactIdentifier contactIdentifier, Guid messageID)
    {
      SupportUnsubscribeMessage unsubscribeMessage = new SupportUnsubscribeMessage
      {
        AddToGlobalOptOutList = false,
        ContactIdentifier = contactIdentifier,
        MessageId = messageID,
        MessageLanguage = LanguageName
      };
      base.ClientApiService.Unsubscribe(unsubscribeMessage);
      return (ExmContext.Message.ManagerRoot.GetConfirmativePageUrl() ?? "/");

    }

    protected override string VerifyContactSubscriptions(ContactIdentifier contactIdentifier, Guid messageID)
    {
      base.VerifyContactSubscriptions(contactIdentifier, messageID);

      #region Sitecore.Support.224113

      string alreadyItemPath = ExmContext.Message.ManagerRoot.Settings.AlreadyUnsubscribedPage;
      Item alreadyItem = Database.GetDatabase("web").GetItem(alreadyItemPath);
      string itemUrl = String.Empty;
      SiteInfo site = null;
      var siteInfoList = Configuration.Factory.GetSiteInfoList();

      foreach (SiteInfo siteInf in siteInfoList)
      {
        if (siteInf.RootPath.ToLowerInvariant().Trim() != "" && siteInf.StartItem.ToLowerInvariant().Trim() != "" && alreadyItem.Paths.FullPath.ToLowerInvariant().Trim().Contains(siteInf.RootPath.ToLowerInvariant().Trim() + siteInf.StartItem.ToLowerInvariant().Trim()))
        {
          site = siteInf;
        }
      }

      using (new SiteContextSwitcher(SiteContextFactory.GetSiteContext(site.Name)))
      {
        itemUrl = LinkManager.GetItemUrl(alreadyItem);
      }
      return itemUrl;

      #endregion
    }

    protected override string LanguageName
    {
      get
      {
        return string.IsNullOrEmpty(ExmContext.Message.TargetLanguage.Name) ? base.LanguageName : ExmContext.Message.TargetLanguage.Name;
      }
    }
  }
}