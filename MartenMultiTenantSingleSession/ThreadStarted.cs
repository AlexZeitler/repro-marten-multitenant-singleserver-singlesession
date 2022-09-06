namespace MartenMultiTenantSingleSession;

public record ThreadStarted(string SenderSubscriptionId, string ReceiverSubscriptionId, string Topic, DateTimeOffset On, string By, string Message);
