using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Threading;
using System.Collections.Generic;


namespace MoteurDeStreaming
{
	public class AsyncLoadingManager
	{
		Scene3D scene;
		bool running;
		bool toLoad;
		
		Stack<List<int>> loadTasks;
		List<int> currLoad;

		Thread oThread;
		string filePath;
		string mtlPath;
		
		public AsyncLoadingManager(ref Scene3D s, string path, string mtl)
		{
			scene = s;
			running = true;
			filePath = path;
			mtlPath = mtl;
			
			initBounds();
			
			loadTasks = new Stack<List<int>>();
			
			oThread = new Thread(new ThreadStart(this.AsyncLoading));
			oThread.Start();
		}
		
		public void AsyncLoading()
		{
			System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");
        	System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
			while (running)
			{
				List<int> load =new List<int>();
				if(loadTasks.Count > 0) {
					load = loadTasks.Peek();
					if(load != null){
						
						List<int> unload = new List<int>();
						foreach(int i in load){
							if(i > 0)
								loadZone(i);
						}

						foreach(Zone Z in scene.zones.Values)
						{
							if(!currLoad.Contains(Z.index))
								unload.Add(Z.index);
						}
					
						foreach(int i in unload){
							unloadZone(i);
						}
					}
					loadTasks.Pop();

				}
			}
		}
		
		public bool initBounds()
		{
			bool Ended = false;
			System.IO.StreamReader file = new System.IO.StreamReader(filePath);
			string line;
			while((line = file.ReadLine()) != null && !Ended)
			{
					if(line.StartsWith("b")){
						string[] boundsParameters = line.Split(' ');
						lock(scene)
						{
							scene.bounds = new SCENEBOUNDS();
							scene.bounds.minX = float.Parse(boundsParameters[1]);
							scene.bounds.maxX = float.Parse(boundsParameters[2]);
							scene.bounds.minZ = float.Parse(boundsParameters[3]);
							scene.bounds.maxZ = float.Parse(boundsParameters[4]);
						}
						Ended = true;
					}
			}
			return Ended;
		}

		public bool loadZone(int index){
			Zone zone;
			bool Ended = false;
			bool founded = false;
			if (!scene.zones.TryGetValue(index, out zone)){
				//Console.Write("loading zone" + index + "\n");
				string line;
				bool first = true;
				int cmptObjets = 0;
				System.IO.StreamReader file = new System.IO.StreamReader(filePath);
				while((line = file.ReadLine()) != null && !Ended)
				{
					if(line.StartsWith("z") && line.Contains(Convert.ToString(index))){
						founded = true;
						string[] zoneParams = line.Split(' ');
						zone = new Zone(zoneParams[2] + index, index);
						Objet3D o = new Objet3D();
						line = file.ReadLine();
						while(!Ended)
						{
							if(line == null)
								Ended = true;
							else {
								if (line.StartsWith("o") || line.StartsWith("g")) {
									if(line.Length > 1){
										if (!first) {
											cmptObjets++;
											zone.addObjet(cmptObjets, o);
										}
										
										o = new Objet3D(); 
										first = false;
									}
								}
								
								if (line.StartsWith("m")) {
									string[] mParams = line.Split(' ');
									readMaterial(mtlPath, mParams[1]);
									o.material = mParams[1];
								}
				
								if (line.StartsWith("v")) {
									
									char[] delimiterChars = {' '};
									string[] positions = line.Split (delimiterChars);
									Vector4 v = new Vector4 (float.Parse (positions [2]), float.Parse (positions [3]), float.Parse (positions [4]), 1.0f);
									
									if(line.StartsWith ("vn"))
										scene.addNormal(int.Parse(positions[1]),v);
				
									else if(line.StartsWith ("vt"))
										scene.addTexture(int.Parse (positions[1]),v);
									
									else
										scene.addVertex(int.Parse (positions[1]),v);
		
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
										f1.vertex[i-1] = (int.Parse(vertexValues[0]));

										if(vertexValues.Length > 2) {
											if(!vertexValues[1].Equals("")){
												f1.texture[i-1] = (int.Parse(vertexValues[1]));
											}
											if(!vertexValues[2].Equals("")){
												f1.normals[i-1] = (int.Parse(vertexValues[2]));
											}
										}
									}
									o.addFace(f1);
								}
								
								if(line.StartsWith("z"))
									Ended = true;

								line = file.ReadLine();
							}
							
						}
						Ended = true;
						zone.addObjet(cmptObjets,o);
					}
				}
				if(!founded)
				{
					zone = new Zone("zone"+index,index);
				}
				lock (scene.access)
						scene.addZone(index, ref zone);
			}
			return Ended;
		}

		public void unloadZone(int index){

				Zone z;
				if(scene.zones.TryGetValue(index, out z))
				{
					lock (z.thisLock)
					{
						//Console.Write("unloading zone" + index + "\n");
						foreach(Objet3D o in scene.zones[index].GetObjets.Values)
						{
							foreach(FACE f in o.faces)
							{
								for(int i = 0; i<3; i++)
								{
									if(f.vertex != null)
										scene.freeVertex(f.vertex[i]);
									if(f.normals != null)
										scene.freeNormal(f.normals[i]);
									if(f.texture != null)
										scene.freeTexture(f.texture[i]);
								}
							}
						}
						scene.zones.Remove(index);
					}
					//Console.Write("zone " + index + "unloaded\n");
				}
		}
		
		public void readMaterial (string path, string matName)
		{
			MATERIAL m;
			if(scene.materials.TryGetValue(matName, out m)){
				m.nbUsers++;	
			}
			else {
				System.IO.StreamReader mtl = new System.IO.StreamReader(path);
				string lineMtl;
				bool Ended = false;
				m = new MATERIAL();
				while((lineMtl = mtl.ReadLine()) != null || !Ended)
				{
					if(lineMtl.StartsWith("newmtl") && lineMtl.Contains(matName)){
						lineMtl = mtl.ReadLine();
						while(!Ended)
						{
							if(lineMtl == null)
							{
								Ended = true;
							}
							else {
									if(lineMtl.StartsWith("Kd"))
									{
										m.diffuse = new float[3];
										char[] delimiterChars = {' '};
										string[] positions = lineMtl.Split(delimiterChars);
										m.diffuse[0] = float.Parse (positions[1]);
										m.diffuse[1] = float.Parse (positions[2]);
										m.diffuse[2] = float.Parse (positions[3]);
									}
									
									if(lineMtl.StartsWith("Ka"))
									{
										m.ambient = new float[3];
										char[] delimiterChars = {' '};
										string[] positions = lineMtl.Split(delimiterChars);
										m.ambient[0] = float.Parse (positions[1]);
										m.ambient[1] = float.Parse (positions[2]);
										m.ambient[2] = float.Parse (positions[3]);
									}
									
									if(lineMtl.StartsWith("Ks"))
									{
										m.specular = new float[3];
										char[] delimiterChars = {' '};
										string[] positions = lineMtl.Split(delimiterChars);
										m.specular[0] = float.Parse (positions[1]);
										m.specular[1] = float.Parse (positions[2]);
										m.specular[2] = float.Parse (positions[3]);
									}
									
									if(lineMtl.StartsWith("d"))
									{
										char[] delimiterChars = {' '};
										string[] positions = lineMtl.Split(delimiterChars);
										m.transparency = float.Parse (positions[1]);
										
									}
									if(lineMtl.StartsWith("map_Kd"))
									{
										string[] param = lineMtl.Split(' ');
										/*Bitmap bm = new Bitmap("../../blender/" +param[1]);
										m.pDiffuse.pixelImage = 
										bm.LockBits(
			            				new System.Drawing.Rectangle(0,0,bm.Width,bm.Height),
			           					System.Drawing.Imaging.ImageLockMode.ReadOnly,
			            				System.Drawing.Imaging.PixelFormat.Format24bppRgb
	        							);*/
									}
									if( lineMtl.Contains("newmtl"))
										Ended = true;

									lineMtl = mtl.ReadLine();
								}



							}
							m.name = matName;
						}
				}
				scene.addMaterial(matName,m);
			}
		}
		
		public void initTexture(MATERIAL m) {
			m.pDiffuse.userData = GL.GenTexture();
			GL.BindTexture(TextureTarget.Texture2D,m.pDiffuse.userData);
			GL.TexEnv(TextureEnvTarget.TextureEnv,TextureEnvParameter.TextureEnvMode,(int)TextureEnvMode.Modulate);
			GL.TexParameter(TextureTarget.Texture2D,TextureParameterName.TextureMinFilter,(int)All.Linear);
			GL.TexParameter(TextureTarget.Texture2D,TextureParameterName.TextureMagFilter,(int)All.Linear);
			
			if(m.pDiffuse.pixelFormat.Equals(PIXEL_FORMAT.rgb)) {
				GL.TexImage2D(TextureTarget.Texture2D,0,PixelInternalFormat.Three,m.pDiffuse.width,m.pDiffuse.height,0,PixelFormat.Rgb,PixelType.UnsignedByte,m.pDiffuse.pixelImage.Scan0);
			}
			else {
				GL.TexImage2D(TextureTarget.Texture2D,0,PixelInternalFormat.Three,m.pDiffuse.width,m.pDiffuse.height,0,PixelFormat.Rgba,PixelType.UnsignedByte,m.pDiffuse.pixelImage.Scan0);
			}
			
		}
		
		public bool run {
			set{running = value;}
		}

		public List<int> Load {
			set
			{
					loadTasks.Push(value);
					currLoad = value;
			}
		}
	}
}
