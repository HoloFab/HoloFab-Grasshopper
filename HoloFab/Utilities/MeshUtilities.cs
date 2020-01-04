using System;
using System.Collections.Generic;

using Rhino.Geometry;
using System.Drawing;

using HoloFab.CustomData;

namespace HoloFab {
	// Tools for processing meshes.
	public static class MeshUtilities {
		// Encode a Mesh.
		public static MeshData EncodeMesh(Mesh _mesh) {
			MeshData meshData = new MeshData();
            
			for (int i = 0; i < _mesh.Vertices.Count; i++) {
				meshData.vertices.Add(EncodeUtilities.EncodeLocation(_mesh.Vertices[i]));
			}
            
			for (int i = 0; i < _mesh.Faces.Count; i++) {
				if (!_mesh.Faces[i].IsQuad) {
					meshData.faces.Add(new int[] { 0, _mesh.Faces[i].A, _mesh.Faces[i].B, _mesh.Faces[i].C });
				} else {
					meshData.faces.Add(new int[] { 1, _mesh.Faces[i].A, _mesh.Faces[i].B, _mesh.Faces[i].C, _mesh.Faces[i].D });
				}
			}
            
			return meshData;
		}
		// Encode a Mesh with a Color.
		public static MeshData EncodeMesh(Mesh _mesh, Color _color) {
			MeshData meshData = EncodeMesh(_mesh);
			meshData.colors = new List<int[]>() {EncodeUtilities.EncodeColor(_color)};
			return meshData;
		}
	}
}