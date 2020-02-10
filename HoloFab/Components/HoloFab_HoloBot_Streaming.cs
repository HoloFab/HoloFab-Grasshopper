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
		public static List<string> debugMessages = new List<string>();
		private static string lastMessage = string.Empty;
        
		/// <summary>
		/// This is the method that actually does the work.
		/// </summary>
		/// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
		protected override void SolveInstance(IGH_DataAccess DA) {
			// Get inputs.
			List<RobotData> inputRobots = new List<RobotData>();
			Connection connect = new Connection();
			if (!DA.GetDataList(0, inputRobots)) return;
			if (!DA.GetData(1, ref connect)) return;
            
			// Process data.
			if (connect.status) {
				// If connection open start acting.
                
				// Send robot data.
				string currentMessage = string.Empty;
				byte[] bytes = EncodeUtilities.EncodeData("HOLOBOTS", inputRobots.ToArray(), out currentMessage);
				if (RobotStreaming.lastMessage != currentMessage) {
					RobotStreaming.lastMessage = currentMessage;
					connect.tcpSender.Send(bytes);
					RobotStreaming.debugMessages.Add("Component: Robot Streaming: Robot data sent over TCP.");
				}
			} else {
				RobotStreaming.lastMessage = string.Empty;
				RobotStreaming.debugMessages.Add("Component: Robot Streaming: Set 'Send' on true in HoloFab 'HoloConnect'.");
			}
            
			// Output.
			// DA.SetData(0, Positioner.debugMessages[Positioner.debugMessages.Count-1]);
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
			//pManager.AddTextParameter("Debug", "D", "Debug console.", GH_ParamAccess.item);
		}
	}
}