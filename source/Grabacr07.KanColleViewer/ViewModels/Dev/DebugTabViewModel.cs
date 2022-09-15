using Livet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Grabacr07.KanColleViewer.Composition;

namespace Grabacr07.KanColleViewer.ViewModels.Dev
{
	public class Tracer : TraceListener
	{
		private Action<string> writeMsg;

		public Tracer(Action<string> action)
		{
			writeMsg = action;
		}

		public override void Write(string message)
		{
			writeMsg(message);
		}

		public override void WriteLine(string message)
		{
			Write(message + "\n");
		}
	}

	public class DebugTabViewModel : TabItemViewModel
	{
		private string _msg;
		public string TraceMessage { get => _msg; set => RaisePropertyChangedIfSet(ref _msg, value); }

		private readonly Tracer trace;


		public override string Name
		{
			get { return Properties.Resources.Debug; }
			protected set { throw new NotImplementedException(); }
		}

		public DebugTabViewModel()
		{
			trace = new Tracer(msg => TraceMessage += msg);
			_ = Debug.Listeners.Add(trace);
		}

		public void Clear()
		{
			TraceMessage = "";
		}
	}
}
