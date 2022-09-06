namespace MartenMultiTenantSingleSession;

public record Thread
{
  private List<ThreadMessage> messages;
  public Guid Id { get; init; }
  public DateTimeOffset StartedOn { get; }
  public string StartedBy { get; }
  public string Topic { get; }

  public IReadOnlyCollection<ThreadMessage> Messages
  {
    get { return messages.AsReadOnly(); }
    private set => messages = value.ToList();
  }

  private Thread()
  {
  }

  private Thread(DateTimeOffset startedOn, string startedBy, string firstMessage, string topic)
  {
    StartedOn = startedOn;
    StartedBy = startedBy;
    Topic = topic;
    messages = new List<ThreadMessage> { new(startedOn, startedBy, firstMessage) };
  }

  public static Thread Create(ThreadStarted started)
  {
    return new Thread(started.On, started.By, started.Message, started.Topic);
  }
}

public record ThreadMessage(DateTimeOffset On, string By, string Text);
