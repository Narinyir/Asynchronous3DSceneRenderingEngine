using System;
using System.Collections.Generic;
using OpenTK;

namespace MoteurDeStreaming
{
	public struct SCENEBOUNDS
	{
		public float minX;
		public float maxX;
		public float minZ;
		public float maxZ;
	}
	
	public enum PIXEL_FORMAT
	{
		rgb,
		rgba
	}
	
	public struct IMAGE_DATA
	{
		public int userData;
		public System.Drawing.Imaging.BitmapData pixelImage;
		public int width;
		public int height;
		public PIXEL_FORMAT pixelFormat;
	}
	
	public struct MATERIAL
	{
		public string name;
		public float[] ambient;
		public float[] diffuse;
		public float[] specular;
		public float transparency;
		public IMAGE_DATA pDiffuse;
		public int nbUsers;
	}

	public class Scene3D
	{
		public SCENEBOUNDS bounds;
		public Dictionary<int, Zone> zones;
		public Dictionary<int,Vector4> vertex;
		public Dictionary<int,Vector4> vertexNormals;
		public Dictionary<int,Vector4> textureCoords;
		public Dictionary<string,MATERIAL> materials;
		public int _taille;
		public int cmptObjets;
		public object access = 0;
		public int _nbrVertex;

		public Scene3D ()
		{
			vertex = new Dictionary<int, Vector4>();
			vertexNormals = new Dictionary<int, Vector4>();
			textureCoords = new Dictionary<int, Vector4>();
			materials = new Dictionary<string, MATERIAL>();
			zones = new Dictionary<int, Zone>();
			bounds = new SCENEBOUNDS();
			bounds.minX = float.MaxValue;
			bounds.maxX = float.MinValue;
			bounds.maxZ = float.MinValue;
			bounds.minZ = float.MaxValue;
			_taille = 0;
			cmptObjets = 1;
			_nbrVertex = 0;
		}

		public void addZone(int cle, ref Zone z)
		{
			if (!zones.ContainsKey(cle))
				zones.Add(cle, z);
		}

		public void addMaterial (string cle, MATERIAL m)
		{
			if (materials.ContainsKey (cle)) {
				MATERIAL mat = materials [cle];
				mat.nbUsers++;
			}
			else
				materials.Add(cle,m);
		}

		public void addVertex (int cle, Vector4 v)
		{
			if(vertex.ContainsKey(cle))
				vertex[cle] += new Vector4(.0f,.0f,.0f,1.0f);
			else
				vertex.Add(cle,v);
		}

		public void addNormal (int cle, Vector4 v)
		{
			if(vertexNormals.ContainsKey(cle))
				vertexNormals[cle] += new Vector4(.0f,.0f,.0f,1.0f);
			else
				vertexNormals.Add(cle,v);
		}

		public void addTexture (int cle, Vector4 v)
		{
			if(textureCoords.ContainsKey(cle))
				textureCoords[cle] += new Vector4(.0f,.0f,.0f,1.0f);
			else
				textureCoords.Add(cle,v);
		}
		
		public void freeVertex (int cle)
		{
			Vector4 v;
			if (vertex.TryGetValue(cle, out v)) {
				v -= new Vector4 (.0f, .0f, .0f, 1.0f);
				if(v.W <= 0)
				{
					vertex.Remove(cle);
				}
			}
		}

		public void freeNormal (int cle)
		{
			Vector4 vn;
			if (vertex.TryGetValue(cle, out vn)) {
				vn -= new Vector4 (.0f, .0f, .0f, 1.0f);
				if(vn.W <= 0)
				{
					vertexNormals.Remove(cle);
				}
			}
		}

		public void freeTexture (int cle)
		{
			Vector4 vt;
			if (vertex.TryGetValue(cle, out vt)) {
				vt -= new Vector4 (.0f, .0f, .0f, 1.0f);
				if(vt.W <= 0)
				{
					textureCoords.Remove(cle);
				}
			}
		}

		public void freeMaterial (string cle)
		{
			if (materials.ContainsKey (cle)) {
					MATERIAL mat = materials [cle];
					mat.nbUsers--;
				if (mat.nbUsers == 0) {
					materials.Remove (cle);
				}
			}
		}

		public int CalculZone (Vector3 point, int taille)
		{
			int x = (int)((Math.Abs (this.bounds.minX) + Math.Abs (this.bounds.maxX)) / 2);  
			int z = (int)((Math.Abs (this.bounds.minZ) + Math.Abs (this.bounds.maxZ)) / 2);
			int nbrZoneX = (int)((Math.Abs (this.bounds.minX) + Math.Abs (this.bounds.maxX)) / taille) + 1;  
			int zoneX, zoneZ;

			if (point.X < bounds.minX || point.X > bounds.maxX || point.Z < bounds.minZ || point.Z > bounds.maxZ) {
				//Console.WriteLine ("Hors Scene");
				return -1;
			} else {
				if (point.X < 0)
					zoneX = (x - (int)Math.Abs(point.X)) / taille;
				else 
					zoneX = (x + (int)(point.X)) / taille;
				if (point.Z < 0)
					zoneZ = (z - (int)Math.Abs(point.Z)) / taille;
				else
					zoneZ = (z + (int)(point.Z * 2)) / taille;
				
				if (zoneZ == 0)
					return zoneX;
				else
					return zoneZ * nbrZoneX + zoneX;
			}
		}

		public List<int> CalculListZone (Vector3 point, int taille, int rayon)
		{
			List<int> listZone = new List<int> ();
			//listZone.Add (CalculZone (point, taille));
			Vector3 actualPoint = new Vector3 (point.X - rayon, point.Y - rayon, point.Z - rayon);
			float initZ = actualPoint.Z;
			int x = (int)((Math.Abs (point.X - rayon) + Math.Abs (point.X + rayon)) / (taille / 2)) + 1;
			int z = (int)((Math.Abs (point.Z - rayon) + Math.Abs (point.Z + rayon)) / (taille / 2)) + 1;
			int zoneToAdd;
			for (int i = 0; i < x; i++) 
			{
				actualPoint.Z = initZ;
				for (int j = 0; j < z; j++)
				{
					zoneToAdd = CalculZone(actualPoint, taille);
					if (!listZone.Contains(zoneToAdd))
					{
						listZone.Add (zoneToAdd);
					}
					actualPoint.Z += (taille / 2);
				}
				actualPoint.X += (taille / 2);
			}
			return listZone;			    
		}

		public void ZoneScene (int taille)
		{
			Scene3D finalScene = new Scene3D();
			foreach (Objet3D o in this.zones[0].GetObjets.Values) 
			{
				Zone z;
				int cle;
				foreach (FACE f in o.GetFace)
				{
					cle = CalculZone(f.center, taille);
					if(!(finalScene.zones.TryGetValue(cle, out z)))
					{
						z = new Zone("Zone " + cle, cle);
						finalScene.addZone(cle, ref z);
					}
					 
					
					if (finalScene.zones[cle].GetObjets.Values.Count == 0)
					{
						Objet3D newO = new Objet3D();
						newO.material = o.material;
						newO.addFace(f);
						finalScene.zones[cle].addObjet(cmptObjets, newO);
						cmptObjets++;
					}
					else
					{
						Objet3D newO = new Objet3D();
						Boolean newObjet = false;
						foreach (Objet3D underO in finalScene.zones[cle].GetObjets.Values)
						{
							//underO.addFace(f);
							if (o.material == null)
								underO.addFace(f);
							else
							{
								if (underO.material == o.material)
								{
									underO.addFace(f);
								}
								else
								{
									newObjet = true;
									newO.material = o.material;
									newO.addFace(f);
									cmptObjets++;
								}
							}
						}

						if (newObjet)
						{
							finalScene.zones[cle].addObjet(cmptObjets, newO);
						}
					}
					for (int i = 0; i < 3; i++)
					{
						finalScene.zones[cle].addVertex(f.vertex[i], this.vertex[f.vertex[i]]);
						if(f.normals != null)
							finalScene.zones[cle].addNormal(f.normals[i], this.vertexNormals[f.normals[i]]);
						//finalScene.addTexture(f.texture[i], this.textureCoords[f.texture[i]]);
					}
				}
				/*cle = CalculZone(o.Center, taille);
				finalScene.addZone(cle, new Zone("Zone " + cle));
				finalScene.zones[cle].addObjet(cmpt, o);
				cmpt++;*/
			}
			this.zones = finalScene.zones;			
		}

		public void SetTaille ()
		{
			int result = Int16.MinValue;
			foreach (Objet3D o in this.zones[0].GetObjets.Values) {
				if (o.taille > result)
					result = o.taille;
			}
			_taille = result;
		}
	}
}

