using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UtilityScripts;

public static class RandomNameGenerator {
	
	private static string[] baseHumanSurnames = new[]{
		"Adams", "Anderson", "Baker", "Ballard", "Bainard", "Barnes", "Bell", "Bennet", "Brooker", "Carpenter", "Clarke",
		"Corbon", "Cooper", "Digby", "Durandal", "Emory", "Fossard", "Fysher", "Fletcher", "Garin", "Gaveston",
		"Godfrey", "Hamond", "Harcourt", "Hughes", "Kent", "Knighton", "Lovell", "Manston", "Madley", "Middleton", "Nelond",
		"Noyers", "Orlebar", "Osmont", "Payne", "Pennant", "Pratt", "Picard", "Raleigh", "Ratcliff", "Renold", "Rolfe", "Talbot",
		"Thornton", "Thibault", "Walter", "Weston", "Williams", "Webb", "Whyte", "Yate", "Vernon", "Villon"
	};
	
	private static string[] baseElvenSurnames = new[]{
		"Shalsyr", "Vaneme", "Tathdhen", "Nel", "Tanth", "Eleneth",	"Shala", "Tinuvaul", "Tinuloth",
		"Galoneme",	"Laelithlhûn",	"Elendiir", "Rolotaur",	"Tanethdren", "Nelraisel", "Aldamin", "Ondo", "Ondel",	
		"Dlardirthor", "Aldatauré", "Wermirina", "Aeraelon", "Haemin", "Nhaya", "Faelan" 	 	 
	};

	private static string[] baseHumanKingdomNames = new[]{
		"Atlantis", "Albane", "Alamid", "Anderland", "Aurelia", "Ashanti", "Bathria", "Benin", "Bismuth", "Boron", "Burgundy", "Calabon", "Canton", "Chyland", "Croatia", "Ceres", 
		"Darmid", "Delta", "Dirland", "Dohati", "Duscany", "Elarus", "Esmeris", "Erebor", "Emusil", "Erathia", "Folgeron", "Fernica", "Fuchia", "Friedland", "Farrah", "Garwinia", "Grazil", "Gunark", 
		"Gehenna", "Gladius", "Hacatid", "Hermani", "Hisoka", "Howland", "Hunan", "Ianland", "Iridia", "Israel", "Indosin", "Illumina", "Jamalaya", "Jinni", "Jorara", "Justinia", "Jelsebed",
		"Karamba", "Kaedwen", "Korolus", "Kumael", "Kestrella", "Losendro", "Liria", "Lunesta", "Lusland", "Lyrica", "Makuria", "Meridian", "Mykland", "Morland", "Muskovich", "Nestori", "Norsica", 
		"Nubland", "Naralan", "Nissin", "Obelin", "Ovid", "Opera", "Osmud", "Obelisk", "Patani", "Petersen", "Polaris", "Prissia", "Pusant", "Quatar", "Qualia", "Quenden", "Rastafar", "Remedi", "Russo", 
		"Roveri", "Scythra", "Shetland", "Sundira", "Silveria", "Sahari", "Takatak", "Thenid", "Tilain", "Tesoro", "Tulisia", "Umbra", "Ulbany", "Uruk", "Unibad", "Virindor", "Visaya", "Vanad", "Vesemin",
		"Vortex", "Zulu", "Zakrand", "Zerena", "Ziria"
	};

	private static string[] humanMaleFirstNames = new[]{
		"Aldred", "Alistair", "Arthur", "Ashton", "Atkins", "Barric", "Bentley", "Blythe", "Braden", "Byram", "Caldwell", "Carlisle", "Clifton", "Colton", "Cuthbert",
		"Dalton", "Darren", "Dawson", "Denver", "Dudley", "Easton", "Edgar", "Elton", "Erwan", "Franklin", "Garett", "Gerard", "Gordon", "Hammond", "Holden", "Howard",
		"Hyde", "Irving", "Ian", "Jamie", "Jeremy", "Jesus", "John", "Keaton", "Kirby", "Kipling", "Lander", "Landon", "Leland", "Macon", "Moses", "Matthew", "Maxwell",
		"Maven", "Niles", "Nigel", "Nestor", "Oakley", "Oliver", "Oswald", "Payton", "Parker", "Preston", "Quentin", "Ramsey", "Randall", "Rhett", "River", "Robert",
		"Sawyer", "Shelton", "Silas", "Spencer", "Stewart", "Tanner", "Tim", "Terrence", "Ulmer", "Ulric", "Uther", "Wallace", "Wesley", "William", "Wolfe", "Yates"
	};

	private static string[] humanFemaleFirstNames = new[]{
		"Aelith", "Alvina", "Amity", "Audrey", "Bathilda", "Blossom", "Bliss", "Brianna", "Bridgit", "Cara", "Chauncey", "Clementine", "Casey", "Daisy", "Dawn", "Diana",
		"Devon", "Edith", "Edwina", "Elga", "Elvina", "Ermengard", "Evelyn", "Farah", "Faye", "Fiona", "Francesca", "Gidget", "Godiva", "Gunnhild", "Greta", "Hadley",
		"Hazel", "Hannah", "Holly", "Ingrid", "Ivy", "Kenley", "Kelby", "Karen", "Kate", "Laila", "Letha", "Lily", "Lindsey", "Madison", "Maggie", "Mildred", "Misty",
		"Norma", "Nyx", "Nana", "Nerissa", "Olga", "Oletha", "Posy", "Patsy", "Piper", "Precious", "Raissa", "Rhiannon", "Rose", "Roxanne", "Sable", "Shea", "Shelby",
		"Susan", "Suzette", "Tanya", "Tatiana", "Taylor", "Velma", "Vanessa", "Violet", "Whitney", "Willow", "Windy"
	};

	private static string[] baseElvenKingdomNames = new[]{
		"Aglarond", "Alqualonde", "Avallone", "Astalen", "Aerwyn", "Arfinal", "Arundel", "Beleriand", "Brithombar", "Berefin", "Bataryn", "Brindelwa", "Curamar", "Celirith", "Cendralien",
		"Casimin", "Carmindel", "Damriel", "Delimar", "Doriath", "Dullahan", "Edhellond", "Eglarest", "Eroahar", "Etheria", "Esinia", "Enlibad", "Fernia", "Finlabad", "Frolien", "Faerwyn", 
		"Fyrwen", "Feanan", "Forlond", "Formenos", "Filarmar", "Gilinia", "Galawad", "Galavaliel", "Galadhon", "Gondolin", "Hamlindras", "Helmongard", "Harlond", "Imladris", "Iberia", "Irluwan", 
		"Kortirion", "Keswen", "Kirith", "Kilawen", "Kalrinien", "Lindon", "Lorien", "Lothlorien", "Lustria", "Legrandos", "Menegroth", "Mithlond", "Marowen", "Meniha", "Miraella", "Nargothrond", 
		"Nargahar", "Nurbad", "Oswyth", "Orifia", "Orfindel", "Pindolwyn", "Pruwyn", "Pithrilen", "Rivendell", "Rilmeroth", "Randilros", "Rilfindor", "Sardinia", "Sherbarad", "Solwyn", "Thauniel", 
		"Thalessa", "Tavrovel", "Thranduil", "Uralen", "Urdwyn", "Valinor", "Vinyamar", "Vasmundin", "Wylia"
	};

	private static string[] baseElvenFemaleNames = new[]{
		"Arwen", "Amarie", "Alassea", "Arasinya", "Aredhel", "Authiel", "Amirala", "Bainwen", "Berissa", "Beriana", "Berondiel" , "Calathiel", "Caladwen", "Celebrian", "Castiel", "Ciristel", "Dayanara", 
		"Darxaniel", "Derenie", "Darla", "Ellenwe", "Ellethwen", "Elarinya", "Eleniel", "Erudessa", "Eldalothe", "Enelye", "Eruraina", "Firinea", "Fyrwen", "Froilwen", "Faralana", "Frindea", "Findemaxa", "Galadriel", 
		"Gwaerindis", "Galhadya", "Gerliana", "Harwel", "Hera", "Hanaxa", "Heloise", "Indis", "Irwindis", "Irsandwen", "Ishmila", "Livindel", "Larthindra", "Levana", "Luthien", "Medea", "Meliantha", "Maerwen", "Medlinya", 
		"Meltoriel", "Miriel", "Nathanya", "Nithiel", "Nimrodel", "Nerdanel", "Nessima", "Nostariel", "Ruanda", "Ruthlien", "Saerwen", "Sandara", "Sirissi", "Santiel", "Sidheil", "Silima", "Thoriel", "Thandiel", 
		"Vanya", "Vanafindiel", "Vanessea"
	};

	private static string[] baseElvenMaleNames = new[]{
		"Adan", "Alyameldir", "Amonost", "Aegnor", "Arandur", "Angrod", "Beren", "Baradhamon", "Beriadan", "Beleg", "Bercalion", "Cadgon", "Curufin", "Canardur", "Ceberlandon", "Calanon", "Caranion", "Castien", 
		"Celebrimbor", "Daeron", "Denethor", "Dramorion", "Durion", "Dirnost", "Eclesteron", "Ecthelion", "Eruadan", "Eglerion", "Eleyond", "Emerion", "Erunestian", "Elrond", "Faeron", "Faelon", "Feredir", "Fandur", "Fistilion", 
		"Fingolfin", "Galion", "Galathil", "Glorfindel", "Glandur", "Gimrinost", "Herion", "Hirgon", "Horthien", "Imrathon", "Ionwe", "Landion", "Legolas", "Limdur", "Lithaldoren", "Maeron", "Maeglad", "Mablung", 
		"Morcion", "Mithrellas", "Mornefindon", "Megildur", "Nendir", "Nedhudir", "Nibencarden", "Nimtolien", "Orodreth", "Orthorien", "Ovorion", "Rainion", "Rhovanion", "Sadron", "Saeldur", "Tangadion", "Taurion", 
		"Thalion", "Turin", "Thorontur", "Voronwe", "Vaessen", "Valanyonnen"
	};

	private static string[] baseAncientRuinPrefixes = new[]{
		"Mystic", "Obsidian", "Shadow", "Glimmer", "Frey", "Arrow", "Deep", "Moon", "Ancient", "Forbidden", "Oblivion", "Still", "Dragon", "Amber", "Wendi", "Ingle", "Isle", "White", "Black", "Grimm", "New", "Small",
		"Hound", "Stone", "Cloud", "Frey", "Crystal", "Rage", "Mist", "Raven", "Troll", "Goblin", "Cold", "Angel", "Evil", "Mythic", "Silent", "Wailing", "Tilted", "Snobby", "Shifty", "Flush", "Greasy", "Anarchy", "Fatal",
		"Haunted", "Junk", "Pleasant", "Lonely", "Moisty", "Forgotten", "Salty", "Cruel", "Thunder", "Oaken", "Wind", "Never", "Basin"
	};

	private static string[] baseAncientRuinSuffixes = new[]{
		"lair", "town", "vault", "ruin", "lost", "hallow", "drift", "shade", "hold", "grove", "shell", "wick", "ville", "hill", "garde", "reach", "den", "dale", "moor", "more", "view", "wich", "borough", "chill", "berg", "burgh", "ster", "stall", 
		"cross", "storm", "fell", "bury", "ward", "hand", "cliff", "valley", "mire", "gulch", "well", "hall", "gall", "pass", "hollow", "mere", "land", "cairn", "wood", "lands", "horn", "keep", "cre", "scar", "shield", "rand", "borne", "port", "guard", "mount", 
		"bay", "high", "bourne", "helm", "frost", "mouth", "dusk", "wild", "ness", "dread", "warts", "nook", "spire", "steep", "frey", "fort"
	};

	private static string[] baseTileNames = new[]{
		"Aecianara", "Graggadalar", "Chussirah", "Strukimelan", "Slaeddithis", "Plobeonata", "Zeaconet", "Ceapiarial", "Inniariel", "Oseotara", "Kruziven", "Creokkiogarth", "Gloyiarial", "Pephadin", "Wubegus",
		"Uqupia", "Vreakkezan", "Kriaggagarth", "Craepenet", "Caeqarus", "Iobeocion", "Headragana", "Jiacrariel", "Yidrelan", "Sleatania", "Edramar", "Abbedran", "Stassuspea", "Gruhorene", "Paddiriel",
		"Eddiothis", "Olasia", "Chommagarth", "Luveaxath", "Pollonor", "Criassatuary", "Bresarea", "Heappeala", "Tiacrithaer", "Iodorune", "Annetika", "Waeqiomond", "Goglalan", "Caecasia", "Ashocia",
		"Chiafoxus", "Bruniothis", "Fliayetha", "Ugrimund", "Ialisia", "Ioppeamos", "Heahearia", "Iocearim", "Aereaphere", "Eachemar", "Ostreatara", "Acliovar", "Eollezan", "Siagrarath", "Flioddearea",
		"Staebbiaphere", "Teastrelar", "Ximiathra", "Wokkelar", "Umular", "Iossaque", "Kloddenet", "Ecether", "Greaxoque", "Ioweotha", "Tatatope", "Funiomond", "Riocrituary", "Dreohasos", "Eossigana",
		"Kreaqiodin", "Meammeaspea", "Bresalan", "Struledore", "Ihorynn", "Bleagirah", "Heanniodu", "Zemmolas", "Cistiapia", "Ottelon", "Heomiorim", "Rummemos", "Wreassiogana", "Accanara", "Wreoddiaryon"
	};

    private static string[] baseRegionNames = new[] {
        "Atlantis", "Albane", "Alamid", "Anderland", "Aurelia", "Ashanti", "Bathria", "Benin", "Bismuth", "Boron", "Burgundy", "Calabon", "Canton", "Croatia", "Ceres",
        "Darmid", "Delta", "Dirland", "Dohati", "Duscany", "Elarus", "Esmeris", "Erebor", "Emusil", "Erathia", "Folgeron", "Fernica", "Fuchia", "Friedland", "Farrah", "Garwinia", "Grazil", "Gunark",
        "Gehenna", "Gladius", "Hacatid", "Hermani", "Hisoka", "Howland", "Hunan", "Iridia", "Israel", "Indosin", "Illumina", "Jamalaya", "Jinni", "Jorara", "Justinia", "Jelsebed",
        "Karamba", "Kaedwen", "Korolus", "Kumael", "Kestrella", "Losendro", "Liria", "Lunesta", "Lusland", "Lyrica", "Makuria", "Meridian", "Morland", "Muskovich", "Nestori", "Norsica",
        "Nubland", "Naralan", "Nissin", "Obelin", "Ovid", "Opera", "Osmud", "Obelisk", "Patani", "Petersen", "Polaris", "Prissia", "Pusant", "Quatar", "Qualia", "Quenden", "Rastafar", "Remedi", "Russo",
        "Roveri", "Scythra", "Shetland", "Sundira", "Silveria", "Sahari", "Takatak", "Thenid", "Tilain", "Tesoro", "Tulisia", "Umbra", "Ulbany", "Uruk", "Unibad", "Virindor", "Visaya", "Vanad", "Vesemin",
        "Vortex", "Zulu", "Zakrand", "Zerena", "Ziria"
    };
    
    private static string[] minionNames = new[] {
        "Abraxas", "Agares", "Aim", "Alloces", "Amdusias", "Amon", "Amy", "Andras", "Andrealphus", "Adromalius", "Asmodeus",
        "Astaroth", "Azazel", "Baal", "Baphomet", "Barbatos", "Barong", "Bathin", "Balam", "Beleth", "Belial", "Belphegor",
        "Berith", "Bifrons", "Botis", "Buer", "Bune", "Caim", "Choronzon", "Crocell", "Dantalion", "Decarabia", "Demogorgon", "Eligos",
        "Flauros", "Focalor", "Foras", "Forneus", "Furcas", "Furfur", "Gaap", "Gremory", "Glasya-labolas", "Gusion", "Haagenti",
        "Halphas", "Ipos", "Kimaris", "Leraje", "Lucifer", "Malphas", "Marax", "Marbas", "Marchosias", "Murmur", "Naberius", "Orias",
        "Orobas", "Ose", "Paimon", "Phenex", "Purson", "Raum", "Ronove", "Sabnock", "Samigina", "Sallos", "Seere", "Shax", "Sitri", "Stolas",
        "Valac", "Vapula", "Vassago", "Valefor", "Vepar", "Vine", "Vual", "Zepar", "Zagan"
    };

    private static string[] spiderNames = new[] {
        "Glork", "Rakkan", "Bumble", "Lartath", "Typchray", "Kahu", "Kaljou", "Tuvok", "Kyshf", "Palluhae", "Ezeroc",
        "Spidrid", "Slerdach", "Meleth", "Snendos", "Libnrak", "Umusaq", "Serpigo", "Ijushir", "Eshacer", "Alakyrr", "Iguker", "Skirax",
        "Ujarak", "Eruanna", "Annelida", "Naraku", "Xaggavea", "Ilphstra", "Ekicak", "Scissa", "Lakkucoa", "Tsuki", "Incey", "Ahmose", "Aurantia", "Alta", "Hesutu",
        "Anastera", "Krigon", "Seti", "Tal Tal", "Feriave", "Elifif", "Arkaitz", "Adiyis", "Shelob", "Xennowua", "Aine", "Gigit",
    };

    private static string[] faeryFemaleNames = new[] {
        "Lorelie", "Nixie", "Sereia", "Tiana", "Naida", "Melia", "Delphine", "Celeste", "Avery", "Asherah", "Ailsa",
        "Diana", "Cyrena", "Fiona", "Spectra", "Siofra", "Zanna", "Sebille", "Radella", "Oona", "Marigold", "Fayette", "Dariyah",
        "Asteria", "Kaia", "Aurora",
    };

    private static string[] faeryMaleNames = new[] {
        "Caspian", "Arion", "Jareth", "Oberon", "Triton", "Zephyr", "Cosmo", "Aelfdene", "Nyx", "Xantho", "Gullveig",
        "Flynn", "Helio", "Flix", "Cleon", "Lazuli", "Trevan", "Aphid", "Tarragon", "Caraway", "Carpus", "Skylark", "Cirro",
        "Alaneo", "Ginko", "Oleander",
    };

    private static string[] goblinFemaleNames = new[] {
        "Shanxee", "Fegrahx", "Stebdois", "Trahxi", "Shalx", "Cholme", "Gnokesh", "Deevons", "Blahossa", "Voplehx", "Dofil",
        "Iofz", "Mikild", "Trohee", "Onxe", "Wraalta", "Retzaga", "Fegsi", "Thriz", "Dyq", "Oinun", "Gyflult", "Klaasai",
        "Barleeth", "Dyteess",
    };

    private static string[] goblinMaleNames = new[] {
        "Kreelk", "Kohdibs", "Lasdoir", "Ukoc", "Sloikz", "Vreg", "Shapvith", "Voss", "Uvrefz", "Yzenk", "Brirx",
        "Sruiz", "Kegdiart", "Taarsots", "Srurmaar", "Jignierk", "Crezlezz", "Fraatukt", "Zolruirm", "Canralk", "Prevrax", "Hioq", "Zees",
        "Hobigs", "Fizigs",
    };

    private static Sobriquet.Generator generatedHumanSurnames;
    private static Sobriquet.Generator generatedElvenSurnames;
    private static Sobriquet.Generator generatedHumanKingdomNames;
    private static Sobriquet.Generator generatedElvenKingdomNames;
    private static Sobriquet.Generator generatedElvenFemaleNames;
    private static Sobriquet.Generator generatedElvenMaleNames;
    private static Sobriquet.Generator generatedAncientRuinNames;
    private static Sobriquet.Generator generatedTileNames;
    private static Sobriquet.Generator generatedRegionNames;

    private static List<string> humanKingdomNames;
    private static List<string> humanSurnames;
    private static List<string> elvenSurnames;
    private static List<string> elvenKingdomNames;
    private static List<string> elvenFemaleNames;
    private static List<string> elvenMaleNames;
	private static List<string> ancientRuinNames;
	private static List<string> tileNames;
    private static List<string> regionNames;
    private static List<string> availableMinionNames;
    private static List<string> availableSpiderNames;
    private static List<string> availableHumanMaleNames;
    private static List<string> availableHumanFemaleNames;
    private static List<string> availableFaeryFemaleNames;
    private static List<string> availableFaeryMaleNames;
    private static List<string> availableGoblinFemaleNames;
    private static List<string> availableGoblinMaleNames;

    public static void Initialize(){
        generatedHumanSurnames = new Sobriquet.Generator(2, baseHumanSurnames);
        generatedElvenSurnames = new Sobriquet.Generator(2, baseElvenSurnames);
        
        generatedHumanKingdomNames = new Sobriquet.Generator(2, baseHumanKingdomNames);

        generatedElvenKingdomNames = new Sobriquet.Generator(2, baseElvenKingdomNames);
        generatedElvenFemaleNames = new Sobriquet.Generator(2, baseElvenFemaleNames);
        generatedElvenMaleNames = new Sobriquet.Generator(2, baseElvenMaleNames);

		generatedAncientRuinNames = new Sobriquet.Generator (2, baseAncientRuinPrefixes);
		generatedTileNames = new Sobriquet.Generator (2, baseTileNames);

        generatedRegionNames = new Sobriquet.Generator(3, baseRegionNames);

        humanKingdomNames = new List<string>();
        for (int i = 5; i <= 8; i++) {
            humanKingdomNames.AddRange(generatedHumanKingdomNames.AllRaw(i).Take(50000).ToList());
        }
        humanKingdomNames.AddRange(generatedHumanKingdomNames.AllRaw(9).Take(20000).ToList());
        humanKingdomNames.AddRange(generatedHumanKingdomNames.AllRaw(10).Take(20000).ToList());
        humanKingdomNames.AddRange(generatedHumanKingdomNames.AllRaw(11).Take(10000).ToList());
        humanKingdomNames.AddRange(generatedHumanKingdomNames.AllRaw(12).Take(10000).ToList());
        humanKingdomNames = CollectionUtilities.Shuffle(humanKingdomNames);


        humanSurnames = new List<string>();
        for (int i = 5; i <= 6; i++) {
            humanSurnames.AddRange(generatedHumanSurnames.AllRaw(i).Take(50000).ToList());
        }
        humanSurnames.AddRange(generatedHumanSurnames.AllRaw(4).Take(20000).ToList());
        humanSurnames.AddRange(generatedHumanSurnames.AllRaw(5).Take(20000).ToList());
        humanSurnames.AddRange(generatedHumanSurnames.AllRaw(6).Take(10000).ToList());
        humanSurnames = CollectionUtilities.Shuffle(humanSurnames);
        
        elvenSurnames = new List<string>();
        for (int i = 5; i <= 6; i++) {
	        elvenSurnames.AddRange(generatedElvenSurnames.AllRaw(i).Take(50000).ToList());
        }
        elvenSurnames.AddRange(generatedElvenSurnames.AllRaw(4).Take(20000).ToList());
        elvenSurnames.AddRange(generatedElvenSurnames.AllRaw(5).Take(20000).ToList());
        elvenSurnames.AddRange(generatedElvenSurnames.AllRaw(6).Take(10000).ToList());
        elvenSurnames = CollectionUtilities.Shuffle(elvenSurnames);

        elvenKingdomNames = new List<string>();
        for (int i = 5; i <= 8; i++) {
            elvenKingdomNames.AddRange(generatedElvenKingdomNames.AllRaw(i).Take(50000).ToList());
        }
        elvenKingdomNames.AddRange(generatedElvenKingdomNames.AllRaw(9).Take(20000).ToList());
        elvenKingdomNames.AddRange(generatedElvenKingdomNames.AllRaw(10).Take(20000).ToList());
        elvenKingdomNames.AddRange(generatedElvenKingdomNames.AllRaw(11).Take(10000).ToList());
        elvenKingdomNames.AddRange(generatedElvenKingdomNames.AllRaw(12).Take(10000).ToList());
        elvenKingdomNames = CollectionUtilities.Shuffle(elvenKingdomNames);

        elvenFemaleNames = new List<string>();
        for (int i = 5; i <= 7; i++) {
            elvenFemaleNames.AddRange(generatedElvenFemaleNames.AllRaw(i).Take(50000).ToList());
        }
        elvenFemaleNames.AddRange(generatedElvenFemaleNames.AllRaw(8).Take(20000).ToList());
        elvenFemaleNames.AddRange(generatedElvenFemaleNames.AllRaw(9).Take(20000).ToList());
        elvenFemaleNames.AddRange(generatedElvenFemaleNames.AllRaw(10).Take(20000).ToList());
        elvenFemaleNames.AddRange(generatedElvenFemaleNames.AllRaw(11).Take(10000).ToList());
        elvenFemaleNames.AddRange(generatedElvenFemaleNames.AllRaw(12).Take(10000).ToList());
        elvenFemaleNames = CollectionUtilities.Shuffle(elvenFemaleNames);

        elvenMaleNames = new List<string>();
        for (int i = 5; i <= 7; i++) {
            elvenMaleNames.AddRange(generatedElvenMaleNames.AllRaw(i).Take(50000).ToList());
        }
        elvenMaleNames.AddRange(generatedElvenMaleNames.AllRaw(8).Take(20000).ToList());
        elvenMaleNames.AddRange(generatedElvenMaleNames.AllRaw(9).Take(20000).ToList());
        elvenMaleNames.AddRange(generatedElvenMaleNames.AllRaw(10).Take(20000).ToList());
        elvenMaleNames.AddRange(generatedElvenMaleNames.AllRaw(11).Take(10000).ToList());
        elvenMaleNames.AddRange(generatedElvenMaleNames.AllRaw(12).Take(10000).ToList());
        elvenMaleNames = CollectionUtilities.Shuffle(elvenMaleNames);

		ancientRuinNames = new List<string>();
		for (int i = 4; i <= 6; i++) {
			ancientRuinNames.AddRange(generatedAncientRuinNames.AllRaw(i).Take(50000).ToList());
		}
		ancientRuinNames = CollectionUtilities.Shuffle(ancientRuinNames);

		tileNames = new List<string>();
		for (int i = 6; i <= 9; i++) {
			tileNames.AddRange(generatedTileNames.AllRaw(i).Take(20000).ToList());
		}
		tileNames = CollectionUtilities.Shuffle(tileNames);

        regionNames = new List<string>();
        for (int i = 6; i <= 9; i++) {
            regionNames.AddRange(generatedRegionNames.AllRaw(i).Take(20000).ToList());
        }
        regionNames = CollectionUtilities.Shuffle(regionNames);

        availableMinionNames = new List<string>(minionNames);
        availableSpiderNames = new List<string>(spiderNames);
        availableHumanFemaleNames = new List<string>(humanFemaleFirstNames);
        availableHumanMaleNames = new List<string>(humanMaleFirstNames);
        availableFaeryFemaleNames = new List<string>(faeryFemaleNames);
        availableFaeryMaleNames = new List<string>(faeryMaleNames);
        availableGoblinFemaleNames = new List<string>(goblinFemaleNames);
        availableGoblinMaleNames = new List<string>(goblinMaleNames);
    }

    public static string GenerateMinionName() {
        if (availableMinionNames.Count <= 0) {
            availableMinionNames.AddRange(minionNames);
        }
        string chosenName = availableMinionNames[Random.Range(0, availableMinionNames.Count)];
        availableMinionNames.Remove(chosenName);
        return chosenName;
    }

    private static string GenerateSpiderName() {
        string chosenName = availableSpiderNames[Random.Range(0, availableSpiderNames.Count)];
        availableSpiderNames.Remove(chosenName);
        return chosenName;
    }
    private static string GenerateFaeryName(GENDER gender) {
        string chosenName;
        if(gender == GENDER.MALE) {
            if (availableFaeryMaleNames.Count <= 0) {
                availableFaeryMaleNames.AddRange(faeryMaleNames);
            }
            int index = Random.Range(0, availableFaeryMaleNames.Count);
            chosenName = availableFaeryMaleNames[index];
            availableFaeryMaleNames.RemoveAt(index);
        } else {
            if (availableFaeryFemaleNames.Count <= 0) {
                availableFaeryFemaleNames.AddRange(faeryFemaleNames);
            }
            int index = Random.Range(0, availableFaeryFemaleNames.Count);
            chosenName = availableFaeryFemaleNames[index];
            availableFaeryFemaleNames.RemoveAt(index);
        }
        return chosenName;
    }

    private static string GenerateGoblinName(GENDER gender) {
        string chosenName;
        if (gender == GENDER.MALE) {
            if (availableGoblinMaleNames.Count <= 0) {
                availableGoblinMaleNames.AddRange(goblinMaleNames);
            }
            int index = Random.Range(0, availableGoblinMaleNames.Count);
            chosenName = availableGoblinMaleNames[index];
            availableGoblinMaleNames.RemoveAt(index);
        } else {
            if (availableGoblinFemaleNames.Count <= 0) {
                availableGoblinFemaleNames.AddRange(goblinFemaleNames);
            }
            int index = Random.Range(0, availableGoblinFemaleNames.Count);
            chosenName = availableGoblinFemaleNames[index];
            availableGoblinFemaleNames.RemoveAt(index);
        }
        return chosenName;
    }

    public static string GenerateRandomName(RACE race, GENDER gender){
		if (race == RACE.HUMANS) {
			return GenerateWholeHumanName(gender);
		} else if(race == RACE.ELVES) {
			return GenerateWholeElvenName(gender);
		} else if (race == RACE.SPIDER) {
            return GenerateSpiderName();
        } else if (race == RACE.FAERY) {
            return GenerateFaeryName(gender);
        } else if (race == RACE.GOBLIN) {
            return GenerateGoblinName(gender);
        }
        return GenerateElvenName(gender);
	}
	public static string GenerateKingdomName(){
        if(humanKingdomNames.Count <= 0) {
            humanKingdomNames = generatedHumanKingdomNames.AllRaw(12).ToList();
        }
        int index = Random.Range(0, humanKingdomNames.Count);
        string humanKingdomName = humanKingdomNames[index];
        return humanKingdomName.Trim();
	}
	public static string GenerateCityName(RACE race){
        if (race == RACE.HUMANS) {
            if (humanKingdomNames.Count <= 0) {
                humanKingdomNames = generatedHumanKingdomNames.AllRaw(12).ToList();
            }
            int index = Random.Range(0, humanKingdomNames.Count);
            string humanKingdomName = humanKingdomNames[index];
            return humanKingdomName;
        } else if (race == RACE.ELVES) {
            if (elvenKingdomNames.Count <= 0) {
                elvenKingdomNames = generatedElvenKingdomNames.AllRaw(12).ToList();
            }
            int index = Random.Range(0, elvenKingdomNames.Count);
            string elvenKingdomName = elvenKingdomNames[index];
            return elvenKingdomName;
        }
        return "";
	}

	private static string GenerateElvenName(GENDER gender){
		if (gender == GENDER.MALE) {
            if (elvenMaleNames.Count <= 0) {
                elvenMaleNames = generatedElvenMaleNames.AllRaw(12).ToList();
            }
            int index = Random.Range(0, elvenMaleNames.Count);
            string elvenMaleName = elvenMaleNames[index];
            return elvenMaleName.Trim();
		} else {
            if (elvenFemaleNames.Count <= 0) {
                elvenFemaleNames = generatedElvenFemaleNames.AllRaw(12).ToList();
            }
            int index = Random.Range(0, elvenFemaleNames.Count);
            string elvenFemaleName = elvenFemaleNames[index];
            return elvenFemaleName.Trim();
		}
	}
	private static string GetElvenSurname(){
		if (elvenSurnames.Count <= 0) {
			elvenSurnames = generatedElvenSurnames.AllRaw(12).ToList();
		}
		int index = Random.Range(0, elvenSurnames.Count);
		string surname = elvenSurnames[index];
		return surname;
	}


	private static string GenerateWholeHumanName(GENDER gender){
		string firstName = GetHumanFirstName(gender);
		string surname = GetHumanSurname ();
		return $"{firstName} {surname}";
	}
	private static string GenerateWholeElvenName(GENDER gender){
		string firstName = GenerateElvenName(gender);
		string surname = GetElvenSurname();
		return $"{firstName} {surname}";
	}

	private static string GetHumanSurname(){
        if (humanSurnames.Count <= 0) {
            humanSurnames = generatedHumanSurnames.AllRaw(12).ToList();
        }
        int index = Random.Range(0, humanSurnames.Count);
        string humanSurname = humanSurnames[index];
        return humanSurname;
	}

	private static string GetHumanFirstName(GENDER gender){
		if (gender == GENDER.MALE) {
            string chosenName = availableHumanMaleNames[Random.Range(0, availableHumanMaleNames.Count)];
            availableHumanMaleNames.Remove(chosenName);
            if (availableHumanMaleNames.Count == 0) {
                availableHumanMaleNames = new List<string>(humanMaleFirstNames);
            }
            return chosenName;
		} else {
            string chosenName = availableHumanFemaleNames[Random.Range(0, availableHumanFemaleNames.Count)];
            availableHumanFemaleNames.Remove(chosenName);
            if (availableHumanFemaleNames.Count == 0) {
                availableHumanFemaleNames = new List<string>(humanFemaleFirstNames);
            }
            return chosenName;
        }
	}
	public static string GetLandmarkName(LANDMARK_TYPE landmarkType) {
        return GetAncientRuinName();
    }

	private static string GetAncientRuinName(){
		if(ancientRuinNames.Count <= 0) {
			ancientRuinNames = generatedAncientRuinNames.AllRaw(6).ToList();
		}
		int index = Random.Range (0, ancientRuinNames.Count);
		string name = ancientRuinNames [index];
		ancientRuinNames.RemoveAt (index);
		return name + baseAncientRuinSuffixes [Random.Range (0, baseAncientRuinSuffixes.Length)];
	}
	public static string GetTileName(){
		if(tileNames.Count <= 0) {
			tileNames = generatedTileNames.AllRaw(6).ToList();
		}
		int index = Random.Range (0, tileNames.Count);
		string name = tileNames [index];
		tileNames.RemoveAt (index);
		return name;
	}
    public static string GetRegionName() {
        if (regionNames.Count <= 0) {
            regionNames = generatedRegionNames.AllRaw(6).ToList();
        }
        int index = Random.Range(0, regionNames.Count);
        string name = regionNames[index];
        regionNames.RemoveAt(index);
        return name;
    }

    public static void RemoveNameAsAvailable(GENDER gender, RACE race, string name) {
        switch (race) {
            case RACE.HUMANS:
                if (gender == GENDER.MALE) {
                    availableHumanMaleNames.Remove(name);
                }else {
                    availableHumanFemaleNames.Remove(name);
                }
                break;
        }
    }
}



