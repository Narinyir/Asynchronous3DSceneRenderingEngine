using System;
using System.Collections.Generic;
using OpenTK;


namespace MoteurDeStreaming
{

	/* Un objet dynamique c'est quoi ?
	 * C'est d'abord un objet 3D donc ca me parait logique qu'il hérite de Objet3D
	 * Ensuite c'est un objet qui n'est pas statique, donc qui peut bouger.
	 * Est ce utile de lui mettre une position pour autant ? Car on peut avoir sa position via Objet3D au final ?
	 * Je pense qu'il devrait avoir des propriétés physiques que Objet3D n'a pas mais est ce que c'est vraiment utile ici,
	 * vu qu'on a pas moteur physique ?
	 * 
	 * */
	public class ObjetDynamique : Objet3D
	{
				
		public ObjetDynamique ()
		{
	
		}
		
}
		
	
}