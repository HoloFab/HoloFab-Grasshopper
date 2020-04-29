// #define DEBUG
#undef DEBUG

using System;
using System.Collections.Generic;

using System.Drawing;
using Rhino.Geometry;
using Grasshopper.Kernel;

using HoloFab.CustomData;

namespace HoloFab {
	public class HoloTag : GH_Component {
		//////////////////////////////////////////////////////////////////////////
		// - history
		private string lastMessage = string.Empty;
		// - default settings
		public float defaultTextSize = 20.0f;
		public Color defaultTextColor = Color.White;
		// - settings
		// If messages in queues - expire solution after this time.
		private static int expireDelay = 40;
		// force messages despite memory or no
		private bool flagForce = false;
		// - debugging
		#if DEBUG
		private string sourceName = "HoloTag Component";
		public List<string> debugMessages = new List<string>();
		#endif
        
		/// <summary>
		/// This is the method that actually does the work.
		/// </summary>
		/// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
		protected override void SolveInstance(IGH_DataAccess DA) {
			// Get inputs.
			List<string> inputText = new List<string>();
			List<Point3d> inputTextLocations = new List<Point3d>();
			List<double> inputTextSize = new List<double>();
			List<Color> inputTextColor = new List<Color>();
			Connection connect = null;
			if (!DA.GetDataList(0, inputText)) return;
			if (!DA.GetDataList(1, inputTextLocations)) return;
			DA.GetDataList(2, inputTextSize);
			DA.GetDataList(3, inputTextColor);
			if (!DA.GetData<Connection>(4, ref connect)) return;
			// Check inputs.
			if (inputTextLocations.Count != inputText.Count) {
				UniversalDebug("The number of 'tag locations' and 'tag texts' should be equal.",
				               GH_RuntimeMessageLevel.Error);
				return;
			}
			if ((inputTextSize.Count > 1) && (inputTextSize.Count != inputText.Count)) {
				UniversalDebug("The number of 'tag text sizes' should be one or equal to one or the number of 'tag texts'.",
				               GH_RuntimeMessageLevel.Error);
				return;
			}
			if ((inputTextColor.Count > 1) && (inputTextColor.Count != inputText.Count)) {
				UniversalDebug("The number of 'tag text colors' should be one or equal to one or the number of 'tag texts'.",
				               GH_RuntimeMessageLevel.Error);
				return;
			}
			//////////////////////////////////////////////////////
			// Process data.
			if (connect.status) {
				// If connection open start acting.
				List<string> currentTexts = new List<string>(){};
				List<float[]> currentTextLocations = new List<float[]>(){};
				List<float> currentTextSizes = new List<float>(){};
				List<int[]> currentTextColors = new List<int[]>(){};
				for(int i=0; i < inputText.Count; i++) {
					float currentSize = (float) ((inputTextSize.Count > 1) ? inputTextSize[i] : inputTextSize[0]);
					Color currentColor = (inputTextColor.Count > 1) ? inputTextColor[i] : inputTextColor[0];
					currentTexts.Add(inputText[i]);
					currentTextLocations.Add(EncodeUtilities.EncodeLocation(inputTextLocations[i]));
					currentTextSizes.Add((float)Math.Round(currentSize/1000.0, 3));
					currentTextColors.Add(EncodeUtilities.EncodeColor(currentColor));
				}
				TagData tags = new TagData(currentTexts, currentTextLocations, currentTextSizes, currentTextColors);
                
				// Send tag data.
				byte[] bytes = EncodeUtilities.EncodeData("HOLOTAG", tags, out string currentMessage);
				if (this.flagForce || (this.lastMessage != currentMessage)) {
					connect.udpSender.QueueUpData(bytes);
					//bool success = connect.udpSender.flagSuccess;
					//string message = connect.udpSender.debugMessages[connect.udpSender.debugMessages.Count-1];
					//if (success)
					//	this.lastMessage = currentMessage;
					//UniversalDebug(message, (success) ? GH_RuntimeMessageLevel.Remark : GH_RuntimeMessageLevel.Error);
				}
			} else {
				this.lastMessage = string.Empty;
				UniversalDebug("Set 'Send' on true in HoloFab 'HoloConnect'", GH_RuntimeMessageLevel.Warning);
			}
			//////////////////////////////////////////////////////
			// Output.
			#if DEBUG
			DA.SetData(0, this.debugMessages[this.debugMessages.Count-1]);
			#endif
			
			// Expire Solution.
			if ((connect.status) && (connect.PendingMessages)) {
				GH_Document document = this.OnPingDocument();
				if (document != null)
					document.ScheduleSolution(HoloTag.expireDelay, ScheduleCallback);
			}
		}
		private void ScheduleCallback(GH_Document document) {
			ExpireSolution(false);
		}
		////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Initializes a new instance of the HoloTag class.
		/// Each implementation of GH_Component must provide a public
		/// constructor without any arguments.
		/// Category represents the Tab in which the component will appear,
		/// Subcategory the panel. If you use non-existing tab or panel names,
		/// new tabs/panels will automatically be created.
		/// </summary>
		public HoloTag()
			: base("HoloTag", "HT",
			       "Streams fabrication data as strings",
			       "HoloFab", "Main") {}
		/// <summary>
		/// Provides an Icon for every component that will be visible in the User Interface.
		/// Icons need to be 24x24 pixels.
		/// </summary>
		protected override System.Drawing.Bitmap Icon {
			get { return Properties.Resources.HoloFab_HoloTag; }
		}
		/// <summary>
		/// Gets the unique ID for this component. Do not change this ID after release.
		/// </summary>
		public override Guid ComponentGuid {
			get { return new Guid("51f68dce-f87b-4ae7-b7b5-4905cb8badd4"); }
		}
		/// <summary>
		/// Registers all the input parameters for this component.
		/// </summary>
		protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
			pManager.AddTextParameter("Text String", "S", "Tags as string", GH_ParamAccess.list);
			pManager.AddPointParameter("Text Location", "L", "Sets the location of Tags", GH_ParamAccess.list);
			pManager.AddNumberParameter("Text Size", "TS", "Size of text Tags in AR environment", GH_ParamAccess.list, this.defaultTextSize);
			pManager.AddColourParameter("Text Color", "C", "Color of text Tags", GH_ParamAccess.list, this.defaultTextColor);
			pManager.AddGenericParameter("Connect", "Cn", "Connection object from Holofab 'Create Connection' component.", GH_ParamAccess.item);
			pManager[2].Optional = true;
			pManager[3].Optional = true;
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
			DebugUtilities.UniversalDebug(this.sourceName, message, ref HoloTag.debugMessages);
			#endif
			this.AddRuntimeMessage(messageType, message);
		}
	}
}