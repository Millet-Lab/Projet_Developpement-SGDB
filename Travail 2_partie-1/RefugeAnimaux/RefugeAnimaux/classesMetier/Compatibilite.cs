using System;

public class Compatibilite
{
    private int idCompatibilite;
	private TypeAnimal type;

    // les propriétés sans logique
    public int IdCompatibilite
    {
        set {  idCompatibilite = value; }
        get { return idCompatibilite; }
    }

    public TypeAnimal Type
    {
        set { type  = value; }
        get { return type; }
    }

    //public TypeAnimal

    public Compatibilite() { }

    public Compatibilite(int idCompatibilite, TypeAnimal type)
    {
        this.idCompatibilite = idCompatibilite;
        this.type = type;
    }

    public Compatibilite(Compatibilite compatibilite)
    {
        ArgumentNullException.ThrowIfNull(compatibilite);

        IdCompatibilite = compatibilite.IdCompatibilite;
        Type = compatibilite.Type;
    }
}
