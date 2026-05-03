using System;

class Role
{
	private int idRole;
	private string nom = string.Empty;

	public string Nom 
	{
		set { nom = value; }
		get { return nom; }
	}

	public int IdRole
	{
		set {  idRole = value; }
		get { return idRole; }
	}

	public Role()
	{
	}
}
