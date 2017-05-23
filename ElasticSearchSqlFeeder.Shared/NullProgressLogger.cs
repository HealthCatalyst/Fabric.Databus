using System;
using System.Collections.Concurrent;
using System.Linq;
using ElasticSearchSqlFeeder.Interfaces;
using ElasticSearchSqlFeeder.ProgressMonitor;

namespace ElasticSearchSqlFeeder.Shared
{
    public class NullProgressLogger : IProgressLogger
    {

        public void Reset()
        {
        }

        public string GetLog()
        {
            throw new NotImplementedException();
        }

        public void LogProgressMonitorItem(int key, ProgressMonitorItem progressMonitorItem)
        {
        }
    }
}