# bodacc
scanne le bodacc pour y trouver les annonces / statuts des PSE qui à procédure de sauvegarde

# developer
- installer vscode
- installer docker
- installer l'extension devcontainer
- ouvrir le dossier dans un conteneur (open locally + CTRL MAJ P -> remote containers)
- create a sqlite table with scheme
    ``` 
   create table annonces(
   ...> ID INT PRIMARY KEY NOT NULL,
   ...> NUMERO TEXT,
   ...> DATE TEXT,
   ...> ADDRESS TEXT,
   ...> NATURE TEXT,
   ...> RCS TEXT,
   ...> TYPE TEXT,
   ...> PREVIOUS TEXT,
   ...> COMPLEMENT TEXT)
```
- build : dotnet build
- run   : dotnet run