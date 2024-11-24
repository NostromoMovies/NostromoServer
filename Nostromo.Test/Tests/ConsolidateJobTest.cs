using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Quartz;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using Nostromo.Server.Scheduling;
using Nostromo.Server.Scheduling.Jobs;

[TestClass]
public class ConsolidateJobTest
{
    [TestMethod]
    public async Task TestConsolidateJobSchedulesHashFileJob()
    {
        var mockScheduler = new Mock<IScheduler>();
        var mockContext = new Mock<IJobExecutionContext>();
        var mockLogger = new Mock<ILogger<ConsolidateJob>>();

        var baseDirectory = Directory.GetCurrentDirectory();
        var testFilePath = Path.Combine(baseDirectory, "..", "Files", "ConsolidateJobTestHash.txt");

        var jobDataMap = new JobDataMap { { "FilePath", testFilePath } };
        mockContext.SetupGet(c => c.MergedJobDataMap).Returns(jobDataMap);
        mockContext.SetupGet(c => c.Scheduler).Returns(mockScheduler.Object);

        mockScheduler.Setup(s => s.ScheduleJob(
            It.IsAny<IJobDetail>(),
            It.IsAny<ITrigger>(),
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(DateTimeOffset.Now);

        var job = new ConsolidateJob(mockLogger.Object);

        await job.Execute(mockContext.Object);

        mockScheduler.Verify(
            s => s.ScheduleJob(It.IsAny<IJobDetail>(), It.IsAny<ITrigger>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
