using System;
using System.Collections.Generic;

namespace HoloFab {
	// Structure to hold Custom data types holding data to be sent.
	namespace CustomData {
		public enum SourceType { TCP, UDP };
		// Custom Mesh item encoding.
		[Serializable]
		public class MeshData {
			public virtual List<float[]> vertices { get; set; }
			public virtual List<int[]> faces { get; set; }
			//public virtual List<float[]> normals { get; set; }
			public virtual List<int[]> colors { get; set; }
            
			public MeshData() {
				this.vertices = new List<float[]>();
				this.faces = new List<int[]>();
				this.colors = new List<int[]>();
			}
		}
		// Custom Tag item encoding.
		[Serializable]
		public struct TagData {
			public List<string> text;
			public List<float[]> textLocation;
			public List<float> textSize;
			public List<int[]> textColor;
            
			public TagData(List<string> _text, List<float[]> _textLocation, List<float> _textSize, List<int[]> _textColor) {
				this.text = _text;
				this.textLocation = _textLocation;
				this.textSize = _textSize;
				this.textColor = _textColor;
			}
		}
		// Cutom UI state encoding.
		[Serializable]
		public class UIData {
			public List<bool> bools;
			public List<int> ints;
			public List<float> floats;
            
			public UIData() {
				this.bools = new List<bool>();
				this.ints = new List<int>();
				this.floats = new List<float>();
			}
			public UIData(List<bool> _bools, List<int> _ints, List<float> _floats) : this() {
				if (_bools.Count > 0) this.bools = _bools;
				if (_ints.Count > 0) this.ints = _ints;
				if (_floats.Count > 0) this.floats = _floats;
			}
		}
	}
}