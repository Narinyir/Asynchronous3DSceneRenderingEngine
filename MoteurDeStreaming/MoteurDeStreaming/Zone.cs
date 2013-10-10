using System;
using System.Collections.Generic;
using OpenTK;

namespace MoteurDeStreaming
{
	public class Zone
	{
		public int acces = 0;
		public Dictionary<int, Vector4> vertex;
		public Dictionary<int, Vector4> normals;
		public Dictionary<int, Vector4> textures;
		public Dictionary<int, Objet3D> objets;
		public Object thisLock = new Object();
		public int index;
		
		public String zoneName;
		
		public Zone(string name, int cle)
		{
			objets = new Dictionary<int, Objet3D>();
			vertex = new Dictionary<int, Vector4>();
			normals = new Dictionary<int, Vector4>();
			textures = new Dictionary<int, Vector4>();
			zoneName = name;
			index = cle;
		}

		public void addVertex (int cle, Vector4 v)
		{
			if (!vertex.ContainsKey(cle))
			    vertex.Add(cle, v);
		}

		public void addNormal (int cle, Vector4 v)
		{
			if (!normals.ContainsKey(cle))
				normals.Add(cle, v);
		}

		public void addTexture (int cle, Vector4 v)
		{
			if (!textures.ContainsKey(cle))
				textures.Add(cle, v);
		}
		
		public void addObjet (int cle, Objet3D o)
		{
			if (!objets.ContainsKey(cle))
				objets.Add (cle, o);
		}

		public Dictionary<int, Objet3D> GetObjets {
			get{ return objets;}
		}
	}
}

