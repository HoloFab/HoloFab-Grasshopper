// #define DEBUG
#undef DEBUG

﻿using System;
using System.Collections.Generic;

using System.Drawing;
using Rhino.Geometry;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;

using HoloFab.CustomData;

namespace HoloFab {
	// A HoloFab class to create Robot object used in other HoloFab components.
	public class HoloBot : GH_Component {
		//////////////////////////////////////////////////////////////////////////
		// - default settings
		public Plane defaultPlane = Plane.WorldXY;
		// - settings
		private static string[] listRobotNames = new string[] { "Choose Robot", "KR150-2_110", "IRB140", "IRB120" };
		// - debugging
		#if DEBUG
		private string sourceName = "HoloBot Component";
		public List<string> debugMessages = new List<string>();
		#endif
        
		/// <summary>
		/// This is the method that actually does the work.
		/// </summary>
		/// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
		protected override void SolveInstance(IGH_DataAccess DA) {
			// Get inputs.
			string inputRobotName = "";
			Mesh inputEndeffectorMesh = new Mesh();
			int inputRobotID = 0;
			Plane inputPlane = this.defaultPlane;
			if (!DA.GetData(0, ref inputRobotName))
				RobotOptionList();
			DA.GetData(1, ref inputEndeffectorMesh);
			DA.GetData(2, ref inputRobotID);
			DA.GetData(3, ref inputPlane);
			//////////////////////////////////////////////////////
			// Process data.
			inputEndeffectorMesh.Weld(Math.PI);
			EndeffectorData endEffector = new EndeffectorData(MeshUtilities.EncodeMesh(inputEndeffectorMesh));
			RobotData robot = new RobotData(inputRobotID, inputRobotName, endEffector, EncodeUtilities.EncodePlane(inputPlane));
			UniversalDebug("Robot Object Created.");
			//////////////////////////////////////////////////////
			// Output.
			DA.SetData(0, robot);
			#if DEBUG
			DA.SetData(1, this.debugMessages[this.debugMessages.Count-1]);
			#endif
		}
		//////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Initializes a new instance of the HoloBot class.
		/// Each implementation of GH_Component must provide a public
		/// constructor without any arguments.
		/// Category represents the Tab in which the component will appear,
		/// Subcategory the panel. If you use non-existing tab or panel names,
		/// new tabs/panels will automatically be created.
		/// </summary>
		public HoloBot()
			: base("HoloBot", "HB",
			       "Create Holographic Robot",
			       "HoloFab", "HoloBot") {}
		/// <summary>
		/// Provides an Icon for every component that will be visible in the User Interface.
		/// Icons need to be 24x24 pixels.
		/// </summary>
		protected override System.Drawing.Bitmap Icon {
			get { return Properties.Resources.HoloFab_HoloBot; }
		}
		/// <summary>
		/// Gets the unique ID for this component. Do not change this ID after release.
		/// </summary>
		public override Guid ComponentGuid {
			get { return new Guid("b1a4efc7-3211-48a0-aa97-178e7331a486"); }
		}
		/// <summary>
		/// Registers all the input parameters for this component.
		/// </summary>
		protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
			pManager.AddTextParameter("Name", "N", "Name of the robot system.", GH_ParamAccess.item);
			pManager.AddMeshParameter("EndEffector", "E", "EndEffector as a single mesh", GH_ParamAccess.item);
			pManager.AddIntegerParameter("ID", "ID", "An integer number as Robot's ID, you can leave it with the default value if there is only one robot in the scene!", GH_ParamAccess.item, 0);
			pManager.AddPlaneParameter("Plane", "P", "Plane representing Robot's position and orientation", GH_ParamAccess.item, this.defaultPlane);
			//pManager.AddPlaneParameter("TCP", "T", "Robot's Tool Center Point", GH_ParamAccess.item,Plane.WorldXY);
			//pManager.AddIntegerParameter("Marker ID", "M", "", GH_ParamAccess.list);
			pManager[0].Optional = true;
			pManager[1].Optional = true;
			pManager[2].Optional = true;
			pManager[3].Optional = true;
		}
		/// <summary>
		/// Registers all the output parameters for this component.
		/// </summary>
		protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
			pManager.AddGenericParameter("HoloBot", "Hb", "A HoloFab holobographic robot object.", GH_ParamAccess.item);
			#if DEBUG
			pManager.AddTextParameter("Debug", "D", "Debug console.", GH_ParamAccess.item);
			#endif
		}
		////////////////////////////////////////////////////////////////////////
		// Create a drop down to select supported robot if no robot selected.
		private void RobotOptionList() {
			// TODO: check instead if alreeady set.
			// Create robot Option List.
			GH_ValueList robotValueList = new GH_ValueList();
			robotValueList.CreateAttributes();
			robotValueList.Attributes.Pivot = new PointF(this.Attributes.Pivot.X-300, this.Attributes.Pivot.Y-41);
			robotValueList.ListItems.Clear();
			robotValueList.NickName = "Robot";
			// Add supported robot names.
			foreach (string robotName in HoloBot.listRobotNames) {
				GH_ValueListItem item = new GH_ValueListItem(robotName, "\""+robotName+"\"");
				robotValueList.ListItems.Add(item);
			}
			// Update grasshopper document.
			GH_Document document = this.OnPingDocument();
			document.AddObject(robotValueList, false);
			this.Params.Input[0].AddSource(robotValueList);
			this.Params.Input[0].CollectData();
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