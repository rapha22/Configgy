using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Configgy.Server.Tests
{
    public class ConfigurationFilesMonitorTests
    {
        public class Construtor
        {
            [Fact]
            public void WhenPassingNullBasePath_ShouldThrow()
            {
                Assert.Throws<ArgumentException>(() => new ConfigurationFilesMonitor(null, "x", new StubLogger()));
            }

            [Fact]
            public void WhenPassingEmptyBasePath_ShouldThrow()
            {
                Assert.Throws<ArgumentException>(() => new ConfigurationFilesMonitor("", "x", new StubLogger()));
            }

            [Fact]
            public void WhenPassingNullFilesFilter_ShouldThrow()
            {
                Assert.Throws<ArgumentException>(() => new ConfigurationFilesMonitor("x", null, new StubLogger()));
            }

            [Fact]
            public void WhenEmptyNullFilesFilter_ShouldThrow()
            {
                Assert.Throws<ArgumentException>(() => new ConfigurationFilesMonitor("x", "", new StubLogger()));
            }
        }

        public class DirectoryMonitoring : IDisposable
        {
            readonly string basePath = "test_files_watcher".ToAbsolutePath();
            readonly string filesFilter = "*.txt";

            ConfigurationFilesMonitor monitor;
            Task<bool> wasTriggered;
            Action cleanup;


            public DirectoryMonitoring()
            {
                monitor = new ConfigurationFilesMonitor(basePath, filesFilter, new StubLogger(), eventDelayingMs: 300);
            }

            public void Dispose()
            {
                monitor.Dispose();

                //Cleanup AFTER disposing the monitor. Otherwise, it will generate exceptions for
                //triggering events and trying to set the task again
                cleanup(); 
            }


            [Fact]
            public void WhenFilesContentChanges_ShouldTrigger()
            {
                monitor.MonitorChanges(AsyncHelper.CreateCompletionTaskAction(out wasTriggered));

                WriteToFile("content_change.txt", DateTime.Now.ToString());

                Assert.True(wasTriggered.Result);

                cleanup = () => File.OpenWrite("content_change.txt").Write(new byte[0], 0, 0);
            }

            [Fact]
            public void WhenFileIsRenamed_ShouldTrigger()
            {
                monitor.MonitorChanges(AsyncHelper.CreateCompletionTaskAction(out wasTriggered));

                File.Move(PathTo("renaming.txt"), PathTo("renamed.txt"));

                Assert.True(wasTriggered.Result);

                //Cleanup
                cleanup = () => File.Move(PathTo("renamed.txt"), PathTo("renaming.txt"));
            }

            [Fact]
            public void WhenFileIsDeleted_ShouldTrigger()
            {
                monitor.MonitorChanges(AsyncHelper.CreateCompletionTaskAction(out wasTriggered));

                File.Delete(PathTo("deleting.txt"));

                Assert.True(wasTriggered.Result);

                //Cleanup
                cleanup = () => File.Create(PathTo("deleting.txt"));
            }

            [Fact]
            public void WhenFileIsCreated_ShouldTrigger()
            {
                monitor.MonitorChanges(AsyncHelper.CreateCompletionTaskAction(out wasTriggered));

                File.Create(PathTo("new.txt"));

                Assert.True(wasTriggered.Result);

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
}
