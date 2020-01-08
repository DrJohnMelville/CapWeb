using AspNetCoreLocalLog.Data;
using Xunit;

namespace AspNetCoreLocalLogTest.Data
{
  public sealed class CircularBufferTest
  {

    private readonly CircularBuffer<int> sut = new CircularBuffer<int>();

    [Fact]
    public void BufferStartsEmpty()
    {
      Assert.Empty(sut.All());
    }

  }
}