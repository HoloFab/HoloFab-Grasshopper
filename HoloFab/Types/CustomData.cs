using System;
using System.Collections.Generic;

namespace HoloFab {
	// Structure to hold Custom data types holding data to be sent.
	namespace CustomData {
		// Custom Mesh item encoding.
		[Serializable]
		public class MeshData {
			public virtual IList<float[]> vertices { get; set; }
			public virtual IList<int[]> faces { get; set; }
			//public virtual IList<float[]> normals { get; set; }
			public virtual IList<int[]> colors { get; set; }
            
			public MeshData() {
				this.vertices = new List<float[]>();
				this.faces = new List<int[]>();
				this.colors = new List<int[]>();
			}
		}
		// Custom Tag item encoding.
		[Serializable]
		public struct TagData {
			public IList<string> text;
			public IList<float[]> textLocation;
			public IList<float> textSize;
			public IList<int[]> textColor;
            
			public TagData (IList<string> _text, IList<float[]> _textLocation, IList<float> _textSize, IList<int[]> _textColor) {
				this.text = _text;
				this.textLocation = _textLocation;
				this.textSize = _textSize;
				this.textColor = _textColor;
			}
		}
		// Cutom UI state encoding.
		[Serializable]
		public class UIData {
			public IList<bool> bools;
			public IList<int> ints;
			public IList<float> floats;
            
			public UIData() {
				this.bools = new List<bool>();
				this.ints = new List<int>();
				this.floats = new List<float>();
			}
			public UIData(IList<bool> _bools, IList<int> _ints, IList<float> _floats) : this() {
				if (_bools.Count > 0) this.bools = _bools;
				if (_ints.Count > 0) this.ints = _ints;
				if (_floats.Count > 0) this.floats = _floats;
			}
		}
	}
}