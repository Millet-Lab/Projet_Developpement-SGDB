using System;

class Vaccin
{
	private int idVaccin;
	private string nomVaccin = string.Empty;

	public int IdVaccin 
	{
		set {  idVaccin = value; }
		get { return idVaccin; }
	}

	public string NomVaccin 
	{
		set {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Le nom du vaccin est obligatoire");
            nomVaccin = value; 
		}

		get { return nomVaccin; }
	}

	public Vaccin (){ }

	public Vaccin (int idVaccin, string nomVaccin) 
	{
		IdVaccin = idVaccin;
		NomVaccin = nomVaccin;
	}

	public Vaccin (Vaccin vaccin)
	{
		if (vaccin == null)
			throw new ArgumentNullException(nameof(vaccin));

		IdVaccin = vaccin.IdVaccin;
		NomVaccin = vaccin.NomVaccin;
	}
}
