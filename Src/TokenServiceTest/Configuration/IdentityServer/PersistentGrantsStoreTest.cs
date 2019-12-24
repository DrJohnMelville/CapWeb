using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using TokenService.Configuration.IdentityServer;
using TokenServiceTest.TestTools;
using Xunit;

namespace TokenServiceTest.Configuration.IdentityServer
{
  public sealed class PersistentGrantsStoreTest
  {
    private readonly TestDatabase testDb = new TestDatabase();
    private readonly PersistentGrantsStore sut1;
    private readonly PersistentGrantsStore sut2;

    public PersistentGrantsStoreTest()
    {
      sut1 = new PersistentGrantsStore(testDb.NewContext()); 
      sut2 = new PersistentGrantsStore(testDb.NewContext()); 
    }

    private PersistedGrant DefaultGrant() =>
      new PersistedGrant
      {
        Key = "k1",
        ClientId = "cid",
        CreationTime = new DateTime(1975, 07, 28),
        Data = "data",
        Expiration = new DateTime(1975, 07, 29),
        SubjectId = "sid",
        Type = "type"
      };

    [Fact]
    public async Task GetAsyncReturnsNukllOnEmptyColl()
    {
      Assert.Null(await sut1.GetAsync("k1"));
    }

    [Fact]
    public async Task WriteAndReadGrant()
    {
      var sourceGrant = DefaultGrant();
      await sut1.StoreAsync(sourceGrant);
      var destGrant = await sut2.GetAsync("k1");
      AssertIdenticalGrant(sourceGrant, destGrant);
    }

    private static void AssertIdenticalGrant(PersistedGrant sourceGrant, PersistedGrant destGrant)
    {
      Assert.Equal(sourceGrant.Key, destGrant.Key);
      Assert.Equal(sourceGrant.Data, destGrant.Data);
      Assert.Equal(sourceGrant.ClientId, destGrant.ClientId);
      Assert.Equal(sourceGrant.CreationTime, destGrant.CreationTime);
      Assert.Equal(sourceGrant.Expiration, destGrant.Expiration);
      Assert.Equal(sourceGrant.SubjectId, destGrant.SubjectId);
      Assert.Equal(sourceGrant.Type, destGrant.Type);
    }

    [Fact]
    public async Task UpdateGrant()
    {
      var sourceGrant = DefaultGrant();
      await sut1.StoreAsync(sourceGrant);
      sourceGrant.Data = "Date2";
      await sut1.StoreAsync(sourceGrant);
      var destGrant = await sut2.GetAsync("k1");
      Assert.Equal(sourceGrant.Key, destGrant.Key);
      Assert.Equal(sourceGrant.Data, destGrant.Data);
    }

    [Fact]
    public async Task GetAllSingleTest()
    {
      var sourceGrant = DefaultGrant();
      await sut1.StoreAsync(sourceGrant);
      var destGrant = (await sut2.GetAllAsync(sourceGrant.SubjectId)).Single();
      AssertIdenticalGrant(sourceGrant, destGrant);
    }
    [Fact]
    public async Task GetAllDoubleTest()
    {
      var sourceGrant = DefaultGrant();
      await sut1.StoreAsync(sourceGrant);
      sourceGrant.Key = "k2";
      await sut1.StoreAsync(sourceGrant);
      var destGrant = (await sut2.GetAllAsync(sourceGrant.SubjectId));
      Assert.Equal(2, destGrant.Count());
    }
    [Theory]
    [InlineData("k1", 1)]
    [InlineData("k2", 1)]
    [InlineData("k3", 2)]
    public async Task RemoveAsync(string delKey, int remaining)
    {
      var sourceGrant = DefaultGrant();
      await sut1.StoreAsync(sourceGrant);
      sourceGrant.Key = "k2";
      await sut1.StoreAsync(sourceGrant);
      Assert.Equal(2, (await sut2.GetAllAsync(sourceGrant.SubjectId)).Count());
      await sut1.RemoveAsync(delKey);
      Assert.Equal(remaining, (await sut2.GetAllAsync(sourceGrant.SubjectId)).Count());
    }
    [Theory]
    [InlineData("cid", "sid", 1)]
    [InlineData("cid2", "sid", 2)]
    public async Task RemoveAllAsync(string cDelete, string sDelete, int remaining)
    {
      var sourceGrant = DefaultGrant();
      await sut1.StoreAsync(sourceGrant);
      sourceGrant.Key = "k2";
      await sut1.StoreAsync(sourceGrant);
      sourceGrant.Key = "k3";
      sourceGrant.ClientId = "cid2";
      await sut1.StoreAsync(sourceGrant);
      Assert.Equal(3, (await sut2.GetAllAsync(sourceGrant.SubjectId)).Count());
      await sut1.RemoveAllAsync(sDelete, cDelete);
      Assert.Equal(remaining, (await sut2.GetAllAsync(sourceGrant.SubjectId)).Count());
    }

    [Theory]
    [InlineData("cid", "sid", 1)]
    [InlineData("cid2", "sid", 2)]
    public async Task RemoveAllAsync2(string cDelete, string sDelete, int remaining)
    {
      var sourceGrant = DefaultGrant();
      await sut1.StoreAsync(sourceGrant);
      sourceGrant.Key = "k2";
      await sut1.StoreAsync(sourceGrant);
      sourceGrant.Key = "k3";
      sourceGrant.ClientId = "cid2";
      await sut1.StoreAsync(sourceGrant);
      Assert.Equal(3, (await sut2.GetAllAsync(sourceGrant.SubjectId)).Count());
      await sut1.RemoveAllAsync(sDelete, cDelete, sourceGrant.Type);
      Assert.Equal(remaining, (await sut2.GetAllAsync(sourceGrant.SubjectId)).Count());
    }
  }
}