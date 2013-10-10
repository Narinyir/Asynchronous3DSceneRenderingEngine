using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Collections.Generic;


namespace MoteurDeStreaming
{
	public struct PLANE
	{
		public Vector3 normal;
		public Vector3 point;
		public float d;
	}

	public struct NEAR
	{
		public Vector3 nc;
		public Vector3 ntl;
		public Vector3 nbl;
		public Vector3 ntr;
		public Vector3 nbr;
	}
	
	public struct FAR
	{
		public Vector3 fc;
		public Vector3 ftl;
		public Vector3 fbl;
		public Vector3 ftr;
		public Vector3 fbr;
	}
	
	public struct FRUSTUM
	{
		public float tang;
		public Vector3 X;
		public Vector3 Y;
		public Vector3 Z;
		public float nearDist;
		public float Hnear;
		public float Wnear;
		public float farDist;
		public float Hfar;
		public float Wfar;
		public NEAR near;
		public FAR far;
		public PLANE[] pl;
	}
	
	public class Camera
	{
		public Matrix4 cameraMatrix;
		private float[] mouseSpeed = new float[2];
		private Vector3 mouseDelta;
		private Vector3 location;
		private Vector3 lookatPoint;
		private Vector3 up = Vector3.UnitY;
		private float pitch = 0.0f;
		private float facing = 0.0f;

		//Frustum
		private FRUSTUM frustum;
		static double ANG2RAD  = 3.14159265358979323846 / 180.0;
		private enum plan {TOP, BOTTOM, LEFT, RIGHT, NEARP, FARP};


		public Camera()
		{
			cameraMatrix = Matrix4.Identity;
			location = new Vector3(0f, 0f, 4f);
			mouseDelta = new Vector3();
			frustum = new FRUSTUM();
		}

		public void Update()
		{
			mouseSpeed[0] *= 0.9f;
			mouseSpeed[1] *= 0.9f;
			mouseSpeed[0] += mouseDelta.X / 1000f;
			mouseSpeed[1] += mouseDelta.Y / 1000f;
			mouseDelta = new Vector3();
			
			facing += mouseSpeed[0];
			pitch += mouseSpeed[1];
			lookatPoint = new Vector3((float)Math.Cos(facing), (float)Math.Sin(pitch), (float)Math.Sin(facing));
			cameraMatrix = Matrix4.LookAt(location, location + lookatPoint, up);
		}

		public void GoForward()
		{
			location.X += (float)Math.Cos(facing) * 0.3f;
			location.Z += (float)Math.Sin(facing) * 0.3f;
		}

		public void GoBackward()
		{
			location.X -= (float)Math.Cos(facing) * 0.3f;
			location.Z -= (float)Math.Sin(facing) * 0.3f;
		}
		public void GoLeft ()
		{
			location.X -= (float)Math.Cos (facing + Math.PI / 2) * 0.3f;
			location.Z -= (float)Math.Sin (facing + Math.PI / 2) * 0.3f;
		}
		public void GoRight ()
		{
			location.X += (float)Math.Cos (facing + Math.PI / 2) * 0.3f;
			location.Z += (float)Math.Sin (facing + Math.PI / 2) * 0.3f;
		}
		
		public void zoom()
		{
			location.Y += (float)Math.Sin(pitch) *0.5f;
			location.X += (float)Math.Cos(facing) * 0.5f;
			location.Z += (float)Math.Sin(facing) * 0.5f;
		}
		
		public void dezoom()
		{
			location.Y -= (float)Math.Sin(pitch) *0.5f;
			location.X -= (float)Math.Cos(facing) * 0.5f;
			location.Z -= (float)Math.Sin(facing) * 0.5f;
		}

		public void CalculFrustum (float nc, float fc, float angle, float ratio)
		{
			frustum.tang = (float)Math.Tan(ANG2RAD * angle * 0.5);
			frustum.nearDist = nc;
			frustum.farDist = fc;
			frustum.Hnear = frustum.nearDist * frustum.tang;
			frustum.Wnear = frustum.Hnear * ratio;
			frustum.Hfar = frustum.farDist * frustum.tang;
			frustum.Wfar = frustum.Hfar * ratio;

			lookatPoint = location + lookatPoint;

			frustum.Z.X = location.X - lookatPoint.X; 
			frustum.Z.Y = location.Y - lookatPoint.Y; 
			frustum.Z.Z = location.Z - lookatPoint.Z;
			frustum.Z.Normalize();
			frustum.X.X = up.Y * frustum.Z.Z - up.Z * frustum.Z.Y;
			frustum.X.Y = up.Z * frustum.Z.X - up.X * frustum.Z.Z;
			frustum.X.Z = up.X * frustum.Z.Y - up.Y * frustum.Z.X;
			frustum.X.Normalize();
			frustum.Y.X = frustum.Z.Y * frustum.X.Z - frustum.Z.Z * frustum.X.Y;
			frustum.Y.Y = frustum.Z.Z * frustum.X.X - frustum.Z.X * frustum.X.Z;
			frustum.Y.Z = frustum.Z.X * frustum.X.Y - frustum.Z.Y * frustum.X.X;

			CalculFar();
			CalculNear();

			frustum.pl = new PLANE[6];

			frustum.pl[(int)plan.TOP] = CreatePlane(frustum.near.ntr, frustum.near.ntl, frustum.far.ftl);
			frustum.pl[(int)plan.BOTTOM] = CreatePlane(frustum.near.nbl, frustum.near.nbr, frustum.far.fbr);
			frustum.pl[(int)plan.LEFT] = CreatePlane(frustum.near.ntl, frustum.near.nbl, frustum.far.fbl);
			frustum.pl[(int)plan.RIGHT] = CreatePlane(frustum.near.nbr, frustum.near.ntr, frustum.far.fbr);
			frustum.pl[(int)plan.NEARP] = CreatePlane(frustum.near.ntl, frustum.near.ntr, frustum.near.nbr);
			frustum.pl[(int)plan.FARP] = CreatePlane(frustum.far.ftr, frustum.far.ftl, frustum.far.fbl);
		}

		public void CalculNear ()
		{
			frustum.near.nc.X = location.X - frustum.Z.X * frustum.nearDist;
			frustum.near.nc.Y = location.Y - frustum.Z.Y * frustum.nearDist;
			frustum.near.nc.Z = location.Z - frustum.Z.Z * frustum.nearDist;

			//ntl
			frustum.near.ntl.X = frustum.near.nc.X + frustum.Y.X * frustum.Hnear - frustum.X.X * frustum.Wnear;
			frustum.near.ntl.Y = frustum.near.nc.Y + frustum.Y.Y * frustum.Hnear - frustum.X.Y * frustum.Wnear;
			frustum.near.ntl.Z = frustum.near.nc.Z + frustum.Y.Z * frustum.Hnear - frustum.X.Z * frustum.Wnear;
			//ntr
			frustum.near.ntr.X = frustum.near.nc.X + frustum.Y.X * frustum.Hnear + frustum.X.X * frustum.Wnear;
			frustum.near.ntr.Y = frustum.near.nc.Y + frustum.Y.Y * frustum.Hnear + frustum.X.Y * frustum.Wnear;
			frustum.near.ntr.Z = frustum.near.nc.Z + frustum.Y.Z * frustum.Hnear + frustum.X.Z * frustum.Wnear;
			//nbl
			frustum.near.nbl.X = frustum.near.nc.X - frustum.Y.X * frustum.Hnear - frustum.X.X * frustum.Wnear;
			frustum.near.nbl.Y = frustum.near.nc.Y - frustum.Y.Y * frustum.Hnear - frustum.X.Y * frustum.Wnear;
			frustum.near.nbl.Z = frustum.near.nc.Z - frustum.Y.Z * frustum.Hnear - frustum.X.Z * frustum.Wnear;
			//nbr
			frustum.near.nbr.X = frustum.near.nc.X - frustum.Y.X * frustum.Hnear + frustum.X.X * frustum.Wnear;
			frustum.near.nbr.Y = frustum.near.nc.Y - frustum.Y.Y * frustum.Hnear + frustum.X.Y * frustum.Wnear;
			frustum.near.nbr.Z = frustum.near.nc.Z - frustum.Y.Z * frustum.Hnear + frustum.X.Z * frustum.Wnear;
		}

		public void CalculFar()
		{
			frustum.far.fc.X = location.X - frustum.Z.X * frustum.farDist;
			frustum.far.fc.Y = location.Y - frustum.Z.Y * frustum.farDist;
			frustum.far.fc.Z = location.Z - frustum.Z.Z * frustum.farDist;

			//ftl
			frustum.far.ftl.X = frustum.far.fc.X + frustum.Y.X * frustum.Hfar - frustum.X.X * frustum.Wfar;
			frustum.far.ftl.Y = frustum.far.fc.Y + frustum.Y.Y * frustum.Hfar - frustum.X.Y * frustum.Wfar;
			frustum.far.ftl.Z = frustum.far.fc.Z + frustum.Y.Z * frustum.Hfar - frustum.X.Z * frustum.Wfar;
			//ftr
			frustum.far.ftr.X = frustum.far.fc.X + frustum.Y.X * frustum.Hfar + frustum.X.X * frustum.Wfar;
			frustum.far.ftr.Y = frustum.far.fc.Y + frustum.Y.Y * frustum.Hfar + frustum.X.Y * frustum.Wfar;
			frustum.far.ftr.Z = frustum.far.fc.Z + frustum.Y.Z * frustum.Hfar + frustum.X.Z * frustum.Wfar;
			//fbl
			frustum.far.fbl.X = frustum.far.fc.X - frustum.Y.X * frustum.Hfar - frustum.X.X * frustum.Wfar;
			frustum.far.fbl.Y = frustum.far.fc.Y - frustum.Y.Y * frustum.Hfar - frustum.X.Y * frustum.Wfar;
			frustum.far.fbl.Z = frustum.far.fc.Z - frustum.Y.Z * frustum.Hfar - frustum.X.Z * frustum.Wfar;
			//fbr
			frustum.far.fbr.X = frustum.far.fc.X - frustum.Y.X * frustum.Hfar + frustum.X.X * frustum.Wfar;
			frustum.far.fbr.Y = frustum.far.fc.Y - frustum.Y.Y * frustum.Hfar + frustum.X.Y * frustum.Wfar;
			frustum.far.fbr.Z = frustum.far.fc.Z - frustum.Y.Z * frustum.Hfar + frustum.X.Z * frustum.Wfar;
		}

		public PLANE CreatePlane (Vector3 v1, Vector3 v2, Vector3 v3)
		{
			PLANE p = new PLANE();
			Vector3 aux1, aux2;
			aux1 = v1 - v2;
			aux2 = v3 - v2;

			p.normal.X = aux2.Y * aux1.Z - aux2.Z * aux1.Y;
			p.normal.Y = aux2.Z * aux1.X - aux2.X * aux1.Z;
			p.normal.Z = aux2.X * aux1.Y - aux2.Y * aux1.X;
			p.normal.Normalize();

			p.point = v2;

			p.d = -(p.normal.X * p.point.X + p.normal.Y * p.point.Y + p.normal.Z + p.point.Z);

			return p;
		}

		public bool PointInFrustum (Vector4 p)
		{
			bool result = true;

			for (int i = 0; i < 6; i++) 
			{
				if ((frustum.pl[i].d + (frustum.pl[i].normal.X * p.X + frustum.pl[i].normal.Y * p.Y + frustum.pl[i].normal.Z * p.Z)) < 0)
					result = false;
			}

			return result;
		}

		public void drawFrustumPoints() 
		{	
			GL.Color3(Color.Aqua);
			GL.Begin(BeginMode.Points);
			
			GL.Vertex3(frustum.near.ntl.X,frustum.near.ntl.Y,frustum.near.ntl.Z);
			GL.Vertex3(frustum.near.ntr.X,frustum.near.ntr.Y,frustum.near.ntr.Z);
			GL.Vertex3(frustum.near.nbl.X,frustum.near.nbl.Y,frustum.near.nbl.Z);
			GL.Vertex3(frustum.near.nbr.X,frustum.near.nbr.Y,frustum.near.nbr.Z);
			
			GL.Vertex3(frustum.far.ftl.X,frustum.far.ftl.Y,frustum.far.ftl.Z);
			GL.Vertex3(frustum.far.ftr.X,frustum.far.ftr.Y,frustum.far.ftr.Z);
			GL.Vertex3(frustum.far.fbl.X,frustum.far.fbl.Y,frustum.far.fbl.Z);
			GL.Vertex3(frustum.far.fbr.X,frustum.far.fbr.Y,frustum.far.fbr.Z);
			
			GL.End();
		}

		public void drawFrustumLines() {

			GL.Color3(Color.Aqua);
			GL.Begin(BeginMode.LineLoop);
			//near plane
			GL.Vertex3(frustum.near.ntl.X,frustum.near.ntl.Y,frustum.near.ntl.Z);
			GL.Vertex3(frustum.near.ntr.X,frustum.near.ntr.Y,frustum.near.ntr.Z);
			GL.Vertex3(frustum.near.nbr.X,frustum.near.nbr.Y,frustum.near.nbr.Z);
			GL.Vertex3(frustum.near.nbl.X,frustum.near.nbl.Y,frustum.near.nbl.Z);
			GL.End();

			GL.Color3(Color.Aqua);
			GL.Begin(BeginMode.LineLoop);
			//far plane
			GL.Vertex3(frustum.far.ftr.X,frustum.far.ftr.Y,frustum.far.ftr.Z);
			GL.Vertex3(frustum.far.ftl.X,frustum.far.ftl.Y,frustum.far.ftl.Z);
			GL.Vertex3(frustum.far.fbl.X,frustum.far.fbl.Y,frustum.far.fbl.Z);
			GL.Vertex3(frustum.far.fbr.X,frustum.far.fbr.Y,frustum.far.fbr.Z);
			GL.End();

			GL.Color3(Color.Aqua);
			GL.Begin(BeginMode.LineLoop);
			//bottom plane
			GL.Vertex3(frustum.near.nbl.X,frustum.near.nbl.Y,frustum.near.nbl.Z);
			GL.Vertex3(frustum.near.nbr.X,frustum.near.nbr.Y,frustum.near.nbr.Z);
			GL.Vertex3(frustum.far.fbr.X,frustum.far.fbr.Y,frustum.far.fbr.Z);
			GL.Vertex3(frustum.far.fbl.X,frustum.far.fbl.Y,frustum.far.fbl.Z);
			GL.End();

			GL.Color3(Color.Aqua);
			GL.Begin(BeginMode.LineLoop);
			//top plane
			GL.Vertex3(frustum.near.ntr.X,frustum.near.ntr.Y,frustum.near.ntr.Z);
			GL.Vertex3(frustum.near.ntl.X,frustum.near.ntl.Y,frustum.near.ntl.Z);
			GL.Vertex3(frustum.far.ftl.X,frustum.far.ftl.Y,frustum.far.ftl.Z);
			GL.Vertex3(frustum.far.ftr.X,frustum.far.ftr.Y,frustum.far.ftr.Z);
			GL.End();

			GL.Color3(Color.Aqua);
			GL.Begin(BeginMode.LineLoop);
			//left plane
			GL.Vertex3(frustum.near.ntl.X,frustum.near.ntl.Y,frustum.near.ntl.Z);
			GL.Vertex3(frustum.near.nbl.X,frustum.near.nbl.Y,frustum.near.nbl.Z);
			GL.Vertex3(frustum.far.fbl.X,frustum.far.fbl.Y,frustum.far.fbl.Z);
			GL.Vertex3(frustum.far.ftl.X,frustum.far.ftl.Y,frustum.far.ftl.Z);
			GL.End();

			GL.Color3(Color.Aqua);
			GL.Begin(BeginMode.LineLoop);
			// right plane
			GL.Vertex3(frustum.near.nbr.X,frustum.near.nbr.Y,frustum.near.nbr.Z);
			GL.Vertex3(frustum.near.ntr.X,frustum.near.ntr.Y,frustum.near.ntr.Z);
			GL.Vertex3(frustum.far.ftr.X,frustum.far.ftr.Y,frustum.far.ftr.Z);
			GL.Vertex3(frustum.far.fbr.X,frustum.far.fbr.Y,frustum.far.fbr.Z);
			GL.End();
		}

		public void drawFrustumPlanes ()
		{
			GL.Color3(Color.Aqua);
			GL.Begin(BeginMode.Quads);

			//near plane
			GL.Vertex3 (frustum.near.ntl.X, frustum.near.ntl.Y, frustum.near.ntl.Z);
			GL.Vertex3 (frustum.near.ntr.X, frustum.near.ntr.Y, frustum.near.ntr.Z);
			GL.Vertex3 (frustum.near.nbr.X, frustum.near.nbr.Y, frustum.near.nbr.Z);
			GL.Vertex3 (frustum.near.nbl.X, frustum.near.nbl.Y, frustum.near.nbl.Z);
			
			//far plane
			GL.Vertex3 (frustum.far.ftr.X, frustum.far.ftr.Y, frustum.far.ftr.Z);
			GL.Vertex3 (frustum.far.ftl.X, frustum.far.ftl.Y, frustum.far.ftl.Z);
			GL.Vertex3 (frustum.far.fbl.X, frustum.far.fbl.Y, frustum.far.fbl.Z);
			GL.Vertex3 (frustum.far.fbr.X, frustum.far.fbr.Y, frustum.far.fbr.Z);
			
			//bottom plane
			GL.Vertex3 (frustum.near.nbl.X, frustum.near.nbl.Y, frustum.near.nbl.Z);
			GL.Vertex3 (frustum.near.nbr.X, frustum.near.nbr.Y, frustum.near.nbr.Z);
			GL.Vertex3 (frustum.far.fbr.X, frustum.far.fbr.Y, frustum.far.fbr.Z);
			GL.Vertex3 (frustum.far.fbl.X, frustum.far.fbl.Y, frustum.far.fbl.Z);
			
			//top plane
			GL.Vertex3 (frustum.near.ntr.X, frustum.near.ntr.Y, frustum.near.ntr.Z);
			GL.Vertex3 (frustum.near.ntl.X, frustum.near.ntl.Y, frustum.near.ntl.Z);
			GL.Vertex3 (frustum.far.ftl.X, frustum.far.ftl.Y, frustum.far.ftl.Z);
			GL.Vertex3 (frustum.far.ftr.X, frustum.far.ftr.Y, frustum.far.ftr.Z);
			
			//left plane
			
			GL.Vertex3 (frustum.near.ntl.X, frustum.near.ntl.Y, frustum.near.ntl.Z);
			GL.Vertex3 (frustum.near.nbl.X, frustum.near.nbl.Y, frustum.near.nbl.Z);
			GL.Vertex3 (frustum.far.fbl.X, frustum.far.fbl.Y, frustum.far.fbl.Z);
			GL.Vertex3 (frustum.far.ftl.X, frustum.far.ftl.Y, frustum.far.ftl.Z);
			
			// right plane
			GL.Vertex3 (frustum.near.nbr.X, frustum.near.nbr.Y, frustum.near.nbr.Z);
			GL.Vertex3 (frustum.near.ntr.X, frustum.near.ntr.Y, frustum.near.ntr.Z);
			GL.Vertex3 (frustum.far.ftr.X, frustum.far.ftr.Y, frustum.far.ftr.Z);
			GL.Vertex3 (frustum.far.fbr.X, frustum.far.fbr.Y, frustum.far.fbr.Z);
			
			GL.End (); 
		}

		//Getter & Setters
		public Vector3 MouseDelta
		{
			set{mouseDelta = value;}
			get{return mouseDelta;}
		}

		public Vector3 Location {
			get{ return location;}
		}
		
		public Vector3 LookAt {
			set{ lookatPoint = value;}
			get{ return lookatPoint;}
		}
	}
}

