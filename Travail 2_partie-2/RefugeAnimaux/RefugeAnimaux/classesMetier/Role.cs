using System;
public enum TypeRoles
{
    Benevole,
    Adoptant,
    Candidat,
    Famille_Accueil
}

class Role
{
	public TypeRoles Nom { set; get; }

	public int IdRole { set; get; }

	public Role() { }

    public Role(int idRole, TypeRoles nom)
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