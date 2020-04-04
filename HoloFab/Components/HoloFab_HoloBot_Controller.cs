using System;
using System.Collections.Generic;

using System.Text;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Newtonsoft.Json;

using HoloFab.CustomData;

namespace HoloFab {
	public class Controller : GH_Component {
		//////////////////////////////////////////////////////////////////////////
		private string sourceName = "Robot Controller Component";
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
			GH_Structure<GH_Number> inputAxisAngles = new GH_Structure<GH_Number> { };
			Connection connect = null;
			if (!DA.GetDataList(0, inputRobots)) return;
			if (!DA.GetDataTree(1, out inputAxisAngles)) return;
			if (!DA.GetData(2, ref connect)) return;
			// Check inputs.
			if ((inputAxisAngles.Paths.Count > 1) && (inputAxisAngles.Paths.Count != inputRobots.Count)) {
				UniversalDebug("The number of Branches of Axis Angles should be one or equal to the number of HoloBot objects.",
				               GH_RuntimeMessageLevel.Error);
				return;
			}
			// if (inputAxisAngles.Count != 6) {
			// 	Positioner.debugMessages.Add("Component: Controller: The number of Axis should be equal to the number of Robot Joints.");
			// 	return;
			// }
			//////////////////////////////////////////////////////
			// Process data.
			if (connect.status) {
				// If connection open start acting.
				UniversalDebug("Robots Found: " + inputRobots.Count + ", Axis Count, " + inputAxisAngles.Paths.Count);
				List<RobotControllerData> robotControllers = new List<RobotControllerData>();
				for (int i = 0; i < inputRobots.Count; i++) {
					List<double> currentRobotAxisValues = new List<double> { };
					int index = (inputAxisAngles.Paths.Count > 1) ? i : 0;
					Console.WriteLine(index);
					double currentValue;
					for (int j = 0; j < inputAxisAngles[index].Count; j++) {
						currentValue = (double)inputAxisAngles[index][j].Value;
						currentRobotAxisValues.Add(currentValue * (180.0 / Math.PI));
					}
					robotControllers.Add(new RobotControllerData(inputRobots[i].robotID, currentRobotAxisValues));
				}
                
				// Send robot controller data.
				byte[] bytes = EncodeUtilities.EncodeData("CONTROLLER", robotControllers, out string currentMessage);
				if (Controller.lastMessage != currentMessage) {
					connect.udpSender.Send(bytes);
					bool success = connect.udpSender.success;
					string message = connect.udpSender.debugMessages[connect.udpSender.debugMessages.Count-1];
					if (success)
						Controller.lastMessage = currentMessage;
					UniversalDebug(message, (success) ? GH_RuntimeMessageLevel.Remark : GH_RuntimeMessageLevel.Error);
                    
				}
			} else {
				Controller.lastMessage = string.Empty;
				UniversalDebug("Set 'Send' on true in HoloFab 'HoloConnect'.", GH_RuntimeMessageLevel.Warning);
			}
			//////////////////////////////////////////////////////
			// Output.
			// DA.SetData(0, AxisController.debugMessages[AxisController.debugMessages.Count-1]);
		}
		//////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Initializes a new instance of the AxisController class.
		/// Each implementation of GH_Component must provide a public
		/// constructor without any arguments.
		/// Category represents the Tab in which the component will appear,
		/// Subcategory the panel. If you use non-existing tab or panel names,
		/// new tabs/panels will automatically be created.
		/// </summary>
		public Controller()
			: base("Robot Controller", "RC",
			       "Streams axis values to the holographic Robot",
			       "HoloFab", "HoloBot") {}
		/// <summary>
		/// Provides an Icon for the component.
		/// </summary>
		protected override System.Drawing.Bitmap Icon {
			get { return Properties.Resources.HoloFab_HoloBot_Controller; }
		}
		/// <summary>
		/// Gets the unique ID for this component. Do not change this ID after release.
		/// </summary>
		public override Guid ComponentGuid {
			get { return new Guid("adf66372-24a7-4e16-a59d-18ef56149eb4"); }
		}
		/// <summary>
		/// Registers all the input parameters for this component.
		/// </summary>
		protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
			pManager.AddGenericParameter("HoloBot", "Hb", "Holographic Robot object from HoloFab 'HoloBot' component.", GH_ParamAccess.list);
			pManager.AddNumberParameter("Axis values", "A", "Values for the axis in radians, in branches per HoloBots.", GH_ParamAccess.tree);
			pManager.AddGenericParameter("Connect", "Cn", "Connection object from Holofab 'Create Connection' component.", GH_ParamAccess.item);
		}
		/// <summary>
		/// Registers all the output parameters for this component.
		/// </summary>
		protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
			//pManager.AddTextParameter("Debug", "D", "Debug console.", GH_ParamAccess.item);
		}
		////////////////////////////////////////////////////////////////////////
		// Common way to Communicate messages.
		private void UniversalDebug(string message, GH_RuntimeMessageLevel messageType = GH_RuntimeMessageLevel.Remark) {
			DebugUtilities.UniversalDebug(this.sourceName, message, ref Controller.debugMessages);
			this.AddRuntimeMessage(messageType, message);
		}
	}
}