using System;

namespace RefAniWpf.Modele
{
    public class RoleContact
    {
        public int IdContact { get; set; }
        public int IdRole { get; set; }
        public Contact contact;
        public Role role;

        public RoleContact()
        {
            contact = new Contact();
            role = new Role();
        }

        public RoleContact(int idContact, int idRole)
        {
            contact = new Contact();
            role = new Role();
            contact.IdContact = idContact;
            role.IdRole = idRole;
        }

        public RoleContact(RoleContact rc)
        {
            if (rc == null)
            {
                throw new ArgumentNullException(nameof(rc));
            }

            contact = new Contact();
            role = new Role();
            contact.IdContact = rc.contact.IdContact;
            role.IdRole = rc.role.IdRole;
        }
    }
}