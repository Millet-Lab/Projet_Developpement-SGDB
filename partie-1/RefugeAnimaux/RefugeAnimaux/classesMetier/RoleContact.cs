using System;

class RoleContact
{
	public Contact contact;
	public Role role;

	public RoleContact ()
	{
		this.contact = new Contact ();
		this.role = new Role ();
	}

    public RoleContact (int idContact, int idRole)
    {
        this.contact = new Contact();
        this.role = new Role();
		contact.IdContact = idContact;
		role.IdRole = idRole;
    }

    public RoleContact (RoleContact rc)
    {
        if (rc == null)
            ArgumentNullException.ThrowIfNull(rc);

        this.contact = new Contact();
        this.role = new Role();
        contact.IdContact = rc.contact.IdContact;
        role.IdRole = rc.role.IdRole;
    }
}
