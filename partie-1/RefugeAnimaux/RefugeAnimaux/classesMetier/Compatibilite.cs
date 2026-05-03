using System;

class Compatibilite
{
    private int idCompatibilite;
	public TypeAnimal type;

    // les propriétés
    public int IdCompatibilite
    {
        set {  this.idCompatibilite = value; }
        get { return this.idCompatibilite; }
    }

    public TypeAnimal Type
    {
        set { this.type  = value; }
        get { return (TypeAnimal) this.type; }
    }

    //public TypeAnimal

    public Compatibilite()
	{

	}

    public Compatibilite(int idCompatibilite, TypeAnimal type)
    {
        this.idCompatibilite = idCompatibilite;
        this.type = type;
    }

    public Compatibilite(Compatibilite compatibilite)
    {
        if (compatibilite == null)
            throw new ArgumentNullException(nameof(compatibilite));

        IdCompatibilite = compatibilite.IdCompatibilite;
        //AniType = compatibilite.Type;
    }
}
