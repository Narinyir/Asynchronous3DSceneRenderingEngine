using System;
using OpenTK;
using System.Globalization;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace MoteurDeStreaming
{
	public static class ObjLoader
	{
		public static void parseObj(string path, string pathMtl, Scene3D s)
		{
			int cmptVertex = 0;
			int cmptObjets = 0;
			string line;
			bool first = true;
			Zone z = new Zone("huge",0);
			Objet3D o = new Objet3D (); 
			System.IO.StreamReader file = new System.IO.StreamReader(path);
			while ((line = file.ReadLine()) != null) {
				if (!line.Equals ("")) {
					if('u'.Equals(line[0]))
					{
						if (line.Contains("usemtl"))
						{
							char[] delimiterChars = {' '};
							string[] positions = line.Split (delimiterChars);
							//o.material = positions[1];
							//readMaterial(pathMtl,positions[1], s);
						}
					}
					// On regarde si on a un nouvel objet
					if ('o'.Equals(line[0]) || 'g'.Equals(line [0])) {
						if(line.Length > 1){
							if (!first) {
								o.Center /= o.VertexCount;
								cmptObjets++;
								z.addObjet(cmptObjets, o);
							}
			
							o = new Objet3D(); 

							first = false;
						}
					}
			
					if ('v'.Equals(line[0])) {
						if('n'.Equals(line[1]))
						{
							// Je sépare la ligne en fonction des espaces
							char[] delimiterChars = {' '};
							string[] positions = line.Split (delimiterChars);
							//Console.WriteLine ("\t" + line);		
							// Je suis obligé de changer les . par des , sinon il n'arrive pas à parser
							Vector4 v = new Vector4 (float.Parse (positions [1]), float.Parse (positions [2]), float.Parse (positions [3]), 1.0f);
							s.addNormal(s.vertexNormals.Count,v);
						}

						else if('t'.Equals(line[1]))
						{
							// Je sépare la ligne en fonction des espaces
							char[] delimiterChars = {' '};
							string[] positions = line.Split (delimiterChars);
							Vector4 v;
							if(positions.Length == 3)
								v = new Vector4 (float.Parse (positions [1]), float.Parse (positions [2]), 0.0f, 1.0f);
							else
								v = new Vector4 (float.Parse (positions [1]), float.Parse (positions [2]), float.Parse (positions [3]), 1.0f);

							s.addTexture(s.textureCoords.Count,v);
						}

						else
						{
							// Je sépare la ligne en fonction des espaces
							char[] delimiterChars = {' '};
							string[] positions = line.Split (delimiterChars);
							//Console.WriteLine ("\t" + line);		
							// Je suis obligé de changer les . par des , sinon il n'arrive pas à parser
							Vector4 v = new Vector4 (float.Parse(positions[1]), float.Parse (positions[2]), float.Parse (positions[3]), 1.0f);
							Vector4 v3 = new Vector4(v.X,v.Y,v.Z, 1);
							tabMinMaxXZScene(v3, s);
							tabMinMaxXZObject(v3, o);
							o.Center += v3;
							o.VertexCount++;
							cmptVertex++;
							s.addVertex(s.vertex.Count, v);
						}
					}

					if ('f'.Equals(line[0])) {
						// Je sépare la ligne en fonction des espaces
						char[] delimiterChars = {' '};
						string[] vertex = line.Split (delimiterChars);
						string[] vertexValues = vertex[1].Split('/');
						FACE f1 = new FACE();
						f1.vertex = new int[vertex.Length-1];
						if(vertexValues.Length > 2) {
							if(!vertexValues[1].Equals(""))
								f1.texture = new int[vertex.Length-1];
							if (!vertexValues[2].Equals(""))
								f1.normals = new int[vertex.Length-1];
						}
						for(int i=1; i<vertex.Length; i++)
						{
							vertexValues = vertex[i].Split('/');
							f1.vertex[i-1] = (int.Parse(vertexValues[0])) -1;
							f1.center += new Vector3(s.vertex[f1.vertex[i-1]]);
							if(vertexValues.Length > 2) {
								if(!vertexValues[1].Equals("")){
									f1.texture[i-1] = (int.Parse(vertexValues[1])) -1;
								}
								if(!vertexValues[2].Equals("")){
									f1.normals[i-1] = (int.Parse(vertexValues[2])) -1;
								}
							}

						}
						f1.center /= vertex.Length-1;
						o.addFace(f1);
					}
				}
			}
			cmptObjets++;
			z.addObjet(cmptObjets, o);
			s.addZone(s.zones.Count, ref z);
			s.SetTaille();
			s._nbrVertex = cmptVertex;
		}
		
		public static void writeObj (string path, Scene3D s)
		{

			string[] lines = {"# Ceci est un commentaire", "# b <xMin> <xMax> <zMin> <zMax>", "# vertice :v <index> <X> <Y> <Z>"
				,"# textureCoord : vt <index> <X> <Y> <Z>","# veticeNormal : vn <index> <X> <Y> <Z>","# face : f <vX/vtX/vnX> <vY/vtY/vnY> <vZ/vtZ/vnZ>",
				"# z <index> <name> # <nbEntry> # pour accelerer la recherche dans le fichier?",
				"# o <centerX> <centerY> <centerZ> # A voir si c'est utile, pas sur. #"
			
			};

			using (StreamWriter sw = new StreamWriter(path))
			{
				// Le blabla juste au dessus
				foreach(string line  in lines) {
					sw.WriteLine(line);
				}

				// On écrit les limites
				sw.WriteLine ("b " + s.bounds.minX +" "+ s.bounds.maxX+" "+ s.bounds.minZ+" " + s.bounds.maxZ);
				foreach(KeyValuePair<int, Zone> z in s.zones) {

					// Juste pour y voir plus clair dans le fichier .zs
					for(int i=0;i<=10;i++) {
						sw.WriteLine(" ");
					}
					// Le nom de chaque zone
					sw.WriteLine ("z "+ z.Key +" "+ z.Value.zoneName);


					// Pour chaque dico de vertexes contenu dans une zone
					foreach(KeyValuePair<int, Vector4> v in z.Value.vertex) {
						sw.WriteLine("v " + v.Key + " " + v.Value.X + " " + v.Value.Y + " " + v.Value.Z);
					}

					foreach(KeyValuePair<int, Vector4> vn in z.Value.normals) {
						sw.WriteLine("vn " + vn.Key + " " + vn.Value.X + " " + vn.Value.Y + " " + vn.Value.Z);
					}

					foreach(KeyValuePair<int, Vector4> vt in z.Value.textures) {
						sw.WriteLine("vt " + vt.Key + " " + vt.Value.X + " " + vt.Value.Y + " " + vt.Value.Z);
					}


					//Pour chaque objet dans chaque zone
					foreach(Objet3D obj in z.Value.GetObjets.Values) {
						sw.WriteLine ("o "+ obj.Center.X + " " + obj.Center.Y + " " + obj.Center.Z);
						//sw.WriteLine ("m "+obj.material);

						// On affiche les vertexes de l'objet
						foreach(FACE f in obj.faces) {											
							sw.Write("f ");
							if(f.normals == null && f.texture == null) {
								for(int i =0; i<f.vertex.Length; i++)
								{
									sw.Write(f.vertex[i]);
									if(i != f.vertex.Length-1)
										sw.Write(" ");
								}
								sw.Write("\n");
							}
							else if(f.normals == null)
							{
								for(int i =0; i<f.vertex.Length; i++)
								{
									sw.Write(f.vertex[i] + "/"+f.texture[i]);
									if(i != f.vertex.Length-1)
										sw.Write(" ");
								}
								sw.Write("\n");
							}
							else if(f.texture == null)
							{
								for(int i =0; i<f.vertex.Length; i++)
								{
									sw.Write(f.vertex[i] + "//"+f.normals[i]);
									if(i != f.vertex.Length-1)
										sw.Write(" ");
								}
								sw.Write("\n");
							}
							else {							
								for(int i =0; i<f.vertex.Length; i++)
								{
									sw.Write(f.vertex[i] + "/"+f.texture[i]+"/"+f.normals[i]);
									if(i != f.vertex.Length-1)
										sw.Write(" ");
								}
								sw.Write("\n");
							}
											
						}


					}
				}
		
			}
				
		}

		public static void objToZs(string input, string output, int taille){
			Scene3D s = new Scene3D();
			ObjLoader.parseObj(Program.pathToModels +".obj",Program.pathToModels + ".mtl", s);
			s.ZoneScene(taille);
			ObjLoader.writeObj(Program.pathToModels +".zs",s);
		}

		/*
		 * tableau des valeurs min et max sur l'axe X et Z
		 * tabMinMaxXZ[0] = minX
		 * tabMinMaxXZ[1] = maxX
		 * tabMinMaxXZ[2] = minZ
		 * tabMinMaxXZ[3] = maxZ
		 */
		public static void tabMinMaxXZScene(Vector4 v, Scene3D s)
		{
			if (v.X < s.bounds.minX)
				s.bounds.minX = v.X;
			if (v.X > s.bounds.maxX)
				s.bounds.maxX = v.X;
			if (v.Z < s.bounds.minZ)
				s.bounds.minZ = v.Z;
			if (v.Z > s.bounds.maxZ)
				s.bounds.maxZ = v.Z;
		}

		public static void tabMinMaxXZObject(Vector4 v, Objet3D o)
		{
			if (v.X < o.bounds.minX)
				o.bounds.minX = v.X;
			if (v.X > o.bounds.maxX)
				o.bounds.maxX = v.X;
			if (v.Z < o.bounds.minZ)
				o.bounds.minZ = v.Z;
			if (v.Z > o.bounds.maxZ)
				o.bounds.maxZ = v.Z;
		}
	}
}

