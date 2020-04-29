// #define DEBUG
#undef DEBUG

using System;
using System.Collections.Generic;

using Newtonsoft.Json;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Text;

using HoloFab.CustomData;

namespace HoloFab {
	public class RobotStreaming : GH_Component {
		//////////////////////////////////////////////////////////////////////////
		// - history
		private string lastMessage = string.Empty;
		// - settings
		// If messages in queues - expire solution after this time.
		private static int expireDelay = 40;
		// force messages despite memory or no
		private bool flagForce = false;
		// - debugging
		#if DEBUG
		private string sourceName = "Robot Streaming Component";
		public List<string> debugMessages = new List<string>();
		#endif
        
		/// <summary>
		/// This is the method that actually does the work.
		/// </summary>
		/// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
		protected override void SolveInstance(IGH_DataAccess DA) {
			// Get inputs.
			List<RobotData> inputRobots = new List<RobotData>();
			Connection connect = null;
			if (!DA.GetDataList(0, inputRobots)) return;
			if (!DA.GetData<Connection>(1, ref connect)) return;
			//////////////////////////////////////////////////////
			// Process data.
			if (connect.status) {
				// If connection open start acting.
                
				// Send robot data.
				byte[] bytes = EncodeUtilities.EncodeData("HOLOBOTS", inputRobots.ToArray(), out string currentMessage);
				if (this.flagForce || (this.lastMessage != currentMessage)) {
					connect.tcpSender.QueueUpData(bytes);
					//bool success = connect.tcpSender.flagSuccess;
					//string message = connect.tcpSender.debugMessages[connect.tcpSender.debugMessages.Count-1];
					//if (success)
					//	this.lastMessage = currentMessage;
					//UniversalDebug(message, (success) ? GH_RuntimeMessageLevel.Remark : GH_RuntimeMessageLevel.Error);
				}
			} else {
				this.lastMessage = string.Empty;
				UniversalDebug("Set 'Send' on true in HoloFab 'HoloConnect'.", GH_RuntimeMessageLevel.Warning);
			}
			//////////////////////////////////////////////////////
			// Output.
			#if DEBUG
			DA.SetData(0, RobotStreaming.debugMessages[RobotStreaming.debugMessages.Count-1]);
			#endif
			
			// Expire Solution.
			if ((connect.status) && (connect.PendingMessages)) {
				GH_Document document = this.OnPingDocument();
				if (document != null)
					document.ScheduleSolution(RobotStreaming.expireDelay, ScheduleCallback);
			}
		}
		private void ScheduleCallback(GH_Document document) {
			ExpireSolution(false);
		}
		//////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Initializes a new instance of the Positioner class.
		/// Each implementation of GH_Component must provide a public
		/// constructor without any arguments.
		/// Category represents the Tab in which the component will appear,
		/// Subcategory the panel. If you use non-existing tab or panel names,
		/// new tabs/panels will automatically be created.
		/// </summary>
		public RobotStreaming()
			: base("Robot Streaming", "RS",
			       "Streams HoloBot(s) to AR device",
			       "HoloFab", "HoloBot") {}
		/// <summary>
		/// Provides an Icon for every component that will be visible in the User Interface.
		/// Icons need to be 24x24 pixels.
		/// </summary>
		protected override System.Drawing.Bitmap Icon {
			get { return Properties.Resources.HoloFab_HoloBot_RobotStreaming; }
		}
		/// <summary>
		/// Gets the unique ID for this component. Do not change this ID after release.
		/// </summary>
		public override Guid ComponentGuid {
			get { return new Guid("2b224787-aa83-4a4b-bbb8-ece437ed1c7b"); }
		}
		/// <summary>
		/// Registers all the input parameters for this component.
		/// </summary>
		protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
			pManager.AddGenericParameter("HoloBot(s)", "Hb", "Holographic Robot object from HoloFab 'HoloBot' component.", GH_ParamAccess.list);
			pManager.AddGenericParameter("Connect", "Cn", "Connection object from Holofab 'Create Connection' component.", GH_ParamAccess.item);
		}
        
		/// <summary>
		/// Registers all the output parameters for this component.
		/// </summary>
		protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
			#if DEBUG
			pManager.AddTextParameter("Debug", "D", "Debug console.", GH_ParamAccess.item);
			#endif
		}
		////////////////////////////////////////////////////////////////////////
		// Common way to Communicate messages.
		private void UniversalDebug(string message, GH_RuntimeMessageLevel messageType = GH_RuntimeMessageLevel.Remark) {
			#if DEBUG
			DebugUtilities.UniversalDebug(this.sourceName, message, ref this.debugMessages);
			#endif
			this.AddRuntimeMessage(messageType, message);
		}
	}
}