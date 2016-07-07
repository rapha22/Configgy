using System;
using System.IO;
using System.Threading.Tasks;
using Configgy.TestUtilities;
using Xunit;

namespace Configgy.Server.Tests
{
    public class ConfigurationFilesMonitorTests : IDisposable
    {
        readonly string basePath = "test_files_watcher".ToAbsolutePath();
        readonly string filesFilter = "*.txt";

        ConfigurationFilesMonitor monitor;
        Task<bool> wasTriggered;
        Action cleanup;


        public ConfigurationFilesMonitorTests()
        {
            monitor = new ConfigurationFilesMonitor(basePath, filesFilter, new StubLogger());
        }

        public void Dispose()
        {
            monitor.Dispose();

            //Cleanup AFTER disposing the monitor. Otherwise, it will generate exceptions for
            //triggering events and trying to set the same task more than once
            cleanup(); 
        }


        [Fact]
        public void WhenFilesContentChanges_ShouldTrigger()
        {
            monitor.ChangeDetected += MonitorTestHelper.CreateCompletionTaskAction(out wasTriggered);
            monitor.Start();

            WriteToFile("content_change.txt", DateTime.Now.ToString());

            Assert.True(wasTriggered.Result);

            cleanup = () =>
            {
                using (var file = File.OpenWrite("content_change.txt"))
                    file.Write(new byte[0], 0, 0);
            };
        }

        [Fact]
        public void WhenFileIsRenamed_ShouldTrigger()
        {
            monitor.ChangeDetected += MonitorTestHelper.CreateCompletionTaskAction(out wasTriggered);
            monitor.Start();

            File.Move(PathTo("renaming.txt"), PathTo("renamed.txt"));

            Assert.True(wasTriggered.Result(1000));

            //Cleanup
            cleanup = () => File.Move(PathTo("renamed.txt"), PathTo("renaming.txt"));
        }

        [Fact]
        public void WhenFileIsDeleted_ShouldTrigger()
        {
            monitor.ChangeDetected += MonitorTestHelper.CreateCompletionTaskAction(out wasTriggered);
            monitor.Start();

            File.Delete(PathTo("deleting.txt"));

            Assert.True(wasTriggered.Result(1000));

            //Cleanup
            cleanup = () => { using (File.Create(PathTo("deleting.txt"))); };
        }

        [Fact]
        public void WhenFileIsCreated_ShouldTrigger()
        {
            monitor.ChangeDetected += MonitorTestHelper.CreateCompletionTaskAction(out wasTriggered);
            monitor.Start();

            using (File.Create(PathTo("new.txt")));

            Assert.True(wasTriggered.Result(1000));

            //Cleanup
            cleanup = () => File.Delete(PathTo("new.txt"));
        }


        string PathTo(string relativePath)
        {
            return Path.Combine(basePath, relativePath);
        }

        void WriteToFile(string relativePath, string text)
        {
            using (var writer = new StreamWriter(PathTo("content_change.txt")))
            {
                writer.WriteLine(text);
            }
        }
    }
}
