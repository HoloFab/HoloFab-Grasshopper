// #define DEBUG
#undef DEBUG

using System;
using System.Collections.Generic;

using System.Windows.Forms;
using System.Drawing;
using Rhino.Geometry;
using Grasshopper.Kernel;
using Newtonsoft.Json;
using System;

using HoloFab.CustomData;

namespace HoloFab {
	// A HoloFab class to send mesh to AR device via TCP.
	public class MeshStreaming : GH_Component {
		//////////////////////////////////////////////////////////////////////////
		// - history
		private string lastMessage = string.Empty;
		// - default settings
		private Color defaultColor = Color.Red;
		// - component Info
		// TODO: doesn't seem to be saved
		private SourceType sourceType;
		// - settings
		// If messages in queues - expire solution after this time.
		private static int expireDelay = 40;
		// force messages despite memory or no
		private bool flagForce = true;
		// - debugging
		#if DEBUG
		private string sourceName = "Mesh Streaming Component";
		public List<string> debugMessages = new List<string>();
		#endif
        
		/// <summary>
		/// This is the method that actually does the work.
		/// </summary>
		/// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
		protected override void SolveInstance(IGH_DataAccess DA) {
			//CheckType();
			// Get inputs.
			string message;
			List<Mesh> inputMeshes = new List<Mesh>();
			List<Color> inputColor = new List<Color>();
			Connection connect = null;
			if (!DA.GetDataList(0, inputMeshes)) return;
			DA.GetDataList(1, inputColor);
			if (!DA.GetData<Connection>(2, ref connect)) return;
			// Check inputs.
			if ((inputColor.Count > 1) && (inputColor.Count != inputMeshes.Count)) {
				message = (inputColor.Count > inputMeshes.Count) ?
				          "The number of Colors does not match the number of Mesh objects. Extra colors will be ignored." :
				          "The number of Colors does not match the number of Mesh objects. The last color will be repeated.";
				UniversalDebug(message, GH_RuntimeMessageLevel.Warning);
			}
			////////////////////////////////////////////////////////////////////
            
			// If connection open start acting.
			if (connect.status) {
				// Encode mesh data.
				List<MeshData> inputMeshData = new List<MeshData> { };
				for (int i = 0; i < inputMeshes.Count; i++) {
					Color currentColor = inputColor[Math.Min(i, inputColor.Count)];
					inputMeshData.Add(MeshUtilities.EncodeMesh(inputMeshes[i], currentColor));
				}
				// Send mesh data.
				byte[] bytes = EncodeUtilities.EncodeData("MESHSTREAMING", inputMeshData, out string currentMessage);
				if (this.flagForce || (this.lastMessage != currentMessage)) {
					//bool success = false;
					if (this.sourceType == SourceType.TCP) {
						connect.tcpSender.QueueUpData(bytes);
						//success = connect.tcpSender.flagSuccess;
						//message = connect.tcpSender.debugMessages[connect.tcpSender.debugMessages.Count-1];
					} else {
						connect.udpSender.QueueUpData(bytes);
						//success = connect.udpSender.flagSuccess;
						//message = connect.udpSender.debugMessages[connect.udpSender.debugMessages.Count-1];
					}
					//if (success)
					//	this.lastMessage = currentMessage;
					//UniversalDebug(message, (success) ? GH_RuntimeMessageLevel.Remark : GH_RuntimeMessageLevel.Error);
				}
			} else {
				this.lastMessage = string.Empty;
				UniversalDebug("Set 'Send' on true in HoloFab 'HoloConnect'", GH_RuntimeMessageLevel.Warning);
			}
            
			// Output.
			#if DEBUG
			DA.SetData(0, this.debugMessages[this.debugMessages.Count-1]);
			#endif
			
			// Expire Solution.
			if ((connect.status) && (connect.PendingMessages)) {
				GH_Document document = this.OnPingDocument();
				if (document != null)
					document.ScheduleSolution(MeshStreaming.expireDelay, ScheduleCallback);
			}
		}
		private void ScheduleCallback(GH_Document document) {
			ExpireSolution(false);
		}
		//////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Initializes a new instance of the MeshStreaming class.
		/// Each implementation of GH_Component must provide a public
		/// constructor without any arguments.
		/// Category represents the Tab in which the component will appear,
		/// Subcategory the panel. If you use non-existing tab or panel names,
		/// new tabs/panels will automatically be created.
		/// </summary>
		public MeshStreaming()
			: base("Mesh Streaming", "MS",
			       "Streams 3D Mesh Data",
			       "HoloFab", "Main") {
			//CheckType();
			UpdateMessage();
		}
		/// <summary>
		/// Provides an Icon for every component that will be visible in the User Interface.
		/// Icons need to be 24x24 pixels.
		/// </summary>
		protected override System.Drawing.Bitmap Icon {
			get { return Properties.Resources.HoloFab_MeshStreaming; }
		}
		/// <summary>
		/// Each component must have a unique Guid to identify it.
		/// It is vital this Guid doesn't change otherwise old ghx files
		/// that use the old ID will partially fail during loading.
		/// </summary>
		public override Guid ComponentGuid {
			get { return new Guid("86810363-cded-40bc-ad04-0d6a5b484b02"); }
		}
		/// <summary>
		/// Registers all the input parameters for this component.
		/// </summary>
		protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
			pManager.AddMeshParameter("Mesh", "M", "Mesh object to be encoded and sent via TCP.", GH_ParamAccess.list);
			pManager.AddColourParameter("Color", "C", "Color for each Mesh object.", GH_ParamAccess.list, this.defaultColor);
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
		// Customize Grasshopper Component Menu.
		protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu) {
			// Create Base Menu
			base.AppendAdditionalComponentMenuItems(menu);
			// Add Custom Settings
			Menu_AppendSeparator(menu);
			Menu_AppendItem(menu, "Change Protocol(TCP/UDP)", SwitchProtocol, true);
		}
		// Action to be performed on Click.
		private void SwitchProtocol(object sender, EventArgs eventArgs) {
			this.sourceType = (this.sourceType == SourceType.UDP) ? SourceType.TCP : SourceType.UDP;
			UpdateMessage();
			// Update Grasshopper.
			Grasshopper.Instances.RedrawCanvas();
		}
		////////////////////////////////////////////////////////////////////////
		// Update Component Message.
		private void UpdateMessage(){
			this.Message = (this.sourceType == SourceType.TCP) ? "TCP" : "UDP";
		}
		// Common way to Communicate messages.
		private void UniversalDebug(string message, GH_RuntimeMessageLevel messageType = GH_RuntimeMessageLevel.Remark) {
			#if DEBUG
			DebugUtilities.UniversalDebug(this.sourceName, message, ref this.debugMessages);
			#endif
			this.AddRuntimeMessage(messageType, message);
		}
		////////////////////////////////////////////////////////////////////////
		// Try to solve the saving of source Type.
		public override bool Write(GH_IO.Serialization.GH_IWriter writer) {
			// First add our own field.
			writer.SetInt32("sourceType", (int)this.sourceType);
			// Then call the base class implementation.
			return base.Write(writer);
		}
		public override bool Read(GH_IO.Serialization.GH_IReader reader) {
			// First read our own field.
			try {
				this.sourceType = (SourceType)reader.GetInt32("sourceType");
			}
			catch {
				this.sourceType = SourceType.UDP;
			}
			UpdateMessage();
			// Then call the base class implementation.
			return base.Read(reader);
		}
	}
}