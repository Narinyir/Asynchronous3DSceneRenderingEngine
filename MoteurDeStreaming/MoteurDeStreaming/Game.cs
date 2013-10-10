using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Collections.Generic;
using QuickFont;



namespace MoteurDeStreaming
{
	class Program : GameWindow
	{
		float DEG2RAD = 3.14159f/180;
		/*3D Scene*/
			Scene3D scene;
			int precZone;
			Zone[] currentZones;
			AsyncLoadingManager alm;
			
		/*Text arguments*/
			QFont mainText;
			int cmptVertexAfficher;
			int cmptVertexZone;

		/*Camera Arguments*/
			Camera camera;
			float _nc;
			float _fc;
			int _angle;
			int _ratio;
			int cmptPremierPassage;

		/*Loading parameters*/
			public static string pathToModels = @"../../../blender/";
			int radius = 20;
			int prev;
			static int splitSize;

		/*Control Parameters */
			bool drawRadius = false;
			bool drawZones = true;
			bool useFrustrum = false;
			bool drawText = false;

		public Program() : base(1024, 768)
		{
			scene = new Scene3D();
			InitCamera();

		
			glEnables();

			FontInit();

			
			alm = new AsyncLoadingManager(ref scene, pathToModels +".zs", pathToModels + ".mtl");
			//camera.Location = new Vector3(scene.bounds.minX + scene.bounds.maxX/2, 0.0f, scene.bounds.minZ + scene.bounds.maxZ/2);
			
			int val = scene.CalculZone(camera.Location, splitSize);
			if(val != precZone)
			{
				Console.Write("zone " + val + "\n");
			}
			precZone = val;
			
			Console.WriteLine(scene.bounds.minX + " " + scene.bounds.maxX + " " + scene.bounds.minZ + " " + scene.bounds.maxZ);
			precZone = 0;

		}
		
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			Mouse.Move += new EventHandler<MouseMoveEventArgs>(OnMouseMove);
		}
		
		void OnMouseMove(object sender, MouseMoveEventArgs e)
		{
			camera.MouseDelta = new Vector3(e.XDelta, e.YDelta, Mouse.Wheel - prev);
		}

		void InitCamera ()
		{
			camera = new Camera();
			_nc = 0.5f;
			_fc = 2560f;
			_angle = 50;
			_ratio = 1;
		}
		

		protected override void OnRenderFrame (FrameEventArgs e)
		{
			base.OnRenderFrame (e);
			GL.MatrixMode (MatrixMode.Modelview);
			GL.Clear (ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			GL.LoadMatrix (ref camera.cameraMatrix);
			if (useFrustrum) {
				camera.drawFrustumPoints ();
				camera.drawFrustumLines ();
			}
			//camera.drawFrustumPlanes();


			if(drawZones)
				RenderZones();
			if(drawRadius)
				RenderRadius();
			if(drawText)
				renderTexts ();

			SwapBuffers();
		}

		private void RenderZones ()
		{
			GL.PushMatrix ();	
			cmptVertexAfficher = 0;
			cmptVertexZone = 0;
			MATERIAL m;
			foreach (Zone z in currentZones)
			{
				if(z != null) {
					lock (z.thisLock)
					{
						foreach(Objet3D o in z.GetObjets.Values) {
							foreach(FACE f in o.faces) {
								GL.Begin(BeginMode.TriangleFan);
								if(o.material != null) {
									if(scene.materials.TryGetValue(o.material, out m)) {
										if(m.diffuse != null) 
											GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Diffuse,m.diffuse);
										if(m.specular != null)
											GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Specular,m.specular);
										if(m.ambient != null)
											GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Ambient,m.ambient);
									}
								}
								else{
									GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Diffuse,Color.White);
									GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Specular,1.0f);
									GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Ambient,1.0f);
								}
								cmptVertexZone += f.vertex.Length;
								for(int i = 0; i<f.vertex.Length; i++) {
									Vector4 v;
									if(scene.vertex.TryGetValue(f.vertex[i], out v))
									{
										if(useFrustrum)
										{
											if(camera.PointInFrustum(v)){
												if(scene.vertexNormals.Count > 0) {
													Vector4 vn;
													if(scene.vertexNormals.TryGetValue(f.normals[i], out vn))
														GL.Normal3(vn.X,vn.Y,vn.Z);
												}

												GL.Vertex3(v.X,v.Y,v.Z);
												cmptVertexAfficher++;
											}
										}
										else
										{
											if(scene.vertexNormals.Count > 0) {
												Vector4 vn;
												if(scene.vertexNormals.TryGetValue(f.normals[i], out vn))
													GL.Normal3(vn.X,vn.Y,vn.Z);
											}
											
											GL.Vertex3(v.X,v.Y,v.Z);
											cmptVertexAfficher++;
										}
									}
								}
								GL.End();
							}
						}
					}
				}
			}
			GL.PopMatrix();
		}

		private void RenderRadius ()
		{
			GL.PushMatrix();
			GL.Begin(BeginMode.LineLoop);

				for (int i=0; i <360; i++)
				{
					float degInRad = i*DEG2RAD;
				GL.Vertex3(camera.Location + new Vector3((float)Math.Cos(degInRad)*radius,-10.0f,(float)Math.Sin(degInRad)*radius));
				}
				
				GL.End();
			GL.PopMatrix();
		}
		
		protected override void OnUpdateFrame (FrameEventArgs e)
		{

			Vector3 cam = camera.MouseDelta;
			cam.Z = Mouse.Wheel - prev;
			camera.MouseDelta = cam;

			prev = Mouse.Wheel;
			if (Keyboard [Key.Escape]) {
				alm.run = false;
				Exit();
			}

			CameraUpdate();
			
			UpdateZones();
			if(useFrustrum)
				camera.CalculFrustum(_nc, _fc, _angle, _ratio);
			//Frustum();
			cmptPremierPassage++;
		}
		
		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			
			GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
			
			Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, Width / (float)Height, _nc, _fc);
			GL.MatrixMode(MatrixMode.Projection);
			GL.LoadMatrix(ref projection);
				camera.CalculFrustum(_nc, _fc, _angle, _ratio);
				//if (cmptPremierPassage == 1)
				//	Frustum();
		}

		private void CameraUpdate()
		{
			if (Keyboard[Key.Z] || Keyboard[Key.S] || Keyboard[Key.Q] || Keyboard[Key.D])
				//Console.WriteLine("Vertex affiche : " + cmptVertexAfficher + " , Vertex des zones : " + cmptVertexZone + ", Vertex total de la scene : " + scene._nbrVertex);

			if (Keyboard[Key.Z])
				camera.GoForward();
			
			if (Keyboard[Key.S])
				camera.GoBackward();
			
			if (Keyboard[Key.Q])
				camera.GoLeft();
			
			if (Keyboard[Key.D])
				camera.GoRight();
			if (camera.MouseDelta.Z > 0)
				camera.zoom();
			if (camera.MouseDelta.Z < 0)
				camera.dezoom();
			if(Keyboard[Key.ControlLeft])
				radius++;
			if(Keyboard[Key.ShiftLeft])
				radius--;
			if(Keyboard[Key.R])
				drawRadius = true;
			if(Keyboard[Key.T])
				drawRadius = false;
			if(Keyboard[Key.O])
				drawZones = drawZones? false : true;
			if(Keyboard[Key.P])
				Console.WriteLine(camera.Location.X + " " + camera.Location.Z);
			if(Keyboard[Key.F])
				useFrustrum =  true;
			if(Keyboard[Key.G])
				useFrustrum = false;
			if(Keyboard[Key.H])
				drawText =  true;
			if(Keyboard[Key.J])
				drawText = false;
				
			
			camera.Update();
		}
		
		private void UpdateZones()
		{
			List<int> loadList;
			lock(scene.access){
				currentZones = new Zone[scene.zones.Count];
				scene.zones.Values.CopyTo(currentZones,0);
				loadList = scene.CalculListZone(camera.Location,splitSize,radius);
			}
			
			int val = scene.CalculZone(camera.Location,splitSize);
			if(val != precZone){
				Console.Write("zone " + val + "\n");
				Console.WriteLine(camera.Location.X + " " + camera.Location.Z);

			
				alm.Load = loadList;
				
			}

			
			precZone = val;
		}
		
			private void Frustum ()
			{
				for (int i = 0; i < currentZones.Length; i++) 
				{
					if(currentZones[i] != null) 
					{
						lock (currentZones[i].thisLock)
						{
							for (int j = 0; j < currentZones[i].GetObjets.Values.Count; j++)
							{
								for (int k = 0; k < currentZones[i].GetObjets[j].GetFace.Count; k++)
								{
									for (int l = 0; l < 3; l++)
									{
										if (camera.PointInFrustum(scene.vertex[currentZones[i].GetObjets[j].faces[k].vertex[l]]))
										{
											FACE temp = new FACE();
											temp = currentZones[i].GetObjets[j].faces[k];
											temp.afficher = true;
											currentZones[i].GetObjets[j].faces[k] = temp;
										}
										else
										{
											FACE temp = new FACE();
											temp = currentZones[i].GetObjets[j].faces[k];
											temp.afficher = false;
											currentZones[i].GetObjets[j].faces[k] = temp;
										}
									}
								}
							}
						}
					}
				}
			}



		private void glEnables(){
			GL.Enable(EnableCap.DepthTest);
			GL.Enable(EnableCap.Lighting);
			GL.Enable(EnableCap.Light0);
			
			float[] ambient = {1f,1f,1f,1f};
			float[] specular = {0f,1f,0f,1f};
			GL.Light(LightName.Light0, LightParameter.Ambient, ambient);
			GL.Light(LightName.Light0, LightParameter.Specular, specular);
		}
		
		/***********************************************************
		 * 						TEXT METHODS					    *
		 ***********************************************************/

		private void PrintComment(string comment, ref float yOffset)
		{
			PrintComment(mainText, comment, QFontAlignment.Justify, ref yOffset);
		}
		
		private void PrintComment(QFont font, string comment,QFontAlignment alignment, ref float yOffset){
			GL.PushMatrix();
			yOffset += 5;
			GL.Translate(30f, yOffset, 0f);
			font.Print(comment, Width - 60, alignment);
			yOffset += font.Measure(comment, Width - 60, alignment).Height;
			GL.PopMatrix();
		}

		void FontInit()
		{
			GL.PushMatrix();
			mainText = new QFont("../../../Fonts/Comfortaa-Regular.ttf", 12,FontStyle.Regular);
			mainText.Options.Colour = new Color4(0.0f, 0.0f, 0.4f, 1.0f);
			GL.PopMatrix();
		}
		
		void renderTexts()
		{

			Matrix4 modelview = Matrix4.LookAt(Vector3.Zero, Vector3.UnitZ, Vector3.UnitY);
			GL.PushMatrix();
			GL.Enable(EnableCap.ColorMaterial);
			GL.LoadMatrix(ref modelview);
			float yOffset = 0;
			QFont.Begin();
				PrintComment("Zones chargées: "+scene.zones.Count, ref yOffset);
				PrintComment("Zone Courante: "+precZone, ref yOffset);
				PrintComment("Rayon : "+radius, ref yOffset);
				PrintComment("Va/Vc : "+cmptVertexAfficher+"/"+cmptVertexZone, ref yOffset);
			QFont.End();
			QFont.Begin();
			GL.Begin(BeginMode.Quads);
			
			GL.Color3(0.0f, 0.0f, 0.0); GL.Vertex2(0, 0);
			GL.Color3(0.0f, 0.0f, 0.0f); GL.Vertex2(Width/4, 0);
			GL.Color3(0.0f, 0.0f, 0.0f); GL.Vertex2(Width/4, Height/5);
			GL.Color3(0.0f, 0.0f, 0.0f); GL.Vertex2(0, Height/5);
			
			GL.End();
			QFont.End();
			GL.Disable(EnableCap.Texture2D);
			GL.Disable(EnableCap.Blend);
			GL.Disable(EnableCap.ColorMaterial);
			GL.PopMatrix();
			glEnables();

		}

		public static void Main(string[] args)
		{
			
			System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");
        	System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
			
			Console.WriteLine("Mode?\np - preprocessing\nr - rendu\n");
			string c = Console.ReadLine();
			Console.WriteLine("Nom scene:\n");
			string scn = Console.ReadLine();
			Console.WriteLine("Taille découpe:\n");
			string size = Console.ReadLine();
			pathToModels += scn;
			splitSize = Convert.ToInt32(size);
			if(c[0] == 'p'){
				
				ObjLoader.objToZs(pathToModels + ".obj",pathToModels +".zs",splitSize);
				Console.WriteLine("Parsing completed in " + pathToModels + ".zs");
			}
			else{
				using (Program p = new Program())
				{
					p.Run();
				}
			}
			
		}
		
	}
}