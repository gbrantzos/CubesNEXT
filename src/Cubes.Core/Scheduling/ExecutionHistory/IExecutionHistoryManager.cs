using System.Collections.Generic;

namespace Cubes.Core.Scheduling.ExecutionHistory
{
    public interface IExecutionHistoryManager
    {
        /// <summary>
        /// Get last execution details for given job names
        /// </summary>
        /// <param name="jobNames">Job names to get execution details for</param>
        /// <returns></returns>
        IEnumerable<ExecutionHistoryDetails> GetLastExecutions(IEnumerable<string> jobNames);

        /// <summary>
        /// Get execution history details for job with given name.
        /// </summary>
        /// <param name="jobName">Name of job</param>
        /// <returns></returns>
        IEnumerable<ExecutionHistoryDetails> Get(string jobName);

        /// <summary>
        /// Store execution details
        /// </summary>
        /// <param name="historyDetails"></param>
        void Save(ExecutionHistoryDetails historyDetails);

        /// <summary>
        /// Delete execution details for given job name, based on <see cref="Retention"/>.
        /// </summary>
        /// <param name="jobName">Name of job</param>
        /// <param name="retention">History retention</param>
        void Delete(string jobName, Retention retention);
    }
}
