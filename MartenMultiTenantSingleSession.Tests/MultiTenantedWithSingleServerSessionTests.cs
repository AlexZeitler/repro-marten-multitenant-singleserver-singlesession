using System;
using System.Threading.Tasks;
using Marten;
using Marten.Events.Projections;
using Npgsql;
using Weasel.Core;

namespace MartenMultiTenantSingleSession.Tests;

public class SessionTests
{
  private static string GetConnectionString()
  {
    var connectionString = new NpgsqlConnectionStringBuilder()
    {
      Pooling = false,
      Port = 5435,
      Host = "localhost",
      CommandTimeout = 20,
      Database = "postgres",
      Password = "123456",
      Username = "postgres"
    }.ToString();
    return connectionString;
  }


  private static DocumentStore GetDocumentStore()
  {
    return DocumentStore.For(options =>
    {
      options
        .MultiTenantedWithSingleServer(GetConnectionString());
      options.AutoCreateSchemaObjects = AutoCreate.All;
      options.Projections.SelfAggregate<Thread>(ProjectionLifecycle.Inline);
      options.UseDefaultSerialization(
        EnumStorage.AsString,
        nonPublicMembersStorage: NonPublicMembersStorage.All
      );
    });
  }

  [Fact]
  public async Task SeparateSessionsShouldCreateThreadsInBothTenants()
  {
    var store = GetDocumentStore();

    var topic = Guid.NewGuid().ToString();
    var @event = new ThreadStarted("ten1", "ten2", topic, DateTimeOffset.Now, "Jane Doe", "Hello World!");

    await using var writeSession1 = store.LightweightSession("ten1");
    await using var writeSession2 = store.LightweightSession("ten2");
    var streamId1 = Guid.NewGuid();
    var streamId2 = Guid.NewGuid();
    writeSession1.Events.StartStream(streamId1, @event);
    writeSession2.Events.StartStream(streamId2, @event);
    await writeSession1.SaveChangesAsync();
    await writeSession2.SaveChangesAsync();

    await using var readSessionTenant1 = store.LightweightSession("ten1");
    await using var readSessionTenant2 = store.LightweightSession("ten2");

    var thread1 = readSessionTenant1.Load<Thread>(streamId1);
    var thread2 = readSessionTenant2.Load<Thread>(streamId2);

    Assert.NotNull(thread1);
    Assert.NotNull(thread2);
  }


  [Fact]
  public async Task SingleSessionWithFirstTenantShouldCreateThreadsInBothTenants()
  {
    var store = GetDocumentStore();

    var topic = Guid.NewGuid().ToString();
    var @event = new ThreadStarted("ten1", "ten2", topic, DateTimeOffset.Now, "Jane Doe", "Hello World!");

    await using var session = store.LightweightSession("ten1");
    var streamId1 = Guid.NewGuid();
    var streamId2 = Guid.NewGuid();
    session.ForTenant("ten1").Events.StartStream(streamId1, @event);
    session.ForTenant("ten2").Events.StartStream(streamId2, @event);
    await session.SaveChangesAsync();

    await using var readSessionTenant1 = store.LightweightSession("ten1");
    await using var readSessionTenant2 = store.LightweightSession("ten2");

    var thread1 = readSessionTenant1.Load<Thread>(streamId1);
    var thread2 = readSessionTenant2.Load<Thread>(streamId2);

    Assert.NotNull(thread1);
    Assert.NotNull(thread2);
  }

  [Fact]
  public async Task SessionWithFirstRandomTenantIdShouldCreateThreadsInBothTenants()
  {
    var store = GetDocumentStore();

    var topic = Guid.NewGuid().ToString();
    var @event = new ThreadStarted("ten1", "ten2", topic, DateTimeOffset.Now, "Jane Doe", "Hello World!");

    await using var session = store.LightweightSession("some-tenant");
    var streamId1 = Guid.NewGuid();
    var streamId2 = Guid.NewGuid();
    session.ForTenant("ten1").Events.StartStream(streamId1, @event);
    session.ForTenant("ten2").Events.StartStream(streamId2, @event);
    await session.SaveChangesAsync();

    await using var readSessionTenant1 = store.LightweightSession("ten1");
    await using var readSessionTenant2 = store.LightweightSession("ten2");

    var thread1 = readSessionTenant1.Load<Thread>(streamId1);
    var thread2 = readSessionTenant2.Load<Thread>(streamId1);

    Assert.NotNull(thread1);
    Assert.NotNull(thread2);
  }
}
