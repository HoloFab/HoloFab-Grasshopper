using System;
using Grasshopper.Kernel;

namespace HoloFab {
	public class HoloFabInfo : GH_AssemblyInfo {
		public override string Name {
			get { return "HoloFab"; }
		}
		public override Guid Id {
			get { return new Guid("5cb528d2-f290-4c6e-907f-f5a3030879e6"); }
		}
		//Return a 24x24 pixel bitmap to represent this GHA library.
		public override System.Drawing.Bitmap Icon {
			get { return Properties.Resources.HoloFab_Logo; }
		}
		//Return a short string describing the purpose of this GHA library.
		public override string Description {
			get { return "A library of components instrumental for communicating between Grasshopper and Augmented Reality devices to assist in digital fabrication."; }
		}
		//Return a string identifying you or your company.
		public override string AuthorName {
			get { return "Armin Akbari, Daniil Koshelyuk, Ardeshir Talaei."; }
		}
		//Return a string representing your preferred contact details.
		public override string AuthorContact {
			get { return "holographic.fabrication@gmail.com"; }
		}
	}
}