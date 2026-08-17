using AwesomeAssertions;
using CraftedSolutions.MarBasCommon.Job;

namespace CraftedSolutions.MarBasCommon.Tests.Job
{
    [TestClass]
    public class BackgroudJobContextTest
    {
        internal class TestDisposable : IDisposable
        {
            public bool Disposed { get; set; }

            public void Dispose()
            {
                Disposed = true;
            }
        }

        public TestContext TestContext { get; set; }

        [TestMethod]
        public void CTor_wrapping_BackgroundJob()
        {
            using var job = new BackgroundJob("testJob", "testOwner");
            var context = new BackgroundJob.Context(job);
            context.Stage.Should().Be(job.Stage);
            context.Status.Should().Be(job.Status);
            context.Progress.Should().Be(job.Progress);
            context.Result.Should().Be(job.Result);

            job.Progress = 50;
            job.Stage = "Testing";
            job.Status = BackgroundJobStatus.Running;
            job.Result = new { Anything = 42 };
            context.Stage.Should().Be(job.Stage);
            context.Status.Should().Be(job.Status);
            context.Progress.Should().Be(job.Progress);
            context.Result.Should().Be(job.Result);

            // double dispose: branch coverage
            job.Dispose();
        }
        [TestMethod]
        public void CTor_throwing_Exception_given_cancelled_CancellationToken()
        {
            using var job = new BackgroundJob("testJob", "testOwner");
            var cts = new CancellationTokenSource();
            cts.Cancel();
            FluentActions.Invoking(() => new BackgroundJob.Context(job, cts.Token)).Should().Throw<OperationCanceledException>();
        }
        [TestMethod]
        public void CTor_throwing_Exception_given_cancelled_Job()
        {
            using var job = new BackgroundJob("testJob", "testOwner", BackgroundJobFlags.Pausable);
            job.Status = BackgroundJobStatus.Cancelled;
            FluentActions.Invoking(() => new BackgroundJob.Context(job, TestContext.CancellationToken)).Should().Throw<OperationCanceledException>();
        }

        [TestMethod]
        public void Set_Status_changing_jobs_Started_timestamp_given_current_value_is_Pending()
        {
            using var job = new BackgroundJob("testJob", "testOwner", BackgroundJobFlags.Pausable, "Testing");
            var context = new BackgroundJob.Context(job, TestContext.CancellationToken);

            job.Started.Should().BeNull();
            var now = DateTime.UtcNow;
            context.Status = BackgroundJobStatus.Running;
            job.Status.Should().Be(BackgroundJobStatus.Running);
            job.Started.Should()
                .NotBeNull().And
                .HaveYear(now.Year).And
                .HaveMonth(now.Month).And
                .HaveDay(now.Day);
        }
        [TestMethod]
        public void Set_Status_disposing_job_given_value_beyond_Paused()
        {
            using var job = new BackgroundJob("testJob", "testOwner", BackgroundJobFlags.Pausable | BackgroundJobFlags.Critical, "Testing");
            var context = new BackgroundJob.Context(job, TestContext.CancellationToken);
            
            var disp = new TestDisposable();
            job.RegisterForDispose(disp);

            context.Status = BackgroundJobStatus.Complete;
            job.Status.Should().Be(BackgroundJobStatus.Complete);
            disp.Disposed.Should().BeTrue();
        }
        [TestMethod]
        public void Set_Status_cancelling_job_given_Cancelled_value()
        {
            using var job = new BackgroundJob("testJob", "testOwner");
            var context = new BackgroundJob.Context(job, TestContext.CancellationToken);

            context.Status = BackgroundJobStatus.Running;
            context.Status.Should().Be(BackgroundJobStatus.Running);

            context.Status = BackgroundJobStatus.Cancelled;
            job.Status.Should().Be(BackgroundJobStatus.Cancelled);
            context.Status.Should().Be(BackgroundJobStatus.Cancelled);
            context.CancellationToken.IsCancellationRequested.Should().BeTrue();
        }
    }
}
