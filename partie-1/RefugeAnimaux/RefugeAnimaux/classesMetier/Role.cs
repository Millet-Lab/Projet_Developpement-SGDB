using System;

class Role
{
	public string? Nom { set; get; }

	public int IdRole { set; get; }

	public Role() { }

    public Role(int idRole, string nom)
    {
		IdRole = idRole;
		Nom = nom;
    }

    public Role(Role role)
    {
        ArgumentNullException.ThrowIfNull(role);
        
        IdRole = role.IdRole;
        Nom = role.Nom;
    }
}