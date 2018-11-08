using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph.Action
{
	class ReadMemory : IAction
	{
		string _id;

		public ReadMemory(string id)
		{
			_id = id;
		}

		public IGraphData Execute(IGraphData input, IContext context)
		{
			var memory = context.ExecutionContext.GetMemory(_id);
			return memory.AsGraphData();
		}

		public void Initialise(string data)
		{
			_id = data;
		}

		public string Serialise()
		{
			return _id;
		}
	}
}
