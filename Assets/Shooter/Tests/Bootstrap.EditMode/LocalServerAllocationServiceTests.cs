using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Shooter.Application;
using Shooter.Bootstrap.Server;

namespace Shooter.Bootstrap.Tests
{
    public sealed class LocalServerAllocationServiceTests
    {
        [Test]
        public async Task WaitForAllocationAsync_ReturnsConfiguredRequest()
        {
            var request = NetworkSessionStartRequest.ForServer("local-session", 27015);
            var service = new LocalServerAllocationService(request);

            var result = await service.WaitForAllocationAsync(CancellationToken.None);

            Assert.That(result, Is.SameAs(request));
        }

        [Test]
        public void WaitForAllocationAsync_RejectsCancellation()
        {
            var request = NetworkSessionStartRequest.ForServer("local-session", 27015);
            var service = new LocalServerAllocationService(request);
            using (var cancellation = new CancellationTokenSource())
            {
                cancellation.Cancel();
                Assert.That(() => service.WaitForAllocationAsync(cancellation.Token), Throws.InstanceOf<System.OperationCanceledException>());
            }
        }
    }
}
