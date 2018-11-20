namespace Sitecore.Support.EmailCampaign.Cm.UI.Handlers
{
  using System.Threading.Tasks;
  using Sitecore.EmailCampaign.Cm;
  using Sitecore.EmailCampaign.Model.Messaging;
  using Sitecore.EmailCampaign.Model.Messaging.Buses;
  using Sitecore.Framework.Conditions;
  using Sitecore.Framework.Messaging;
  using Sitecore.Framework.Messaging.DeferStrategies;
  using Sitecore.Globalization;
  using Sitecore.Modules.EmailCampaign.Core.Contacts;
  using static System.FormattableString;
  using ILogger = ExM.Framework.Diagnostics.ILogger;
  using SupportUnsubscribeMessage = Model.Messaging.UnsubscribeMessage;

  public class UnsubscribeMessageHandler : IMessageHandler<UnsubscribeMessage>
  {
    private readonly ILogger _logger;
    private readonly ISubscriptionManager _subscriptionManager;
    private readonly IDeferStrategy<DeferDetectionByResultBase<HandlerResult>> _deferStrategy;
    private readonly IMessageBus<UnsubscribeMessagesBus> _bus;

    public UnsubscribeMessageHandler([NotNull] ILogger logger, [NotNull] ISubscriptionManager subscriptionManager, IDeferStrategy<DeferDetectionByResultBase<HandlerResult>> deferStrategy, IMessageBus<UnsubscribeMessagesBus> bus)
    {
      Condition.Requires(logger, nameof(logger)).IsNotNull();
      Condition.Requires(subscriptionManager, nameof(subscriptionManager)).IsNotNull();
      Condition.Requires(deferStrategy, nameof(deferStrategy)).IsNotNull();
      Condition.Requires(bus, nameof(bus)).IsNotNull();

      _logger = logger;
      _subscriptionManager = subscriptionManager;
      _deferStrategy = deferStrategy;
      _bus = bus;
    }

    public async Task Handle([NotNull] UnsubscribeMessage message, IMessageReceiveContext receiveContext, IMessageReplyContext replyContext)
    {
      Condition.Requires(message, nameof(message)).IsNotNull();
      Condition.Requires(receiveContext, nameof(receiveContext)).IsNotNull();
      Condition.Requires(replyContext, nameof(replyContext)).IsNotNull();

      DeferResult<HandlerResult> result = await _deferStrategy.ExecuteAsync(
          _bus,
          message,
          receiveContext,
          () => Unsubscribe(message)).ConfigureAwait(false);

      if (result.Deferred)
      {
        _logger.LogDebug($"[{nameof(UnsubscribeMessageHandler)}] defered message.");
      }
      else
      {
        _logger.LogDebug($"[{nameof(UnsubscribeMessageHandler)}] processed message.'");
      }
    }

    protected HandlerResult Unsubscribe(UnsubscribeMessage message)
    {
      _logger.LogDebug(Invariant($"[{nameof(UnsubscribeMessageHandler)}] Unsubscribing '{message.ContactIdentifier.ToLogFile()}' from '{message.MessageId}'"));
      bool unsubscribed = false;
      Language language;

      if(this.ParseModel(message, out language))
      {
        using (new LanguageSwitcher(language))
        {
          unsubscribed = _subscriptionManager.Unsubscribe(message.ContactIdentifier, message.MessageId, message.AddToGlobalOptOutList);
        }
      }
      else
      {
        unsubscribed = _subscriptionManager.Unsubscribe(message.ContactIdentifier, message.MessageId, message.AddToGlobalOptOutList);
      }

      if (unsubscribed)
      {
        return new HandlerResult(HandlerResultType.Successful);
      }

      _logger.LogError(Invariant($"[{nameof(UnsubscribeMessageHandler)}] Failed to unsubscribe '{message.ContactIdentifier.ToLogFile()}' from '{message.MessageId}'"));
      return new HandlerResult(HandlerResultType.Error);
    }

    private bool ParseModel(UnsubscribeMessage message, out Language language)
    {
      bool result = false;
      language = null;
      SupportUnsubscribeMessage supportMessage = message as SupportUnsubscribeMessage;

      if (supportMessage == null)
      {
        return false;
      }

      if(!string.IsNullOrEmpty(supportMessage.MessageLanguage))
      {
        result = Language.TryParse(supportMessage.MessageLanguage, out language);
      }

      return result;
    }
  }
}