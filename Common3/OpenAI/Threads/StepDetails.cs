using System.Collections.Generic;
using Newtonsoft.Json;

namespace OpenAI.Threads
{
    /// <summary>
    /// The details of the run step.
    /// </summary>
    public sealed class StepDetails
    {
        /// <summary>
        /// Details of the message creation by the run step.
        /// </summary>
        
        [JsonProperty("message_creation")]
        public RunStepMessageCreation MessageCreation { get; private set; }

        /// <summary>
        /// An array of tool calls the run step was involved in.
        /// These can be associated with one of three types of tools: 'code_interpreter', 'retrieval', or 'function'.
        /// </summary>
        
        [JsonProperty("tool_calls")]
        public IReadOnlyList<ToolCall> ToolCalls { get; private set; }
    }
}