using System;
using System.Collections.Generic;
using System.Drawing;

using Grasshopper.Kernel;

using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel.Attributes;

using HoloFab.CustomData;
using System.Net;
using System.Net.Sockets;

namespace HoloFab
{
    // A HoloFab class to create Connection object used in other HoloFab components.
    public class HoloConnect : GH_Component
    {
        //////////////////////////////////////////////////////////////////////////
        // - settings
        public bool status = false;
        private static string defaultIP = "127.0.0.1";
        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Get inputs.
            string remoteIP = HoloConnect.defaultIP;
            if (!DA.GetData(0, ref remoteIP)) return;

            TCPSend tcp = new TCPSend();

            if (this.status)
            {
                // Start TCP
                if (!tcp.connect(remoteIP))
                {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Connection failed, please check your network connection and try again.");
                    return;
                }
                else
                {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Connection established.");
                }
            }
            else
            {
                tcp.disconnect();
            }

            // Process data.
            Connection connect = new Connection(remoteIP, status, tcp);

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
                   "HoloFab", "Communication")
        { }

        public override void CreateAttributes()
        {
            m_attributes = new Attributes_Custom(this);
        }
        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get { return Properties.Resources.HoloFab_Logo; }
        }
        /// <summary>
        /// Each component must have a unique Guid to identify it.
        /// It is vital this Guid doesn't change otherwise old ghx files
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("54aac8c6-1cfa-43e2-9a0c-0f5c3422146c"); }
        }
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Address", "@", "Remote IP address of the AR device.", GH_ParamAccess.item, HoloConnect.defaultIP);
        }
        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Connect", "Cn", "Connection object to be used in other HoloFab components.", GH_ParamAccess.list);
        }
    }



    public class Attributes_Custom : Grasshopper.Kernel.Attributes.GH_ComponentAttributes
    {
        HoloConnect comp;
        public Attributes_Custom(GH_Component owner) : base(owner)
        {
            comp = Owner as HoloConnect;
        }

        protected override void Layout()
        {
            base.Layout();

            System.Drawing.Rectangle rec0 = GH_Convert.ToRectangle(Bounds);
            rec0.Height += 22;

            System.Drawing.Rectangle rec1 = rec0;
            System.Drawing.Rectangle rec2 = rec0;
            rec1.Y = rec1.Bottom - 22;
            rec2.Y = rec2.Bottom;
            rec2.X -= 25;

            rec1.Height = 22;
            rec2.Height = 33;
            rec2.Width += 50;

            rec1.Inflate(-2, -2);
            rec2.Inflate(-2, -2);


            Bounds = rec0;
            ButtonBounds = rec1;
            TextBounds = rec2;
        }
        private System.Drawing.Rectangle ButtonBounds { get; set; }
        private System.Drawing.Rectangle TextBounds { get; set; }

        protected override void Render(GH_Canvas canvas, System.Drawing.Graphics graphics, GH_CanvasChannel channel)
        {
            base.Render(canvas, graphics, channel);

            if (channel == GH_CanvasChannel.Objects)
            {
                IPAddress ipv4Addresse = Array.FindLast(
                    Dns.GetHostEntry(string.Empty).AddressList,
                    a => a.AddressFamily == AddressFamily.InterNetwork);
                GH_Capsule button = GH_Capsule.CreateTextCapsule(ButtonBounds, ButtonBounds, GH_Palette.Black, comp.status ? "Disconnect" : "Connect", 2, 0);
                button.Render(graphics, Selected, Owner.Locked, false);
                graphics.DrawString("Machine IP:\n" + ipv4Addresse.ToString(), GH_FontServer.NewFont(FontFamily.GenericMonospace, 6, FontStyle.Regular), Brushes.Black, TextBounds, GH_TextRenderingConstants.CenterCenter);
                button.Dispose();
            }
        }
        public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                System.Drawing.RectangleF rec = ButtonBounds;
                if (rec.Contains(e.CanvasLocation))
                {
                    comp.status = !comp.status;
                    comp.ExpireSolution(true);
                    return GH_ObjectResponse.Handled;
                }
            }
            return base.RespondToMouseDown(sender, e);
        }
    }
}