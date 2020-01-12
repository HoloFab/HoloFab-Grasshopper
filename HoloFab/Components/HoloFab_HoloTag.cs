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
		public static List<string> debugMessages = new List<string>();
		private static string lastMessage = string.Empty;
        
		/// <summary>
		/// This is the method that actually does the work.
		/// </summary>
		/// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
		protected override void SolveInstance(IGH_DataAccess DA) {
			// Get inputs.
			List<string> inputText = new List<string>();
			List<Point3d> inputTextLocations = new List<Point3d>();
			List<Color> inputTextColor = new List<Color> { };
			List<double> inputTextSize = new List<double>();
			Connection connect = new Connection();
			if (!DA.GetDataList(0, inputText)) return;
			if (!DA.GetDataList(1, inputTextLocations)) return;
			if (!DA.GetDataList(2, inputTextSize)) return;
			if (!DA.GetDataList(3, inputTextColor)) return;
			if (!DA.GetData(4, ref connect)) return;
			// Check inputs.
			if(inputTextLocations.Count != inputText.Count) {
				HoloTag.debugMessages.Add("Component: HoloTag: The number of 'tag locations' and 'tag texts' should be equal.");
				return;
			}
			if ((inputTextSize.Count > 1) && (inputTextSize.Count != inputText.Count)) {
				HoloTag.debugMessages.Add("Component: HoloTag: The number of 'tag text sizes' should be one or equal to one or the number of 'tag texts'.");
				return;
			}
			if ((inputTextColor.Count > 1) && (inputTextColor.Count != inputText.Count)) {
				HoloTag.debugMessages.Add("Component: HoloTag: The number of 'tag text colors' should be one or equal to one or the number of 'tag texts'.");
				return;
			}
            
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
				string currentMessage = string.Empty;
				byte[] bytes = EncodeUtilities.EncodeData("HOLOTAG", tags, out currentMessage);
				if (HoloTag.lastMessage != currentMessage) {
					HoloTag.lastMessage = currentMessage;
					UDPSend.Send(bytes, connect.remoteIP);
					HoloTag.debugMessages.Add("Component: HoloTag: Mesh data sent over UDP.");
				}
			}
            
			// Output.
			// DA.SetData(0, HoloTag.debugMessages[HoloTag.debugMessages.Count-1]);
		}
		//////////////////////////////////////////////////////////////////////////
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
			pManager.AddNumberParameter("Text Size", "TS", "Size of text Tags in AR environment", GH_ParamAccess.list, 20.0);
			pManager.AddColourParameter("Text Color", "C", "Color of text Tags", GH_ParamAccess.list, Color.White);
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