using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace OpenAI.Threads
{
    public sealed class CreateRunRequest
    {
        public CreateRunRequest(string assistantId, CreateRunRequest request)
            : this(assistantId, request.Model, request.Instructions, request.Tools, request.Metadata)
        {
        }

        public CreateRunRequest(string assistantId, string model = null, string instructions = null, IEnumerable<Tool> tools = null, IReadOnlyDictionary<string, string> metadata = null)
        {
            AssistantId = assistantId;
            Model = model;
            Instructions = instructions;
            Tools = tools.ToList();
            Metadata = metadata;
        }

        /// <summary>
        /// The ID of the assistant used for execution of this run.
        /// </summary>
        [JsonProperty("assistant_id")]
        public string AssistantId { get; private set; }

        /// <summary>
        /// The model that the assistant used for this run.
        /// </summary>
        [JsonProperty("model")]
        public string Model { get; private set; }

        /// <summary>
        /// The instructions that the assistant used for this run.
        /// </summary>
        [JsonProperty("instructions")]
        public string Instructions { get; private set; }

        /// <summary>
        /// The list of tools that the assistant used for this run.
        /// </summary>
        [JsonProperty("tools")]
        public IReadOnlyList<Tool> Tools { get; private set; }

        /// <summary>
        /// Set of 16 key-value pairs that can be attached to an object.
        /// This can be useful for storing additional information about the object in a structured format.
        /// Keys can be a maximum of 64 characters long and values can be a maximum of 512 characters long.
        /// </summary>
        [JsonProperty("metadata")]
        public IReadOnlyDictionary<string, string> Metadata { get; private set; }
    }
}