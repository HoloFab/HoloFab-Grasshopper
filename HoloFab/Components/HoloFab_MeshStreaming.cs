using System;
using System.Collections.Generic;

using System.Drawing;
using Rhino.Geometry;
using Grasshopper.Kernel;
using Newtonsoft.Json;

using HoloFab.CustomData;
using System.Windows.Forms;

namespace HoloFab
{
    // A HoloFab class to send mesh to AR device via TCP.
    public class MeshStreaming : GH_Component
    {
        public static List<string> debugMessages = new List<string>();
        private string lastMessage = string.Empty;
        private static Color defaultColor = Color.Red;
        bool protocolIsTCP = true;

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
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                    inputColor.Count > inputMeshes.Count ?
                    "The number of Colors does not match the number of Mesh objects. Extra colors will be ignored." :
                    "The number of Colors does not match the number of Mesh objects. The last color will be repeated.");
            }
            // If connection open start acting.
            if (connect.status)
            {
                // Encode mesh data.
                List<MeshData> inputMeshData = new List<MeshData> { };
                for (int i = 0; i < inputMeshes.Count; i++)
                {
                    Color currentColor = inputColor[Math.Min(i, inputColor.Count)];
                    inputMeshData.Add(MeshUtilities.EncodeMesh(inputMeshes[i], currentColor));
                }
                // Send mesh data.
                string currentMessage = string.Empty;
                byte[] bytes = EncodeUtilities.EncodeData("MESHSTREAMING", inputMeshData, out currentMessage);
                if (this.lastMessage != currentMessage)     // Is it required?
                {
                    this.lastMessage = currentMessage;
                    if (this.protocolIsTCP)
                    {
                        string result = connect.tcp.Send(bytes);
                        if (result != "Sent")
                        {
                            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Sending failed. Check the connection and try again. \n" + result);
                            return;
                        }
                        this.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, result);
                    }
                    else
                    {
                        UDPSend.Send(bytes, connect.remoteIP);
                    }
                }
            }
            else
            {
                this.lastMessage = string.Empty;
                MeshStreaming.debugMessages.Add("Component: MeshStreaming: Set 'Send' on true in HoloFab 'HoloConnect'.");
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Set 'Send' on true in HoloFab 'HoloConnect'.");
            }

            // Output.
            // DA.SetData(0, MeshStreaming.debugMessages[MeshStreaming.debugMessages.Count-1]);
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
                   "HoloFab", "Main")
        {
            this.Message = "TCP";
        }
        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get { return Properties.Resources.HoloFab_MeshStreaming; }
        }
        /// <summary>
        /// Each component must have a unique Guid to identify it.
        /// It is vital this Guid doesn't change otherwise old ghx files
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("86810363-cded-40bc-ad04-0d6a5b484b02"); }
        }
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh object to be encoded and sent via TCP.", GH_ParamAccess.list);
            pManager.AddColourParameter("Color", "C", "Color for each Mesh object.", GH_ParamAccess.list, MeshStreaming.defaultColor);
            pManager.AddGenericParameter("Connect", "Cn", "Connection object from Holofab 'Create Connection' component.", GH_ParamAccess.item);
        }
        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            // pManager.AddTextParameter("Debug", "D", "Debug console.", GH_ParamAccess.item);
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalComponentMenuItems(menu);
            Menu_AppendSeparator(menu);
            Menu_AppendItem(menu, "Change Protocol(TCP/UDP)", SwitchProtocol, true);
        }

        private void SwitchProtocol(object sender, EventArgs e)
        {
            this.protocolIsTCP = !this.protocolIsTCP;
            if (this.protocolIsTCP)
            {
                this.Message = "TCP";
            }
            else
            {
                this.Message = "UDP";
            }
            Grasshopper.Instances.RedrawCanvas();
        }
    }
}