using System;
using System.Collections.Generic;
using OpenTK;


namespace MoteurDeStreaming
{
	public struct OBJECTBOUNDS
	{
		public float minX;
		public float maxX;
		public float minZ;
		public float maxZ;
	}

	public struct FACE
	{
		public int[] vertex;
		public int[] normals;
		public int[] texture;
		public Vector3 center;
		public bool afficher;
	}

	public class Objet3D
	{
		public OBJECTBOUNDS bounds;

		public List<FACE> faces;

		private int u32VertexCount;
		public Vector4 Center;

		public string material;
		
		public int index;
		
		public Objet3D ()
		{
			faces = new List<FACE>();
			Center = Vector4.Zero;
			u32VertexCount = 0;
			bounds.minX = float.MaxValue;
			bounds.maxX = float.MinValue;
			bounds.maxZ = float.MinValue;
			bounds.minZ = float.MaxValue;
		}

		public Objet3D (Objet3D o)
		{
			this.faces = new List<FACE>(o.faces);
		}

		public void addFace(FACE f)
		{
			faces.Add(f);
		}

		public int VertexCount {
			get{ return u32VertexCount;}
			set{ u32VertexCount = value;}
		}

		public List<FACE> GetFace {
			get { return faces;}
		}

		public int taille 
		{
			get
			{ 
				int x = (int)(Math.Abs(bounds.minX) + Math.Abs(bounds.maxX));
				int z = (int)(Math.Abs(bounds.minZ) + Math.Abs(bounds.maxZ));
				return Math.Max(x, z) + 1;
			}
		}
		
	}
}