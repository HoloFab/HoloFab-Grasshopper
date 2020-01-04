using System;
using System.Collections.Generic;

using Grasshopper.Kernel;

using HoloFab.CustomData;

namespace HoloFab {
	// A HoloFab class to create Connection object used in other HoloFab components.
	public class HoloConnect : GH_Component {
		//////////////////////////////////////////////////////////////////////////
		// - settings
		private static string defaultIP = "127.0.0.1";
		/// <summary>
		/// This is the method that actually does the work.
		/// </summary>
		/// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
		protected override void SolveInstance(IGH_DataAccess DA) {
			// Get inputs.
			string remoteIP = HoloConnect.defaultIP;
			bool status = false;
			if (!DA.GetData(0, ref remoteIP)) return;
			if (!DA.GetData(1, ref status)) return;
            
			// Process data.
			Connection connect = new Connection(remoteIP, status);
            
			// Output.
			DA.SetData(0, connect);
		}
		//////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Initializes a new instance of the CreateConnection class.
		/// Each implementation of GH_Component must provide a public
		/// constructor without any arguments.
		/// Category represents the Tab in which the component will appear,
		/// Subcategory the panel. If you use non-existing tab or panel names,
		/// new tabs/panels will automatically be created.
		/// </summary>
		public HoloConnect()
			: base("Create Connection", "C",
			       "Sets the Ip address of receiver",
			       "HoloFab", "Communication") {}
		/// <summary>
		/// Provides an Icon for every component that will be visible in the User Interface.
		/// Icons need to be 24x24 pixels.
		/// </summary>
		protected override System.Drawing.Bitmap Icon {
			get { return Properties.Resources.HoloFab_Logo; }
		}
		/// <summary>
		/// Each component must have a unique Guid to identify it.
		/// It is vital this Guid doesn't change otherwise old ghx files
		/// that use the old ID will partially fail during loading.
		/// </summary>
		public override Guid ComponentGuid {
			get { return new Guid("54aac8c6-1cfa-43e2-9a0c-0f5c3422146c"); }
		}
		/// <summary>
		/// Registers all the input parameters for this component.
		/// </summary>
		protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
			pManager.AddTextParameter("Address", "@", "Remote IP address of the AR device.", GH_ParamAccess.item, HoloConnect.defaultIP);
			pManager.AddBooleanParameter("Send", "S", "Status of the connection.", GH_ParamAccess.item, false);
		}
		/// <summary>
		/// Registers all the output parameters for this component.
		/// </summary>
		protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
			pManager.AddGenericParameter("Connect", "Cn", "Connection object to be used in other HoloFab components.", GH_ParamAccess.list);
		}
	}
}