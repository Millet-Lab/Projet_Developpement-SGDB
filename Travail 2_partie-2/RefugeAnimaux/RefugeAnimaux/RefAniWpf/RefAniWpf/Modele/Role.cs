using System;

namespace RefAniWpf.Modele
{
    public enum TypeRoles
    {
        Benevole,
        Adoptant,
        Candidat,
        Famille_Accueil
    }

    public class Role
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
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            IdRole = role.IdRole;
            Nom = role.Nom;
        }
    }
}