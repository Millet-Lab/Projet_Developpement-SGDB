-- =============================================
-- * Nettoyage des tables avec leurs références
-- =============================================
drop table if exists contacts cascade;
drop table if exists animal cascade;
drop table if exists sorties cascade;
drop table if exists entrees cascade;
drop table if exists couleurs cascade;
drop table if exists couleurs_animal cascade;
drop table if exists compatibilites cascade;
drop table if exists adoptions cascade;
drop table if exists vaccins cascade;
drop table if exists roles cascade;
drop table if exists appariements cascade;
drop table if exists accueils cascade;
drop table if exists vaccinations cascade;
drop table if exists roles_contact cascade;

-- ============================================
-- * Nettoyage des types avec leurs références
-- ============================================
drop type if exists type_adresse cascade;
drop type if exists type_animal cascade;
drop type if exists genre cascade;
drop type if exists motifs_sortie cascade;
drop type if exists motifs_entree cascade;
drop type if exists statut_adoption cascade;
drop type if exists type_roles cascade;

-- ===========================================
-- * Nettoyage des triggers  des contraintes
-- ===========================================
drop trigger if exists trig_check_date_vaccination on vaccinations cascade;
drop trigger if exists trig_check_unique_sortie on sorties cascade;
drop trigger if exists trig_check_deces_animal on animal cascade;
drop trigger if exists trig_check_sortie_deces on sorties cascade;
drop trigger if exists trig_check_animal_entree on entrees cascade;
drop trigger if exists trig_check_adoption_sortie on sorties cascade;
drop trigger if exists trig_check_unique_accueil_actif on sorties cascade;

-- ===========================================
-- * Création des types pour la gestion refuge
-- ===========================================
create type type_adresse as (
	rue varchar(45),
	cp char(4),
	localite varchar(26)
);

create type type_animal as enum (
	'chien',
	'chat'
);

create type genre as enum (
	'M',
	'F'
);

create type motifs_sortie as enum (
	'adoption',
	'retour_proprietaire',
	'deces_animal',
	'famille_accueil'
);

create type motifs_entree as enum (
	'abandon',
	'errant',
	'deces_proprietaire',
	'saisie',
	'retour_adoption',
	'retour_famille_accueil'
);

create type statut_adoption as enum (
	'demande',
	'acceptee',
	'rejet_environnement',
	'rejet_comportement'
);

create type type_roles as enum (
	'benevole',
	'adoptant',
	'candidat',
	'Famille_accueil'
);

-- =============================================
-- * Création des tables pour la gestion refuge
-- =============================================
create table contacts (
	id_contact integer generated always as identity primary key,
	nom varchar(20),
	prenom varchar(20),
	adresse type_adresse,
	registre_national varchar(15) unique,
	gsm char(10),
	telephone char(9),
	email varchar(100),

	--* Format valide d'un registre national : yy.mm.dd-999.99
	constraint check_format_registre
    check (registre_national ~ '^[0-9]{2}\.[0-9]{2}\.[0-9]{2}-[0-9]{3}\.[0-9]{2}$'),

	-- Format valide du nom (au moins 2 caractères)
	constraint check_nom
	check (
			char_length(trim(nom)) >= 2 and
			nom ~ '^[A-Za-zÀ-ÿ]+([\- ][A-Za-zÀ-ÿ]+)*$'
	),

	-- Format valide du nom (au moins 2 caractères)
	constraint check_prenom
	check (
			char_length(trim(prenom)) >= 2 and
			prenom ~ '^[A-Za-zÀ-ÿ]+([\- ][A-Za-zÀ-ÿ]+)*$'
	),

	-- * Au moins un moyen de contact : gsm OR telephone OR email NOT NULL
    constraint check_contact_obligatoire
    check (
        gsm is not null or
        telephone is not null or
        email is not null
    ),

	-- * Format valide d'une email
    constraint check_format_email
    check (
        email is null or
        email ~ '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$'
    )
);

create table animal (
	id_animal varchar(11) primary key,
	nom_animal varchar(15) not null,
	sexe genre not null,
	animal type_animal not null,
	date_naissance date not null,
	date_deces date,
	sterilise boolean not null,
	date_sterilisation date,
	description varchar(50) not null,
	particularites varchar(50) not null,

	-- * id_animal: format valide yymmdd99999 (11 caractères)
    constraint check_identifiant
	check (id_animal ~ '^[0-9]{11}$'
    and to_char(to_date(substr(id_animal,1,6),'YYMMDD'),'YYMMDD') = substr(id_animal,1,6)),

    -- * Si l’animal n’est pas stérilisé alors la date de stérilisation est nulle
    constraint check_sterilisation
    check (sterilise = TRUE or date_sterilisation is null),

    -- * La date de stérilisation est soit nulle ou soit supérieure à sa date de naissance
    constraint check_date_sterilisation
    check (date_sterilisation is null or date_sterilisation > date_naissance),

    -- * Date de décès d’un animal est soit nulle ou soit supérieure à sa date de naissance
    constraint check_date_deces
    check (date_deces is null or date_deces > date_naissance),

    -- * Date de naissance doit être inférieure ou égal à la date courante
    constraint check_date_naissance
    check (date_naissance <= current_date)
);

create table sorties (
    motif motifs_sortie not null,
	date_sortie date not null,
	id_animal_sorti varchar(11) not null,
    sortie_contact integer,

    constraint pk_sorties
    primary key (date_sortie, id_animal_sorti, sortie_contact),

    constraint fk_id_animal_sorti
    foreign key (id_animal_sorti)
    references animal(id_animal)
    on delete cascade,

    constraint fk_sortie_contact
    foreign key (sortie_contact)
    references contacts(id_contact)
    on delete set null
);

create table entrees (
    motif motifs_entree not null,
	date_entree date not null,
	id_animal_entre varchar(11) not null,
    entree_contact integer,

    -- Définition des contraintes
    constraint pk_entrees
    primary key (date_entree, id_animal_entre, entree_contact),

    constraint fk_id_animal_entre
    foreign key (id_animal_entre)
    references animal(id_animal)
    on delete cascade,

    constraint fk_entree_contact
    foreign key (entree_contact)
    references contact(id_contact)
    on delete set null
);

create table couleurs (
	id_couleur integer generated always as identity primary key,
	nom_couleur varchar(20) not null
);

create table couleurs_animal (
	id_couleurs_animal integer generated always as identity primary key,
	id_animal varchar(11) not null
);

create table compatibilites (
	id_compatibilite integer generated always as identity primary key,
	animal type_animal not null
);

create table adoptions (
	statut statut_adoption not null,
	date_adoption date not null,
	id_animal_adopte char(11) not null,
	adoptant integer not null,

	-- Définitions des contraintes
	constraint pk_adoptions
    primary key (date_adoption, id_animal_adopte, adoptant),

	constraint fk_id_animal_adopte
    foreign key (id_animal_adopte)
    references animal(id_animal)
    on delete cascade,

    constraint fk_adoptant
    foreign key (adoptant)
    references contact(id_contact)
    on delete set null
);

create table vaccins (
	id_vaccin integer generated always as identity primary key,
	nom_vaccin varchar(20)
);

create table appariements (
	valeur boolean not null,
	description text not null,
	id_compatibilite_appar integer not null,
	id_animal_teste char(11) not null,

	-- Définitions des contraintes
	constraint pk_appariements
    primary key (id_compatibilite_appar, id_animal_teste),

	constraint fk_ani_identifiant
    foreign key (id_animal_teste)
    references animal (id_animal)
    on delete cascade,

    constraint fk_id_compatibilite_appar
    foreign key (id_compatibilite_appar)
    references compatibilites (id_compatibilite)
    on delete set null
);

create table accueils (
	date_debut date,
	date_fin date,
	id_animal_accueilli char(11),
	id_accueillant integer,

	-- Définitions des contraintes
	constraint pk_accueils
    primary key (id_animal_accueilli, id_accueillant),

	constraint fk_id_animal_accueilli
    foreign key (id_animal_accueilli)
    references animal(id_animal)
    on delete cascade,

    constraint fk_id_accueillant
    foreign key (id_accueillant)
    references contact(id_contact)
    on delete set null,

	constraint check_date_fin
    check (date_fin >= date_debut)
);

create table vaccinations (
	date_vaccination date,
	id_animal_vaccine varchar(11),
	id_vaccin_utilise integer,

	-- Définitions des contraintes
	constraint pk_vaccinations
    primary key (date_vaccination, id_animal_vaccine, id_vaccin_utilise),

	constraint fk_id_animal_vaccine
    foreign key (id_animal_vaccine)
    references animal(id_animal)
    on delete cascade,

    constraint fk_id_vaccin_utilise
    foreign key (id_vaccin_utilise)
    references vaccins(id_vaccin)
    on delete set null
);

create table roles (
	id_roles integer generated always as identity primary key,
	rol type_roles
);

create table roles_contact (
	id_role_contact integer,
	id_role integer,

	-- Définitions des contraintes
	constraint pk_roles_contact
    primary key (id_role_contact, id_role),

	constraint fk_id_role_contact
    foreign key (id_role_contact)
    references contacts(id_contact)
    on delete cascade,

    constraint fk_id_role
    foreign key (id_role)
    references roles(id_roles)
    on delete set null
);

-- ==================================================
-- * Création des déclencheurs pour la gestion refuge
-- ==================================================
-- ==> TRIGGER vérification de la date de vaccination
create or replace function check_date_vaccination()
returns trigger as $$
declare
    naissance date;
begin
    select date_naissance
    into naissance
    from animal
    where id_animal = new.ani_identifiant;

    if new.date_vaccination is not null
       and new.date_vaccination <= naissance then
        raise exception
        'La date de vaccination doit être après la naissance';
    end if;

    return new;
end;
$$ language plpgsql;

create trigger trig_check_date_vaccination
before insert or update on vaccinations
for each row
execute function check_date_vaccination();

-- ==> TRIGGER de la seule sortie depuis la plus récente date d’entrée de l’animal
create or replace function check_unique_sortie()
returns trigger as $$
declare
    derniere_entree date;
begin
    -- la dernière date d'entrée
    select max(date_entree)
    into derniere_entree
    from entrees
    where id_animal_sorti = new.ani_identifiant;

    -- Lève une exception s’il existe déjà une sortie après cette entrée
    if exists (
        select 1
        from sorties
        where id_animal_sorti = new.ani_identifiant
        and date_sortie > derniere_entree
    ) then
        raise exception
        'Une sortie existe déjà après la dernière entrée';
    end if;

    return new;
end;
$$ language plpgsql;

create trigger trig_check_unique_sortie
before insert on sorties
for each row
execute function check_unique_sortie();

-- ==> TRIGGER si date_deces est non nulle alors la raison
--     de la dernière sortie (sortie la plus récente) est 'decede’
create or replace function check_deces_animal()
returns trigger as $$
declare
    motif_sortie_animal text;
begin
    if new.date_deces is not null then
        -- récupérer la dernière raison de sortie
        select motif
        into motif_sortie_animal
        from sorties
        where id_animal_sorti = new.id_animal
        order by date_sortie desc
        limit 1;

        if motif_sortie_animal is distinct from 'deces_animal' then
            raise exception
            'La dernière sortie doit être décédé';
        end if;
    end if;

    return new;
end;
$$ language plpgsql;

create trigger trig_check_deces_animal
before insert or update on animal
for each row
execute function check_deces_animal();

-- ==> TRIGGER si la raison de sortie est 'deces_animal', alors date_deces est non null
create or replace function check_sortie_deces()
returns trigger as $$
declare
    date_deces_animal date;
begin
    if new.raison = 'deces_animal' then
        select date_deces
        into date_deces_animal
        from animal
        where id_animal = new.ani_identifiant;

        if date_deces_animal is null then
            raise exception
            'date_deces doit être non nulle si raison est le décès de l''animal';
        end if;
    end if;

    return new;
end;
$$ language plpgsql;

create trigger trig_check_sortie_deces
before insert or update on sorties
for each row
execute function check_sortie_deces();

-- ==> TRIGGER chaque animal DOIT avoir au moins une ENTREE
create or replace function check_animal_entree()
returns trigger as $$
begin
    if not exists (
        select 1
        from entrees
        where id_animal_entre = new.id_animal
    ) then
        raise exception
        'Un animal DOIT avoir au moins une entrée';
    end if;

    return new;
end;
$$ language plpgsql;

create constraint trigger trig_check_animal_entree
after insert on animal
deferrable initially deferred
for each row
execute function check_animal_entree();

-- ==> TRIGGER si la raison de sortie est 'adoption', il devrait exister
--     une ADOPTION avec statut = 'acceptee' pour cet animal
create or replace function check_adoption_sortie()
returns trigger as $$
begin
    if new.motif = 'adoption' then
        if not exists (
            select 1
            from adoptions
            where id_animal_adopte = new.ani_identifiant
            and statut = 'acceptee'
        ) then
            raise exception
            'Aucune adoption acceptée pour cet animal';
        end if;
    end if;

    return new;
end;
$$ language plpgsql;

create trigger trig_check_adoption_sortie
before insert or update on sorties
for each row
execute function check_adoption_sortie();

-- ==> TRIGGER un animal ne peut pas avoir plusieurs accueils actifs
--     simultanément (un seul ACCUEIL avec date_fin = NULL à la fois)
create or replace function check_unique_accueil_actif()
returns trigger as $$
begin
    if new.date_fin is null then
        if exists (
            select 1
            from accueils
            where id_animal_accueilli = new.ani_identifiant
            and date_fin is null
        ) then
            raise exception
            'Un animal ne peut avoir qu''un seul accueil actif';
        end if;
    end if;

    return new;
end;
$$ language plpgsql;

create trigger trig_check_unique_accueil_actif
before insert or update on accueils
for each row
execute function check_unique_accueil_actif();