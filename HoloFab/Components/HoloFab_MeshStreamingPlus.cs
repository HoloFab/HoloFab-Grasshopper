using System;
using System.Collections.Generic;

using System.Drawing;
using Rhino.Geometry;
using Grasshopper.Kernel;
using Newtonsoft.Json;

using HoloFab.CustomData;

namespace HoloFab
{
    // A HoloFab class to send mesh to AR device via UDP.
    public class MeshStreamingPlus : GH_Component
    {
        //////////////////////////////////////////////////////////////////////////
        // - history
        public static List<string> debugMessages = new List<string>();
        private static string lastMessage = string.Empty;
        // - settings
        private static Color defaultColor = Color.Red;

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Get inputs.
            List<Mesh> inputMeshes = new List<Mesh> { };
            List<Color> inputColor = new List<Color> { };
            Connection connect = new Connection();
            if (!DA.GetDataList(0, inputMeshes)) return;
            if (!DA.GetDataList(1, inputColor)) return;
            if (!DA.GetData(2, ref connect)) return;
            // Check inputs.
            if ((inputColor.Count > 1) && (inputColor.Count != inputMeshes.Count))
            {
                MeshStreamingPlus.debugMessages.Add("Component: MeshStreamingPlus: The number of Colors should be one or equal to the number of Mesh objects.");
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                    inputColor.Count > inputMeshes.Count ?
                    "The number of Colors does not match the number of Mesh objects. Extra colors will be ignored." :
                    "The number of Colors does not match the number of Mesh objects. The last color will be repeated.");
            }

            // Process data.
            if (connect.status)
            {
                // If connection open start acting.

                // Encode mesh data.
                List<MeshData> inputMeshData = new List<MeshData> { };
                for (int i = 0; i < inputMeshes.Count; i++)
                {
                    Color currentColor = inputColor[Math.Min(i, inputColor.Count)];
                    inputMeshData.Add(MeshUtilities.EncodeMesh(inputMeshes[i], currentColor));
                }
                // Send mesh data.
                string currentMessage = string.Empty;
                byte[] bytes = EncodeUtilities.EncodeData("MESHSTREAMINGPLUS", inputMeshData, out currentMessage);
                if (MeshStreamingPlus.lastMessage != currentMessage)
                {
                    MeshStreamingPlus.lastMessage = currentMessage;
                    UDPSend.Send(bytes, connect.remoteIP);
                    MeshStreamingPlus.debugMessages.Add("Component: MeshStreamingPlus: Mesh data sent over UDP.");
                }
            }
            else
            {
                MeshStreamingPlus.lastMessage = string.Empty;
                MeshStreamingPlus.debugMessages.Add("Component: MeshStreamingPlus: Set 'Send' on true in HoloFab 'HoloConnect'.");
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Component: MeshStreaming: Set 'Send' on true in HoloFab 'HoloConnect'.");
            }

            // Output.
            // DA.SetData(0, MeshStreamingPlus.debugMessages[MeshStreamingPlus.debugMessages.Count-1]);
        }
        //////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Initializes a new instance of the MeshStreamingPlus class.
        /// Each implementation of GH_Component must provide a public
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear,
        /// Subcategory the panel. If you use non-existing tab or panel names,
        /// new tabs/panels will automatically be created.
        /// </summary>
        public MeshStreamingPlus()
            : base("Mesh Streaming Plus", "MS+",
                   "A faster real-time 3D-data streamer, appropriate for Iterative visualisation, It can transport meshes up to 1300 faces and vertices in total",
                   "HoloFab", "Main")
        { }
        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get { return Properties.Resources.HoloFab_MeshStreamingPlus; }
        }
        /// <summary>
        /// Each component must have a unique Guid to identify it.
        /// It is vital this Guid doesn't change otherwise old ghx files
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("460c42df-46bf-4b0b-bde9-732d79a14317"); }
        }
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh object to be encoded and sent via UDP.", GH_ParamAccess.list);
            pManager.AddColourParameter("Color", "C", "Color for each Mesh object.", GH_ParamAccess.list, MeshStreamingPlus.defaultColor);
            pManager.AddGenericParameter("Connect", "Cn", "Connection object from Holofab 'Create Connection' component.", GH_ParamAccess.item);
        }
        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            //pManager.AddTextParameter("Debug", "D", "Debug console.", GH_ParamAccess.item);
        }
    }
}