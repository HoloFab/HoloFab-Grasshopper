// #define DEBUG
#undef DEBUG

using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Newtonsoft.Json;

using HoloFab.CustomData;

namespace HoloFab {
	// A HoloFab class to receive UI elements from AR device.
	public class UIReceiver : GH_Component {
		//////////////////////////////////////////////////////////////////////////
		// - currents
		private static string currentInput;
		private static List<bool> currentBools = new List<bool>();
		private static List<int> currentInts = new List<int>();
		private static List<float> currentFloats = new List<float>();
		// - history
		private static string lastInputs;
		//private static bool flagProcessed = false;
		// - settings
		private static int expireDelay = 40;
		// - debugging
		#if DEBUG
		private string sourceName = "UI Receiving Component";
		public static List<string> debugMessages = new List<string>();
		#endif
        
		/// <summary>
		/// This is the method that actually does the work.
		/// </summary>
		/// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
		protected override void SolveInstance(IGH_DataAccess DA) {
			// Get inputs.
			Connection connect = null;
			if (!DA.GetData(0, ref connect)) return;
			//////////////////////////////////////////////////////
			// Process data.
			if (connect.status) {
				// If connection open start acting.
				// Prepare to receive UI data.
				try {
					if (connect.udpReceiver.dataMessages.Count > 0) {
						UIReceiver.currentInput = connect.udpReceiver.dataMessages.Peek();
						UIReceiver.currentInput = EncodeUtilities.StripSplitter(UIReceiver.currentInput);
						if (UIReceiver.lastInputs != UIReceiver.currentInput) {
							UIReceiver.lastInputs = UIReceiver.currentInput;
							UniversalDebug("New Message without Message Splitter removed: " + currentInput);
							string[] messageComponents = UIReceiver.currentInput.Split(new string[] {EncodeUtilities.headerSplitter}, 2, StringSplitOptions.RemoveEmptyEntries);
							if (messageComponents.Length > 1) {
								string header = messageComponents[0], content = messageComponents[1];
								UniversalDebug("Header: " + header + ", content: " + content);
								if (header == "UIDATA") {
									// If any new data received - process it.
									UIData data = JsonConvert.DeserializeObject<UIData>(content);
									UIReceiver.currentBools = new List<bool> (data.bools);
									UIReceiver.currentInts = new List<int> (data.ints);
									UIReceiver.currentFloats = new List<float> (data.floats);
									UniversalDebug("Data Received!");
									connect.udpReceiver.dataMessages.Dequeue(); // Actually remove from the queue since it has been processed.
								}
								// else
								//	UniversalDebug("Header Not Recognized!", GH_RuntimeMessageLevel.Warning);
							} else
								UniversalDebug("Data not Received!", GH_RuntimeMessageLevel.Warning);
						} else
							UniversalDebug("Improper Message!", GH_RuntimeMessageLevel.Warning);
					} else
						UniversalDebug("No data received.");
				} catch {
					UniversalDebug("Error Processing Data.", GH_RuntimeMessageLevel.Error);
				}
			} else {
				// If connection disabled - reset memoty.
				UIReceiver.lastInputs = string.Empty;
				UIReceiver.currentBools = new List<bool>();
				UIReceiver.currentInts = new List<int>();
				UIReceiver.currentFloats = new List<float>();
				UniversalDebug("Set 'Send' on true in HoloFab 'HoloConnect'", GH_RuntimeMessageLevel.Warning);
			}
			//////////////////////////////////////////////////////
			// Output.
			DA.SetDataList(0, UIReceiver.currentBools);
			DA.SetDataList(1, UIReceiver.currentInts);
			DA.SetDataList(2, UIReceiver.currentFloats);
			#if DEBUG
			DA.SetData(3, this.debugMessages[this.debugMessages.Count-1]);
			#endif
            
			// Expire Solution.
			if (connect.status) {
				GH_Document document = this.OnPingDocument();
				if (document != null)
					document.ScheduleSolution(UIReceiver.expireDelay, ScheduleCallback);
			}
		}
		private void ScheduleCallback(GH_Document document) {
			ExpireSolution(false);
		}
		//////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Initializes a new instance of the UIReceiver class.
		/// Each implementation of GH_Component must provide a public
		/// constructor without any arguments.
		/// Category represents the Tab in which the component will appear,
		/// Subcategory the panel. If you use non-existing tab or panel names,
		/// new tabs/panels will automatically be created.
		/// </summary>
		public UIReceiver()
			: base("UI Receiver", "UI",
			       "Receieves Incoming data from the User Interface of AR device",
			       "HoloFab", "UserInterface") {}
		/// <summary>
		/// Provides an Icon for every component that will be visible in the User Interface.
		/// Icons need to be 24x24 pixels.
		/// </summary>
		protected override System.Drawing.Bitmap Icon {
			get { return Properties.Resources.HoloFab_UIReceiver; }
		}
		/// <summary>
		/// Each component must have a unique Guid to identify it.
		/// It is vital this Guid doesn't change otherwise old ghx files
		/// that use the old ID will partially fail during loading.
		/// </summary>
		public override Guid ComponentGuid {
			get { return new Guid("ac5f5de3-cdf2-425a-b435-c97b718e1a09"); }
		}
		/// <summary>
		/// Registers all the input parameters for this component.
		/// </summary>
		protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
			pManager.AddGenericParameter("Connect", "Cn", "Connection object from Holofab 'Create Connection' component.", GH_ParamAccess.item);
		}
		/// <summary>
		/// Registers all the output parameters for this component.
		/// </summary>
		protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
			pManager.AddBooleanParameter("Toggles", "T", "Boolean values coming from UI.", GH_ParamAccess.list);
			pManager.AddIntegerParameter("Counters", "C", "Integer values coming from UI.", GH_ParamAccess.list);
			pManager.AddNumberParameter("Sliders", "S", "Float values coming from UI.", GH_ParamAccess.list);
			#if DEBUG
			pManager.AddTextParameter("Debug", "D", "Debug console.", GH_ParamAccess.item);
			#endif
		}
		////////////////////////////////////////////////////////////////////////
		// Common way to Communicate messages.
		private void UniversalDebug(string message, GH_RuntimeMessageLevel messageType = GH_RuntimeMessageLevel.Remark) {
			#if DEBUG
			DebugUtilities.UniversalDebug(this.sourceName, message, ref UIReceiver.debugMessages);
			#endif
			this.AddRuntimeMessage(messageType, message);
		}
	}
}